using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Reads recorded bank-$86 operands as installed program frames, for checks against native captures. Frames that draw the
/// same native operand on different sheets are different pictures (#2045: the waiting Kago bug, on its Kago's sheet, and
/// Kraid's resting rocks, on his sets' shared sheet), so an operand is read as a frame from the sheet the projectile
/// showing it draws from.
/// </summary>
internal static class EnemyProjectileProgramFrameOperands
{
    /// <summary>
    /// The installed program frames, by the bank-$86 operand each is stored at, and by each operand that shows its picture
    /// again (#2210, #2249).
    /// </summary>
    private static readonly ILookup<ushort, EnemyProjectileFrameId> Frames =
        EnemyProjectileProgramFrameDefinitions.Operands.Select(pair => (Operand: pair.Value, Frame: pair.Key))
            .Concat(EnemyProjectileProgramFrameDefinitions.AlsoStoredAt.Select(pair => (Operand: pair.Key, Frame: pair.Value)))
            .ToLookup(entry => entry.Operand, entry => entry.Frame);

    /// <summary>
    /// The bank-$86 operand the installed frame shown from <paramref name="operand"/> is stored at: the operand itself, or,
    /// for an operand that stores a frame's picture again (#2234), that frame's key (<see cref="KeyOf"/>), zero for an
    /// operand that draws nothing (#2431).
    /// </summary>
    internal static ushort OwnOperand(ushort operand) =>
        EnemyProjectileProgramFrameDefinitions.DrawNothing.Contains(operand) ? HiddenKey
        : EnemyProjectileProgramFrameDefinitions.AlsoStoredAt.TryGetValue(operand, out EnemyProjectileFrameId frame)
            ? KeyOf(frame)
            : operand;

    /// <summary>The key a native check reads for an operand that draws nothing, as a hidden slot reads (#2431, #2558).</summary>
    internal const ushort HiddenKey = 0;

    /// <summary>The key a native check reads shown frame <paramref name="frame"/> by: the bank-$86 operand its installed program frame is stored at.</summary>
    internal static ushort KeyOf(EnemyProjectileFrameId frame) => EnemyProjectileProgramFrameSources.Operand(frame);

    /// <summary>
    /// The bank-$86 operands that store the picture of the installed frame stored at <paramref name="operand"/> again
    /// (#2273), in operand order.
    /// </summary>
    internal static IEnumerable<ushort> ShowingAgain(ushort operand) =>
        EnemyProjectileProgramFrameDefinitions.AlsoStoredAt.Keys.Where(again => OwnOperand(again) == operand).Order();

    /// <summary>Whether bank-$86 operand <paramref name="operand"/> stores an installed program frame.</summary>
    internal static bool Contains(ushort operand) => Frames.Contains(operand);

    /// <summary>
    /// The installed program frame stored at bank-$86 operand <paramref name="operand"/> that a projectile drawing from
    /// <paramref name="sheet"/> shows: the one frame stored there, or, where several are, the one that sheet holds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No installed program frame is stored at the operand.</exception>
    /// <exception cref="InvalidDataException">Several frames are stored at the operand and the sheet holds none or more than one of them.</exception>
    internal static EnemyProjectileFrameId Frame(ushort operand, SpawnedObjectOwner sheet)
    {
        EnemyProjectileFrameId[] stored = [.. Frames[operand]];
        if (stored.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(operand), operand, $"No installed program frame at $86:{operand:X4}.");
        if (stored.Length == 1)
            return stored[0];
        EnemyProjectileFrameId[] held = [.. stored.Where(frame => SheetHolds(sheet, frame))];
        return held.Length == 1
            ? held[0]
            : throw new InvalidDataException(
                $"$86:{operand:X4} stores {string.Join(", ", stored)}, of which the {sheet} sheet holds {held.Length}.");
    }

    /// <summary>
    /// Whether <paramref name="sheet"/> holds <paramref name="frame"/>: a spawner's own sheet the frames it spawns with its own
    /// graphics, Mother Brain's tank-glass sheet its shards and sparkles, and the shared sheet those spawned with the room's.
    /// </summary>
    private static bool SheetHolds(SpawnedObjectOwner sheet, EnemyProjectileFrameId frame)
    {
        if (SpawnedObjectFrameDefinitions.BySheet.TryGetValue(sheet, out SpawnedObjects? own))
            return own.Projectiles.Contains(frame);
        if (sheet.Spawner is { } spawner)
            return SpawnedObjectFrameDefinitions.ByEnemy.TryGetValue(spawner, out SpawnedObjects? spawned) && spawned.Projectiles.Contains(frame);
        return SpawnedObjectFrameDefinitions.SharedByEnemy.Values.Any(shared => shared.Projectiles.Contains(frame))
            || SpawnedObjectFrameDefinitions.Common.Any(common => common.Objects.Projectiles.Contains(frame));
    }
}
