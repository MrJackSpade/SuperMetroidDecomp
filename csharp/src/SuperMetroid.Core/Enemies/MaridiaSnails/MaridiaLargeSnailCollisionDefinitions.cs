using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Physical collision for Oum's 30 bank-$A2 extended frames. Every native frame
/// at $CB87-$CCA9 has exactly one component at (0,0); only its hitbox-list
/// pointer changes. The editable OAM and the three callback identities are
/// intentionally separate from this catalog.
/// </summary>
internal static class MaridiaLargeSnailCollisionDefinitions
{
    /// <summary>Native bank containing Oum's extended frames and hitbox lists.</summary>
    /// <summary>Bank-$A2 callback used by Oum's extended hitboxes that hurt Samus on contact.</summary>
    private const EnemyInteractionCallback DamageTouch = EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch;
    /// <summary>Bank-$A2 callback for Oum surfaces that permit harmless contact.</summary>
    private const EnemyInteractionCallback SafeTouch = EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch;
    /// <summary>Bank-$A2 callback for the shot-sensitive portion of an Oum frame.</summary>
    private const EnemyInteractionCallback Shot = EnemyInteractionCallback.MaridiaLargeSnailShot;
    /// <summary>Bank-$A0 no-op callback on Oum hitboxes that do not react to shots.</summary>
    private const EnemyInteractionCallback NoShot = EnemyInteractionCallback.NoOp;

    /// <summary>The 30 hitbox lists, one per frame in frame order; the rectangles are fitted to the drawings.</summary>
    internal static readonly MaridiaLargeSnailCollisionHitbox[][] Lists =
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
    internal static EnemyFrameCollision<MaridiaLargeSnailCollisionHitbox> Frames { get; } = new()
    {
        { EnemyFrameId.OumPose0, Lists[0] },
        { EnemyFrameId.OumPose1, Lists[1] },
        { EnemyFrameId.OumPose2, Lists[2] },
        { EnemyFrameId.OumPose3, Lists[3] },
        { EnemyFrameId.OumPose4, Lists[4] },
        { EnemyFrameId.OumPose5, Lists[5] },
        { EnemyFrameId.OumPose6, Lists[6] },
        { EnemyFrameId.OumPose7, Lists[7] },
        { EnemyFrameId.OumPose8, Lists[8] },
        { EnemyFrameId.OumPose9, Lists[9] },
        { EnemyFrameId.OumPose10, Lists[10] },
        { EnemyFrameId.OumPose11, Lists[11] },
        { EnemyFrameId.OumPose12, Lists[12] },
        { EnemyFrameId.OumPose13, Lists[13] },
        { EnemyFrameId.OumPose14, Lists[14] },
        { EnemyFrameId.OumPose15, Lists[15] },
        { EnemyFrameId.OumPose16, Lists[16] },
        { EnemyFrameId.OumPose17, Lists[17] },
        { EnemyFrameId.OumPose18, Lists[18] },
        { EnemyFrameId.OumPose19, Lists[19] },
        { EnemyFrameId.OumPose20, Lists[20] },
        { EnemyFrameId.OumPose21, Lists[21] },
        { EnemyFrameId.OumPose22, Lists[22] },
        { EnemyFrameId.OumPose23, Lists[23] },
        { EnemyFrameId.OumPose24, Lists[24] },
        { EnemyFrameId.OumPose25, Lists[25] },
        { EnemyFrameId.OumPose26, Lists[26] },
        { EnemyFrameId.OumPose27, Lists[27] },
        { EnemyFrameId.OumPose28, Lists[28] },
        { EnemyFrameId.OumPose29, Lists[29] },
    };
}
