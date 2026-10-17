using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>
/// #1300: where each typed demo-input program and object stands in bank $91, for the program oracle and for comparing a
/// typed object with the native interpreter.
/// </summary>
internal static class DemoInputNativeListAddresses
{
    /// <summary>Each program's native list entry.</summary>
    internal static (DemoInputProgram Program, ushort Entry)[] Entries => [
        (DemoInputProgram.AttractAdvancedGrappleBeam, 0x99c8),
        (DemoInputProgram.AttractBrinstarDiagonalRoom, 0x9560),
        (DemoInputProgram.AttractCrystalFlash, 0x9dae),
        (DemoInputProgram.AttractDachora, 0x8e64),
        (DemoInputProgram.AttractDelete, 0x8776),
        (DemoInputProgram.AttractDiagonalShinespark, 0x933c),
        (DemoInputProgram.AttractDiagonalShinesparkContinuation, 0x9346),
        (DemoInputProgram.AttractEyeDoor, 0x9154),
        (DemoInputProgram.AttractFirefleaRoom, 0x946c),
        (DemoInputProgram.AttractGauntletEntrance, 0x99ae),
        (DemoInputProgram.AttractGrappleBeam, 0x973a),
        (DemoInputProgram.AttractIceBeam, 0x989e),
        (DemoInputProgram.AttractInfiniteBombJump, 0x9af0),
        (DemoInputProgram.AttractKraid, 0x9464),
        (DemoInputProgram.AttractLandingSite, 0x8ace),
        (DemoInputProgram.AttractLowerNorfairEntrance, 0x8fe4),
        (DemoInputProgram.AttractMissileDoor, 0x93cc),
        (DemoInputProgram.AttractPrePhantoonHall, 0x8f3a),
        (DemoInputProgram.AttractPreSporeSpawnHall, 0x965a),
        (DemoInputProgram.AttractPseudoScrewAttack, 0x8c3e),
        (DemoInputProgram.AttractRedBrinstarElevator, 0x8eb4),
        (DemoInputProgram.AttractScrewAttack, 0x950a),
        (DemoInputProgram.AttractSpecialBeamAttack, 0x9cd8),
        (DemoInputProgram.AttractSpeedBooster, 0x8df0),
        (DemoInputProgram.AttractTourianEntrance, 0x9da6),
        (DemoInputProgram.IntroBabyDiscoveryEnd, 0x864b),
        (DemoInputProgram.IntroBabyDiscoveryRunningLeft, 0x860d),
        (DemoInputProgram.IntroBabyDiscoveryRunningLeftLoop, 0x8619),
        (DemoInputProgram.IntroBabyDiscoveryStopAndLook, 0x8623),
        (DemoInputProgram.IntroBabyDiscoveryStopAndLookLoop, 0x8641),
        (DemoInputProgram.IntroMotherBrainFlashback, 0x8694),
    ];

