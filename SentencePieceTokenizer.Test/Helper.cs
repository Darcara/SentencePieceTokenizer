namespace SentencePieceTokenizer.Test;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Console;

public static class Helper {
	public static readonly ILoggerFactory LogFactory = LoggerFactory.Create(builder => builder
		.SetMinimumLevel(LogLevel.Debug)
		.AddSimpleConsole(conf => {
			conf.ColorBehavior = LoggerColorBehavior.Enabled;
			conf.SingleLine = false;
		}));

	public static readonly ILogger Logger = LogFactory.CreateLogger("Test");

	public static async Task DownloadTestData() {
		(String uri, String target)[] requiredFiles = [
			("https://huggingface.co/onnx-community/opus-mt-en-de/resolve/main/source.spm?download=true", "data/en-de/source.spm"),
			("https://huggingface.co/onnx-community/opus-mt-en-de/resolve/main/vocab.json?download=true", "data/en-de/vocab.json"),
			("https://huggingface.co/FacebookAI/xlm-roberta-base/resolve/main/sentencepiece.bpe.model?download=true", "data/xlm-roberta-base-sentencepiece.bpe.model"),
		];

		using HttpClient client = new();

		foreach ((String uri, String target) requiredFile in requiredFiles) {
			Uri uri = new(requiredFile.uri);
			if (File.Exists(requiredFile.target)) continue;

			Console.WriteLine($"Downloading {requiredFile.target} from {uri}");
			String targetFileAbs = Path.GetFullPath(requiredFile.target);
			Directory.CreateDirectory(Path.GetDirectoryName(targetFileAbs) ?? ".");
			String tempFile = targetFileAbs + ".tmp";
			await using (Stream netStream = await client.GetStreamAsync(uri).ConfigureAwait(false)) {
				await using FileStream fileStream = File.Open(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);
				await netStream.CopyToAsync(fileStream).ConfigureAwait(false);
			}

			File.Move(tempFile, targetFileAbs, true);
		}
	}

	internal static void CopyIfNecessary(String fileToTest, String source) {
		FileInfo fileInfo = new(fileToTest);
		FileInfo sourceInfo = new(source);
		if (!fileInfo.Exists || fileInfo.Length != sourceInfo.Length || fileInfo.LastWriteTimeUtc != sourceInfo.LastWriteTimeUtc) {
			Console.WriteLine($"Copying {fileToTest} from {source}");
			sourceInfo.CopyTo(fileToTest, true);
		}
	}

	internal static void EnsureNativeFilesPresent() {
		switch (RuntimeInformation.RuntimeIdentifier) {
			case "win-x64":
				CopyIfNecessary($"./{SentencePieceApi.LibraryName}.dll",$"../../../../SentencePieceTokenizer/runtimes/win-x64/native/{SentencePieceApi.LibraryName}.dll");
				CopyIfNecessary("./sentencepiece.lib","../../../../SentencePieceTokenizer/runtimes/win-x64/native/sentencepiece.lib");
				break;
			case "linux-x64":
				CopyIfNecessary($"./{SentencePieceApi.LibraryName}.so",$"../../../../SentencePieceTokenizer/runtimes/win-x64/native/{SentencePieceApi.LibraryName}.so");
				CopyIfNecessary("./sentencepiece.so","../../../../SentencePieceTokenizer/runtimes/win-x64/native/sentencepiece.so");
				break;
			default: throw new InvalidOperationException($"Unsupported runtime id: {RuntimeInformation.RuntimeIdentifier}");
		}
	}

	internal static void PrintColums(params (String header, IEnumerable data)[] columns) {
		StringBuilder sb = new();
		List<(Boolean padRight, Int32 padSize, List<String> data)> columnsWithData = [];
		for (int index = 0; index < columns.Length; index++) {
			(string header, IEnumerable data) column = columns[index];
			List<string> evaluatedData = column.data.Cast<Object?>().Select(o => o?.ToString() ?? "?").ToList();
			Int32 maxSize = 1 + Math.Max(column.header.Length, evaluatedData.Max(o => o.ToString().Length));
			Boolean padRight = column.header.StartsWith('-');
			String headerToPrint = padRight ? ' ' + column.header.Substring(1).PadRight(maxSize) : column.header.PadLeft(maxSize) + " ";
			columnsWithData.Add((padRight, maxSize, evaluatedData));
			sb.Append($"{headerToPrint}");
		}

		sb.AppendLine();
		Int32 maxData = columnsWithData.Max(tpl => tpl.data.Count);
		for (int idx = 0; idx < maxData; idx++) {
			foreach ((Boolean padRight, Int32 padSize, List<String> data) in columnsWithData) {
				String dataToPrint = idx >= data.Count ? "-" : data[idx];
				sb.Append(padRight ? ' ' + dataToPrint.PadRight(padSize) : dataToPrint.PadLeft(padSize) + ' ');
			}

			sb.AppendLine();
		}

		Console.WriteLine(sb.ToString());
	}
}