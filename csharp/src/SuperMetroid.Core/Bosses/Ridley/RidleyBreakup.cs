using static SuperMetroid.Core.Bosses.Ridley.RidleyExplosionDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's death breakup: the order its parts spawn in, the tail pose of each tail fragment and the position and pose
/// of each body fragment.
/// </summary>
internal static class RidleyBreakup
{
    /// <summary>Spawns every part's authored parameter in the breakup's allocation order.</summary>
    internal static void SpawnInBreakupOrder(Action<int> spawn)
    {
        Ensure.NotNull(spawn);
        foreach (RidleyExplosionPart part in BreakupOrder)
            spawn(PartParameters.Parameter(part));
    }

    /// <summary>
    /// Selects the tail pose loaded by <c>$A6:C6FE-$A6:C7B9</c>; the tip's comes from its orientation, as the sixteen
    /// tip lists selected at $A6:C7BA do. <paramref name="tailTipOrientation"/> is ignored for the first six segments.
    /// </summary>
    public static RidleyBreakupFrame SelectTailFrame(RidleyExplosionPart parameter, int tailTipOrientation)
    {
        if (TailFrames.TryGetValue(parameter, out RidleyBreakupFrame frame))
            return frame;
        if (parameter == RidleyExplosionPart.TailTip)
            return (uint)tailTipOrientation < TailTipOrientationCount
                ? RidleyBreakupFrame.TailTip0 + tailTipOrientation
                : throw new ArgumentOutOfRangeException(nameof(tailTipOrientation));
        return Enum.IsDefined(parameter)
            ? throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Body fragments have no tail program.")
            : throw new InvalidOperationException($"Undefined Ridley explosion part {parameter}.");
    }

    /// <summary>
    /// Selects the body-relative position and pose loaded by
    /// <c>$A6:C7DA-$A6:C8D3</c>. Facing zero selects the native left-facing record.
    /// </summary>
    public static RidleyExplosionBodyPartDefinition SelectBodyPart(RidleyExplosionPart parameter, bool facingRight) =>
        BodyParts.TryGetValue(new RidleyExplosionPartFacing(parameter, facingRight), out RidleyExplosionBodyPartDefinition part)
            ? part
            : throw new ArgumentOutOfRangeException(nameof(parameter));
}
