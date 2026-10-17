namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Physical component and hitbox records for the 33 native Crocomire corpse frames, $A4:E1FE..E71F.</summary>
internal static class CrocomireCorpseCollisionDefinitions
{
    /// <summary>The corpse's two body rectangles.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] CorpseBody = [new(-38, -16, 0, 31, 0xb950, 0xb968), new(0, -29, 26, 28, 0xb950, 0xb968)];
    /// <summary>The corpse's empty list, for components without collision.</summary>
    private static readonly CrocomireBodyCollisionHitbox[] CorpseEmpty = [];

    // The hitbox lists precede the frames that reference them: static fields initialise in declaration order.
    /// <summary>Physical component offsets and hitbox lists for each of the 33 corpse frames.</summary>
    private static readonly Dictionary<ushort, CrocomireBodyCollisionComponent[]> Frames = new()
    {
        // ExtendedSpritemap_CrocomireCorpse_0, $A4:E1FE.
        [0xe1fe] = [new(new(-16, 7), CorpseEmpty), new(new(-4, -47), CorpseBody), new(new(-25, -8), CorpseEmpty), new(new(-1, 5), CorpseEmpty), new(new(-28, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_1, $A4:E228.
        [0xe228] = [new(new(-16, 4), CorpseEmpty), new(new(-3, -49), CorpseBody), new(new(-25, -8), CorpseEmpty), new(new(-1, 3), CorpseEmpty), new(new(-30, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_2, $A4:E252.
        [0xe252] = [new(new(-11, -1), CorpseEmpty), new(new(-1, -50), CorpseBody), new(new(-25, -10), CorpseEmpty), new(new(-1, 2), CorpseEmpty), new(new(-31, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_3, $A4:E27C.
        [0xe27c] = [new(new(-16, 3), CorpseEmpty), new(new(0, -51), CorpseBody), new(new(-25, -8), CorpseEmpty), new(new(-1, 1), CorpseEmpty), new(new(-32, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_4, $A4:E2A6.
        [0xe2a6] = [new(new(-12, 7), CorpseEmpty), new(new(2, -48), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 4), CorpseEmpty), new(new(-33, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_5, $A4:E2D0.
        [0xe2d0] = [new(new(-12, 6), CorpseEmpty), new(new(2, -44), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 6), CorpseEmpty), new(new(-34, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_6, $A4:E2FA.
        [0xe2fa] = [new(new(-11, 4), CorpseEmpty), new(new(3, -47), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 4), CorpseEmpty), new(new(-34, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_7, $A4:E324.
        [0xe324] = [new(new(-12, 5), CorpseEmpty), new(new(2, -48), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 3), CorpseEmpty), new(new(-35, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_8, $A4:E34E.
        [0xe34e] = [new(new(-16, 14), CorpseEmpty), new(new(9, -47), CorpseBody), new(new(-25, 0), CorpseEmpty), new(new(1, 5), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_9, $A4:E378.
        [0xe378] = [new(new(-21, 26), CorpseEmpty), new(new(16, -39), CorpseBody), new(new(-25, 7), CorpseEmpty), new(new(5, 11), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_A, $A4:E3A2.
        [0xe3a2] = [new(new(-22, 35), CorpseEmpty), new(new(21, -35), CorpseBody), new(new(-26, 19), CorpseEmpty), new(new(7, 15), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_B, $A4:E3CC.
        [0xe3cc] = [new(new(-22, 47), CorpseEmpty), new(new(31, -20), CorpseBody), new(new(-25, 30), CorpseEmpty), new(new(10, 20), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_C, $A4:E3F6.
        [0xe3f6] = [new(new(-22, 54), CorpseEmpty), new(new(36, -14), CorpseBody), new(new(-25, 35), CorpseEmpty), new(new(10, 25), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_D, $A4:E420.
        [0xe420] = [new(new(46, -4), CorpseBody), new(new(-25, 43), CorpseEmpty), new(new(12, 30), CorpseEmpty), new(new(1, 36), CorpseEmpty), new(new(-21, 32), CorpseEmpty), new(new(-40, 34), CorpseEmpty), new(new(-58, 30), CorpseEmpty), new(new(-74, 41), CorpseEmpty), new(new(-93, 41), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_E, $A4:E46A.
        [0xe46a] = [new(new(55, 7), CorpseBody), new(new(41, 39), CorpseEmpty), new(new(-7, 20), CorpseEmpty), new(new(25, 44), CorpseEmpty), new(new(25, 28), CorpseEmpty), new(new(-8, 37), CorpseEmpty), new(new(8, 25), CorpseEmpty), new(new(9, 42), CorpseEmpty), new(new(-9, 49), CorpseEmpty), new(new(-24, 36), CorpseEmpty), new(new(-45, 40), CorpseEmpty), new(new(-61, 28), CorpseEmpty), new(new(-76, 43), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_F, $A4:E4D4.
        [0xe4d4] = [new(new(60, 12), CorpseBody), new(new(41, 44), CorpseEmpty), new(new(-7, 25), CorpseEmpty), new(new(28, 48), CorpseEmpty), new(new(26, 32), CorpseEmpty), new(new(-3, 37), CorpseEmpty), new(new(9, 30), CorpseEmpty), new(new(13, 45), CorpseEmpty), new(new(-8, 51), CorpseEmpty), new(new(-23, 42), CorpseEmpty), new(new(-48, 42), CorpseEmpty), new(new(-62, 33), CorpseEmpty), new(new(-82, 47), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_10, $A4:E53E.
        [0xe53e] = [new(new(62, 17), CorpseBody), new(new(41, 49), CorpseEmpty), new(new(-8, 32), CorpseEmpty), new(new(28, 48), CorpseEmpty), new(new(24, 37), CorpseEmpty), new(new(-2, 41), CorpseEmpty), new(new(9, 35), CorpseEmpty), new(new(15, 48), CorpseEmpty), new(new(-10, 54), CorpseEmpty), new(new(-24, 47), CorpseEmpty), new(new(-50, 45), CorpseEmpty), new(new(-64, 38), CorpseEmpty), new(new(-84, 51), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_11, $A4:E5A8.
        [0xe5a8] = [new(new(64, 22), CorpseBody), new(new(41, 49), CorpseEmpty), new(new(-9, 38), CorpseEmpty), new(new(28, 44), CorpseEmpty), new(new(22, 40), CorpseEmpty), new(new(2, 44), CorpseEmpty), new(new(12, 39), CorpseEmpty), new(new(15, 48), CorpseEmpty), new(new(-10, 54), CorpseEmpty), new(new(-23, 52), CorpseEmpty), new(new(-46, 50), CorpseEmpty), new(new(-66, 44), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_12, $A4:E60A.
        [0xe60a] = [new(new(64, 20), CorpseBody), new(new(41, 54), CorpseEmpty), new(new(-10, 43), CorpseEmpty), new(new(28, 49), CorpseEmpty), new(new(22, 45), CorpseEmpty), new(new(2, 44), CorpseEmpty), new(new(12, 44), CorpseEmpty), new(new(15, 53), CorpseEmpty), new(new(-22, 57), CorpseEmpty), new(new(-46, 55), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_13, $A4:E65C.
        [0xe65c] = [new(new(64, 19), CorpseBody), new(new(-13, 46), CorpseEmpty), new(new(29, 51), CorpseEmpty), new(new(20, 47), CorpseEmpty), new(new(2, 49), CorpseEmpty), new(new(12, 44), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_14, $A4:E68E.
        [0xe68e] = [new(new(64, 20), CorpseBody), new(new(-13, 49), CorpseEmpty), new(new(14, 48), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_15, $A4:E6A8.
        [0xe6a8] = [new(new(64, 21), CorpseBody)],
        // ExtendedSpritemap_CrocomireCorpse_16, $A4:E6B2.
        [0xe6b2] = [new(new(0, 0), CorpseBody)],
        // ExtendedSpritemap_CrocomireCorpse_17, $A4:E6BC.
        [0xe6bc] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_18, $A4:E6C6.
        [0xe6c6] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_19, $A4:E6D0.
        [0xe6d0] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_1A, $A4:E6DA.
        [0xe6da] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_1B, $A4:E6E4.
        [0xe6e4] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_1C, $A4:E6EE.
        [0xe6ee] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_1D, $A4:E6F8.
        [0xe6f8] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_1E, $A4:E702.
        [0xe702] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_1F, $A4:E70C.
        [0xe70c] = [new(new(0, 0), CorpseEmpty)],
        // ExtendedSpritemap_CrocomireCorpse_20, $A4:E716.
        [0xe716] = [new(new(32, 32), CorpseEmpty)],
    };

    /// <summary>Checks whether a native extended-spritemap pointer belongs to the corpse sequence.</summary>
    internal static bool HasFrame(ushort frame) => Frames.ContainsKey(frame);
    /// <summary>Returns physical component placement for a compiled corpse frame.</summary>
    internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
        Frames.TryGetValue(frame, out var parts) ? parts : throw new InvalidDataException($"Unknown Crocomire corpse frame $A4:{frame:X4}.");
}
