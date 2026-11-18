using SuperMetroid.Core.Bosses.Ridley.Artwork;
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
    internal static RidleyTailLink? TailLinkBecoming(RidleyExplosionPart part, RidleyTailState tail) => part switch
    {
        RidleyExplosionPart.TailBase => tail.Base,
        RidleyExplosionPart.TailFirstShaftLink => tail.Shaft[0],
        RidleyExplosionPart.TailSecondShaftLink => tail.Shaft[1],
        RidleyExplosionPart.TailThirdShaftLink => tail.Shaft[2],
        RidleyExplosionPart.TailFourthShaftLink => tail.Shaft[3],
        RidleyExplosionPart.TailInnerJoint => tail.InnerJoint,
        RidleyExplosionPart.TailTip => tail.Tip,
        RidleyExplosionPart.Wings or RidleyExplosionPart.Legs or RidleyExplosionPart.OpenHeadAndNeck or RidleyExplosionPart.Torso
            or RidleyExplosionPart.Claw => null,
        _ => throw new ArgumentOutOfRangeException(nameof(part), part, "Ridley's breakup has no such part."),
    };

    /// <summary>
    /// Selects the tail pose the breakup setup loads; the tip's is the drawn tip's for the direction it points, turned by
    /// the link before it (<see cref="RidleySupplementalVisualDefinitions.TailTipFrames"/>). <paramref name="tailTipDirection"/> is ignored for the first
    /// six segments.
    /// </summary>
    public static EnemyFrameId SelectTailFrame(RidleyExplosionPart part, TrigonometryAngle tailTipDirection)
    {
        if (TailFrames.TryGetValue(part, out EnemyFrameId frame))
            return frame;
        if (part == RidleyExplosionPart.TailTip)
            return RidleySupplementalVisualDefinitions.TailTipFrames.At(tailTipDirection);
        throw new ArgumentOutOfRangeException(nameof(part), part, "Body fragments show no tail frame.");
    }

    /// <summary>Where body fragment <paramref name="part"/> starts from Ridley's body, by the way he faced: facing right mirrors its left-facing place.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="part"/> is not a body fragment.</exception>
    public static Offset16<RoomPixels> BodyPartOffset(RidleyExplosionPart part, HorizontalDirection facing) =>
        facing == HorizontalDirection.Right ? BodyPart(part).FacingLeftOffset.MirroredX() : BodyPart(part).FacingLeftOffset;

    /// <summary>The pose body fragment <paramref name="part"/> shows, by the way Ridley faced.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="part"/> is not a body fragment.</exception>
    public static EnemyFrameId BodyPartFrame(RidleyExplosionPart part, HorizontalDirection facing) =>
        facing == HorizontalDirection.Right ? BodyPart(part).FacingRightFrame : BodyPart(part).FacingLeftFrame;

    /// <summary>Body fragment <paramref name="part"/>'s place and poses.</summary>
    private static RidleyExplosionBodyPartDefinition BodyPart(RidleyExplosionPart part) =>
        BodyParts.TryGetValue(part, out RidleyExplosionBodyPartDefinition definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(part), part, "Tail fragments have no body place.");
}
