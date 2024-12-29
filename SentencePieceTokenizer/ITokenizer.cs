namespace SentencePieceTokenizer;

using System;

public interface ITokenizer<TOutput> : IDisposable where TOutput : struct{
	public TOutput UnknownToken { get; init; }
	/// <summary>
	/// BOS Token, sometimes known as CLS
	/// </summary>
	public TOutput BeginOfSentenceToken { get; init; }
	/// <summary>
	/// EOS Token, sometimes known as SEP
	/// </summary>
	public TOutput EndOfSentenceToken { get; init; }
	public TOutput PadToken { get; init; }
	public TOutput MaskToken { get; init; }
	public Int32 NumberOfTokens { get; init; }
	
	public String[] EncodeToStrings(ReadOnlySpan<Char> text);
	public String[] EncodeToStrings(ReadOnlySpan<Byte> utf8Bytes);
	public TOutput[] EncodeToIds(ReadOnlySpan<Char> text, ReadOnlySpan<TOutput> prefixIds = default, ReadOnlySpan<TOutput> suffixIds = default);
	public TOutput[] EncodeToIds(ReadOnlySpan<Byte> utf8Bytes, ReadOnlySpan<TOutput> prefixIds = default, ReadOnlySpan<TOutput> suffixIds = default);
	public (TOutput[] ids, TokenSpan[] tokens) EncodeToSpans(ReadOnlySpan<Byte> utf8Bytes);

	public String Decode(TOutput id);
	public String Decode(ReadOnlySpan<TOutput> ids);
	
	public String Decode((TOutput[] ids, TokenSpan[] tokens) tuple, ReadOnlySpan<Byte> utf8Bytes);
	public String Decode(ReadOnlySpan<TokenSpan> tokens, ReadOnlySpan<Byte> utf8Bytes);
}