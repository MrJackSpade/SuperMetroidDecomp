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
    internal static readonly SpacePirateHitboxListNativeAddressesRun[] Runs =
    [
        new SpacePirateHitboxListNativeAddressesRun(0x8059, 1),
        new SpacePirateHitboxListNativeAddressesRun(0x9690, 38),
        new SpacePirateHitboxListNativeAddressesRun(0x9922, 12),
        new SpacePirateHitboxListNativeAddressesRun(0x9a3a, 33),
        new SpacePirateHitboxListNativeAddressesRun(0x9c5c, 1),
        new SpacePirateHitboxListNativeAddressesRun(0x9c78, 2),
        new SpacePirateHitboxListNativeAddressesRun(0x9cee, 20),
        new SpacePirateHitboxListNativeAddressesRun(0x9e5a, 3),
        new SpacePirateHitboxListNativeAddressesRun(0x9ede, 8),
        new SpacePirateHitboxListNativeAddressesRun(0x9f78, 8),
        new SpacePirateHitboxListNativeAddressesRun(0xa074, 1),
        new SpacePirateHitboxListNativeAddressesRun(0xa1ee, 1),
        new SpacePirateHitboxListNativeAddressesRun(0xa27a, 3),
        new SpacePirateHitboxListNativeAddressesRun(0xa314, 1),
        new SpacePirateHitboxListNativeAddressesRun(0xa392, 2),
        new SpacePirateHitboxListNativeAddressesRun(0xa3f4, 1),
        new SpacePirateHitboxListNativeAddressesRun(0xa4b8, 1),
        new SpacePirateHitboxListNativeAddressesRun(0xa560, 2),
        new SpacePirateHitboxListNativeAddressesRun(0xa5ea, 3),
        new SpacePirateHitboxListNativeAddressesRun(0xa69c, 1),
        new SpacePirateHitboxListNativeAddressesRun(0xa744, 2),
        new SpacePirateHitboxListNativeAddressesRun(0xa7c0, 3),
    ];
}
