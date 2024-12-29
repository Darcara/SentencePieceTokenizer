namespace SentencePieceTokenizer.Test;

using System.Collections.Generic;
using FluentAssertions.Execution;

[TestFixture]
public class SentencePieceTokenizerTests : BaseTokenizerTests<Int32> {
	protected override ITokenizer<Int32> Create() => new SentencePieceTokenizer(TestData.SentencePieceModels.XlmRobertaBase);

	[Test]
	public void ReadsSpecialTokensCorrectly() {
		using ITokenizer<Int32> tok = Create();
		using (new AssertionScope()) {
			tok.PadToken.Should().Be(-1);
			tok.BeginOfSentenceToken.Should().Be(1);
			tok.EndOfSentenceToken.Should().Be(2);
			tok.UnknownToken.Should().Be(0);
			tok.NumberOfTokens.Should().Be(250_000);
		}
	}

	[Test]
	public void ErrorOnWrongModelFile() {
		Assert.Throws<InvalidOperationException>(() => {
			using SentencePieceTokenizer tok = new("invalid");
		});
	}
}