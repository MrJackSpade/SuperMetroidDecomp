using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Verification.Input;

/// <summary>
/// #1300: where each typed demo-input program and object stands in bank $91, for the program oracle and for comparing a
/// typed object with the native interpreter.
/// </summary>
internal static class DemoInputNativeListAddresses
{
    /// <summary>Each program's native list entry.</summary>
    internal static DemoInputNativeListAddressesEntry[] Entries => [
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractAdvancedGrappleBeam, 0x99c8),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractBrinstarDiagonalRoom, 0x9560),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractCrystalFlash, 0x9dae),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractDachora, 0x8e64),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractDelete, 0x8776),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractDiagonalShinespark, 0x933c),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractDiagonalShinesparkContinuation, 0x9346),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractEyeDoor, 0x9154),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractFirefleaRoom, 0x946c),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractGauntletEntrance, 0x99ae),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractGrappleBeam, 0x973a),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractIceBeam, 0x989e),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractInfiniteBombJump, 0x9af0),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractKraid, 0x9464),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractLandingSite, 0x8ace),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractLowerNorfairEntrance, 0x8fe4),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractMissileDoor, 0x93cc),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractPrePhantoonHall, 0x8f3a),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractPreSporeSpawnHall, 0x965a),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractPseudoScrewAttack, 0x8c3e),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractRedBrinstarElevator, 0x8eb4),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractScrewAttack, 0x950a),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractSpecialBeamAttack, 0x9cd8),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractSpeedBooster, 0x8df0),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.AttractTourianEntrance, 0x9da6),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.IntroBabyDiscoveryEnd, 0x864b),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.IntroBabyDiscoveryRunningLeft, 0x860d),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.IntroBabyDiscoveryRunningLeftLoop, 0x8619),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.IntroBabyDiscoveryStopAndLook, 0x8623),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.IntroBabyDiscoveryStopAndLookLoop, 0x8641),
        new DemoInputNativeListAddressesEntry(DemoInputProgram.IntroMotherBrainFlashback, 0x8694),
    ];

    /// <summary>Each object's native header and its typed definition.</summary>
    internal static Object[] Objects => [
        Title(0x9e52, AttractDemoInputDefinitions.AttractLandingSite),
        Title(0x9e58, AttractDemoInputDefinitions.AttractPseudoScrewAttack),
        Title(0x9e5e, AttractDemoInputDefinitions.AttractSpeedBooster),
        Title(0x9e64, AttractDemoInputDefinitions.AttractDachora),
        Title(0x9e6a, AttractDemoInputDefinitions.AttractRedBrinstarElevator),
        Title(0x9e70, AttractDemoInputDefinitions.AttractPrePhantoonHall),
        Title(0x9e76, AttractDemoInputDefinitions.AttractLowerNorfairEntrance),
        Title(0x9e7c, AttractDemoInputDefinitions.AttractEyeDoor),
        Title(0x9e82, AttractDemoInputDefinitions.AttractDiagonalShinespark),
        Title(0x9e88, AttractDemoInputDefinitions.AttractMissileDoor),
        Title(0x9e8e, AttractDemoInputDefinitions.AttractKraid),
        Title(0x9e94, AttractDemoInputDefinitions.AttractFirefleaRoom),
        Title(0x9e9a, AttractDemoInputDefinitions.AttractScrewAttack),
        Title(0x9ea0, AttractDemoInputDefinitions.AttractBrinstarDiagonalRoom),
        Title(0x9eac, AttractDemoInputDefinitions.AttractPreSporeSpawnHall),
        Title(0x9eb2, AttractDemoInputDefinitions.AttractGrappleBeam),
        Title(0x9eb8, AttractDemoInputDefinitions.AttractIceBeam),
        Title(0x9ebe, AttractDemoInputDefinitions.AttractGauntletEntrance),
        Title(0x9ec4, AttractDemoInputDefinitions.AttractAdvancedGrappleBeam),
        Title(0x9eca, AttractDemoInputDefinitions.AttractInfiniteBombJump),
        Title(0x9ed0, AttractDemoInputDefinitions.AttractSpecialBeamAttack),
        Title(0x9ed6, AttractDemoInputDefinitions.AttractTourianEntrance),
        Title(0x9edc, AttractDemoInputDefinitions.AttractCrystalFlash),
        new Object(DemoInputListWords.IntroMotherBrainFlashbackObject, ReturnOnlyAddress, DemoInputProgram.IntroMotherBrainFlashback),
        new Object(DemoInputListWords.IntroBabyDiscoveryObject, PhaseAddress(IntroBabyDiscoveryDemoPhase.RunningLeft),
            DemoInputProgram.IntroBabyDiscoveryRunningLeft),
    ];

    /// <summary>A title demonstration's native object: its first watch's pre-instruction word and its program.</summary>
    private static Object Title(ushort header, AttractDemoInputDefinition input) =>
        new(header, WatchAddress(input.FirstWatch), input.Program);

    /// <summary>The native list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(DemoInputProgram program) => Entries.Single(entry => entry.Program == program).Entry;

    /// <summary>
    /// The native instruction pointer a typed cursor stands for: its program's entry plus the bytes of the steps before
    /// it (six per input record, two per routine), or the next list's entry for a cursor resting on a fall-through.
    /// </summary>
    internal static ushort Address(AnimationProgramCursor<DemoInputProgram> cursor)
    {
        var steps = DemoInputAnimations.Programs.Steps(cursor.Program);
        if (steps[cursor.Position] is AnimationProgramStep<DemoInputProgram, DemoInputFrame, DemoInputCue>.FallIntoStep fallInto)
            return Entry(fallInto.Target);
        int address = Entry(cursor.Program);
        for (int position = 0; position < cursor.Position; position++)
            address += steps[position] is AnimationProgramStep<DemoInputProgram, DemoInputFrame, DemoInputCue>.ShowStep ? DemoInputListWords.InputRecordBytes : sizeof(ushort);
        return (ushort)address;
    }

    /// <summary>The native pre-instruction word of an object whose owner watches for nothing: the return-only routine.</summary>
    internal const ushort ReturnOnlyAddress = (ushort)NativeDemoFrameBehavior.ReturnOnly;

    /// <summary>The native pre-instruction word a title demonstration's <paramref name="watch"/> stands for.</summary>
    internal static ushort WatchAddress(AttractDemoInputWatch watch) => watch switch
    {
        AttractDemoInputWatch.TitleLeaving => (ushort)NativeDemoFrameBehavior.AttractCheckLeave,
        AttractDemoInputWatch.ShinesparkRelease => (ushort)NativeDemoFrameBehavior.AttractShinespark,
        _ => throw new ArgumentOutOfRangeException(nameof(watch), watch, "Undefined title demonstration watch."),
    };

    /// <summary>The native pre-instruction word the intro Baby discovery's <paramref name="phase"/> stands for.</summary>
    internal static ushort PhaseAddress(IntroBabyDiscoveryDemoPhase phase) => phase switch
    {
        IntroBabyDiscoveryDemoPhase.RunningLeft => (ushort)NativeDemoFrameBehavior.IntroBabyRunningLeft,
        IntroBabyDiscoveryDemoPhase.StoppedAndLooking => (ushort)NativeDemoFrameBehavior.IntroBabyStopAndLook,
        IntroBabyDiscoveryDemoPhase.Ending => ReturnOnlyAddress,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined Baby discovery demo phase."),
    };

    /// <summary>The native instruction pointer of an object's cursor; a deleted object's is zero.</summary>
    internal static ushort Address(AnimationProgramCursor<DemoInputProgram>? cursor) => cursor is { } live ? Address(live) : (ushort)0;
}
