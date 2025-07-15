namespace SentencePieceTokenizer;

using System;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

/// <summary>
/// The HuggingFace SentencePiece tokenizer adds '1' to the original outout of the library.
/// This small helper class aids with the permant value increase and the conversion to <see cref="Int64"/> which is often required. 
/// </summary>
public static class HugginFaceHack {
	/// <summary>Adds a value to all elments of the tokens-span.</summary>
	/// <param name="tokens">The tokens, represented as a span.</param>
	/// <param name="value">The scalar value to add to the tokens.</param>
	/// <param name="target">The destination tensor, represented as a span.</param>
	/// <remarks>
	/// This method effectively computes <c>target[i] = tokens[i] + value</c>.
	/// </remarks>
	/// <seealso cref="TensorPrimitives.Add{T}(System.ReadOnlySpan{T},T,System.Span{T})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Add<T>(ReadOnlySpan<T> tokens, T value, Span<T> target) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> {
		TensorPrimitives.Add(tokens, value, target);
	}

	/// <summary>Adds a value to all elments of the tokens-span, modifying the span itself in-place.</summary>
	/// <param name="tokens">The tokens, represented as a span.</param>
	/// <param name="value">The scalar value to add to the tokens.</param>
	/// <remarks>
	/// This method effectively computes <c>tokens[i] = tokens[i] + value</c>.
	/// </remarks>
	///  <seealso cref="Add{T}"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddInPlace<T>(Span<T> tokens, T value) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> {
		TensorPrimitives.Add(tokens, value, tokens);
	}

	/// <summary>
	/// Converts the <see cref="Int32"/> tokens to <see cref="Int64"/> and simultaneously adds 1.
	/// </summary>
	/// <param name="tokens">The tokens to convert, representet as a span.</param>
	/// <param name="target">The destination tensor, represented as a span.</param>
	/// <remarks>The HuggingFace sentencepiece tokenizers add '1' for some reason</remarks>
	public static unsafe void ConvertToInt64AndAdd1(ReadOnlySpan<Int32> tokens, Span<Int64> target) {
		ArgumentOutOfRangeException.ThrowIfLessThan(target.Length, tokens.Length);
		fixed (Int32* tokenPtr = tokens) {
			Int32* ptr = tokenPtr;
			Int32 tokensRemaining = tokens.Length;

			if (Avx2.IsSupported && Vector256.IsHardwareAccelerated) {
				Vector256<Int32> addMe = Vector256<Int32>.One;

				while (tokensRemaining >= Vector256<Int32>.Count) {
					Vector256<Int32> sourceTokens = Vector256.Load(ptr);
					Vector256<Int32> addResult = Avx2.Add(sourceTokens, addMe);

					(Vector256<Int64> lower, Vector256<Int64> upper) = Vector256.Widen(addResult);
					lower.CopyTo(target);
					upper.CopyTo(target.Slice(Vector256<Int64>.Count));
					target = target.Slice(Vector256<Int64>.Count * 2);

					ptr += Vector256<Int32>.Count;
					tokensRemaining -= Vector256<Int32>.Count;
				}
			}

			for (Int32 i = 0; i < tokensRemaining; ++i) {
				target[i] = *ptr + 1L;
				++ptr;
			}
		}
	}
}