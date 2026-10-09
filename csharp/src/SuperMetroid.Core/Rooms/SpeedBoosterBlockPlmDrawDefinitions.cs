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
    /// <summary>Provides the one-run draw list that pairs the physical parent word with the bomb-reveal artwork.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList BombReveal =>
        new(SpeedBoosterBlockPlmProgramDefinitions.BombRevealDraw,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(1, new ushort[] { BombRevealWord }, 0, 0),
            });

    /// <summary>Enumerates the sole calculated bomb-reveal record for artwork import/export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get { yield return BombReveal; }
    }

    /// <summary>Resolves the bomb-reveal draw instruction to its stable artwork identifier.</summary>
    /// <param name="pointer">Instruction pointer for the Speed Booster block draw program.</param>
    /// <returns>The visual ID used by the artwork import and export records.</returns>
    /// <exception cref="InvalidDataException">The pointer is not the compiled bomb-reveal draw instruction.</exception>
    internal static string VisualId(ushort pointer) => pointer == SpeedBoosterBlockPlmProgramDefinitions.BombRevealDraw
        ? BombRevealVisualId
        : throw new InvalidDataException(
            $"Speed Booster draw ${pointer:X4} has no visual ID.");

    /// <summary>Finds the calculated draw list associated with the bomb-reveal artwork identifier.</summary>
    /// <param name="id">Visual identifier compared using ordinal, case-sensitive matching.</param>
    /// <param name="list">Receives the bomb-reveal draw list on success, or the default value when not found.</param>
    /// <returns><see langword="true"/> when <paramref name="id"/> names the bomb-reveal artwork.</returns>
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
}
