namespace SentencePieceTokenizer.Test;

using System.Linq;

[TestFixture]
public class HuggingFaceHackTests {

	[Test]
	public void AddsCorrectly() {
		Int32[] data = new Int32[1024];
		Int32[] output = new Int32[1024];
		for (var i = 0; i < data.Length; i++) {
			data[i] = Random.Shared.Next();
		}
		
		HugginFaceHack.Add(data, 3, output);

		output.Should().BeEquivalentTo(data.Select(i => i + 3).ToArray());
	}
	
	[Test]
	public void AddsCorrectlyInPlace() {
		Int32[] originalData = new Int32[1024];
		Int32[] data = new Int32[1024];
		for (var i = 0; i < data.Length; i++) {
			data[i] = Random.Shared.Next();
			originalData[i] = data[i];
		}
		
		HugginFaceHack.AddInPlace(data, 3);
		data.Should().BeEquivalentTo(originalData.Select(i => i + 3).ToArray());
		
		Array.Copy(originalData, 0, data, 0, originalData.Length);
		
		HugginFaceHack.AddInPlace(data.AsSpan(3,3), 3);
		data.AsSpan(3, 3).SequenceEqual(originalData.Skip(3).Take(3).Select(i => i + 3).ToArray()).Should().BeTrue();
	}
	
	[Test]
	public void AddsCorrectlyAndCastsToInt64() {
		Int32[] data = new Int32[1026];
		Int64[] output = new Int64[1026];
		for (var i = 0; i < data.Length; i++) {
			data[i] = Random.Shared.Next();
		}
		
		HugginFaceHack.ConvertToInt64AndAdd1(data, output);

		output.Should().BeEquivalentTo(data.Select(i => i + 1).ToArray());
	}
}