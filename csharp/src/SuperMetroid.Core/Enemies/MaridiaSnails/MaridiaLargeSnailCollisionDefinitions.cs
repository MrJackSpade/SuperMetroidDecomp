using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Physical collision for Oum's 30 frames. Every frame
/// has exactly one component at (0,0); only its hitbox-list
/// changes. The editable sprite pieces and the three callback identities are
/// intentionally separate from this catalog.
/// </summary>
internal static class MaridiaLargeSnailCollisionDefinitions
{
    /// <summary>Callback used by Oum's extended hitboxes that hurt Samus on contact.</summary>
    private const EnemyReaction DamageTouch = EnemyReaction.MaridiaLargeSnailDamagingTouch;
    /// <summary>Callback for Oum surfaces that permit harmless contact.</summary>
    private const EnemyReaction SafeTouch = EnemyReaction.MaridiaLargeSnailNonDamagingTouch;
    /// <summary>Callback for the shot-sensitive portion of an Oum frame.</summary>
    private const EnemyReaction Shot = EnemyReaction.MaridiaLargeSnailShot;
    /// <summary>No-op callback on Oum hitboxes that do not react to shots.</summary>
    private const EnemyReaction NoShot = EnemyReaction.Ignore;

    /// <summary>The 30 hitbox lists, one per frame in frame order; the rectangles are fitted to the drawings.</summary>
    internal static readonly EnemyCollisionHitbox[][] Lists =
    [
        [new(-16, -17, -8, 16, SafeTouch, Shot), new(-8, -17, 14, 16, SafeTouch, NoShot)],
        [new(-16, -17, 14, 16, SafeTouch, NoShot)],
        [new(-1, -17, 14, 16, SafeTouch, NoShot), new(-17, -17, 0, 16, SafeTouch, Shot)],
        [new(-20, -8, 0, 8, DamageTouch, Shot), new(0, -17, 13, 16, DamageTouch, NoShot)],
        [new(-22, -8, 0, 7, DamageTouch, Shot), new(0, -17, 14, 16, DamageTouch, NoShot)],
        [new(-25, -9, 0, 8, DamageTouch, Shot), new(0, -18, 14, 16, DamageTouch, NoShot)],
        [new(-24, -8, 0, 9, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)],
        [new(-27, -8, 0, 8, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)],
        [new(-16, 0, 0, 16, SafeTouch, Shot), new(-16, -16, 0, 0, SafeTouch, NoShot), new(0, -16, 14, 16, SafeTouch, NoShot)],
        [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)],
        [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, NoShot), new(0, 0, 15, 16, SafeTouch, Shot)],
        [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 16, SafeTouch, Shot)],
        [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, Shot), new(0, 0, 15, 16, SafeTouch, NoShot)],
        [new(-16, -18, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)],
        [new(-16, 0, 0, 16, SafeTouch, NoShot), new(-16, -17, 0, 0, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)],
        [new(-16, -17, 8, 16, SafeTouch, NoShot), new(8, -17, 16, 16, SafeTouch, Shot)],
        [new(-16, -17, 16, 16, SafeTouch, NoShot)],
        [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 16, SafeTouch, Shot)],
        [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 20, 8, DamageTouch, Shot)],
        [new(-16, -17, -1, 16, DamageTouch, NoShot), new(0, -8, 22, 8, DamageTouch, Shot)],
        [new(-16, -18, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)],
        [new(-15, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)],
        [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 25, 8, DamageTouch, Shot)],
        [new(-15, -16, 0, 16, SafeTouch, NoShot), new(0, -16, 16, 0, SafeTouch, NoShot), new(0, 0, 16, 16, SafeTouch, Shot)],
        [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)],
        [new(-16, 0, 0, 17, SafeTouch, Shot), new(-16, -17, 0, 0, SafeTouch, NoShot), new(0, -17, 14, 17, SafeTouch, NoShot)],
        [new(-16, -17, 0, 16, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)],
        [new(-16, -17, 0, 0, SafeTouch, Shot), new(-16, 0, 0, 16, SafeTouch, NoShot), new(0, -17, 14, 16, SafeTouch, NoShot)],
        [new(-16, -17, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)],
        [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 0, SafeTouch, Shot), new(0, 0, 16, 16, SafeTouch, NoShot)],
    ];

    /// <summary>Each of the 30 one-component Oum frames, whose single zero-offset component holds its hitbox list.</summary>
    internal static EnemyFrameCollision<EnemyCollisionHitbox> Frames { get; } = new()
    {
        { EnemyFrameId.OumFacingLeftIdle, Lists[0] },
        { EnemyFrameId.OumFacingLeftAttackingSecond, Lists[1] },
        { EnemyFrameId.OumFacingLeftAttackingThird, Lists[2] },
        { EnemyFrameId.OumFacingLeftAttackingFourth, Lists[3] },
        { EnemyFrameId.OumFacingLeftAttackingFifth, Lists[4] },
        { EnemyFrameId.OumFacingLeftAttackingSixth, Lists[5] },
        { EnemyFrameId.OumFacingLeftAttackingSeventh, Lists[6] },
        { EnemyFrameId.OumFacingLeftAttackingEighth, Lists[7] },
        { EnemyFrameId.OumFacingLeftRollingForwardsSecond, Lists[8] },
        { EnemyFrameId.OumFacingLeftRollingForwardsThird, Lists[9] },
        { EnemyFrameId.OumFacingLeftRollingForwardsFourth, Lists[10] },
        { EnemyFrameId.OumFacingLeftRollingForwardsFifth, Lists[11] },
        { EnemyFrameId.OumFacingLeftRollingForwardsSixth, Lists[12] },
        { EnemyFrameId.OumFacingLeftRollingForwardsSeventh, Lists[13] },
        { EnemyFrameId.OumFacingLeftRollingForwardsEighth, Lists[14] },
        { EnemyFrameId.OumFacingRightIdle, Lists[15] },
        { EnemyFrameId.OumFacingRightAttackingSecond, Lists[16] },
        { EnemyFrameId.OumFacingRightAttackingThird, Lists[17] },
        { EnemyFrameId.OumFacingRightAttackingFourth, Lists[18] },
        { EnemyFrameId.OumFacingRightAttackingFifth, Lists[19] },
        { EnemyFrameId.OumFacingRightAttackingSixth, Lists[20] },
        { EnemyFrameId.OumFacingRightAttackingSeventh, Lists[21] },
        { EnemyFrameId.OumFacingRightAttackingEighth, Lists[22] },
        { EnemyFrameId.OumFacingRightRollingForwardsSecond, Lists[23] },
        { EnemyFrameId.OumFacingRightRollingForwardsThird, Lists[24] },
        { EnemyFrameId.OumFacingRightRollingForwardsFourth, Lists[25] },
        { EnemyFrameId.OumFacingRightRollingForwardsFifth, Lists[26] },
        { EnemyFrameId.OumFacingRightRollingForwardsSixth, Lists[27] },
        { EnemyFrameId.OumFacingRightRollingForwardsSeventh, Lists[28] },
        { EnemyFrameId.OumFacingRightRollingForwardsEighth, Lists[29] },
    };
}
