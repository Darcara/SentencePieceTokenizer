namespace SentencePieceTokenizer;

using System;

public class XlmRobertaTokenizer : SentencePieceTokenizer{
	/// <inheritdoc />
	public XlmRobertaTokenizer(String modelFile) : base(modelFile) {
		
		BeginOfSentenceToken = 0; // == <s>
		PadToken = 1; // == <pad>
		EndOfSentenceToken = 2; // == </s>
		UnknownToken = 3; // == <unk>
		MaskToken = 250001;
	}
}