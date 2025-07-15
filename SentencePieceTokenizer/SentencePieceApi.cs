namespace SentencePieceTokenizer;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

[SuppressMessage("Security", "CA5393:Do not use unsafe DllImportSearchPath value")]
internal static partial class SentencePieceApi {
	internal const String LibraryName = "SentencePieceWrapper";
	
	/// <summary>
	/// Multiple string-tokens are delimited by space when copied from the native library to dotnet 
	/// </summary>
	internal const Byte TokenizationDelimiter = (Byte)' ';

	/// <summary>
	/// Tokens that start a new word are prefixed with '▁'
	/// </summary>
	internal const Char TokenWordStartPrefix = '\u2581';

	/// <seealso href="https://github.com/google/sentencepiece/blob/master/third_party/protobuf-lite/google/protobuf/stubs/status.h"/>
	internal enum StatusCode {
		Ok = 0,
		Cancelled = 1,
		Unknown = 2,
		InvalidArgument = 3,
		DeadlineExceeded = 4,
		NotFound = 5,
		AlreadyExists = 6,
		PermissionDenied = 7,
		ResourceExhausted = 8,
		FailedPrecondition = 9,
		Aborted = 10,
		OutOfRange = 11,
		Unimplemented = 12,
		Internal = 13,
		Unavailable = 14,
		DataLoss = 15,
		Unauthenticated = 16,
	}

	[LibraryImport(LibraryName)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
	public static partial IntPtr CreateProcessor();

	[LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
	public static partial StatusCode LoadModel(IntPtr processorHandle, String filename, out Int32 bos, out Int32 eos, out Int32 pad, out Int32 unk, out Int32 size);

	[LibraryImport(LibraryName)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
	public static partial StatusCode EncodeAsSpans(IntPtr processorHandle, IntPtr input, Int32 inputLength, IntPtr outputIds, IntPtr outputSpans, Int32 capacity, out Int32 length);

	[LibraryImport(LibraryName)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
	public static partial StatusCode EncodeAsIds(IntPtr processorHandle, IntPtr input, Int32 inputLength, IntPtr output, Int32 capacity, out Int32 length);

	[LibraryImport(LibraryName)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
	public static partial StatusCode EncodeAsPieces(IntPtr processorHandle, IntPtr input, Int32 inputLength, IntPtr output, Int32 capacity, out Int32 length, out Int32 numberOfTokens);

	[LibraryImport(LibraryName)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
	public static partial StatusCode DecodeIds(IntPtr processorHandle, IntPtr input, Int32 numberOfIds, IntPtr output, Int32 capacity, out Int32 length);
	
	[LibraryImport(LibraryName)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
	public static partial void DisposeProcessor(IntPtr processorHandle);

	public static unsafe IntPtr ConvertStringToNativeUtf8(ReadOnlySpan<Char> managedString, out Int32 numBytes) {
		// from Utf8StringMarshaller.ConvertToUnmanaged but with processHeap instead of COM-Heap
		Int32 exactByteCount = Encoding.UTF8.GetByteCount(managedString) + 1; // + 1 for null terminator
		Byte* mem = (Byte*)Marshal.AllocHGlobal(exactByteCount);
		Span<Byte> buffer = new(mem, exactByteCount);

		Int32 byteCount = Encoding.UTF8.GetBytes(managedString, buffer);
		buffer[byteCount] = 0;
		numBytes = byteCount;
		return new IntPtr(mem);
	}
}