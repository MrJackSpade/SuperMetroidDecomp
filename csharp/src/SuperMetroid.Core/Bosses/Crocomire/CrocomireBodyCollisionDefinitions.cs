using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Engine-owned collision data for Crocomire's fifty selected body frames at
/// $A4:BFC4..CAC4 and their fifteen distinct hitbox lists at $A4:CB07..CC2D.
/// OAM and BG2 art are deliberately absent: installed visual overrides cannot
/// alter the cartridge's component offsets, rectangles, or callback identities.
/// Values are pinned to the project's NTSC J/U v1.0 retail cartridge.
/// Corpse-frame requests delegate to the separate native corpse collision catalog.
/// </summary>
/// <remarks>
/// Reviewed for #1165. The 50 frame records lie in four contiguous runs, so frame pointers
/// derive from run starts and record sizes. Component placements (jaw, legs, body bands) and
/// list rectangles are fitted to the drawings and retained; Crocomire has no mirrored facing.
/// </remarks>
internal static class CrocomireBodyCollisionDefinitions
{
    /// <summary>Lower-front body rectangle using the standard touch and shot callbacks.</summary>
    /// <remarks>The hitbox lists precede the frames that reference them: static fields initialise in declaration
    /// order.</remarks>
    private static readonly EnemyCollisionHitbox[] LowerFrontBody = [new(-78, 32, -16, 43, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Thin horizontal jaw/body contact strip using standard callbacks.</summary>
    private static readonly EnemyCollisionHitbox[] JawStrip = [new(-95, 11, -16, 11, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Lower body rectangle for another mouth pose, retaining the standard callbacks.</summary>
    private static readonly EnemyCollisionHitbox[] LowerBodyOtherMouthPose = [new(-69, 31, -16, 44, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Mid-body rectangle with the standard damage callbacks.</summary>
    private static readonly EnemyCollisionHitbox[] MidBody = [new(-60, 18, -16, 32, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>Wider mid-body rectangle with the standard damage callbacks.</summary>
    private static readonly EnemyCollisionHitbox[] WideMidBody = [new(-80, 13, -16, 27, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireAlternateDustHitboxShot)];
    /// <summary>The empty list of the two low components every fight frame carries.</summary>
    private static readonly EnemyCollisionHitbox[] EmptyLowComponent = [];
    /// <summary>Raised mouth rectangle whose shot callback differs from the standard body set.</summary>
    private static readonly EnemyCollisionHitbox[] RaisedMouth = [new(-95, -6, -16, 27, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];
    /// <summary>Upper mouth rectangle using the alternate shot callback.</summary>
    private static readonly EnemyCollisionHitbox[] UpperMouth = [new(-59, -9, -16, 5, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];
    /// <summary>High mouth rectangle used by a raised body pose with alternate shot handling.</summary>
    private static readonly EnemyCollisionHitbox[] HighMouth = [new(-93, -36, -30, -13, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];
    /// <summary>The empty list of the upper component in the attack frames.</summary>
    private static readonly EnemyCollisionHitbox[] EmptyAttackUpperComponent = [];
    /// <summary>Three-piece upper body collision list with separate shot handling for its first piece.</summary>
    private static readonly EnemyCollisionHitbox[] UpperBody = [new(-50, -37, 40, -16, EnemyReaction.NormalEnemyTouch, EnemyReaction.CrocomireMouthShot), new(-52, -58, 42, -38, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot), new(-32, -13, 40, 0, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Three-piece raised upper-body list with a taller first rectangle.</summary>
    private static readonly EnemyCollisionHitbox[] RaisedUpperBody = [new(-46, -53, 13, -16, EnemyReaction.NormalEnemyTouch, EnemyReaction.CrocomireMouthShot), new(-41, -74, 16, -57, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot), new(-37, -16, 16, -3, EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Broad high body rectangle whose shot callback is the common no-op.</summary>
    private static readonly EnemyCollisionHitbox[] BroadHighBody = [new(-37, -50, 38, -4, EnemyReaction.CrocomireClawTouch, EnemyReaction.NormalEnemyShot)];
    /// <summary>Full-height central body rectangle with the main fight shot callback.</summary>
    private static readonly EnemyCollisionHitbox[] CentralBody = [new(-38, -48, 37, 52, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireDustHitboxShot)];
    /// <summary>Lower central body rectangle using the alternate shot callback.</summary>
    private static readonly EnemyCollisionHitbox[] LowerCentralBody = [new(-38, -4, 42, 52, EnemyReaction.CrocomireClawTouch, EnemyReaction.CrocomireHitCountShot)];

    /// <summary>The fifty living-body frames' physical component lists, with presentation art excluded.</summary>
    internal static EnemyFrameCollision<CrocomireBodyCollisionComponent> Frames { get; } = new()
    {
        { EnemyFrameId.CrocomireBody0, [new(new(3, 11), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBody1, [new(new(1, 11), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody2, [new(new(0, 8), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody3, [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody4, [new(new(1, 10), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody5, [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody6, [new(new(1, 10), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBody7, [new(new(1, 10), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBody8, [new(new(1, 10), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody9, [new(new(1, 12), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody10, [new(new(1, 13), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody11, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody12, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody13, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody14, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody15, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody16, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody17, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody18, [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody19, [new(new(1, 11), UpperMouth), new(new(0, 37), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody20, [new(new(0, 8), HighMouth), new(new(0, 39), EmptyLowComponent), new(new(-29, 39), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody21, [new(new(1, 8), RaisedMouth), new(new(0, 41), EmptyLowComponent), new(new(-29, 32), EmptyLowComponent), new(new(0, -4), CentralBody), new(new(0, -4), CentralBody), new(new(0, -4), CentralBody)] },
        { EnemyFrameId.CrocomireBody22, [new(new(0, 10), JawStrip), new(new(0, 39), EmptyLowComponent), new(new(-29, 37), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody23, [new(new(1, 12), LowerFrontBody), new(new(0, 37), EmptyLowComponent), new(new(-29, 40), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBody24, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody25, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), UpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody26, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody27, [new(new(3, 11), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, -1), CentralBody)] },
        { EnemyFrameId.CrocomireBody28, [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody29, [new(new(1, 10), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody30, [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)] },
        { EnemyFrameId.CrocomireBody31, [new(new(1, 10), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody32, [new(new(1, 10), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody33, [new(new(1, 10), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody34, [new(new(1, 12), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody35, [new(new(1, 13), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), UpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody36, [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody37, [new(new(1, 11), UpperMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody38, [new(new(0, 6), HighMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody39, [new(new(1, 4), RaisedMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody40, [new(new(0, 8), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody41, [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)] },
        { EnemyFrameId.CrocomireBody42, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBody43, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBody44, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBody45, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBody46, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBody47, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBody48, [new(new(0, 0), JawStrip)] },
        { EnemyFrameId.CrocomireBody49, [new(new(0, 0), JawStrip)] },
    };
}
