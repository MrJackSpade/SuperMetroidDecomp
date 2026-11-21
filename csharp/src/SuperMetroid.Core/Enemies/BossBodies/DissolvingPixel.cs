namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// How a dissolve erases one pixel of a boss body picture (#1652, #2478): the band of the dissolve that shows the pixel,
/// and the steps of the dissolve after which the pixel is erased once its band shows them.
/// </summary>
/// <param name="Band">The band of the dissolve that shows the pixel, counted from zero.</param>
/// <param name="ErasedAfterSteps">The steps of the dissolve after which the pixel is erased, one or more.</param>
public readonly partial record struct DissolvingPixel(int Band, int ErasedAfterSteps);
