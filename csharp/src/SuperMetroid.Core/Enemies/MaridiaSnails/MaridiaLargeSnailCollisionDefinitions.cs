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
        { EnemyFrameId.OumFacingLeftIdle, [new(-16, -17, -8, 16, SafeTouch, Shot), new(-8, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftAttackingSecond, [new(-16, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftAttackingThird, [new(-1, -17, 14, 16, SafeTouch, NoShot), new(-17, -17, 0, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftAttackingFourth, [new(-20, -8, 0, 8, DamageTouch, Shot), new(0, -17, 13, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftAttackingFifth, [new(-22, -8, 0, 7, DamageTouch, Shot), new(0, -17, 14, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftAttackingSixth, [new(-25, -9, 0, 8, DamageTouch, Shot), new(0, -18, 14, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftAttackingSeventh, [new(-24, -8, 0, 9, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftAttackingEighth, [new(-27, -8, 0, 8, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftRollingForwardsSecond, [new(-16, 0, 0, 16, SafeTouch, Shot), new(-16, -16, 0, 0, SafeTouch, NoShot), new(0, -16, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftRollingForwardsThird, [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftRollingForwardsFourth, [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, NoShot), new(0, 0, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftRollingForwardsFifth, [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingLeftRollingForwardsSixth, [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, Shot), new(0, 0, 15, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftRollingForwardsSeventh, [new(-16, -18, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingLeftRollingForwardsEighth, [new(-16, 0, 0, 16, SafeTouch, NoShot), new(-16, -17, 0, 0, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightIdle, [new(-16, -17, 8, 16, SafeTouch, NoShot), new(8, -17, 16, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightAttackingSecond, [new(-16, -17, 16, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightAttackingThird, [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightAttackingFourth, [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 20, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightAttackingFifth, [new(-16, -17, -1, 16, DamageTouch, NoShot), new(0, -8, 22, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightAttackingSixth, [new(-16, -18, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightAttackingSeventh, [new(-15, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightAttackingEighth, [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 25, 8, DamageTouch, Shot)] },
        { EnemyFrameId.OumFacingRightRollingForwardsSecond, [new(-15, -16, 0, 16, SafeTouch, NoShot), new(0, -16, 16, 0, SafeTouch, NoShot), new(0, 0, 16, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightRollingForwardsThird, [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)] },
        { EnemyFrameId.OumFacingRightRollingForwardsFourth, [new(-16, 0, 0, 17, SafeTouch, Shot), new(-16, -17, 0, 0, SafeTouch, NoShot), new(0, -17, 14, 17, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightRollingForwardsFifth, [new(-16, -17, 0, 16, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightRollingForwardsSixth, [new(-16, -17, 0, 0, SafeTouch, Shot), new(-16, 0, 0, 16, SafeTouch, NoShot), new(0, -17, 14, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightRollingForwardsSeventh, [new(-16, -17, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)] },
        { EnemyFrameId.OumFacingRightRollingForwardsEighth, [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 0, SafeTouch, Shot), new(0, 0, 16, 16, SafeTouch, NoShot)] },
    };
}
