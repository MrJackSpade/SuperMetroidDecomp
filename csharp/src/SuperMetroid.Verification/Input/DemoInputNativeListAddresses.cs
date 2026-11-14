using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Verification.Input;

/// <summary>
/// #1300, #1600: where each typed demo recording and object stands in bank $91, for the recording oracle and for comparing a
/// typed playback with the native interpreter.
/// </summary>
internal static class DemoInputNativeListAddresses
{
    /// <summary>Each recording's native list entry.</summary>
    internal static DemoInputNativeListAddressesEntry[] Entries => [
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractAdvancedGrappleBeam, 0x99c8),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractBrinstarDiagonalRoom, 0x9560),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractCrystalFlash, 0x9dae),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractDachora, 0x8e64),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractDelete, 0x8776),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractDiagonalShinespark, 0x933c),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractDiagonalShinesparkContinuation, 0x9346),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractEyeDoor, 0x9154),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractFirefleaRoom, 0x946c),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractGauntletEntrance, 0x99ae),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractGrappleBeam, 0x973a),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractIceBeam, 0x989e),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractInfiniteBombJump, 0x9af0),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractKraid, 0x9464),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractLandingSite, 0x8ace),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractLowerNorfairEntrance, 0x8fe4),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractMissileDoor, 0x93cc),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractPrePhantoonHall, 0x8f3a),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractPreSporeSpawnHall, 0x965a),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractPseudoScrewAttack, 0x8c3e),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractRedBrinstarElevator, 0x8eb4),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractScrewAttack, 0x950a),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractSpecialBeamAttack, 0x9cd8),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractSpeedBooster, 0x8df0),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.AttractTourianEntrance, 0x9da6),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.IntroBabyDiscoveryEnd, 0x864b),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.IntroBabyDiscoveryRunningLeft, 0x860d),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.IntroBabyDiscoveryStopAndLook, 0x8623),
        new DemoInputNativeListAddressesEntry(DemoInputRecordingId.IntroMotherBrainFlashback, 0x8694),
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
        new Object(DemoInputListWords.IntroMotherBrainFlashbackObject, ReturnOnlyAddress, DemoInputRecordingId.IntroMotherBrainFlashback),
        new Object(DemoInputListWords.IntroBabyDiscoveryObject, PhaseAddress(IntroBabyDiscoveryDemoPhase.RunningLeft),
            DemoInputRecordingId.IntroBabyDiscoveryRunningLeft),
    ];

    /// <summary>A title demonstration's native object: its first watch's pre-instruction word and its program.</summary>
    private static Object Title(ushort header, AttractDemoInputDefinition input) =>
        new(header, WatchAddress(input.FirstWatch), input.Animation);

    /// <summary>
    /// The recordings whose loop the cartridge authors as a list of its own, which the recording's last list runs on into
    /// and which goes to itself: the input the loop starts at and that list's entry.
    /// </summary>
    internal static DemoInputNativeLoopEntry[] LoopEntries => [
        new(DemoInputRecordingId.IntroBabyDiscoveryRunningLeft, 2, 0x8619),
        new(DemoInputRecordingId.IntroBabyDiscoveryStopAndLook, 5, 0x8641),
    ];

    /// <summary>The native list entry of <paramref name="recording"/>.</summary>
    internal static ushort Entry(DemoInputRecordingId recording) => Entries.Single(entry => entry.Program == recording).Entry;

    /// <summary>
    /// The native instruction pointer a playback position stands for: its recording's entry plus six bytes for each input
    /// played, counted from the loop's own list once the position is at or past an authored loop's first input.
    /// </summary>
    internal static ushort Address(DemoPlaybackPosition position)
    {
        DemoInputNativeLoopEntry? loop = LoopEntries.Where(entry => entry.Recording == position.Recording).Cast<DemoInputNativeLoopEntry?>().SingleOrDefault();
        return loop is { } authored && position.NextInput >= authored.FirstRecord
            ? (ushort)(authored.Entry + (position.NextInput - authored.FirstRecord) * DemoInputListWords.InputRecordBytes)
            : (ushort)(Entry(position.Recording) + position.NextInput * DemoInputListWords.InputRecordBytes);
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

    /// <summary>The native instruction pointer of a playback's position; an ended playback's is zero.</summary>
    internal static ushort Address(DemoPlaybackPosition? position) => position is { } live ? Address(live) : (ushort)0;
}
