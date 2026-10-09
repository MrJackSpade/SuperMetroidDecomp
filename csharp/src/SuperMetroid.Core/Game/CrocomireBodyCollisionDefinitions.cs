namespace SuperMetroid.Core.Game;

/// <summary>One native physical component of a Crocomire body frame.</summary>
/// <param name="X">Horizontal component offset from the actor origin in pixels.</param>
/// <param name="Y">Vertical component offset from the actor origin in pixels.</param>
/// <param name="HitboxPointer">Bank-$A4 address selecting this component's native hitbox list.</param>
internal readonly record struct CrocomireBodyCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>One native physical hitbox and its independently selected callbacks.</summary>
/// <param name="Left">Signed left edge relative to the actor origin.</param>
/// <param name="Top">Signed top edge relative to the actor origin.</param>
/// <param name="Right">Signed right edge relative to the actor origin.</param>
/// <param name="Bottom">Signed bottom edge relative to the actor origin.</param>
/// <param name="TouchAi">Native callback address used when Samus touches the rectangle.</param>
/// <param name="ShotAi">Native callback address used when a shot hits the rectangle.</param>
internal readonly record struct CrocomireBodyCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

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

    /// <summary>Compact address-derived runs containing Crocomire's selected body-frame component placements.</summary>
private static readonly CollisionRecordRuns<CrocomireBodyCollisionComponent> Frames = CollisionRecordRuns<CrocomireBodyCollisionComponent>.Frames(
        new(0xbfc4,
        [
            [new(3, 11, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, -1, 0xcc1f)],
            [new(1, 11, 0xcb3f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
            [new(0, 8, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
            [new(1, 11, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -2, 0xcc1f)],
            [new(1, 10, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
            [new(1, 10, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
            [new(1, 10, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f)],
            [new(1, 10, 0xcb3f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f)],
            [new(1, 10, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
            [new(1, 12, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
            [new(1, 13, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
            [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
        ]),
        new(0xc2ec,
        [
            [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 11, 0xcb23), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 11, 0xcb5f), new(0, 37, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
            [new(0, 8, 0xcb6d), new(0, 39, 0xcb4d), new(-29, 39, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
            [new(1, 8, 0xcb4f), new(0, 41, 0xcb4d), new(-29, 32, 0xcb4d), new(0, -4, 0xcc1f), new(0, -4, 0xcc1f), new(0, -4, 0xcc1f)],
            [new(0, 10, 0xcb15), new(0, 39, 0xcb4d), new(-29, 37, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
            [new(1, 12, 0xcb07), new(0, 37, 0xcb4d), new(-29, 40, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f)],
            [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbc5), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        ]),
        new(0xc6a4,
        [
            [new(3, 11, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, -1, 0xcc1f)],
        ]),
        new(0xc752,
        [
            [new(1, 11, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, -2, 0xcc1f)],
            [new(1, 10, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, -2, 0xcc1f)],
            [new(1, 10, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, -2, 0xcc1f)],
            [new(1, 10, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 10, 0xcb3f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 10, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 12, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 13, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcbc5), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 11, 0xcb5f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 6, 0xcb6d), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 4, 0xcb4f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 8, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(1, 11, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
            [new(0, 0, 0xcb15)],
            [new(0, 0, 0xcb15)],
            [new(0, 0, 0xcb15)],
            [new(0, 0, 0xcb15)],
            [new(0, 0, 0xcb15)],
            [new(0, 0, 0xcb15)],
            [new(0, 0, 0xcb15)],
            [new(0, 0, 0xcb15)],
        ]));

    /// <summary>One-rectangle list at $A4:CB07 for the standing head-area body component.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB07 = [new(-78, 32, -16, 43, 0xb93d, 0xbab4)];
    /// <summary>One-rectangle list at $A4:CB15 for the narrow upper body component.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB15 = [new(-95, 11, -16, 11, 0xb93d, 0xbab4)];
    /// <summary>One-rectangle list at $A4:CB23 for the lower body component.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB23 = [new(-69, 31, -16, 44, 0xb93d, 0xbab4)];
    /// <summary>One-rectangle list at $A4:CB31 for the rear body component.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB31 = [new(-60, 18, -16, 32, 0xb93d, 0xbab4)];
    /// <summary>One-rectangle list at $A4:CB3F for the forward body component.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB3F = [new(-80, 13, -16, 27, 0xb93d, 0xbab4)];
    /// <summary>One-rectangle list at $A4:CB4F with the alternate shot callback used by upper frames.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB4F = [new(-95, -6, -16, 27, 0xb93d, 0xb951)];
    /// <summary>One-rectangle list at $A4:CB5F for the uppermost body placement.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB5F = [new(-59, -9, -16, 5, 0xb93d, 0xb951)];
    /// <summary>One-rectangle list at $A4:CB6D for the high head placement.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCB6D = [new(-93, -36, -30, -13, 0xb93d, 0xb951)];
    /// <summary>Three-rectangle list at $A4:CBC5 covering the jaw and adjoining upper body.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCBC5 = [new(-50, -37, 40, -16, 0x8023, 0xba05), new(-52, -58, 42, -38, 0x8023, 0x802d), new(-32, -13, 40, 0, 0x8023, 0x802d)];
    /// <summary>Three-rectangle list at $A4:CBEB for the extended jaw and body regions.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCBEB = [new(-46, -53, 13, -16, 0x8023, 0xba05), new(-41, -74, 16, -57, 0x8023, 0x802d), new(-37, -16, 16, -3, 0x8023, 0x802d)];
    /// <summary>Single-rectangle list at $A4:CC11 for the alternate body pose.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCC11 = [new(-37, -50, 38, -4, 0xb93d, 0x802d)];
    /// <summary>Single-rectangle list at $A4:CC1F spanning the main body and legs.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCC1F = [new(-38, -48, 37, 52, 0xb93d, 0xb968)];
    /// <summary>Single-rectangle list at $A4:CC2D for the lower body region.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] ListCC2D = [new(-38, -4, 42, 52, 0xb93d, 0xb951)];

    /// <summary>Checks body-frame ownership, including frames delegated to the separate corpse collision catalog.</summary>
    /// <param name="frame">Native bank-$A4 body-frame pointer to check.</param>
    /// <returns>Whether either body or corpse collision data defines the frame.</returns>
    internal static bool HasFrame(ushort frame) => Frames.TryGet(frame, out _) ||
        CrocomireCorpseCollisionDefinitions.HasFrame(frame);

    /// <summary>Returns the physical component placements for a body frame, falling back to corpse data when needed.</summary>
    /// <param name="frame">Native bank-$A4 body-frame pointer.</param>
    /// <returns>Component offsets and hitbox-list identities for the selected frame.</returns>
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