    /// <summary>Each object's native header and its typed definition.</summary>
    internal static (ushort Header, DemoInputObjectDefinition Definition)[] Objects => [
        (0x9e52, DemoInputObjectDefinitions.StockAttract(0x9e52)),
        (0x9e58, DemoInputObjectDefinitions.StockAttract(0x9e58)),
        (0x9e5e, DemoInputObjectDefinitions.StockAttract(0x9e5e)),
        (0x9e64, DemoInputObjectDefinitions.StockAttract(0x9e64)),
        (0x9e6a, DemoInputObjectDefinitions.StockAttract(0x9e6a)),
        (0x9e70, DemoInputObjectDefinitions.StockAttract(0x9e70)),
        (0x9e76, DemoInputObjectDefinitions.StockAttract(0x9e76)),
        (0x9e7c, DemoInputObjectDefinitions.StockAttract(0x9e7c)),
        (0x9e82, DemoInputObjectDefinitions.StockAttract(0x9e82)),
        (0x9e88, DemoInputObjectDefinitions.StockAttract(0x9e88)),
        (0x9e8e, DemoInputObjectDefinitions.StockAttract(0x9e8e)),
        (0x9e94, DemoInputObjectDefinitions.StockAttract(0x9e94)),
        (0x9e9a, DemoInputObjectDefinitions.StockAttract(0x9e9a)),
        (0x9ea0, DemoInputObjectDefinitions.StockAttract(0x9ea0)),
        (0x9eac, DemoInputObjectDefinitions.StockAttract(0x9eac)),
        (0x9eb2, DemoInputObjectDefinitions.StockAttract(0x9eb2)),
        (0x9eb8, DemoInputObjectDefinitions.StockAttract(0x9eb8)),
        (0x9ebe, DemoInputObjectDefinitions.StockAttract(0x9ebe)),
        (0x9ec4, DemoInputObjectDefinitions.StockAttract(0x9ec4)),
        (0x9eca, DemoInputObjectDefinitions.StockAttract(0x9eca)),
        (0x9ed0, DemoInputObjectDefinitions.StockAttract(0x9ed0)),
        (0x9ed6, DemoInputObjectDefinitions.StockAttract(0x9ed6)),
        (0x9edc, DemoInputObjectDefinitions.StockAttract(0x9edc)),
        (DemoInputListWords.IntroMotherBrainFlashbackObject, IntroDemoInputObjectDefinitions.IntroMotherBrainFlashback),
        (DemoInputListWords.IntroBabyDiscoveryObject, IntroDemoInputObjectDefinitions.IntroBabyDiscovery),
    ];

    /// <summary>The native list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(DemoInputProgram program) => Entries.Single(entry => entry.Program == program).Entry;

    /// <summary>
    /// The native instruction pointer a typed cursor stands for: its program's entry plus the bytes of the steps before
    /// it (six per input record, two per routine), or the next list's entry for a cursor resting on a fall-through.
    /// </summary>
    internal static ushort Address(AnimationProgramCursor<DemoInputProgram> cursor)
    {
        var steps = DemoInputAnimations.Programs.Steps(cursor.Program);
        if (steps[cursor.Position].Kind == AnimationProgramStepKind.FallInto)
            return Entry(steps[cursor.Position].Target);
        int address = Entry(cursor.Program);
        for (int position = 0; position < cursor.Position; position++)
            address += steps[position].Kind == AnimationProgramStepKind.Show ? DemoInputListWords.InputRecordBytes : sizeof(ushort);
        return (ushort)address;
    }

    /// <summary>
    /// The native pre-instruction word a typed object's <paramref name="preInstruction"/> stands for; a cleared object's
    /// is zero.
    /// </summary>
    internal static ushort PreInstructionAddress(DemoInputPreInstruction preInstruction) => preInstruction switch
    {
        DemoInputPreInstruction.None => 0,
        DemoInputPreInstruction.ReturnOnly => (ushort)NativeDemoPreInstruction.ReturnOnly,
        DemoInputPreInstruction.AttractCheckLeave => (ushort)NativeDemoPreInstruction.AttractCheckLeave,
        DemoInputPreInstruction.AttractShinespark => (ushort)NativeDemoPreInstruction.AttractShinespark,
        DemoInputPreInstruction.IntroBabyRunningLeft => (ushort)NativeDemoPreInstruction.IntroBabyRunningLeft,
        DemoInputPreInstruction.IntroBabyStopAndLook => (ushort)NativeDemoPreInstruction.IntroBabyStopAndLook,
        _ => throw new ArgumentOutOfRangeException(nameof(preInstruction), preInstruction, "Undefined demo-input pre-instruction."),
    };

    /// <summary>The native instruction pointer of an object's cursor; a deleted object's is zero.</summary>
    internal static ushort Address(AnimationProgramCursor<DemoInputProgram>? cursor) => cursor is { } live ? Address(live) : (ushort)0;
}
