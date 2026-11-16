using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The hitboxes of each of Oum's 30 frames, which change as it rolls and attacks, each with the reaction it gives;
/// they are independent of the pictures the frames show.
/// </summary>
internal static class MaridiaLargeSnailCollisionDefinitions
{
    /// <summary>Reaction used by Oum's extended hitboxes that hurt Samus on contact.</summary>
    private static readonly EnemyReaction DamageTouch = MaridiaLargeSnailReactions.DamagingTouch;
    /// <summary>Reaction for Oum surfaces that permit harmless contact.</summary>
    private static readonly EnemyReaction SafeTouch = MaridiaLargeSnailReactions.NonDamagingTouch;
    /// <summary>Reaction for the shot-sensitive portion of an Oum frame.</summary>
    private static readonly EnemyReaction Shot = MaridiaLargeSnailReactions.Shot;
    /// <summary>No-op reaction on Oum hitboxes that do not react to shots.</summary>
    private static readonly EnemyReaction NoShot = EnemyReaction.Ignore;

    /// <summary>
    /// Each of the 30 one-component Oum frames, whose single zero-offset component holds the frame's own hitbox list; the
    /// rectangles are fitted to the drawings.
    /// </summary>
    internal static EnemyFrameCollision<EnemyCollisionHitbox> Frames { get; } = new()
    {
        { EnemyFrameId.OumFacingLeftTurned0, [new(-16, -17, -8, 16, SafeTouch, Shot), new(-8, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftVentOpening, [new(-16, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftVentOpen, [new(-1, -17, 14, 16, SafeTouch, NoShot), new(-17, -17, 0, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftJetFlaring, [new(-20, -8, 0, 8, DamageTouch, Shot), new(0, -17, 13, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftJetShort, [new(-22, -8, 0, 7, DamageTouch, Shot), new(0, -17, 14, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftJetMedium, [new(-25, -9, 0, 8, DamageTouch, Shot), new(0, -18, 14, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftJetLong, [new(-24, -8, 0, 9, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftJetLongest, [new(-27, -8, 0, 8, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftTurned45, [new(-16, 0, 0, 16, SafeTouch, Shot), new(-16, -16, 0, 0, SafeTouch, NoShot), new(0, -16, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftTurned90, [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftTurned135, [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, NoShot), new(0, 0, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftTurned180, [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftTurned225, [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, Shot), new(0, 0, 15, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftTurned270, [new(-16, -18, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftTurned315, [new(-16, 0, 0, 16, SafeTouch, NoShot), new(-16, -17, 0, 0, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightTurned0, [new(-16, -17, 8, 16, SafeTouch, NoShot), new(8, -17, 16, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightVentOpening, [new(-16, -17, 16, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightVentOpen, [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightJetFlaring, [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 20, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightJetShort, [new(-16, -17, -1, 16, DamageTouch, NoShot), new(0, -8, 22, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightJetMedium, [new(-16, -18, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightJetLong, [new(-15, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightJetLongest, [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 25, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightTurned45, [new(-15, -16, 0, 16, SafeTouch, NoShot), new(0, -16, 16, 0, SafeTouch, NoShot), new(0, 0, 16, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightTurned90, [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightTurned135, [new(-16, 0, 0, 17, SafeTouch, Shot), new(-16, -17, 0, 0, SafeTouch, NoShot), new(0, -17, 14, 17, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightTurned180, [new(-16, -17, 0, 16, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightTurned225, [new(-16, -17, 0, 0, SafeTouch, Shot), new(-16, 0, 0, 16, SafeTouch, NoShot), new(0, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightTurned270, [new(-16, -17, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightTurned315, [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 0, SafeTouch, Shot), new(0, 0, 16, 16, SafeTouch, NoShot)] },
    };
}
