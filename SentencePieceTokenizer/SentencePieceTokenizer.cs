namespace SentencePieceTokenizer;

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// 
/// </summary>
/// <remarks>Should be thread-safe, as SentencePiece itself is thread-safe, but that has not been extensivly tested.</remarks>
[SuppressMessage("Naming", "CA1724:Type names should not match namespaces")]
public class SentencePieceTokenizer : ITokenizer<Int32> {
	private IntPtr _processorHandle;
	/// <inheritdoc />
	public Int32 UnknownToken { get; init; }
	/// <inheritdoc />
	public Int32 BeginOfSentenceToken { get; init; }
	/// <inheritdoc />
	public Int32 EndOfSentenceToken { get; init; }
	/// <inheritdoc />
	public Int32 PadToken { get; init; }
	/// <inheritdoc />
	public Int32 MaskToken { get; init; }
	/// <inheritdoc />
	public Int32 NumberOfTokens { get; init; }

	public SentencePieceTokenizer(String modelFile) {
		ArgumentException.ThrowIfNullOrEmpty(modelFile);

		_processorHandle = SentencePieceApi.CreateProcessor();
		if (_processorHandle == IntPtr.Zero) {
			Dispose();
			throw new InvalidOperationException("Error occurred while creating sentence piece processor.");
		}

		SentencePieceApi.StatusCode code = SentencePieceApi.LoadModel(_processorHandle, modelFile, out Int32 bos, out Int32 eos, out Int32 pad, out Int32 unk, out Int32 size);
		if (code != SentencePieceApi.StatusCode.Ok) {
			Dispose();
			throw new InvalidOperationException($"Error '{code}' occurred while loading sentence piece model: {modelFile}");
		}

		MaskToken = -1;
		UnknownToken = unk;
		BeginOfSentenceToken = bos;
		EndOfSentenceToken = eos;
		PadToken = pad;
		NumberOfTokens = size;
	}

	private Byte[] TokenizeToDelimitedUtf8(ReadOnlySpan<Char> data, out Int32 length, out Int32 numberOfTokens) {
		IntPtr inputPtr = SentencePieceApi.ConvertStringToNativeUtf8(data, out Int32 numBytes);
		try {
			return TokenizeToDelimitedUtf8(inputPtr, numBytes, out length, out numberOfTokens);
		}
		finally {
			Marshal.FreeHGlobal(inputPtr);
		}
	}

	private unsafe Byte[] TokenizeToDelimitedUtf8(ReadOnlySpan<Byte> utf8Bytes, out Int32 length, out Int32 numberOfTokens) {
		fixed (Byte* ptr = utf8Bytes) {
			return TokenizeToDelimitedUtf8(new IntPtr(ptr), utf8Bytes.Length, out length, out numberOfTokens);
		}
	}

	private Byte[] TokenizeToDelimitedUtf8(IntPtr inputPtr, Int32 numberOfBytes, out Int32 length, out Int32 numberOfTokens) {
		// on average one token encodes more than 3 characters of text, but here we have to account for the space-delimiter and the token prefix
		// That would be numberOfBytes + numberOfBytes / 3 * 2, but for ease of use we simply allocate double the amount of bytes as the input.
		Int32 capacity = Math.Max(512, numberOfBytes * 2);
		IntPtr outputPtr = Marshal.AllocHGlobal(capacity);
		try {
			SentencePieceApi.StatusCode code = SentencePieceApi.EncodeAsPieces(_processorHandle, inputPtr, numberOfBytes, outputPtr, capacity, out length, out numberOfTokens);
			if (code != SentencePieceApi.StatusCode.Ok)
				throw new InvalidOperationException($"Error occurred while encoding, code: {code}.");

			// Resize if initial estimate was too low
			if (length > capacity) {
				capacity = length;
				outputPtr = Marshal.ReAllocHGlobal(outputPtr, capacity);
				code = SentencePieceApi.EncodeAsPieces(_processorHandle, inputPtr, numberOfBytes, outputPtr, capacity, out length, out numberOfTokens);
				if (code != SentencePieceApi.StatusCode.Ok)
					throw new InvalidOperationException($"Error occurred while encoding, code: {code}.");
			}

			if (numberOfTokens == 0) return [];
			Byte[] buffer = ArrayPool<Byte>.Shared.Rent(length);
			Marshal.Copy(outputPtr, buffer, 0, length);

			// TODO inline EncodeToStrings if possible to save the array creation and copy

			return buffer;
		}
		finally {
			Marshal.FreeHGlobal(outputPtr);
		}
	}

