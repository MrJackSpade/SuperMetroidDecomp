namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Application-owned window motion, in left/right/top/bottom order.</summary>
/// <param name="Timer">
/// Starting value of the window's countdown: the window moves on that many calls plus the call that reaches zero, and
/// completes when the countdown goes negative. A stock window starts between 32 and 64; the window rejects a negative
/// start.
/// </param>
/// <param name="Left">Per-call velocity of the left edge.</param>
/// <param name="Right">Per-call velocity of the right edge.</param>
/// <param name="Top">Per-call velocity of the top edge.</param>
/// <param name="Bottom">Per-call velocity of the bottom edge.</param>
internal readonly record struct FileSelectMapWindowMotion(
    int Timer,
    SignedSixteenSixteen Left,
    SignedSixteenSixteen Right,
    SignedSixteenSixteen Top,
    SignedSixteenSixteen Bottom);
