namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>A file-select map reveal's motion (#1555): how long its edges move, and each edge's speed.</summary>
/// <param name="MovingUpdates">
/// The updates the reveal's edges move on, the last completing it: a stock reveal opens over 33 to 65 updates. The
/// reveal rejects fewer than one.
/// </param>
/// <param name="Left">The left edge's speed per update, in screen pixels and fractions of one; negative moves it left.</param>
/// <param name="Right">The right edge's speed per update; positive moves it right.</param>
/// <param name="Top">The top edge's speed per update; negative moves it up.</param>
/// <param name="Bottom">The bottom edge's speed per update; positive moves it down.</param>
internal readonly record struct FileSelectMapWindowMotion(
    int MovingUpdates,
    SignedSixteenSixteen Left,
    SignedSixteenSixteen Right,
    SignedSixteenSixteen Top,
    SignedSixteenSixteen Bottom);
