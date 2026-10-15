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

    /// <summary>Four native frame runs containing the fifty physical component lists, with presentation art excluded.</summary>
    private static readonly CollisionRecordRuns<CrocomireBodyCollisionComponent> Frames = CollisionRecordRuns<CrocomireBodyCollisionComponent>.Frames(
        new(0xbfc4,
        [
            [new(new(3, 11), 0xcb31), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f), new(new(0, -1), 0xcc1f)],
            [new(new(1, 11), 0xcb3f), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f)],
            [new(new(0, 8), 0xcb15), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f)],
            [new(new(1, 11), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, -1), 0xcc1f), new(new(0, -2), 0xcc1f)],
            [new(new(1, 10), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f)],
            [new(new(1, 10), 0xcb23), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f)],
            [new(new(1, 10), 0xcb31), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, -1), 0xcc1f), new(new(0, -1), 0xcc1f)],
            [new(new(1, 10), 0xcb3f), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, -1), 0xcc1f), new(new(0, -1), 0xcc1f)],
            [new(new(1, 10), 0xcb15), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f)],
            [new(new(1, 12), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f)],
            [new(new(1, 13), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f)],
            [new(new(1, 11), 0xcb23), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f)],
        ]),
        new(0xc2ec,
        [
            [new(new(0, 11), 0xcb23), new(new(-3, -28), 0xcbc3), new(new(0, 41), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 11), 0xcb23), new(new(-3, -28), 0xcbc3), new(new(0, 41), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 11), 0xcb23), new(new(-3, -28), 0xcbc3), new(new(0, 41), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 11), 0xcb23), new(new(-3, -28), 0xcbc3), new(new(0, 41), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 11), 0xcb23), new(new(-3, -28), 0xcbc3), new(new(0, 41), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 11), 0xcb23), new(new(-3, -28), 0xcbc3), new(new(0, 41), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 11), 0xcb23), new(new(0, 41), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 11), 0xcb5f), new(new(0, 37), 0xcb4d), new(new(-29, 41), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc1f)],
            [new(new(0, 8), 0xcb6d), new(new(0, 39), 0xcb4d), new(new(-29, 39), 0xcb4d), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f)],
            [new(new(1, 8), 0xcb4f), new(new(0, 41), 0xcb4d), new(new(-29, 32), 0xcb4d), new(new(0, -4), 0xcc1f), new(new(0, -4), 0xcc1f), new(new(0, -4), 0xcc1f)],
            [new(new(0, 10), 0xcb15), new(new(0, 39), 0xcb4d), new(new(-29, 37), 0xcb4d), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f), new(new(0, -2), 0xcc1f)],
            [new(new(1, 12), 0xcb07), new(new(0, 37), 0xcb4d), new(new(-29, 40), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, -1), 0xcc1f), new(new(0, -1), 0xcc1f)],
            [new(new(1, 11), 0xcb23), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 11), 0xcb23), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcbc5), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 11), 0xcb23), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
        ]),
        new(0xc6a4,
        [
            [new(new(3, 11), 0xcb31), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, -1), 0xcc1f)],
        ]),
        new(0xc752,
        [
            [new(new(1, 11), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc2d), new(new(0, -2), 0xcc1f)],
            [new(new(1, 10), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc2d), new(new(0, -2), 0xcc1f)],
            [new(new(1, 10), 0xcb23), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc2d), new(new(0, -2), 0xcc1f)],
            [new(new(1, 10), 0xcb31), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 10), 0xcb3f), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, -1), 0xcc1f), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 10), 0xcb15), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 12), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 13), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcbc5), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 11), 0xcb23), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc11), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 11), 0xcb5f), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 6), 0xcb6d), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 4), 0xcb4f), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 8), 0xcb15), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(1, 11), 0xcb07), new(new(0, 38), 0xcb4d), new(new(-29, 38), 0xcb4d), new(new(0, 0), 0xcbeb), new(new(0, 0), 0xcc1f), new(new(0, 0), 0xcc2d), new(new(0, 0), 0xcc1f)],
            [new(new(0, 0), 0xcb15)],
            [new(new(0, 0), 0xcb15)],
            [new(new(0, 0), 0xcb15)],
            [new(new(0, 0), 0xcb15)],
            [new(new(0, 0), 0xcb15)],
            [new(new(0, 0), 0xcb15)],
            [new(new(0, 0), 0xcb15)],
            [new(new(0, 0), 0xcb15)],
        ]));

    /// <summary>Lower-front body rectangle using the standard touch and shot callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB07 = [new(-78, 32, -16, 43, 0xb93d, 0xbab4)];
    /// <summary>Thin horizontal jaw/body contact strip using standard callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB15 = [new(-95, 11, -16, 11, 0xb93d, 0xbab4)];
    /// <summary>Lower body rectangle for another mouth pose, retaining the standard callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB23 = [new(-69, 31, -16, 44, 0xb93d, 0xbab4)];
    /// <summary>Mid-body rectangle with the standard damage callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB31 = [new(-60, 18, -16, 32, 0xb93d, 0xbab4)];
    /// <summary>Wider mid-body rectangle with the standard damage callbacks.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB3F = [new(-80, 13, -16, 27, 0xb93d, 0xbab4)];
    /// <summary>Raised mouth rectangle whose shot callback differs from the standard body set.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB4F = [new(-95, -6, -16, 27, 0xb93d, 0xb951)];
    /// <summary>Upper mouth rectangle using the alternate shot callback.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB5F = [new(-59, -9, -16, 5, 0xb93d, 0xb951)];
    /// <summary>High mouth rectangle used by a raised body pose with alternate shot handling.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB6D = [new(-93, -36, -30, -13, 0xb93d, 0xb951)];
    /// <summary>Three-piece upper body collision list with separate shot handling for its first piece.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCBC5 = [new(-50, -37, 40, -16, 0x8023, 0xba05), new(-52, -58, 42, -38, 0x8023, 0x802d), new(-32, -13, 40, 0, 0x8023, 0x802d)];
    /// <summary>Three-piece raised upper-body list with a taller first rectangle.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCBEB = [new(-46, -53, 13, -16, 0x8023, 0xba05), new(-41, -74, 16, -57, 0x8023, 0x802d), new(-37, -16, 16, -3, 0x8023, 0x802d)];
    /// <summary>Broad high body rectangle whose shot callback is the common no-op.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCC11 = [new(-37, -50, 38, -4, 0xb93d, 0x802d)];
    /// <summary>Full-height central body rectangle with the main fight shot callback.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCC1F = [new(-38, -48, 37, 52, 0xb93d, 0xb968)];
    /// <summary>Lower central body rectangle using the alternate shot callback.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCC2D = [new(-38, -4, 42, 52, 0xb93d, 0xb951)];

    /// <summary>Accepts a living-body frame or delegates defeated-body frames to the corpse catalog.</summary>
    internal static bool HasFrame(ushort frame) => Frames.TryGet(frame, out _) ||
        CrocomireCorpseCollisionDefinitions.HasFrame(frame);

    /// <summary>Finds the physical component offsets and hitbox pointers for a living or defeated frame.</summary>
    internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
        Frames.TryGet(frame, out CrocomireBodyCollisionComponent[] components)
            ? components
            : CrocomireCorpseCollisionDefinitions.ComponentsAt(frame);

    /// <summary>Body hitbox lists by native identity; rectangles are fitted to the drawings.</summary>
    internal static ReadOnlySpan<CrocomireBodyCollisionHitbox> HitboxesAt(ushort list) =>
        list switch
        {
            0xcb07 => ListCB07,
            0xcb15 => ListCB15,
            0xcb23 => ListCB23,
            0xcb31 => ListCB31,
            0xcb3f => ListCB3F,
            0xcb4d => [],
            0xcb4f => ListCB4F,
            0xcb5f => ListCB5F,
            0xcb6d => ListCB6D,
            0xcbc3 => [],
            0xcbc5 => ListCBC5,
            0xcbeb => ListCBEB,
            0xcc11 => ListCC11,
            0xcc1f => ListCC1F,
            0xcc2d => ListCC2D,
            _ => CrocomireCorpseCollisionDefinitions.HitboxesAt(list),
        };
}
