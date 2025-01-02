namespace SentencePieceTokenizer;

using System;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class HugginFaceHack {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Add<T>(ReadOnlySpan<T> tokens, T value, Span<T> target) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> {
		TensorPrimitives.Add(tokens, value, target);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddInPlace<T>(Span<T> tokens, T value) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> {
		TensorPrimitives.Add(tokens, value, tokens);
	}

	// This feels wrong
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

			for (Int32 i = 0; i < target.Length; ++i) {
				target[i] = *ptr + 1L;
				++ptr;
			}
		}
	}
}