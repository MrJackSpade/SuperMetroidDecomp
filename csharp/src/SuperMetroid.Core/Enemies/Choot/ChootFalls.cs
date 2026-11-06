using static SuperMetroid.Core.Enemies.Choot.ChootFallingPathDefinitions;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Where a falling Choot is: its pattern, and the sample of its path at each fall frame.</summary>
internal static class ChootFalls
{
    /// <summary>How far, in pixels, the loop origin of <paramref name="fallingPath"/> advances per loop.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="fallingPath"/> is not a defined path.</exception>
    internal static int LoopYDistance(ChootFallingPath fallingPath) =>
        ChootPatternDefinitions.LoopYDistances.TryGetValue(fallingPath, out int distance)
            ? distance
            : throw new InvalidOperationException($"Undefined {nameof(ChootFallingPath)} {fallingPath}.");

    /// <summary>Returns the exact physical sample of one falling path at one frame.</summary>
    internal static ChootFallingPathPoint At(ChootFallingPath fallingPath, int frameIndex)
    {
        if (!Paths.TryGetValue(fallingPath, out Offset16<RoomPixels>[]? path))
            throw new InvalidOperationException($"Undefined ChootFallingPath {fallingPath}.");

        int sourceFrame = CollapseExpandedPlateaus(frameIndex, PlateauHolds[fallingPath]);
        if ((uint)sourceFrame >= (uint)path.Length)
        {
            throw new InvalidDataException(
                $"Choot falling-pattern frame {frameIndex} exceeds stream {fallingPath}.");
        }

        return new ChootFallingPathPoint(path[sourceFrame]);
    }

    /// <summary>Maps a slow-path physical frame back to the normal stream, repeating the two authored plateau samples.</summary>
    private static int CollapseExpandedPlateaus(int frameIndex, int expandedFrames)
    {
        if (frameIndex < 0)
            return frameIndex;
        if (frameIndex < PositivePlateauIndex)
            return frameIndex;
        if (frameIndex < PositivePlateauIndex + expandedFrames)
            return PositivePlateauIndex - 1;

        frameIndex -= expandedFrames;
        if (frameIndex < NegativePlateauIndex)
            return frameIndex;
        if (frameIndex < NegativePlateauIndex + expandedFrames)
            return NegativePlateauIndex - 1;
        return frameIndex - expandedFrames;
    }
}
