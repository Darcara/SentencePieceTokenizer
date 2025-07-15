namespace SentencePieceTokenizer;

using System;

/// <summary>
/// The default XlmRoberta Tokenizer
/// </summary>
/// <seealso href="https://huggingface.co/FacebookAI/xlm-roberta-base/resolve/main/sentencepiece.bpe.model?download=true">Official xlm roberta repository</seealso>
public class XlmRobertaTokenizer : SentencePieceTokenizer {
	/// <inheritdoc />
	public XlmRobertaTokenizer(String modelFile) : base(modelFile) {
		BeginOfSentenceToken = 0; // == <s>
		PadToken = 1; // == <pad>
		EndOfSentenceToken = 2; // == </s>
		UnknownToken = 3; // == <unk>
		MaskToken = 250001;
	}
}