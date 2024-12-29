namespace SentencePieceTokenizer;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind.Sequential, Size = sizeof(Int32) * 2)]
public readonly struct TokenSpan(Int32 begin, Int32 end) {
	public readonly Int32 Begin = begin;
	public readonly Int32 End = end;

	public Int32 Length {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => End - Begin;
	}
	public ReadOnlySpan<Byte> AsSpan(ReadOnlySpan<Byte> utf8Bytes) => utf8Bytes.Slice(Begin, Length);
	public String AsString(ReadOnlySpan<Byte> utf8Bytes) => Encoding.UTF8.GetString(utf8Bytes.Slice(Begin, Length));
	public Boolean AsChars(ReadOnlySpan<Byte> utf8Bytes, Span<Char> destination, out Int32 charsWritten) => Encoding.UTF8.TryGetChars(utf8Bytes.Slice(Begin, Length), destination, out charsWritten);
}