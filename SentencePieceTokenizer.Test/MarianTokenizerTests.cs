namespace SentencePieceTokenizer.Test;

using System.Text.RegularExpressions;
using FluentAssertions.Execution;
using Neco.Common;
using Neco.Common.Helper;

[TestFixture]
public class MarianTokenizerTests : BaseTokenizerTests<Int64> {
	protected override ITokenizer<Int64> Create() => new MarianTokenizer(TestData.SentencePieceModels.EnDeMarian, TestData.SentencePieceModels.EnDeMarianVocab);

	[Test]
	public void ReadsSpecialTokensCorrectly() {
		using ITokenizer<Int64> tok = Create();
		using (new AssertionScope()) {
			tok.PadToken.Should().Be(58100);
			tok.BeginOfSentenceToken.Should().Be(-1);
			tok.EndOfSentenceToken.Should().Be(0);
			tok.UnknownToken.Should().Be(1);
			tok.NumberOfTokens.Should().Be(58101);
		}
	}
	
	[Test]
	public void TokenToString() {
		using ITokenizer<Int64> tokenizer = Create();
		tokenizer.Decode(3).Should().Be(".");
		tokenizer.Decode(tokenizer.EndOfSentenceToken).Should().Be("</s>");
		tokenizer.Decode([tokenizer.UnknownToken, tokenizer.EndOfSentenceToken, tokenizer.PadToken]).Should().BeEquivalentTo("<unk></s><pad>");
	}
}