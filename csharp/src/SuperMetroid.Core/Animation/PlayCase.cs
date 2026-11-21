namespace SuperMetroid.Core.Animation;

/// <summary>
/// What a play does on one case of a closed choice it passes (#2553), the play handed by reference so the cases can move
/// it and still be built once.
/// </summary>
/// <typeparam name="TCase">The case.</typeparam>
/// <typeparam name="TPlay">The play passing it.</typeparam>
/// <typeparam name="TResult">What the case comes to.</typeparam>
/// <param name="played">The case.</param>
/// <param name="play">The play, which the case may move.</param>
/// <returns>What the case comes to.</returns>
internal delegate TResult PlayCase<in TCase, TPlay, out TResult>(TCase played, ref TPlay play);
