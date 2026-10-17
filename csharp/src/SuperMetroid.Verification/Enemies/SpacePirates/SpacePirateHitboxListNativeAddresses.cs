namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// The verification oracle's knowledge of where the ordinary Pirates' hitbox lists lie in bank $B2. The catalog holds
/// the 147 lists in native record order without addresses; each contiguous native run here names its first record's
/// address and how many of the catalog's lists, in order, it holds, so every list's address derives from its run start
/// and the preceding record sizes.
/// </summary>
internal static class SpacePirateHitboxListNativeAddresses
{
    /// <summary>The 22 contiguous hitbox-list runs, in catalog order.</summary>
    internal static readonly (ushort Start, int Count)[] Runs =
    [
        (0x8059, 1),
        (0x9690, 38),
        (0x9922, 12),
        (0x9a3a, 33),
        (0x9c5c, 1),
        (0x9c78, 2),
        (0x9cee, 20),
        (0x9e5a, 3),
        (0x9ede, 8),
        (0x9f78, 8),
        (0xa074, 1),
        (0xa1ee, 1),
        (0xa27a, 3),
        (0xa314, 1),
        (0xa392, 2),
        (0xa3f4, 1),
        (0xa4b8, 1),
        (0xa560, 2),
        (0xa5ea, 3),
        (0xa69c, 1),
        (0xa744, 2),
        (0xa7c0, 3),
    ];
}
