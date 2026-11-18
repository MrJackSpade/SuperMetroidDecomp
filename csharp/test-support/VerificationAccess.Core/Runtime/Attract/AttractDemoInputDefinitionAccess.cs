using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The oracles' map between bank-$91 demo-input object headers and the title demonstrations' named inputs.</summary>
internal static class AttractDemoInputDefinitionAccess
{
    /// <summary>Each stock title object, by its bank-$91 header.</summary>
    private static readonly Dictionary<ushort, AttractDemoInputDefinition> ByHeader = new()
    {
        [0x9e52] = new(DemoInputRecordingId.AttractLandingSite),
        [0x9e58] = new(DemoInputRecordingId.AttractPseudoScrewAttack),
        [0x9e5e] = new(DemoInputRecordingId.AttractSpeedBooster),
        [0x9e64] = new(DemoInputRecordingId.AttractDachora),
        [0x9e6a] = new(DemoInputRecordingId.AttractRedBrinstarElevator),
        [0x9e70] = new(DemoInputRecordingId.AttractPrePhantoonHall),
        [0x9e76] = new(DemoInputRecordingId.AttractLowerNorfairEntrance),
        [0x9e7c] = new(DemoInputRecordingId.AttractEyeDoor),
        [0x9e82] = new(DemoInputRecordingId.AttractDiagonalShinespark, Continuation: DemoInputRecordingId.AttractDiagonalShinesparkContinuation),
        [0x9e88] = new(DemoInputRecordingId.AttractMissileDoor),
        [0x9e8e] = new(DemoInputRecordingId.AttractKraid),
        [0x9e94] = new(DemoInputRecordingId.AttractFirefleaRoom),
        [0x9e9a] = new(DemoInputRecordingId.AttractScrewAttack),
        [0x9ea0] = new(DemoInputRecordingId.AttractBrinstarDiagonalRoom),
        [0x9eac] = new(DemoInputRecordingId.AttractPreSporeSpawnHall),
        [0x9eb2] = new(DemoInputRecordingId.AttractGrappleBeam),
        [0x9eb8] = new(DemoInputRecordingId.AttractIceBeam),
        [0x9ebe] = new(DemoInputRecordingId.AttractGauntletEntrance),
        [0x9ec4] = new(DemoInputRecordingId.AttractAdvancedGrappleBeam),
        [0x9eca] = new(DemoInputRecordingId.AttractInfiniteBombJump),
        [0x9ed0] = new(DemoInputRecordingId.AttractSpecialBeamAttack),
        [0x9ed6] = new(DemoInputRecordingId.AttractTourianEntrance),
        [0x9edc] = new(DemoInputRecordingId.AttractCrystalFlash),
    };

    /// <summary>Resolves native object headers.</summary>
    extension(AttractDemoInputDefinition)
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
