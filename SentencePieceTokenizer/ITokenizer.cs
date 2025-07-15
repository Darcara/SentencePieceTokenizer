namespace SentencePieceTokenizer;

using System;

/// <summary>
/// Common interface for all tokenizers, that convert a string into a span of integers, or vice versa.
/// </summary>
public interface ITokenizer<TOutput> : IDisposable where TOutput : struct {
	/// <summary>
	/// The UNK token
	/// </summary>
	public TOutput UnknownToken { get; init; }

	/// <summary>
	/// BOS Token, sometimes known as CLS
	/// </summary>
	public TOutput BeginOfSentenceToken { get; init; }

	/// <summary>
	/// EOS Token, sometimes known as SEP
	/// </summary>
	public TOutput EndOfSentenceToken { get; init; }

	/// <summary>
	/// The PAD token
	/// </summary>
	public TOutput PadToken { get; init; }

	/// <summary>
	/// The MASK token, if available
	/// </summary>
	public TOutput MaskToken { get; init; }

	/// <summary>
	/// Total number of tokens in the loaded model
	/// </summary>
	public Int32 NumberOfTokens { get; init; }

	/// <summary>
	/// Encodes a String into an array of String-tokens. The result will contain the sentencepiece '▁' for starting words.
	/// </summary>
	/// <remarks>This is usually not that helpful, but aids in debugging.</remarks>
	public String[] EncodeToStrings(ReadOnlySpan<Char> text);

	/// <summary>
	/// Encodes an utf8 byte string into an array of String-tokens. The result will contain the sentencepiece '▁' for starting words.
	/// </summary>
	/// <remarks>This is usually not that helpful, but aids in debugging.</remarks>
	public String[] EncodeToStrings(ReadOnlySpan<Byte> utf8Bytes);

	/// <summary>
	/// Encodes a string into tokens
	/// </summary>
	/// <param name="text">The string to encode.</param>
	/// <param name="prefixIds">Id's that will be at the start of the resulting token array, directly before the tokenized string. Usually &lt;s&gt; / BOS / <see cref="BeginOfSentenceToken"/>. Default: none</param>
	/// <param name="suffixIds">Id's that will be at the end of the resulting token array, directly after the tokenized string. Usually &lt;/s&gt; / EOS / <see cref="EndOfSentenceToken"/>. Default: none</param>
	public TOutput[] EncodeToIds(ReadOnlySpan<Char> text, ReadOnlySpan<TOutput> prefixIds = default, ReadOnlySpan<TOutput> suffixIds = default);

	/// <summary>
	/// Encodes an utf8 byte string into tokens
	/// </summary>
	/// <param name="utf8Bytes">The utf8 byte string to encode.</param>
	/// <param name="prefixIds">Id's that will be at the start of the resulting token array, directly before the tokenized string. Usually &lt;s&gt;, BOS, or <see cref="BeginOfSentenceToken"/>. Default: none</param>
	/// <param name="suffixIds">Id's that will be at the end of the resulting token array, directly after the tokenized string. Usually &lt;/s&gt;, EOS, or <see cref="EndOfSentenceToken"/>. Default: none</param>
	public TOutput[] EncodeToIds(ReadOnlySpan<Byte> utf8Bytes, ReadOnlySpan<TOutput> prefixIds = default, ReadOnlySpan<TOutput> suffixIds = default);

	/// <summary>
	/// Encodes an utf8 byte string into tokens and <see cref="TokenSpan"/>s that contain the start and end indices for each token.
	/// </summary>
	public (TOutput[] ids, TokenSpan[] tokens) EncodeToSpans(ReadOnlySpan<Byte> utf8Bytes);

	/// <summary>
	/// Decodes a single token id to its string representation. The result will not contain the sentencepiece '▁' for starting words. 
	/// </summary>
	public String Decode(TOutput id);

	/// <summary>
	/// Decodes a span of token ids into its string representation. The result will not contain the sentencepiece '▁' for starting words.
	/// </summary>
	public String Decode(ReadOnlySpan<TOutput> ids);

	/// <summary>
	/// Decodes a span of <see cref="TokenSpan"/>s into its string representation, using the provided utf8 byte string.
	/// </summary>
	/// <remarks>the token-ids will be ignored</remarks>
	/// <seealso cref="Decode(ReadOnlySpan{TokenSpan}, ReadOnlySpan{Byte})"/>
	public String Decode((TOutput[] ids, TokenSpan[] tokens) tuple, ReadOnlySpan<Byte> utf8Bytes);

	/// <summary>
	/// Decodes a span of <see cref="TokenSpan"/>s into its string representation, using the provided utf8 byte string.
	/// </summary>
	public String Decode(ReadOnlySpan<TokenSpan> tokens, ReadOnlySpan<Byte> utf8Bytes);
}