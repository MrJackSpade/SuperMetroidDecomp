namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The oracles' map between bank-$91 demo-input object headers and the title demonstrations' named inputs.</summary>
internal static class AttractDemoInputDefinitionsAccess
{
    /// <summary>Each stock title object, by its bank-$91 header.</summary>
    private static readonly Dictionary<ushort, AttractDemoInputDefinition> ByHeader = new()
    {
        [0x9e52] = AttractDemoInputDefinitions.AttractLandingSite,
        [0x9e58] = AttractDemoInputDefinitions.AttractPseudoScrewAttack,
        [0x9e5e] = AttractDemoInputDefinitions.AttractSpeedBooster,
        [0x9e64] = AttractDemoInputDefinitions.AttractDachora,
        [0x9e6a] = AttractDemoInputDefinitions.AttractRedBrinstarElevator,
        [0x9e70] = AttractDemoInputDefinitions.AttractPrePhantoonHall,
        [0x9e76] = AttractDemoInputDefinitions.AttractLowerNorfairEntrance,
        [0x9e7c] = AttractDemoInputDefinitions.AttractEyeDoor,
        [0x9e82] = AttractDemoInputDefinitions.AttractDiagonalShinespark,
        [0x9e88] = AttractDemoInputDefinitions.AttractMissileDoor,
        [0x9e8e] = AttractDemoInputDefinitions.AttractKraid,
        [0x9e94] = AttractDemoInputDefinitions.AttractFirefleaRoom,
        [0x9e9a] = AttractDemoInputDefinitions.AttractScrewAttack,
        [0x9ea0] = AttractDemoInputDefinitions.AttractBrinstarDiagonalRoom,
        [0x9eac] = AttractDemoInputDefinitions.AttractPreSporeSpawnHall,
        [0x9eb2] = AttractDemoInputDefinitions.AttractGrappleBeam,
        [0x9eb8] = AttractDemoInputDefinitions.AttractIceBeam,
        [0x9ebe] = AttractDemoInputDefinitions.AttractGauntletEntrance,
        [0x9ec4] = AttractDemoInputDefinitions.AttractAdvancedGrappleBeam,
        [0x9eca] = AttractDemoInputDefinitions.AttractInfiniteBombJump,
        [0x9ed0] = AttractDemoInputDefinitions.AttractSpecialBeamAttack,
        [0x9ed6] = AttractDemoInputDefinitions.AttractTourianEntrance,
        [0x9edc] = AttractDemoInputDefinitions.AttractCrystalFlash,
    };

    /// <summary>Resolves native object headers.</summary>
    extension(AttractDemoInputDefinitions)
    {
        /// <summary>The stock title object whose bank-$91 header is <paramref name="header"/>.</summary>
        internal static AttractDemoInputDefinition FromNativeHeader(ushort header) =>
            ByHeader.TryGetValue(header, out AttractDemoInputDefinition found)
                ? found
                : throw new ArgumentOutOfRangeException(nameof(header), header, "Header is not a stock title demo-input object.");

        /// <summary>The bank-$91 header of the stock title object <paramref name="definition"/>.</summary>
        internal static ushort NativeHeaderOf(AttractDemoInputDefinition definition) =>
            ByHeader.Single(pair => pair.Value == definition).Key;
    }
}