	private static String[] SplitTokenizedUtf8IntoStrings(ReadOnlySpan<Byte> buffer, Int32 numberOfTokens) {
		// The following is basically
		// return length == 0 || numberOfTokens == 0 ? [] : Encoding.UTF8.GetString(buffer, 0, length).Split(' ');
		String[] ret = new String[numberOfTokens];

		Int32 offset = 0;
		const Byte delim = SentencePieceApi.TokenizationDelimiter;
		for (Int32 i = 0; i < numberOfTokens; ++i) {
			Int32 idx = buffer.Slice(offset).IndexOf(delim);
			if (idx == -1) idx = buffer.Length - offset;
			ret[i] = Encoding.UTF8.GetString(buffer.Slice(offset, idx));
			offset += idx + 1;
		}

		return ret;
	}

	#region Implementation of ITokenizer<int>

	/// <inheritdoc />
	public String[] EncodeToStrings(ReadOnlySpan<Char> text) {
		Byte[] buffer = TokenizeToDelimitedUtf8(text, out Int32 length, out Int32 numberOfTokens);
		String[] encoded = SplitTokenizedUtf8IntoStrings(buffer.AsSpan(0, length), numberOfTokens);
		ArrayPool<Byte>.Shared.Return(buffer);
		return encoded;
	}

	/// <inheritdoc />
	public String[] EncodeToStrings(ReadOnlySpan<Byte> utf8Bytes) {
		Byte[] buffer = TokenizeToDelimitedUtf8(utf8Bytes, out Int32 length, out Int32 numberOfTokens);
		String[] encoded = SplitTokenizedUtf8IntoStrings(buffer.AsSpan(0, length), numberOfTokens);
		ArrayPool<Byte>.Shared.Return(buffer);
		return encoded;
	}

	/// <inheritdoc />
	public Int32[] EncodeToIds(ReadOnlySpan<Char> text, ReadOnlySpan<Int32> prefixIds = default, ReadOnlySpan<Int32> suffixIds = default) {
		IntPtr inputPtr = SentencePieceApi.ConvertStringToNativeUtf8(text, out Int32 numBytes);
		try {
			return InternalEncodeToIds(inputPtr, numBytes, prefixIds, suffixIds);
		}
		finally {
			Marshal.FreeHGlobal(inputPtr);
		}
	}

	/// <inheritdoc />
	public unsafe Int32[] EncodeToIds(ReadOnlySpan<Byte> utf8Bytes, ReadOnlySpan<Int32> prefixIds = default, ReadOnlySpan<Int32> suffixIds = default) {
		fixed (Byte* ptr = utf8Bytes) {
			return InternalEncodeToIds(new IntPtr(ptr), utf8Bytes.Length, prefixIds, suffixIds);
		}
	}

	private Int32[] InternalEncodeToIds(IntPtr inputPtr, Int32 numBytes, ReadOnlySpan<Int32> prefixIds, ReadOnlySpan<Int32> suffixIds) {
		// on average one token encodes more than 3 characters of text
		Int32 capacityItems = numBytes / 3;
		Int32 capacityBytes = Math.Max(512, capacityItems * sizeof(Int32));
		IntPtr outputPtr = Marshal.AllocHGlobal(capacityBytes);
		try {
			SentencePieceApi.StatusCode code = SentencePieceApi.EncodeAsIds(_processorHandle, inputPtr, numBytes, outputPtr, capacityItems, out Int32 length);
			if (code != SentencePieceApi.StatusCode.Ok)
				throw new InvalidOperationException($"Error occurred while encoding, code: {code}.");

			// Resize if initial estimate was too low
			if (length > capacityItems) {
				capacityItems = length;
				capacityBytes = capacityItems * sizeof(Int32);
				outputPtr = Marshal.ReAllocHGlobal(outputPtr, capacityBytes);
				code = SentencePieceApi.EncodeAsIds(_processorHandle, inputPtr, numBytes, outputPtr, capacityItems, out length);
				if (code != SentencePieceApi.StatusCode.Ok)
					throw new InvalidOperationException($"Error occurred while encoding, code: {code}.");
			}

			if (length == 0) return [];
			Int32[] buffer = new Int32[prefixIds.Length + length + suffixIds.Length];
			Marshal.Copy(outputPtr, buffer, prefixIds.Length, length);
			if (prefixIds.Length > 0) prefixIds.CopyTo(buffer.AsSpan());
			if (suffixIds.Length > 0) suffixIds.CopyTo(buffer.AsSpan(prefixIds.Length + length));

			return buffer;
		}
		finally {
			Marshal.FreeHGlobal(outputPtr);
		}
	}

	/// <inheritdoc />
	public String Decode(Int32 id) => Decode([id]);

