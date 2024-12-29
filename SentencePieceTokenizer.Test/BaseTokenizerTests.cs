namespace SentencePieceTokenizer.Test;

using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Neco.Common;
using Neco.Common.Helper;

[TestFixture]
public abstract class BaseTokenizerTests<T> where T : struct, INumber<T> {
	[SetUp]
	public Task EnsureTestModelAvailable() => Helper.DownloadTestData();

	[SetUp]
	public void EnsureNativeFilesPresent() => Helper.EnsureNativeFilesPresent();

	protected abstract ITokenizer<T> Create();

	[TestCase(TestData.ExampleText.OneCharacterWord)]
	[TestCase(TestData.ExampleText.OneTokenWord)]
	[TestCase(TestData.ExampleText.TwoTokenWord)]
	[TestCase(TestData.ExampleText.ThreeTokenWord)]
	[TestCase(TestData.ExampleText.ShortSentence, TestName = nameof(TestData.ExampleText.ShortSentence))]
	[TestCase(TestData.ExampleText.Paragraph, TestName = nameof(TestData.ExampleText.Paragraph))]
	public void TokenizesSmallTextToSpans(String text) {
		using ITokenizer<T> tok = Create();
		T[] tokens = tok.EncodeToIds(text);
		String[] stringTokens = tok.EncodeToStrings(text);

		Byte[] utf8Text = MagicNumbers.Utf8NoBom.GetBytes(text);
		String[] utf8StringTokens = tok.EncodeToStrings(utf8Text);
		T[] tokensByBytes = tok.EncodeToIds(utf8Text);
		(T[] ids, TokenSpan[] tokens) tokenSpans = tok.EncodeToSpans(utf8Text);

		Console.WriteLine($"Text:{text.Length} --> Tokens: {tokens.Length} = TextPerToken: {text.Length / (Double)tokens.Length:N3}");

		Helper.PrintColums(
			("Id", tokens),
			("-Tokens", stringTokens),
			("Span-Id", tokenSpans.ids.Select(ts => ts)),
			("Loc", tokenSpans.tokens.Select(ts => $"{ts.Begin,2}-{ts.End,2}")),
			("-Span-Token", tokenSpans.tokens.Select((ts) => "'" + ts.AsString(utf8Text) + "'")),
			("-Decoded", tokenSpans.ids.Select(ts => "'" + tok.Decode(ts) + "'"))
		);

		tokens.Should().BeEquivalentTo(tokensByBytes);
		tokens.Should().HaveCount(stringTokens.Length);
		stringTokens.Should().BeEquivalentTo(utf8StringTokens);

		String decodedTextByTokens = tok.Decode(tokens);
		String decodedTextBySpans = tok.Decode(tokenSpans.ids);
		String decodedTextByTokenSpans = tok.Decode(tokenSpans, utf8Text);
		String decodedTextBySpansWithUtf8Text = tok.Decode(tokenSpans, utf8Text);

		// Due to normalization line-breaks ond multiple whitespace characters will be folded into a single whitespace
		String reconstructableText = Regex.Replace(text, "[ \r\n\t]+", " ");

		decodedTextByTokens.Should().BeEquivalentTo(reconstructableText);
		decodedTextBySpans.Should().BeEquivalentTo(reconstructableText);

		// this is the only correct one
		decodedTextByTokenSpans.Should().BeEquivalentTo(text);
		decodedTextBySpansWithUtf8Text.Should().BeEquivalentTo(text);
	}

	[TestCase(TestData.ExampleText.OneCharacterWord)]
	[TestCase(TestData.ExampleText.OneTokenWord)]
	[TestCase(TestData.ExampleText.TwoTokenWord)]
	[TestCase(TestData.ExampleText.ThreeTokenWord)]
	[TestCase(TestData.ExampleText.ShortSentence, TestName = nameof(TestData.ExampleText.ShortSentence))]
	[TestCase(TestData.ExampleText.Paragraph, TestName = nameof(TestData.ExampleText.Paragraph))]
	public void TokenizesSmallTextToId(String text) {
		using ITokenizer<T> tok = Create();
		T[] tokens = tok.EncodeToIds(text);
		String[] stringTokens = tok.EncodeToStrings(text);

		Console.WriteLine($"Text:{text.Length} --> Tokens: {tokens.Length} = TextPerToken: {text.Length / (Double)tokens.Length:N3}");
		for (Int32 i = 0; i < tokens.Length; i++) {
			Console.WriteLine($"{tokens[i],7}: {stringTokens[i]}");
		}

		tokens.Should().HaveCount(stringTokens.Length);
	}

