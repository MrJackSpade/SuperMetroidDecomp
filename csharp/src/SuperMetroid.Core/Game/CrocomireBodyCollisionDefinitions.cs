namespace SuperMetroid.Core.Game;

/// <summary>One native physical component of a Crocomire body frame.</summary>
internal readonly record struct CrocomireBodyCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>One native physical hitbox and its independently selected callbacks.</summary>
internal readonly record struct CrocomireBodyCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Engine-owned collision data for Crocomire's fifty selected body frames at
/// $A4:BFC4..CAC4 and their fifteen distinct hitbox lists at $A4:CB07..CC2D.
/// OAM and BG2 art are deliberately absent: installed visual overrides cannot
/// alter the cartridge's component offsets, rectangles, or callback identities.
/// Values are pinned to the project's NTSC J/U v1.0 retail cartridge.
/// </summary>
internal static class CrocomireBodyCollisionDefinitions
{
    /// <summary>Native bank containing Crocomire's body frames and hitboxes.</summary>
    internal const byte Bank = 0xa4;

    private static readonly Dictionary<ushort, CrocomireBodyCollisionComponent[]> Frames = new()
    {
        [0xbfc4] = [new(3, 11, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, -1, 0xcc1f)],
        [0xbff6] = [new(1, 11, 0xcb3f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
        [0xc028] = [new(0, 8, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
        [0xc05a] = [new(1, 11, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -2, 0xcc1f)],
        [0xc08c] = [new(1, 10, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
        [0xc0be] = [new(1, 10, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
        [0xc0f0] = [new(1, 10, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f)],
        [0xc122] = [new(1, 10, 0xcb3f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f)],
        [0xc154] = [new(1, 10, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
        [0xc186] = [new(1, 12, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
        [0xc1b8] = [new(1, 13, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
        [0xc1ea] = [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
        [0xc2ec] = [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc326] = [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc360] = [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc39a] = [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc3d4] = [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc40e] = [new(0, 11, 0xcb23), new(-3, -28, 0xcbc3), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc448] = [new(0, 11, 0xcb23), new(0, 41, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc47a] = [new(1, 11, 0xcb5f), new(0, 37, 0xcb4d), new(-29, 41, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f), new(0, 0, 0xcc1f)],
        [0xc4ac] = [new(0, 8, 0xcb6d), new(0, 39, 0xcb4d), new(-29, 39, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
        [0xc4de] = [new(1, 8, 0xcb4f), new(0, 41, 0xcb4d), new(-29, 32, 0xcb4d), new(0, -4, 0xcc1f), new(0, -4, 0xcc1f), new(0, -4, 0xcc1f)],
        [0xc510] = [new(0, 10, 0xcb15), new(0, 39, 0xcb4d), new(-29, 37, 0xcb4d), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f), new(0, -2, 0xcc1f)],
        [0xc542] = [new(1, 12, 0xcb07), new(0, 37, 0xcb4d), new(-29, 40, 0xcb4d), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f), new(0, -1, 0xcc1f)],
        [0xc574] = [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc5ae] = [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbc5), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc5e8] = [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc6a4] = [new(3, 11, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc11), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, -1, 0xcc1f)],
        [0xc752] = [new(1, 11, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, -2, 0xcc1f)],
        [0xc78c] = [new(1, 10, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, -2, 0xcc1f)],
        [0xc7c6] = [new(1, 10, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, -2, 0xcc1f)],
        [0xc800] = [new(1, 10, 0xcb31), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc83a] = [new(1, 10, 0xcb3f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, -1, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc874] = [new(1, 10, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc8ae] = [new(1, 12, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcbeb), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc8e8] = [new(1, 13, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcbc5), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc922] = [new(1, 11, 0xcb23), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcc1f), new(0, 0, 0xcc11), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc95c] = [new(1, 11, 0xcb5f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc996] = [new(0, 6, 0xcb6d), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xc9d0] = [new(1, 4, 0xcb4f), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xca0a] = [new(0, 8, 0xcb15), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xca44] = [new(1, 11, 0xcb07), new(0, 38, 0xcb4d), new(-29, 38, 0xcb4d), new(0, 0, 0xcbeb), new(0, 0, 0xcc1f), new(0, 0, 0xcc2d), new(0, 0, 0xcc1f)],
        [0xca7e] = [new(0, 0, 0xcb15)],
        [0xca88] = [new(0, 0, 0xcb15)],
        [0xca92] = [new(0, 0, 0xcb15)],
        [0xca9c] = [new(0, 0, 0xcb15)],
        [0xcaa6] = [new(0, 0, 0xcb15)],
        [0xcab0] = [new(0, 0, 0xcb15)],
        [0xcaba] = [new(0, 0, 0xcb15)],
        [0xcac4] = [new(0, 0, 0xcb15)],
    };

    private static readonly Dictionary<ushort, CrocomireBodyCollisionHitbox[]> HitboxLists = new()
    {
        [0xcb07] = [new(-78, 32, -16, 43, 0xb93d, 0xbab4)],
        [0xcb15] = [new(-95, 11, -16, 11, 0xb93d, 0xbab4)],
        [0xcb23] = [new(-69, 31, -16, 44, 0xb93d, 0xbab4)],
        [0xcb31] = [new(-60, 18, -16, 32, 0xb93d, 0xbab4)],
        [0xcb3f] = [new(-80, 13, -16, 27, 0xb93d, 0xbab4)],
        [0xcb4d] = [],
        [0xcb4f] = [new(-95, -6, -16, 27, 0xb93d, 0xb951)],
        [0xcb5f] = [new(-59, -9, -16, 5, 0xb93d, 0xb951)],
        [0xcb6d] = [new(-93, -36, -30, -13, 0xb93d, 0xb951)],
        [0xcbc3] = [],
        [0xcbc5] = [new(-50, -37, 40, -16, 0x8023, 0xba05), new(-52, -58, 42, -38, 0x8023, 0x802d), new(-32, -13, 40, 0, 0x8023, 0x802d)],
        [0xcbeb] = [new(-46, -53, 13, -16, 0x8023, 0xba05), new(-41, -74, 16, -57, 0x8023, 0x802d), new(-37, -16, 16, -3, 0x8023, 0x802d)],
        [0xcc11] = [new(-37, -50, 38, -4, 0xb93d, 0x802d)],
        [0xcc1f] = [new(-38, -48, 37, 52, 0xb93d, 0xb968)],
        [0xcc2d] = [new(-38, -4, 42, 52, 0xb93d, 0xb951)],
    };

    internal static bool HasFrame(ushort frame) => Frames.ContainsKey(frame);

    internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
        Frames.TryGetValue(frame, out CrocomireBodyCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException(
                $"Crocomire body frame $A4:{frame:X4} has no compiled collision.");

    internal static ReadOnlySpan<CrocomireBodyCollisionHitbox> HitboxesAt(ushort list) =>
        HitboxLists.TryGetValue(list, out CrocomireBodyCollisionHitbox[]? hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Crocomire body hitbox list $A4:{list:X4} is not compiled.");
}
