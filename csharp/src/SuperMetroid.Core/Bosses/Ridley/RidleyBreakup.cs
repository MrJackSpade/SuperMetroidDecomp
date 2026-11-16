using SuperMetroid.Core.Enemies.Artwork;
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

    /// <summary>The link of <paramref name="tail"/> that becomes tail fragment <paramref name="part"/>, or null for a body fragment.</summary>
    internal static RidleyTailLink? TailLinkBecoming(RidleyExplosionPart part, RidleyTailState tail) =>
        tail.Links.Zip(TailLinkParts, (link, linkPart) => linkPart == part ? link : null).OfType<RidleyTailLink>().SingleOrDefault();

    /// <summary>
    /// Selects the tail pose the breakup initializer loads; the tip's comes from the direction it points, turned by the
    /// link before it (<see cref="TailTipBreakupFrames"/>). <paramref name="tailTipDirection"/> is ignored for the first
    /// six segments.
    /// </summary>
    public static EnemyFrameId SelectTailFrame(RidleyExplosionPart parameter, TrigonometryAngle tailTipDirection)
    {
        if (TailFrames.TryGetValue(parameter, out EnemyFrameId frame))
            return frame;
        if (parameter == RidleyExplosionPart.TailTip)
            return TailTipBreakupFrames.At(tailTipDirection);
        throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Body fragments show no tail frame.");
    }

    /// <summary>The body-relative position and pose a breakup fragment starts with, by the way Ridley faced.</summary>
    public static RidleyExplosionBodyPartDefinition SelectBodyPart(RidleyExplosionPart parameter, bool facingRight) =>
        BodyParts.TryGetValue(new RidleyExplosionPartFacing(parameter, facingRight), out RidleyExplosionBodyPartDefinition part)
            ? part
            : throw new ArgumentOutOfRangeException(nameof(parameter));
}