	[Test]
	public void TokenizesLargeTextToId() {
		using ITokenizer<T> tok = Create();
		String tomSawyerText = TestData.ExampleText.TomSawyerText;
		T[] tokens = tok.EncodeToIds(tomSawyerText);
		String[] stringTokens = tok.EncodeToStrings(tomSawyerText);

		Console.WriteLine($"Text:{tomSawyerText.Length} --> Tokens: {tokens.Length} = TextPerToken: {tomSawyerText.Length / (Double)tokens.Length:N3}");
		for (Int32 i = 0; i < 25; i++) {
			Console.WriteLine($"{tokens[i],7}: {stringTokens[i]}");
		}

		tokens.Should().HaveCount(stringTokens.Length);
	}

	[TestCaseSource(typeof(TestData.ExampleText), nameof(TestData.ExampleText.NonStandardTexts))]
	public void TokenizesNonStandardToSpans(String text) {
		using ITokenizer<T> tok = Create();
		T[] tokens = tok.EncodeToIds(text);
		String[] stringTokens = tok.EncodeToStrings(text);

		Byte[] utf8Text = MagicNumbers.Utf8NoBom.GetBytes(text);
		(T[] ids, TokenSpan[] tokens) tokenSpans = tok.EncodeToSpans(utf8Text);

		Console.WriteLine($"Text:{text.Length} --> Tokens: {tokens.Length} = TextPerToken: {text.Length / (Double)tokens.Length:N3}");

		Helper.PrintColums(
			("Id", tokens),
			("-Tokens", stringTokens),
			("Span-Id", tokenSpans.ids),
			("Loc", tokenSpans.tokens.Select(ts => $"{ts.Begin,2}-{ts.End,2}")),
			("-Span-Token", tokenSpans.tokens.Select(ts => "'" + ts.AsString(utf8Text) + "'")),
			("-Decoded", tokenSpans.ids.Select(ts => "'" + tok.Decode(ts) + "'"))
		);


		tokens.Should().HaveCount(stringTokens.Length);
	}

	[Test]
	public void DecodesInvalid() {
		using ITokenizer<T> tokenizer = Create();
		Assert.That(() => tokenizer.Decode(T.Zero - T.One), Throws.Exception);
	}

	[Test]
	public void AddsExtraTokens() {
		using ITokenizer<T> tokenizer = Create();
		T[] ids = tokenizer.EncodeToIds(TestData.ExampleText.ShortSentence);
		Console.WriteLine(String.Join(", ", ids));

		var idsWithToken = tokenizer.EncodeToIds(TestData.ExampleText.ShortSentence, suffixIds: [tokenizer.EndOfSentenceToken]);
		idsWithToken[^2].Should().Be(ids[^1]);
		idsWithToken[^1].Should().Be(tokenizer.EndOfSentenceToken);
		idsWithToken.Length.Should().Be(ids.Length + 1);

		idsWithToken = tokenizer.EncodeToIds(TestData.ExampleText.ShortSentence, suffixIds: [tokenizer.PadToken]);
		idsWithToken[^2].Should().Be(ids[^1]);
		idsWithToken[^1].Should().Be(tokenizer.PadToken);
		idsWithToken.Length.Should().Be(ids.Length + 1);

		idsWithToken = tokenizer.EncodeToIds(TestData.ExampleText.ShortSentence, suffixIds: [tokenizer.EndOfSentenceToken, tokenizer.PadToken]);
		idsWithToken[^3].Should().Be(ids[^1]);
		idsWithToken[^2].Should().Be(tokenizer.EndOfSentenceToken);
		idsWithToken[^1].Should().Be(tokenizer.PadToken);
		idsWithToken.Length.Should().Be(ids.Length + 2);
		
		idsWithToken = tokenizer.EncodeToIds(TestData.ExampleText.ShortSentence, prefixIds: [tokenizer.BeginOfSentenceToken]);
		idsWithToken[0].Should().Be(tokenizer.BeginOfSentenceToken);
		idsWithToken[1].Should().Be(ids[0]);
		idsWithToken[^1].Should().Be(ids[^1]);
		idsWithToken.Length.Should().Be(ids.Length + 1);
		
		idsWithToken = tokenizer.EncodeToIds(TestData.ExampleText.ShortSentence, prefixIds: [tokenizer.BeginOfSentenceToken], suffixIds: [tokenizer.EndOfSentenceToken, tokenizer.PadToken]);
		idsWithToken[0].Should().Be(tokenizer.BeginOfSentenceToken);
		idsWithToken[1].Should().Be(ids[0]);
		idsWithToken[^3].Should().Be(ids[^1]);
		idsWithToken[^2].Should().Be(tokenizer.EndOfSentenceToken);
		idsWithToken[^1].Should().Be(tokenizer.PadToken);
		idsWithToken.Length.Should().Be(ids.Length + 3);
	}

