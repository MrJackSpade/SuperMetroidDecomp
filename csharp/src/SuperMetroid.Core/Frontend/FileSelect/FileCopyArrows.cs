namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Chooses the file-copy arrow (<see cref="FileCopyArrowDefinitions"/>) between a source and a destination slot.
/// </summary>
internal static class FileCopyArrows
{
    /// <summary>Chooses one of four up/down, short/long arrows and its screen Y from two distinct file-slot indexes.</summary>
    internal static FileCopyArrowPlacement Select(int source, int destination)
    {
        if ((uint)source >= 3 || (uint)destination >= 3 || source == destination)
            throw new ArgumentOutOfRangeException(nameof(destination));
        int distance = Math.Abs(source - destination);
        int shape = (distance == 2 ? 2 : 0) + (source > destination ? 1 : 0);
        return new FileCopyArrowPlacement(shape, CoordinateAxis.Wrap(distance == 2 ? 104 : 88 + 32 * Math.Min(source, destination)));
    }
}
