using static SuperMetroid.Core.Bosses.Ridley.RidleyExplosionDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's death breakup: the order its parts spawn in, the tail pose of each tail fragment and the position and pose
/// of each body fragment.
/// </summary>
internal static class RidleyBreakup
{
    /// <summary>Spawns every part in the breakup's allocation order.</summary>
    internal static void SpawnInBreakupOrder(Action<RidleyExplosionPart> spawn)
    {
        Ensure.NotNull(spawn);
        foreach (RidleyExplosionPart part in BreakupOrder)
            spawn(part);
    }

    /// <summary>
    /// Selects the tail pose the breakup initializer loads; the tip's comes from the direction it points, turned by the
    /// link before it (<see cref="TailTipBreakupFrames"/>). <paramref name="tailTipDirection"/> is ignored for the first
    /// six segments.
    /// </summary>
    public static RidleyBreakupFrame SelectTailFrame(RidleyExplosionPart parameter, TrigonometryAngle tailTipDirection)
    {
        if (TailFrames.TryGetValue(parameter, out RidleyBreakupFrame frame))
            return frame;
        if (parameter == RidleyExplosionPart.TailTip)
            return TailTipBreakupFrames.At(tailTipDirection);
        return Enum.IsDefined(parameter)
            ? throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Body fragments have no tail program.")
            : throw new InvalidOperationException($"Undefined Ridley explosion part {parameter}.");
    }

    /// <summary>
    /// Selects the body-relative position and pose the breakup initializer
    /// loads. Facing zero selects the left-facing record.
    /// </summary>
    public static RidleyExplosionBodyPartDefinition SelectBodyPart(RidleyExplosionPart parameter, bool facingRight) =>
        BodyParts.TryGetValue(new RidleyExplosionPartFacing(parameter, facingRight), out RidleyExplosionBodyPartDefinition part)
            ? part
            : throw new ArgumentOutOfRangeException(nameof(parameter));
}
