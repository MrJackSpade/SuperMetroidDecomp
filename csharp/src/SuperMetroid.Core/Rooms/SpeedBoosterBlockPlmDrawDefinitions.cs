namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The one-block native bank-$84:A4F3 reveal used by bomb-special Speed Booster
/// terrain. Its type-$B level word remains physical even when appearance changes.
/// </summary>
internal static class SpeedBoosterBlockPlmDrawDefinitions
{
    /// <summary><c>$84:A4F3</c>: bomb reveal of one Speed Booster block.</summary>
    internal const string BombRevealVisualId = "bomb-reveal";

    /// <summary>Physical type-B parent with the named Speed Booster visual block.</summary>
    internal static ushort BombRevealWord => (ushort)(0xb000 | RoomPlmVisualBlockIndexes.SpeedBoosterParent);

    // Materialize records only for existing artwork import/export interfaces.
    internal static RoomPlmShotBlockDrawDefinitions.DrawList BombReveal =>
        new(SpeedBoosterBlockPlmProgramDefinitions.BombRevealDraw,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(1, new ushort[] { BombRevealWord }, 0, 0),
            });

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        [BombReveal];

    internal static string VisualId(ushort pointer) => pointer == SpeedBoosterBlockPlmProgramDefinitions.BombRevealDraw
        ? BombRevealVisualId
        : throw new InvalidDataException(
            $"Speed Booster draw ${pointer:X4} has no visual ID.");

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (string.Equals(id, BombRevealVisualId, StringComparison.Ordinal))
        {
            list = BombReveal;
            return true;
        }
        list = default;
        return false;
    }

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (pointer == SpeedBoosterBlockPlmProgramDefinitions.BombRevealDraw)
        {
            list = BombReveal;
            return true;
        }
        list = default;
        return false;
    }
}