	/*
Tokenizer-String-Single 4,084,394 ops in 5,000.001ms = clean per operation: 1.190µs or 840,169.561op/s with 64 Bytes per run and GC 31/0/0
Tokenizer-String-Single TotalCPUTime per operation: 5,000.000ms or clean 840,169.664op/s for a factor of 1.000
Tokenizer-String-Medium 802,406 ops in 5,000.001ms = clean per operation: 6.198µs or 161,347.459op/s with 368 Bytes per run and GC 35/0/0
Tokenizer-String-Medium TotalCPUTime per operation: 5,000.000ms or clean 161,347.485op/s for a factor of 1.000
Tokenizer-String-Large 44,447 ops in 5,000.072ms = clean per operation: 112.462µs or 8,891.915op/s with 6.60 KiB per run and GC 35/0/0
Tokenizer-String-Large TotalCPUTime per operation: 5,000.000ms or clean 8,892.043op/s for a factor of 1.000
Tokenizer-String-Gigantic 60 ops in 5,030.073ms = clean per operation: 83,834.511µs or 11.928op/s with 6.21 MiB per run and GC 51/49/22
Tokenizer-String-Gigantic TotalCPUTime per operation: 5,015.625ms or clean 11.963op/s for a factor of 0.997
	 */
	[Test]
	[Category("Benchmark")]
	public void PerformanceEstimatesToString() {
		using ITokenizer<T> tokenizer = Create();
		PerformanceHelper.GetPerformanceRough("Tokenizer-String-Single", static tok => { tok.EncodeToStrings("I"); }, tokenizer);
		PerformanceHelper.GetPerformanceRough("Tokenizer-String-Medium", static tok => { tok.EncodeToStrings(TestData.ExampleText.ShortSentence); }, tokenizer);
		PerformanceHelper.GetPerformanceRough("Tokenizer-String-Large", static tok => { tok.EncodeToStrings(TestData.ExampleText.Paragraph); }, tokenizer);
		PerformanceHelper.GetPerformanceRough("Tokenizer-String-Gigantic", static tok => { tok.EncodeToStrings(TestData.ExampleText.TomSawyerText); }, tokenizer);
	}

	/*
Little Difference between SentencePiece & Marian
Tokenizer-Id-Single 2,103,668 ops in 5,000.002ms = clean per operation: 2.342µs or 426,902.708op/s with 64 Bytes per run and GC 16/0/0
Tokenizer-Id-Single TotalCPUTime per operation: 5,015.625ms or clean 425,553.526op/s for a factor of 1.003
Tokenizer-Id-Medium 868,789 ops in 5,000.002ms = clean per operation: 5.722µs or 174,773.228op/s with 144 Bytes per run and GC 14/0/0
Tokenizer-Id-Medium TotalCPUTime per operation: 5,000.000ms or clean 174,773.306op/s for a factor of 1.000
Tokenizer-Id-Large 58,859 ops in 5,000.063ms = clean per operation: 84.917µs or 11,776.272op/s with 1.98 KiB per run and GC 14/0/0
Tokenizer-Id-Large TotalCPUTime per operation: 5,000.000ms or clean 11,776.420op/s for a factor of 1.000
Tokenizer-Id-Gigantic 74 ops in 5,054.623ms = clean per operation: 68,305.679µs or 14.640op/s with 3.50 MiB per run and GC 36/33/29
Tokenizer-Id-Gigantic TotalCPUTime per operation: 5,140.625ms or clean 14.395op/s for a factor of 1.017
	 */
	[Test]
	[Category("Benchmark")]
	public void PerformanceEstimatesToId() {
		using ITokenizer<T> tokenizer = Create();
		PerformanceHelper.GetPerformanceRough("Tokenizer-Id-Single", static tok => { tok.EncodeToIds("I"); }, tokenizer);
		PerformanceHelper.GetPerformanceRough("Tokenizer-Id-Medium", static tok => { tok.EncodeToIds(TestData.ExampleText.ShortSentence); }, tokenizer);
		PerformanceHelper.GetPerformanceRough("Tokenizer-Id-Large", static tok => { tok.EncodeToIds(TestData.ExampleText.Paragraph); }, tokenizer);
		PerformanceHelper.GetPerformanceRough("Tokenizer-Id-Gigantic", static tok => { tok.EncodeToIds(TestData.ExampleText.TomSawyerText); }, tokenizer);
	}
}