	/// <inheritdoc />
	public unsafe String Decode(ReadOnlySpan<Int32> ids) {
		fixed (Int32* idPtr = ids) {
			IntPtr idPtrForInterop = new(idPtr);
			Int32 capacity = Math.Max(512, ids.Length * 4);
			IntPtr outputPtr = Marshal.AllocHGlobal(capacity);
			try {
				SentencePieceApi.StatusCode code = SentencePieceApi.DecodeIds(_processorHandle, idPtrForInterop, ids.Length, outputPtr, capacity, out Int32 length);
				if (code != SentencePieceApi.StatusCode.Ok)
					throw new InvalidOperationException($"Error occurred while decoding, code: {code}.");

				// Resize if initial estimate was too low
				if (length > capacity) {
					capacity = length;
					outputPtr = Marshal.ReAllocHGlobal(outputPtr, capacity);
					code = SentencePieceApi.DecodeIds(_processorHandle, idPtrForInterop, ids.Length, outputPtr, capacity, out length);
					if (code != SentencePieceApi.StatusCode.Ok)
						throw new InvalidOperationException($"Error occurred while decoding, code: {code}.");
				}

				return Encoding.UTF8.GetString((Byte*)outputPtr.ToPointer(), length);
			}
			finally {
				Marshal.FreeHGlobal(outputPtr);
			}
		}
	}

	/// <inheritdoc />
	public unsafe (Int32[] ids, TokenSpan[] tokens) EncodeToSpans(ReadOnlySpan<Byte> utf8Bytes) {
		fixed (Byte* inputPtr = utf8Bytes) {
			IntPtr inputPtrForInterop = new(inputPtr);
			// on average one token encodes more than 3 characters of text
			Int32 capacityItems = utf8Bytes.Length / 3;
			Int32 capacityBytes = Math.Max(512, capacityItems * (sizeof(TokenSpan) + sizeof(Int32)));
			IntPtr outputPtr = Marshal.AllocHGlobal(capacityBytes);
			try {
				SentencePieceApi.StatusCode code = SentencePieceApi.EncodeAsSpans(_processorHandle, inputPtrForInterop, utf8Bytes.Length, outputPtr, outputPtr + capacityItems * sizeof(Int32), capacityItems, out Int32 length);
				if (code != SentencePieceApi.StatusCode.Ok)
					throw new InvalidOperationException($"Error occurred while encoding, code: {code}.");

				// Resize if initial estimate was too low
				if (length > capacityItems) {
					capacityItems = length;
					capacityBytes = length * (sizeof(TokenSpan) + sizeof(Int32));
					outputPtr = Marshal.ReAllocHGlobal(outputPtr, capacityBytes);
					code = SentencePieceApi.EncodeAsSpans(_processorHandle, inputPtrForInterop, utf8Bytes.Length, outputPtr, outputPtr + capacityItems * sizeof(Int32), capacityItems, out length);
					if (code != SentencePieceApi.StatusCode.Ok)
						throw new InvalidOperationException($"Error occurred while encoding, code: {code}.");
				}

				if (length == 0) return ([], []);
				Int32[] ids = new Int32[length];
				TokenSpan[] tokens = new TokenSpan[length];
				fixed (Int32* ptr = ids) {
					Buffer.MemoryCopy(outputPtr.ToPointer(), ptr, length * sizeof(Int32), length * sizeof(Int32));
				}

				fixed (TokenSpan* ptr = tokens) {
					Buffer.MemoryCopy((outputPtr + capacityItems * sizeof(Int32)).ToPointer(), ptr, length * sizeof(TokenSpan), length * sizeof(TokenSpan));
				}

				return (ids, tokens);
			}
			finally {
				Marshal.FreeHGlobal(outputPtr);
			}
		}
	}

	/// <inheritdoc />
	public String Decode((Int32[] ids, TokenSpan[] tokens) tuple, ReadOnlySpan<Byte> utf8Bytes) => Decode(tuple.tokens, utf8Bytes);

	/// <inheritdoc />
	public String Decode(ReadOnlySpan<TokenSpan> tokens, ReadOnlySpan<Byte> utf8Bytes) {
		// 1024 should be large enough for the largest token
		Char[] buffer = ArrayPool<Char>.Shared.Rent(1024);
		Span<Char> span = buffer.AsSpan();
		StringBuilder builder = new(tokens.Length * 3);
		for (Int32 index = 0; index < tokens.Length; index++) {
			if (tokens[index].AsChars(utf8Bytes, span, out Int32 charsWritten))
				builder.Append(span.Slice(0, charsWritten));
		}

		ArrayPool<Char>.Shared.Return(buffer);
		return builder.ToString();
	}

	#endregion

	#region IDisposable

	private void ReleaseUnmanagedResources() {
		IntPtr handle = _processorHandle;
		_processorHandle = IntPtr.Zero;
		if (handle != IntPtr.Zero)
			SentencePieceApi.DisposeProcessor(_processorHandle);
	}
	
	/// <inheritdoc cref="IDisposable.Dispose"/>
	protected virtual void Dispose(Boolean disposing) {
		ReleaseUnmanagedResources();
	}

	/// <inheritdoc />
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	~SentencePieceTokenizer() {
		Dispose(false);
	}

	#endregion
}