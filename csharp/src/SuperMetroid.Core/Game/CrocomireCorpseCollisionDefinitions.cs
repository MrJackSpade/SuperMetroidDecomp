namespace SuperMetroid.Core.Game;

/// <summary>Physical component and hitbox records for the 33 native Crocomire corpse frames, $A4:E1FE..E71F.</summary>
internal static class CrocomireCorpseCollisionDefinitions
{
    private static readonly Dictionary<ushort, CrocomireBodyCollisionComponent[]> Frames = new()
    {
        // ExtendedSpritemap_CrocomireCorpse_0, $A4:E1FE.
        [0xe1fe] = [new(-16, 7, 0xe748), new(-4, -47, 0xe72e), new(-25, -8, 0xe748), new(-1, 5, 0xe748), new(-28, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_1, $A4:E228.
        [0xe228] = [new(-16, 4, 0xe748), new(-3, -49, 0xe72e), new(-25, -8, 0xe748), new(-1, 3, 0xe748), new(-30, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_2, $A4:E252.
        [0xe252] = [new(-11, -1, 0xe748), new(-1, -50, 0xe72e), new(-25, -10, 0xe748), new(-1, 2, 0xe748), new(-31, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_3, $A4:E27C.
        [0xe27c] = [new(-16, 3, 0xe748), new(0, -51, 0xe72e), new(-25, -8, 0xe748), new(-1, 1, 0xe748), new(-32, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_4, $A4:E2A6.
        [0xe2a6] = [new(-12, 7, 0xe748), new(2, -48, 0xe72e), new(-25, -5, 0xe748), new(-1, 4, 0xe748), new(-33, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_5, $A4:E2D0.
        [0xe2d0] = [new(-12, 6, 0xe748), new(2, -44, 0xe72e), new(-25, -5, 0xe748), new(-1, 6, 0xe748), new(-34, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_6, $A4:E2FA.
        [0xe2fa] = [new(-11, 4, 0xe748), new(3, -47, 0xe72e), new(-25, -5, 0xe748), new(-1, 4, 0xe748), new(-34, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_7, $A4:E324.
        [0xe324] = [new(-12, 5, 0xe748), new(2, -48, 0xe72e), new(-25, -5, 0xe748), new(-1, 3, 0xe748), new(-35, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_8, $A4:E34E.
        [0xe34e] = [new(-16, 14, 0xe748), new(9, -47, 0xe72e), new(-25, 0, 0xe748), new(1, 5, 0xe748), new(-38, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_9, $A4:E378.
        [0xe378] = [new(-21, 26, 0xe748), new(16, -39, 0xe72e), new(-25, 7, 0xe748), new(5, 11, 0xe748), new(-38, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_A, $A4:E3A2.
        [0xe3a2] = [new(-22, 35, 0xe748), new(21, -35, 0xe72e), new(-26, 19, 0xe748), new(7, 15, 0xe748), new(-38, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_B, $A4:E3CC.
        [0xe3cc] = [new(-22, 47, 0xe748), new(31, -20, 0xe72e), new(-25, 30, 0xe748), new(10, 20, 0xe748), new(-38, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_C, $A4:E3F6.
        [0xe3f6] = [new(-22, 54, 0xe748), new(36, -14, 0xe72e), new(-25, 35, 0xe748), new(10, 25, 0xe748), new(-38, 20, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_D, $A4:E420.
        [0xe420] = [new(46, -4, 0xe72e), new(-25, 43, 0xe748), new(12, 30, 0xe748), new(1, 36, 0xe748), new(-21, 32, 0xe748), new(-40, 34, 0xe748), new(-58, 30, 0xe748), new(-74, 41, 0xe748), new(-93, 41, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_E, $A4:E46A.
        [0xe46a] = [new(55, 7, 0xe72e), new(41, 39, 0xe748), new(-7, 20, 0xe748), new(25, 44, 0xe748), new(25, 28, 0xe748), new(-8, 37, 0xe748), new(8, 25, 0xe748), new(9, 42, 0xe748), new(-9, 49, 0xe748), new(-24, 36, 0xe748), new(-45, 40, 0xe748), new(-61, 28, 0xe748), new(-76, 43, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_F, $A4:E4D4.
        [0xe4d4] = [new(60, 12, 0xe72e), new(41, 44, 0xe748), new(-7, 25, 0xe748), new(28, 48, 0xe748), new(26, 32, 0xe748), new(-3, 37, 0xe748), new(9, 30, 0xe748), new(13, 45, 0xe748), new(-8, 51, 0xe748), new(-23, 42, 0xe748), new(-48, 42, 0xe748), new(-62, 33, 0xe748), new(-82, 47, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_10, $A4:E53E.
        [0xe53e] = [new(62, 17, 0xe72e), new(41, 49, 0xe748), new(-8, 32, 0xe748), new(28, 48, 0xe748), new(24, 37, 0xe748), new(-2, 41, 0xe748), new(9, 35, 0xe748), new(15, 48, 0xe748), new(-10, 54, 0xe748), new(-24, 47, 0xe748), new(-50, 45, 0xe748), new(-64, 38, 0xe748), new(-84, 51, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_11, $A4:E5A8.
        [0xe5a8] = [new(64, 22, 0xe72e), new(41, 49, 0xe748), new(-9, 38, 0xe748), new(28, 44, 0xe748), new(22, 40, 0xe748), new(2, 44, 0xe748), new(12, 39, 0xe748), new(15, 48, 0xe748), new(-10, 54, 0xe748), new(-23, 52, 0xe748), new(-46, 50, 0xe748), new(-66, 44, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_12, $A4:E60A.
        [0xe60a] = [new(64, 20, 0xe72e), new(41, 54, 0xe748), new(-10, 43, 0xe748), new(28, 49, 0xe748), new(22, 45, 0xe748), new(2, 44, 0xe748), new(12, 44, 0xe748), new(15, 53, 0xe748), new(-22, 57, 0xe748), new(-46, 55, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_13, $A4:E65C.
        [0xe65c] = [new(64, 19, 0xe72e), new(-13, 46, 0xe748), new(29, 51, 0xe748), new(20, 47, 0xe748), new(2, 49, 0xe748), new(12, 44, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_14, $A4:E68E.
        [0xe68e] = [new(64, 20, 0xe72e), new(-13, 49, 0xe748), new(14, 48, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_15, $A4:E6A8.
        [0xe6a8] = [new(64, 21, 0xe72e)],
        // ExtendedSpritemap_CrocomireCorpse_16, $A4:E6B2.
        [0xe6b2] = [new(0, 0, 0xe72e)],
        // ExtendedSpritemap_CrocomireCorpse_17, $A4:E6BC.
        [0xe6bc] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_18, $A4:E6C6.
        [0xe6c6] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_19, $A4:E6D0.
        [0xe6d0] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_1A, $A4:E6DA.
        [0xe6da] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_1B, $A4:E6E4.
        [0xe6e4] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_1C, $A4:E6EE.
        [0xe6ee] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_1D, $A4:E6F8.
        [0xe6f8] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_1E, $A4:E702.
        [0xe702] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_1F, $A4:E70C.
        [0xe70c] = [new(0, 0, 0xe748)],
        // ExtendedSpritemap_CrocomireCorpse_20, $A4:E716.
        [0xe716] = [new(32, 32, 0xe748)],
    };

    private static readonly Dictionary<ushort, CrocomireBodyCollisionHitbox[]> Hitboxes = new()
    {
        [0xe72e] = [new(-38, -16, 0, 31, 0xb950, 0xb968), new(0, -29, 26, 28, 0xb950, 0xb968)],
        [0xe748] = [],
    };

    internal static bool HasFrame(ushort frame) => Frames.ContainsKey(frame);
    internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
        Frames.TryGetValue(frame, out var parts) ? parts : throw new InvalidDataException($"Unknown Crocomire corpse frame $A4:{frame:X4}.");
    internal static ReadOnlySpan<CrocomireBodyCollisionHitbox> HitboxesAt(ushort address) =>
        Hitboxes.TryGetValue(address, out var parts) ? parts : throw new InvalidDataException($"Unknown Crocomire corpse hitboxes $A4:{address:X4}.");
}
