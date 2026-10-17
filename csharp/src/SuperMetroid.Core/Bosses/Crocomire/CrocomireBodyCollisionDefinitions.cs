using SuperMetroid.Core.Rooms.LevelData;

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
    /// <summary>Native bank containing Crocomire's body frames and hitboxes.</summary>
    internal const byte Bank = 0xa4;

    // The hitbox lists precede the frames that reference them: static fields initialise in declaration order.
    /// <summary>Lower-front body rectangle using the standard touch and shot callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] LowerFrontBody = [new(-78, 32, -16, 43, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.AlternateDust)];
    /// <summary>Thin horizontal jaw/body contact strip using standard callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] JawStrip = [new(-95, 11, -16, 11, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.AlternateDust)];
    /// <summary>Lower body rectangle for another mouth pose, retaining the standard callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] LowerBodyOtherMouthPose = [new(-69, 31, -16, 44, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.AlternateDust)];
    /// <summary>Mid-body rectangle with the standard damage callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] MidBody = [new(-60, 18, -16, 32, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.AlternateDust)];
    /// <summary>Wider mid-body rectangle with the standard damage callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] WideMidBody = [new(-80, 13, -16, 27, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.AlternateDust)];
    /// <summary>The empty list of the two low components every fight frame carries.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] EmptyLowComponent = [];
    /// <summary>Raised mouth rectangle whose shot callback differs from the standard body set.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] RaisedMouth = [new(-95, -6, -16, 27, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.CountHit)];
    /// <summary>Upper mouth rectangle using the alternate shot callback.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] UpperMouth = [new(-59, -9, -16, 5, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.CountHit)];
    /// <summary>High mouth rectangle used by a raised body pose with alternate shot handling.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] HighMouth = [new(-93, -36, -30, -13, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.CountHit)];
    /// <summary>The empty list of the upper component in the attack frames.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] EmptyAttackUpperComponent = [];
    /// <summary>Three-piece upper body collision list with separate shot handling for its first piece.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] UpperBody = [new(-50, -37, 40, -16, CrocomireHitboxTouch.Common, CrocomireHitboxShot.Mouth), new(-52, -58, 42, -38, CrocomireHitboxTouch.Common, CrocomireHitboxShot.Common), new(-32, -13, 40, 0, CrocomireHitboxTouch.Common, CrocomireHitboxShot.Common)];
    /// <summary>Three-piece raised upper-body list with a taller first rectangle.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] RaisedUpperBody = [new(-46, -53, 13, -16, CrocomireHitboxTouch.Common, CrocomireHitboxShot.Mouth), new(-41, -74, 16, -57, CrocomireHitboxTouch.Common, CrocomireHitboxShot.Common), new(-37, -16, 16, -3, CrocomireHitboxTouch.Common, CrocomireHitboxShot.Common)];
    /// <summary>Broad high body rectangle whose shot callback is the common no-op.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] BroadHighBody = [new(-37, -50, 38, -4, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.Common)];
    /// <summary>Full-height central body rectangle with the main fight shot callback.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] CentralBody = [new(-38, -48, 37, 52, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.Dust)];
    /// <summary>Lower central body rectangle using the alternate shot callback.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] LowerCentralBody = [new(-38, -4, 42, 52, CrocomireHitboxTouch.Claw, CrocomireHitboxShot.CountHit)];

    /// <summary>Four native frame runs containing the fifty physical component lists, with presentation art excluded.</summary>
    private static readonly CollisionRecordRuns<CrocomireBodyCollisionComponent> Frames = CollisionRecordRuns<CrocomireBodyCollisionComponent>.Frames(
        new(0xbfc4,
        [
            [new(new(3, 11), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, -1), CentralBody)],
            [new(new(1, 11), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)],
            [new(new(0, 8), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 10), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 10), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)],
            [new(new(1, 10), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)],
            [new(new(1, 10), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 12), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 13), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)],
        ]),
        new(0xc2ec,
        [
            [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 11), LowerBodyOtherMouthPose), new(new(-3, -28), EmptyAttackUpperComponent), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 11), LowerBodyOtherMouthPose), new(new(0, 41), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 11), UpperMouth), new(new(0, 37), EmptyLowComponent), new(new(-29, 41), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 8), HighMouth), new(new(0, 39), EmptyLowComponent), new(new(-29, 39), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 8), RaisedMouth), new(new(0, 41), EmptyLowComponent), new(new(-29, 32), EmptyLowComponent), new(new(0, -4), CentralBody), new(new(0, -4), CentralBody), new(new(0, -4), CentralBody)],
            [new(new(0, 10), JawStrip), new(new(0, 39), EmptyLowComponent), new(new(-29, 37), EmptyLowComponent), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 12), LowerFrontBody), new(new(0, 37), EmptyLowComponent), new(new(-29, 40), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody), new(new(0, -1), CentralBody)],
            [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), UpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
        ]),
        new(0xc6a4,
        [
            [new(new(3, 11), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), BroadHighBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, -1), CentralBody)],
        ]),
        new(0xc752,
        [
            [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 10), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 10), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, -2), CentralBody)],
            [new(new(1, 10), MidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 10), WideMidBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, -1), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 10), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 12), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), RaisedUpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 13), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), UpperBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 11), LowerBodyOtherMouthPose), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), CentralBody), new(new(0, 0), BroadHighBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 11), UpperMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 6), HighMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 4), RaisedMouth), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 8), JawStrip), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(1, 11), LowerFrontBody), new(new(0, 38), EmptyLowComponent), new(new(-29, 38), EmptyLowComponent), new(new(0, 0), RaisedUpperBody), new(new(0, 0), CentralBody), new(new(0, 0), LowerCentralBody), new(new(0, 0), CentralBody)],
            [new(new(0, 0), JawStrip)],
            [new(new(0, 0), JawStrip)],
            [new(new(0, 0), JawStrip)],
            [new(new(0, 0), JawStrip)],
            [new(new(0, 0), JawStrip)],
            [new(new(0, 0), JawStrip)],
            [new(new(0, 0), JawStrip)],
            [new(new(0, 0), JawStrip)],
        ]));

    /// <summary>Accepts a living-body frame or delegates defeated-body frames to the corpse catalog.</summary>
    internal static bool HasFrame(ushort frame) => Frames.TryGet(frame, out _) ||
        CrocomireCorpseCollisionDefinitions.HasFrame(frame);

    /// <summary>Finds the physical components, each with its own hitbox list, for a living or defeated frame.</summary>
    internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
        Frames.TryGet(frame, out CrocomireBodyCollisionComponent[] components)
            ? components
            : CrocomireCorpseCollisionDefinitions.ComponentsAt(frame);
}
