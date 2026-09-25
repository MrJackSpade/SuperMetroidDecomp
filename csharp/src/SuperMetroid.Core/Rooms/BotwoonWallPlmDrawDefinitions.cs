namespace SuperMetroid.Core.Rooms;

/// <summary>The bounded nine-block vertical wall-clear draw at $84:930F.</summary>
internal static class BotwoonWallPlmDrawDefinitions
{
    /// <summary><c>$84:930F</c>: clear nine consecutive wall blocks.</summary>
    internal const ushort ClearPointer = 0x930f;
    /// <summary><c>$84:9325</c>: first byte of the following unused draw list.</summary>
    internal const ushort EndExclusive = 0x9325;

    internal static readonly RoomPlmShotBlockDrawDefinitions.DrawList Clear =
        new(ClearPointer, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(0x8009, Enumerable.Repeat((ushort)0x00ff, 9).ToArray(), 0, 0),
        });

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        [Clear];

    internal static string VisualId(ushort pointer) => pointer == ClearPointer
        ? "clear-wall"
        : throw new InvalidDataException(
            $"Botwoon wall draw ${pointer:X4} has no visual ID.");
}
