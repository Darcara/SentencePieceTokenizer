namespace SentencePieceTokenizer.Test;

using System.Collections.Generic;
using Neco.Common.Extensions;

public static class TestData {
	public static class SentencePieceModels {
		public static String XlmRobertaBase => "data/xlm-roberta-base-sentencepiece.bpe.model";
		public static String EnDeMarian => "data/en-de/source.spm";
		public static String EnDeMarianVocab => "data/en-de/vocab.json";
	}

	public static class ExampleText {
		public const String OneCharacterWord = "I";
		public const String OneTokenWord = "answer";
		public const String TwoTokenWord = "thoughtful";
		public const String ThreeTokenWord = "CHAPTER";
		public const String ShortSentence = "The children became silent and thoughtful.";
		public const String Paragraph = "A frightened look in Becky's face brought Tom to his senses and he saw that he had made a blunder. Becky was not to have gone home that night! The children became silent and thoughtful. In a moment a new burst of grief from Becky showed Tom that the thing in his mind had struck hers also -- that the Sabbath morning might be half spent before Mrs. Thatcher discovered that Becky was not at Mrs. Harper's.\n\nThe children fastened their eyes upon their bit of candle and watched it melt slowly and pitilessly away; saw the half inch of wick stand alone at last; saw the feeble flame rise and fall, climb the thin column of smoke, linger at its top a moment, and then -- the horror of utter darkness reigned!";
		public const String TomSawyerFile = "data/TomSawyer.txt";
		public static String TomSawyerText => File.ReadAllText(TomSawyerFile).Substring(7499, 386044);

		public static IEnumerable<String> NonStandardTexts() {
			yield return "B081BBEB-52FE-464B-B38F-1D8D249A9906";
			yield return "Text\t\twith\r\n\r\nmany\n\n\nnewlines";
			yield return "NonWidthWhitespace-->\uFEFF<--here and-->\u200B<--here";
			yield return "The characters Ö and Ø and Œ are not the same.";
			yield return "\u2122";
			yield return "\ud83e\udd17";
			yield return "\ud83c\udff4\udb40\udc67\udb40\udc62\udb40\udc65\udb40\udc6e\udb40\udc67\udb40\udc7f";
		}
	}
}