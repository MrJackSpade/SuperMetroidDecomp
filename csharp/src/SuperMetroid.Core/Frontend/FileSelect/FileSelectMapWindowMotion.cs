namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Application-owned signed 16.16 window motion, in left/right/top/bottom order.</summary>
/// <param name="Timer">
/// Starting value of the window's countdown: the window moves on that many calls plus the call that reaches zero, and
/// completes when the countdown goes negative. A stock window starts between 32 and 64; the window rejects a start
/// that is already negative as a native word.
/// </param>
/// <param name="Left">Signed 16.16 per-call velocity of the left edge.</param>
/// <param name="Right">Signed 16.16 per-call velocity of the right edge.</param>
/// <param name="Top">Signed 16.16 per-call velocity of the top edge.</param>
/// <param name="Bottom">Signed 16.16 per-call velocity of the bottom edge.</param>
internal readonly record struct FileSelectMapWindowMotion(int Timer, uint Left, uint Right, uint Top, uint Bottom);
