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
	public static unsafe void ConvertToInt64AndAdd1(Int32[] tokens, Int64[] target, Int32 targetOffset) {
		//tokens.Select(i => (Int64)i + 1).ToArray().CopyTo(target.AsSpan(targetOffset));
		ArgumentOutOfRangeException.ThrowIfLessThan(target.Length, tokens.Length + targetOffset);

		// fixed (Int64* targetPtr = target)
		fixed (Int32* tokenPtr = tokens) {
			Int32* ptr = tokenPtr;
			Int32 tokensRemaining = tokens.Length;

			if (Avx2.IsSupported && Vector256.IsHardwareAccelerated) {
				// we can add / convert 8 integers at a time

				Vector256<Int32> addMe = Vector256<Int32>.One;

				while (tokensRemaining >= Vector256<Int32>.Count) {
					Vector256<Int32> sourceTokens = Vector256.Load(ptr);
					Vector256<Int32> addResult = Avx2.Add(sourceTokens, addMe);

					Vector256<Int64> result = Avx2.ConvertToVector256Int64(addResult.GetLower());
					result.StoreUnsafe(ref target[targetOffset]);
					targetOffset += Vector256<Int64>.Count;

					result = Avx2.ConvertToVector256Int64(addResult.GetUpper());
					result.StoreUnsafe(ref target[targetOffset]);
					targetOffset += Vector256<Int64>.Count;

					ptr += Vector256<Int32>.Count;
					tokensRemaining -= Vector256<Int32>.Count;
				}
			}

			while (tokensRemaining > 0) {
				target[targetOffset++] = (Int64)((*ptr) + 1);
				++ptr;
				--tokensRemaining;
			}
		}
	}
}