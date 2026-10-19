namespace SuperMetroid.Core.Input;

/// <summary>The oracles' map between bank-$91 demo-input object headers and the title demonstrations' named objects.</summary>
internal static class DemoInputObjectDefinitionsAccess
{
    /// <summary>Each stock title object, by its bank-$91 header.</summary>
    private static readonly Dictionary<ushort, DemoInputObjectDefinition> ByHeader = new()
    {
        [0x9e52] = DemoInputObjectDefinitions.AttractLandingSite,
        [0x9e58] = DemoInputObjectDefinitions.AttractPseudoScrewAttack,
        [0x9e5e] = DemoInputObjectDefinitions.AttractSpeedBooster,
        [0x9e64] = DemoInputObjectDefinitions.AttractDachora,
        [0x9e6a] = DemoInputObjectDefinitions.AttractRedBrinstarElevator,
        [0x9e70] = DemoInputObjectDefinitions.AttractPrePhantoonHall,
        [0x9e76] = DemoInputObjectDefinitions.AttractLowerNorfairEntrance,
        [0x9e7c] = DemoInputObjectDefinitions.AttractEyeDoor,
        [0x9e82] = DemoInputObjectDefinitions.AttractDiagonalShinespark,
        [0x9e88] = DemoInputObjectDefinitions.AttractMissileDoor,
        [0x9e8e] = DemoInputObjectDefinitions.AttractKraid,
        [0x9e94] = DemoInputObjectDefinitions.AttractFirefleaRoom,
        [0x9e9a] = DemoInputObjectDefinitions.AttractScrewAttack,
        [0x9ea0] = DemoInputObjectDefinitions.AttractBrinstarDiagonalRoom,
        [0x9eac] = DemoInputObjectDefinitions.AttractPreSporeSpawnHall,
        [0x9eb2] = DemoInputObjectDefinitions.AttractGrappleBeam,
        [0x9eb8] = DemoInputObjectDefinitions.AttractIceBeam,
        [0x9ebe] = DemoInputObjectDefinitions.AttractGauntletEntrance,
        [0x9ec4] = DemoInputObjectDefinitions.AttractAdvancedGrappleBeam,
        [0x9eca] = DemoInputObjectDefinitions.AttractInfiniteBombJump,
        [0x9ed0] = DemoInputObjectDefinitions.AttractSpecialBeamAttack,
        [0x9ed6] = DemoInputObjectDefinitions.AttractTourianEntrance,
        [0x9edc] = DemoInputObjectDefinitions.AttractCrystalFlash,
    };

    /// <summary>Resolves native object headers.</summary>
    extension(DemoInputObjectDefinitions)
    {
        /// <summary>The stock title object whose bank-$91 header is <paramref name="header"/>.</summary>
        internal static DemoInputObjectDefinition FromNativeHeader(ushort header) =>
            ByHeader.TryGetValue(header, out DemoInputObjectDefinition found)
                ? found
                : throw new ArgumentOutOfRangeException(nameof(header), header, "Header is not a stock title demo-input object.");

        /// <summary>The bank-$91 header of the stock title object <paramref name="definition"/>.</summary>
        internal static ushort NativeHeaderOf(DemoInputObjectDefinition definition) =>
            ByHeader.Single(pair => pair.Value == definition).Key;
    }
}
