namespace SentencePieceTokenizer;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Very similar to <see cref="ReadOnlySpan{T}"/> holds start and end indices, usually into utf8 byte string arrays or spans
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = sizeof(Int32) * 2)]
public readonly struct TokenSpan(Int32 begin, Int32 end) : IEquatable<TokenSpan> {
	/// <summary>
	/// Begin index
	/// </summary>
	public readonly Int32 Begin = begin;

	/// <summary>
	/// End index, this is not the <see cref="Length"/>
	/// </summary>
	public readonly Int32 End = end;

	/// <summary>
	/// The length of this span = <see cref="End"/> - <see cref="Begin"/>
	/// </summary>
	public Int32 Length {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => End - Begin;
	}

	/// <summary>
	/// Converts a span of bytes to the slice, represented by this <see cref="TokenSpan"/> 
	/// </summary>
	public ReadOnlySpan<Byte> AsSpan(ReadOnlySpan<Byte> utf8Bytes) => utf8Bytes.Slice(Begin, Length);

	/// <summary>
	/// Converts a span of utf8 bytes to the string, representing the protion/slice of this <see cref="TokenSpan"/>
	/// </summary>
	public String AsString(ReadOnlySpan<Byte> utf8Bytes) => Encoding.UTF8.GetString(utf8Bytes.Slice(Begin, Length));

	/// <summary>
	/// Converts a span of utf8 bytes to the chars represented by this portion/slice of this <see cref="TokenSpan"/>
	/// </summary>
	/// <param name="utf8Bytes">A read-only span containing the utf8 bytes to decode.</param>
	/// <param name="destination">The character span receiving the decoded bytes.</param>
	/// <param name="charsWritten">Upon successful completion of the operation, the number of chars decoded into <paramref name="destination"/>.</param>
	/// <returns><see langword="true"/> if all of the characters were decoded into the destination; <see langword="false"/> if the destination was too small to contain all the decoded chars.</returns>
	public Boolean AsChars(ReadOnlySpan<Byte> utf8Bytes, Span<Char> destination, out Int32 charsWritten) => Encoding.UTF8.TryGetChars(utf8Bytes.Slice(Begin, Length), destination, out charsWritten);

	#region Equality members

	/// <inheritdoc />
	public Boolean Equals(TokenSpan other) => Begin == other.Begin && End == other.End;

	/// <inheritdoc />
	public override Boolean Equals(Object? obj) => obj is TokenSpan other && Equals(other);

	/// <inheritdoc />
	public override Int32 GetHashCode() => HashCode.Combine(Begin, End);

	/// <inheritdoc cref="Equals(TokenSpan)"/>
	public static Boolean operator ==(TokenSpan left, TokenSpan right) => left.Equals(right);

	/// <summary>
	/// Returns a value indicating whether this instance is not equal to a specified <see cref="TokenSpan"/> value.
	/// </summary>
	public static Boolean operator !=(TokenSpan left, TokenSpan right) => !left.Equals(right);

	#endregion
}