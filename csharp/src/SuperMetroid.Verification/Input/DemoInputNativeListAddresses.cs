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

    /// <summary>
    /// The list a title demonstration's leave check redirects to: the delete alone, which a typed playback's
    /// <c>DemoInputState.End</c> stands for (#2007).
    /// </summary>
    internal const ushort TitleLeavingDeleteList = 0x8776;

    /// <summary>Each object's native header and its typed definition.</summary>
    internal static Object[] Objects => [
        Title(0x9e52, new(DemoInputRecordingId.AttractLandingSite)),
        Title(0x9e58, new(DemoInputRecordingId.AttractPseudoScrewAttack)),
        Title(0x9e5e, new(DemoInputRecordingId.AttractSpeedBooster)),
        Title(0x9e64, new(DemoInputRecordingId.AttractDachora)),
        Title(0x9e6a, new(DemoInputRecordingId.AttractRedBrinstarElevator)),
        Title(0x9e70, new(DemoInputRecordingId.AttractPrePhantoonHall)),
        Title(0x9e76, new(DemoInputRecordingId.AttractLowerNorfairEntrance)),
        Title(0x9e7c, new(DemoInputRecordingId.AttractEyeDoor)),
        Title(0x9e82, new(DemoInputRecordingId.AttractDiagonalShinespark, Continuation: DemoInputRecordingId.AttractDiagonalShinesparkContinuation)),
        Title(0x9e88, new(DemoInputRecordingId.AttractMissileDoor)),
        Title(0x9e8e, new(DemoInputRecordingId.AttractKraid)),
        Title(0x9e94, new(DemoInputRecordingId.AttractFirefleaRoom)),
        Title(0x9e9a, new(DemoInputRecordingId.AttractScrewAttack)),
        Title(0x9ea0, new(DemoInputRecordingId.AttractBrinstarDiagonalRoom)),
        Title(0x9eac, new(DemoInputRecordingId.AttractPreSporeSpawnHall)),
        Title(0x9eb2, new(DemoInputRecordingId.AttractGrappleBeam)),
        Title(0x9eb8, new(DemoInputRecordingId.AttractIceBeam)),
        Title(0x9ebe, new(DemoInputRecordingId.AttractGauntletEntrance)),
        Title(0x9ec4, new(DemoInputRecordingId.AttractAdvancedGrappleBeam)),
        Title(0x9eca, new(DemoInputRecordingId.AttractInfiniteBombJump)),
        Title(0x9ed0, new(DemoInputRecordingId.AttractSpecialBeamAttack)),
        Title(0x9ed6, new(DemoInputRecordingId.AttractTourianEntrance)),
        Title(0x9edc, new(DemoInputRecordingId.AttractCrystalFlash)),
        new Object(DemoInputListWords.IntroMotherBrainFlashbackObject, ReturnOnlyAddress, DemoInputRecordingId.IntroMotherBrainFlashback),
        new Object(DemoInputListWords.IntroBabyDiscoveryObject, PhaseAddress(IntroBabyDiscoveryDemoPhase.RunningLeft),
            DemoInputRecordingId.IntroBabyDiscoveryRunningLeft),
    ];

    /// <summary>A title demonstration's native object: its first watch's pre-instruction word and its program.</summary>
    private static Object Title(ushort header, AttractDemoInputDefinition input) =>
        new(header, input.Continuation is null ? TitleLeavingCheckAddress : ShinesparkReleaseAddress, input.Recording);

    /// <summary>
    /// The recordings whose loop the cartridge authors as a list of its own, which the recording's last list runs on into
    /// and which goes to itself: the input the loop starts at and that list's entry.
    /// </summary>
    internal static DemoInputNativeLoopEntry[] LoopEntries => [
        new(DemoInputRecordingId.IntroBabyDiscoveryRunningLeft, 2, 0x8619),
        new(DemoInputRecordingId.IntroBabyDiscoveryStopAndLook, 5, 0x8641),
    ];

    /// <summary>
    /// The typed inputs that each play a run of identical consecutive native records (#2557), and how many records each
    /// stands for; every other input stands for one. The recording oracle checks each run against bank $91.
    /// </summary>
    internal static DemoInputNativeRun[] Runs => [
        new(DemoInputRecordingId.AttractLowerNorfairEntrance, 55, 2),
        new(DemoInputRecordingId.AttractPreSporeSpawnHall, 8, 2),
        new(DemoInputRecordingId.AttractPreSporeSpawnHall, 28, 2),
        new(DemoInputRecordingId.IntroMotherBrainFlashback, 5, 2),
    ];

    /// <summary>How many native records the typed input <paramref name="input"/> of <paramref name="recording"/> stands for.</summary>
    internal static int RecordsOf(DemoInputRecordingId recording, int input) =>
        Runs.Where(run => run.Recording == recording && run.Input == input).Select(run => run.Records).DefaultIfEmpty(1).Single();

    /// <summary>How many native records the typed inputs of <paramref name="recording"/> before <paramref name="input"/> stand for.</summary>
    private static int RecordsBefore(DemoInputRecordingId recording, int input) =>
        Enumerable.Range(0, input).Sum(earlier => RecordsOf(recording, earlier));

    /// <summary>The native list entry of <paramref name="recording"/>.</summary>
    internal static ushort Entry(DemoInputRecordingId recording) => Entries.Single(entry => entry.Program == recording).Entry;

    /// <summary>
    /// The native instruction pointer a playback position stands for: its recording's entry plus six bytes for each native
    /// record the inputs played stand for (<see cref="RecordsOf"/>), counted from the loop's own list once the position is
    /// at or past an authored loop's first input.
    /// </summary>
    internal static ushort Address(DemoPlaybackPosition position)
    {
        DemoInputNativeLoopEntry? loop = LoopEntries.Where(entry => entry.Recording == position.Recording).Cast<DemoInputNativeLoopEntry?>().SingleOrDefault();
        int records = RecordsBefore(position.Recording, position.NextInput);
        return loop is { } authored && position.NextInput >= authored.FirstRecord
            ? (ushort)(authored.Entry + (records - RecordsBefore(position.Recording, authored.FirstRecord)) * DemoInputListWords.InputRecordBytes)
            : (ushort)(Entry(position.Recording) + records * DemoInputListWords.InputRecordBytes);
    }

    /// <summary>The native pre-instruction word of an object whose owner watches for nothing: the return-only routine.</summary>
    internal const ushort ReturnOnlyAddress = (ushort)NativeDemoFrameBehavior.ReturnOnly;

    /// <summary>The native pre-instruction word of a title demonstration that checks only for the title leaving.</summary>
    internal const ushort TitleLeavingCheckAddress = (ushort)NativeDemoFrameBehavior.AttractCheckLeave;

    /// <summary>The native pre-instruction word of the diagonal shinespark's demonstration until its release continues it.</summary>
    internal const ushort ShinesparkReleaseAddress = (ushort)NativeDemoFrameBehavior.AttractShinespark;

    /// <summary>
    /// The native pre-instruction word a title demonstration's playback stands for (#2385): the shinespark's release while a
    /// scene whose input names a continuation still plays its first recording, otherwise the leave check.
    /// </summary>
    internal static ushort WatchAddress(AttractDemoInputDefinition input, DemoInputState playback) =>
        input.Continuation is not null && playback.Position is { } position && position.Recording == input.Recording
            ? ShinesparkReleaseAddress
            : TitleLeavingCheckAddress;

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
