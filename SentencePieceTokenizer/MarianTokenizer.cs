namespace SentencePieceTokenizer;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf;
using Sentencepiece;

/// <summary>
/// Utility class for JSON (de)serialization source generation.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(Dictionary<String, Int64>))]
internal partial class VocabularySerializerContext : JsonSerializerContext;


public sealed class MarianTokenizer : ITokenizer<Int64> {
	private readonly SentencePieceTokenizer _baseTokenizer;
	private readonly String[] _idToTokenLookup;
	private readonly Int64[] _idToIdLookup;
	/// <inheritdoc />	
	public Int64 UnknownToken { get; init; }
	/// <inheritdoc />
	public Int64 BeginOfSentenceToken { get; init; } = -1;
	/// <inheritdoc />
	public Int64 EndOfSentenceToken { get; init; }
	/// <inheritdoc />
	public Int64 PadToken { get; init; }
	/// <inheritdoc />
	public Int64 MaskToken { get; init; } = -1;
	/// <inheritdoc />
	public Int32 NumberOfTokens { get; init; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="modelFile"></param>
	/// <param name="vocabFile"></param>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="FileNotFoundException"></exception>
	public MarianTokenizer(String modelFile, String vocabFile) {
		ArgumentException.ThrowIfNullOrEmpty(vocabFile);
		ArgumentException.ThrowIfNullOrEmpty(modelFile);
		if (!File.Exists(modelFile)) throw new FileNotFoundException($"Model file not found: {modelFile} in {Path.GetFullPath(modelFile)}", modelFile);

		_baseTokenizer = new SentencePieceTokenizer(modelFile);

		Dictionary<String, Int64> vocabularyDictionary = JsonSerializer.Deserialize(File.ReadAllBytes(vocabFile), VocabularySerializerContext.Default.DictionaryStringInt64) ?? throw new ArgumentException("Unable to load vocabulary from file", nameof(vocabFile));
		_idToTokenLookup = new String[vocabularyDictionary.Max(kv => kv.Value) + 1];
		foreach ((String key, Int64 value) in vocabularyDictionary) {
			_idToTokenLookup[value] = key;
		}

		NumberOfTokens = vocabularyDictionary.Count;

		UnknownToken = vocabularyDictionary["<unk>"];
		EndOfSentenceToken = vocabularyDictionary["</s>"];
		PadToken = vocabularyDictionary["<pad>"];

		_idToIdLookup = new Int64[vocabularyDictionary.Count];
		using FileStream fileStream = File.OpenRead(modelFile);
		ModelProto modelProto = Serializer.Deserialize(fileStream, default(ModelProto)) ?? throw new ArgumentException("Unable to load model from file", nameof(modelFile));
		for (Int32 index = 0; index < modelProto.Pieces.Count; index++) {
			ModelProto.SentencePiece modelProtoPiece = modelProto.Pieces[index];
			if (modelProtoPiece.type != ModelProto.SentencePiece.Type.Normal) {
				_idToIdLookup[index] = UnknownToken;
			} else
				_idToIdLookup[index] = vocabularyDictionary.GetValueOrDefault(modelProtoPiece.Piece, UnknownToken);
		}
	}

	#region Implementation of ITokenizer<long>

	/// <inheritdoc />
	public String[] EncodeToStrings(ReadOnlySpan<Char> text) => _baseTokenizer.EncodeToStrings(text);

	/// <inheritdoc />
	public String[] EncodeToStrings(ReadOnlySpan<Byte> utf8Bytes) => _baseTokenizer.EncodeToStrings(utf8Bytes);

	private Int64[] BaseIdsToIds(Int32[] baseTokenIds, ReadOnlySpan<Int64> prefixIds, ReadOnlySpan<Int64> suffixIds) {
		Int64[] encodedIds = new Int64[prefixIds.Length + baseTokenIds.Length + suffixIds.Length];
		for (Int32 index = 0; index < baseTokenIds.Length; index++) {
			encodedIds[index + prefixIds.Length] = _idToIdLookup[baseTokenIds[index]];
		}

		if (prefixIds.Length > 0) prefixIds.CopyTo(encodedIds.AsSpan());
		if (suffixIds.Length > 0) suffixIds.CopyTo(encodedIds.AsSpan(prefixIds.Length + baseTokenIds.Length));

		return encodedIds;
	}

	/// <inheritdoc />
	public Int64[] EncodeToIds(ReadOnlySpan<Char> text, ReadOnlySpan<Int64> prefixIds = default, ReadOnlySpan<Int64> suffixIds = default) {
		Int32[] baseTokenIds = _baseTokenizer.EncodeToIds(text);
		return BaseIdsToIds(baseTokenIds, prefixIds, suffixIds);
	}

	/// <inheritdoc />
	public Int64[] EncodeToIds(ReadOnlySpan<Byte> utf8Bytes, ReadOnlySpan<Int64> prefixIds = default, ReadOnlySpan<Int64> suffixIds = default) {
		Int32[] baseTokenIds = _baseTokenizer.EncodeToIds(utf8Bytes);
		return BaseIdsToIds(baseTokenIds, prefixIds, suffixIds);
	}

	/// <inheritdoc />
	public String Decode(Int64 id) {
		if (id < 0 || id > NumberOfTokens) throw new ArgumentOutOfRangeException(nameof(id), id, $"Id({id}) must be 0 or greater and less than: {NumberOfTokens}");
		return _idToTokenLookup[id];
	}

	/// <inheritdoc />
	public String Decode(ReadOnlySpan<Int64> ids) {
		StringBuilder sb = new();
		for (Int32 index = 0; index < ids.Length; ++index) {
			String token = Decode(ids[index]);
			if (token.StartsWith(SentencePieceApi.TokenWordStartPrefix)) {
				// Append a space, but not at the beginning
				if (sb.Length > 0) sb.Append(' ');
				sb.Append(token.AsSpan(1));
			} else {
				sb.Append(token);
			}
		}

		return sb.ToString();
	}

	/// <inheritdoc />
	public (Int64[] ids, TokenSpan[] tokens) EncodeToSpans(ReadOnlySpan<Byte> utf8Bytes) {
		(Int32[] ids, TokenSpan[] tokens) = _baseTokenizer.EncodeToSpans(utf8Bytes);
		Int64[] properIds = new Int64[ids.Length];

		for (Int32 index = 0; index < properIds.Length; index++) {
			// properIds[index] = _tokenToIdMap.GetValueOrDefault(stringTokens[index], UnknownToken);
			properIds[index] = _idToIdLookup[ids[index]];
		}

		return (properIds, tokens);
	}

	/// <inheritdoc />
	public String Decode((Int64[] ids, TokenSpan[] tokens) tuple, ReadOnlySpan<Byte> utf8Bytes) => Decode(tuple.tokens, utf8Bytes);

	/// <inheritdoc />
	public String Decode(ReadOnlySpan<TokenSpan> tokens, ReadOnlySpan<Byte> utf8Bytes) => _baseTokenizer.Decode(tokens, utf8Bytes);

	#endregion

	#region IDisposable

	[SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = "May happen in .ctor-Exception")]
	private void ReleaseUnmanagedResources() {
		_baseTokenizer?.Dispose();
	}

	/// <inheritdoc />
	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	~MarianTokenizer() {
		ReleaseUnmanagedResources();
	}

	#endregion
}