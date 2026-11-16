using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Maps Mother Brain's native body, brain, neck, and rainbow-sequence WRAM into a loaded encounter.</summary>
internal static class NativeSnapshotMotherBrainImport
{
    /// <summary>The native sprite row Mother Brain's brain and neck draw in.</summary>
    private const int NativeBrainRow = 1;

    /// <summary>The native sprite row her death copies her brain's colours into, which her detached brain draws in.</summary>
    private const int NativeCorpseRow = 7;

    /// <summary>
    /// Imports Mother Brain's shared encounter state (enemy records $EC7F/$EC3F) from the
    /// native body/brain slot words and her extended WRAM ($7E:7800-$787B, $7E:8000-$8069).
    /// Called only when the room loaded her; the physical slot words were imported first.
    /// Native list pointers become typed playbacks (#641, #1600) through the ROM oracle's reading of each list.
    /// </summary>
    internal static void ImportNativeMotherBrain(SuperMetroidRuntime runtime, IImportCartridgeSource cartridge, Func<int, ushort> W)
    {
        MotherBrainEnemyState state = runtime.Enemies.BossEncounters.MotherBrain ?? throw new InvalidDataException(
            "The native snapshot holds Mother Brain, but the room load did not create her.");
        state.Form = DecodeForm(W(MotherBrainMemory.Form));
        // The body's function word names the stage her body is in and its phase (#2208).
        MotherBrainNativeWords.ImportBodyPhase(state, W(MotherBrainMemory.BodyFunction));
        ImportMotherBrainBodyImage(runtime, state, W);
        state.Pose = DecodePose(W(MotherBrainMemory.Pose));
        state.BodyPlacement = BodyPlacementOf(state);
        state.HitboxesEnabled = MotherBrainHitboxPartsCodec.FromNativeWord(W(MotherBrainMemory.HitboxesEnabled));
        state.BrainAction = MotherBrainNativeListAddresses.NativeBrainActions[W(MotherBrainMemory.BrainFunction)];
        state.PhaseTimer.Count = CoordinateAxis.AsSigned(W(MotherBrainMemory.BodyFunctionTimer));
        ushort flash = W(MotherBrainMemory.RoomPaletteAnimationScript);
        state.RoomFlashFrame = flash == 0 ? null
            : MotherBrainNativeListAddresses.RoomFlashRecords.Single(record => unchecked((ushort)record.Record) == flash).Frame;
        state.RoomFlashTimer = W(MotherBrainMemory.RoomPaletteFrameHold);
        // The sub-function word is shared: outside the tube collapse it holds the death-explosion interval timer
        // (imported below), so only a collapse routine pointer decodes to a collapse function.
        if (MotherBrainNativeListAddresses.NativeTubeCollapseSteps.TryGetValue(W(MotherBrainMemory.BrainSubFunction), out MotherBrainTubeCollapseStep collapse))
            state.TubeCollapseStep = collapse;
        state.TubeCollapseTimer.Count = CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunctionTimer));
        PrivateState.Set(state.Bursts, nameof(MotherBrainBurstCycle.Place), CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunctionTimer)));
        state.FakeDeathExplosionTimer.Count = CoordinateAxis.AsSigned(W(MotherBrainMemory.FakeDeathExplosionTimer));
        state.FakeDeathExplosionIndex = W(MotherBrainMemory.FakeDeathExplosionIndex);
        // The native palette words name rows: the neck draws in her brain's row, and her brain in it or, once her death
        // copied her brain's colours there, in the corpse's (#1512).
        int neckRow = SnesObjAttributeWordConstants.FromPaletteBits(W(MotherBrainMemory.NeckPaletteIndex)).PaletteIndex;
        int brainRow = SnesObjAttributeWordConstants.FromPaletteBits(W(MotherBrainMemory.BrainPaletteIndex)).PaletteIndex;
        if (neckRow != NativeBrainRow || brainRow is not (NativeBrainRow or NativeCorpseRow))
            throw new InvalidDataException($"Mother Brain's neck row {neckRow} and brain row {brainRow} are not rows her encounter draws in.");
        if (brainRow == NativeCorpseRow)
            state.Colours.DetachBrain();
        ushort brain = W(MotherBrainMemory.BrainAnimationScript);
        // Bit 15 clear means the brain list is not live.
        state.BrainPlayback = unchecked((short)brain) >= 0 ? null : BrainPlaybackAt(cartridge, brain);
        state.BrainFrameTimer = W(MotherBrainMemory.BrainFrameHold);
        Dictionary<MotherBrainBodySegment, NativeEnemyList<MotherBrainBodySegment>> body = MotherBrainNativeListAddresses.CompareBody(cartridge);
        // Each record's instruction-list word is read from the snapshot; the port keeps the typed playback it stands for.
        ushort AnimationScript(RoomEnemyPlace place) => W(NativeSnapshotMemory.EnemyBase +
            SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(place.Ordinal) + MovieDesyncMemory.EnemyAnimationScriptOffset);
        AnimationPlayback<MotherBrainBodySegment> BodyPlaybackAt(RoomEnemyPlace place) => NativeEnemyAnimationOracle.PlaybackAt(
            body, MotherBrainNativeListAddresses.BodyEntries, MotherBrainAnimations.BodyStartingAt, AnimationScript(place));
        // A body still on the parts' initial list has had no animation yet (#2220).
        state.BodyPlayback = MotherBrainNativeListAddresses.InitialListWords.Contains(AnimationScript(state.Body))
            ? null
            : BodyPlaybackAt(state.Body);
        Dictionary<MotherBrainTubePiece, NativeEnemyList<MotherBrainTubePiece>> tubes = MotherBrainNativeListAddresses.CompareTubes(cartridge);
        foreach (RoomEnemyPlace tube in runtime.Enemies.Places.InUpdateOrder.Where(place => place.DefinitionId == EnemyDefinitionId.MotherBrainTubes))
        {
            tube.RequireSpeciesState<MotherBrainFallingTubeState>().Playback = NativeEnemyAnimationOracle.PlaybackAt(
                tubes, MotherBrainNativeListAddresses.TubeEntries, MotherBrainTubeAnimations.StartingAt, AnimationScript(tube));
        }
        state.BrainMainShakeTimer = W(MotherBrainMemory.BrainMainShakeTimer);
        state.DeleteTurretsAndRinkas = W(MotherBrainMemory.DeleteTurretsAndRinkas) != 0;
        state.NextRainbowFrame = DecodeNextRainbowFrame(W(MotherBrainMemory.RainbowPaletteIndex));
        state.DrawNeck = state.BrainAction == MotherBrainBrainAction.SetupBrainAndNeckToBeDrawn;
        for (int joint = 0; joint < MotherBrainNeckJointDefinitions.Count; joint++)
        {
            int record = MotherBrainMemory.NeckSegments + joint * 6;
            state.NeckJoints[joint] = new(W(record), W(record + 2));
            state.NeckJointDistances[joint] = W(record + 4);
        }
        state.LowerNeckAngle = RotationAngleConstants.FromRaw(W(MotherBrainMemory.LowerNeckAngle));
        state.UpperNeckAngle = RotationAngleConstants.FromRaw(W(MotherBrainMemory.UpperNeckAngle));
        state.NeckTurnPerUpdate = RotationAngleConstants.FromRaw(W(MotherBrainMemory.NeckAngleDelta));
        state.NeckMovementEnabled = W(MotherBrainMemory.NeckMovementEnabled) != 0;
        state.LowerNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.LowerNeckMovementIndex), "Mother Brain lower neck movement index");
        state.UpperNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.UpperNeckMovementIndex), "Mother Brain upper neck movement index");
        state.StandingBodyReadyingElapsed = DecodeLegTilePage(W(MotherBrainMemory.SpriteTileTransferEntry));
        // The cartridge counts every fade but her fake death's greying on one word; each fade keeps its own progress
        // (#2251). The one running continues from the recorded count, and every other restarts before its first step.
        int transitionSteps = W(MotherBrainMemory.GreyTransitionCounter);
        foreach (MotherBrainFadeProgress fade in new[] { state.FromFakeDeathGrey, state.DrainedGreying, state.RevivalFromGrey, state.DeathBodyFade, state.CorpseGreying })
            fade.Steps = transitionSteps;
        state.DroolGenerationEnabled = W(MotherBrainMemory.DroolGeneration) != 0;
        state.LastDroolMouthPoint = W(MotherBrainMemory.DroolProjectileParameter);
        state.SmallPurpleBreathGenerationEnabled = W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0;
        state.SmallPurpleBreathActive = W(MotherBrainMemory.SmallPurpleBreathActive) != 0;
        state.WalkCounter = SuperMetroid.Core.Compatibility.MotherBrainWalkCreditConstants.FromNativeWord(W(MotherBrainMemory.WalkCounter));
        state.AttackPhase = MotherBrainNativeWords.AttackPhases.Decode(W(MotherBrainMemory.AttackPhase), "Mother Brain attack phase");
        state.AttackCooldown = W(MotherBrainMemory.BodyAttackCooldown);
        state.BombCounter = W(MotherBrainMemory.BombCounter);
        state.BodyTargetXPosition = W(MotherBrainMemory.BodyFunctionTimer);
        state.HandBeamPhase = MotherBrainNativeWords.HandBeamPhases.Decode(W(MotherBrainMemory.DeathBeamAttackPhase), "Mother Brain hand-beam phase");
        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromStoredParts(
            W(MotherBrainMemory.DeathBeamNext + 2), W(MotherBrainMemory.DeathBeamNext),
            W(MotherBrainMemory.DeathBeamNext + 6), W(MotherBrainMemory.DeathBeamNext + 4));
        state.HandBeamNextVelocity = new EightEightVector<RoomPixels>(
            new SignedEightEight(W(MotherBrainMemory.DeathBeamNext + 8)),
            new SignedEightEight(W(MotherBrainMemory.DeathBeamNext + 10)));
        // The game reads only the low byte of each aim angle, the byte angle it stored.
        state.HandBeamNextAngle = new TrigonometryAngle(W(MotherBrainMemory.DeathBeamNext + 12) & byte.MaxValue);
        state.OnionRingsTargetAngle = new TrigonometryAngle(W(MotherBrainMemory.OnionRingsTargetAngle) & byte.MaxValue);
        if (LateFightStages.Contains(state.Stage))
            ImportNativeMotherBrainRainbowSequence(state, W);
    }

    /// <summary>
    /// The brain playback the recorded brain list pointer <paramref name="pointer"/> stands for. The native pointer rests
    /// on the beat it plays next, so a pointer at a list's entry rests on that list's first beat, entered.
    /// </summary>
    private static AnimationPlayback<MotherBrainHeadSegment> BrainPlaybackAt(IImportCartridgeSource cartridge, ushort pointer)
    {
        AnimationPlayback<MotherBrainHeadSegment> playback = NativeEnemyAnimationOracle.PlaybackAt(MotherBrainNativeListAddresses.CompareHead(cartridge),
            MotherBrainNativeListAddresses.HeadEntries, MotherBrainAnimations.HeadStartingAt, pointer);
        return playback.Place.Beat == AnimationPlace.NotEntered ? playback with { Place = AnimationPlace.StartOf(playback.Place.Phase) } : playback;
    }

    /// <summary>The stages the late fight's sequence runs, which a recorded body function in one of them resumes (#2208).</summary>
    private static readonly HashSet<MotherBrainBodyStage> LateFightStages =
    [
        MotherBrainBodyStage.RainbowBeam,
        MotherBrainBodyStage.FinishingSamusOff,
        MotherBrainBodyStage.DrainedByBaby,
        MotherBrainBodyStage.Reviving,
        MotherBrainBodyStage.Phase3Fight,
        MotherBrainBodyStage.Dying,
    ];

    /// <summary>
    /// Rebuilds the rainbow-beam sequence that runs phase two from the beam onward. It runs over the imported encounter,
    /// whose phase, neck and flags were imported above (#2209), so only what the late fight alone keeps is imported here.
    /// </summary>
    private static void ImportNativeMotherBrainRainbowSequence(MotherBrainEnemyState state, Func<int, ushort> W)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence(state);
        // The cartridge keeps the brain's fall speed in the body's phase timer.
        sequence.State.BrainFallSpeed = new SignedEightEight(W(MotherBrainMemory.BodyFunctionTimer));
        sequence.RainbowAttack.AngularWidth = RotationAngleConstants.FromRaw(W(MotherBrainMemory.RainbowBeamAngularWidth));
        // The cartridge counts the attempts left less one, by sign: six at the start, minus one once they are spent.
        sequence.RainbowAttack.SoundAttemptsLeft = CoordinateAxis.AsSigned(W(MotherBrainMemory.RainbowBeamSoundQueueCount)) + 1;
        sequence.State.RainbowBeamSoundPlaying = W(MotherBrainMemory.RainbowBeamSoundPlaying) != 0;
        sequence.State.HealthColoursEnabled = W(MotherBrainMemory.HealthBasedPaletteHandling) != 0;
        sequence.State.Phase2CorpseState = DecodePhase2CorpseState(W(MotherBrainMemory.Phase2CorpseState));
        sequence.Revival.BabyMetroidAttackCounter = W(MotherBrainMemory.BabyMetroidAttackCounter);
        // Once the sequence runs, the sub-function word holds the bursts' interval; their place was imported with the
        // encounter, whose cycle the sequence shares.
        PrivateState.Set(state.Bursts, nameof(MotherBrainBurstCycle.Interval), CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunction)));
        state.RainbowBeamSequence = sequence;
    }

    /// <summary>The phases of her rise from which it has placed her body with her (#1949, #2031).</summary>
    private static readonly HashSet<MotherBrainAscentPhase> RisingWithBodyAreaPhases =
    [
        MotherBrainAscentPhase.StartMusicAndEarthquake,
        MotherBrainAscentPhase.RaiseMotherBrain,
        MotherBrainAscentPhase.WaitUntilUncrouched,
        MotherBrainAscentPhase.TransitionFromGray,
    ];

    /// <summary>The phases of her death after her escape's start has placed her body at the room's corner for the warning.</summary>
    private static readonly HashSet<MotherBrainDeathPhase> EscapeWarningPhases =
    [
        MotherBrainDeathPhase.SpawnTimeBombSetSubtitle,
        MotherBrainDeathPhase.TypeOutZebesEscapeText,
        MotherBrainDeathPhase.DoorExplodingStartTimer,
        MotherBrainDeathPhase.BlowUpEscapeDoor,
        MotherBrainDeathPhase.KeepEarthquakeGoing,
    ];

    /// <summary>
    /// Where her body is placed at the imported form and body phase of <paramref name="state"/> (#1949, #2031): unplaced
    /// until her rise places it, with her from then on, and at the room's corner once her escape's start has shown the
    /// warning there, as the port's rise and escape start place it.
    /// </summary>
    internal static MotherBrainBodyPlacement BodyPlacementOf(MotherBrainEnemyState state) =>
        state.Stage == MotherBrainBodyStage.Dying && EscapeWarningPhases.Contains(state.DeathPhase) ? MotherBrainBodyPlacement.AtRoomCorner
        : state.Form >= MotherBrainForm.SecondPhase || (state.Form == MotherBrainForm.FakeDeath &&
            state.Stage == MotherBrainBodyStage.FakeDeathAscent && RisingWithBodyAreaPhases.Contains(state.AscentPhase))
            ? MotherBrainBodyPlacement.FollowsBody
            : MotherBrainBodyPlacement.Unplaced;

    /// <summary>Decodes the snapshot's form word, rejecting a stage the encounter does not have.</summary>
    internal static MotherBrainForm DecodeForm(ushort word) => word switch
    {
        0 => MotherBrainForm.Glass,
        1 => MotherBrainForm.FakeDeath,
        2 => MotherBrainForm.SecondPhase,
        3 => MotherBrainForm.DrainedByBabyMetroid,
        4 => MotherBrainForm.ThirdPhase,
        _ => throw new InvalidDataException($"${word:X4} is not a Mother Brain form."),
    };

    /// <summary>Decodes the snapshot's body-pose word, rejecting a value no body animation cue writes.</summary>
    internal static MotherBrainBodyPose DecodePose(ushort word) => word switch
    {
        0 => MotherBrainBodyPose.Standing,
        1 => MotherBrainBodyPose.Walking,
        2 => MotherBrainBodyPose.CrouchingTransition,
        3 => MotherBrainBodyPose.Crouched,
        4 => MotherBrainBodyPose.DeathBeam,
        6 => MotherBrainBodyPose.LeaningDown,
        _ => throw new InvalidDataException($"${word:X4} is not a Mother Brain body pose."),
    };

    /// <summary>Decodes the snapshot's corpse-handshake word, rejecting a value outside living, corpse and revived.</summary>
    internal static MotherBrainPhase2CorpseState DecodePhase2CorpseState(ushort word) => word switch
    {
        0 => MotherBrainPhase2CorpseState.Living,
        1 => MotherBrainPhase2CorpseState.Corpse,
        2 => MotherBrainPhase2CorpseState.Revived,
        _ => throw new InvalidDataException($"${word:X4} is not a Mother Brain phase-two corpse state."),
    };

    /// <summary>
    /// Decodes the snapshot's two-bytes-per-entry rainbow palette list offset into the next rainbow frame. The offset
    /// one past the last frame is the list's terminator, which the next palette step reads as a return to the first
    /// frame; an odd offset or one beyond the terminator is not a state the game produces.
    /// </summary>
    internal static int DecodeNextRainbowFrame(ushort word) =>
        word % 2 == 0 && word / 2 <= MotherBrainRainbowColourFormat.RainbowFrameCount
            ? word / 2 % MotherBrainRainbowColourFormat.RainbowFrameCount
            : throw new InvalidDataException($"Mother Brain rainbow palette list offset {word} is outside the list.");

    /// <summary>
    /// Decodes the snapshot's leg-transfer list pointer into the next leg-artwork page. A zero pointer is a list that
    /// is not running, which starts from the first page; any other pointer must name one of the twelve records.
    /// </summary>
    internal static int DecodeLegTilePage(ushort word) =>
        word == 0 ? 0 : MotherBrainLegLoadListConstants.IndexOf(word);

    /// <summary>
    /// The phase-two transition reveals her body and her room shows it from then on (#1356, #1466, #1515). A snapshot
    /// taken after the transition never ran it in the port, so her body starts shown, showing the installed body frame
    /// whose words the staged screen holds, or nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The staged screen is not one installed body frame over her blank.</exception>
    private static void ImportMotherBrainBodyImage(SuperMetroidRuntime runtime, MotherBrainEnemyState state, Func<int, ushort> W)
    {
        bool transferred = state.Form >= MotherBrainForm.SecondPhase || (state.Form == MotherBrainForm.FakeDeath &&
            state.Stage == MotherBrainBodyStage.FakeDeathAscent &&
            state.AscentPhase is not (MotherBrainAscentPhase.DrawMural or MotherBrainAscentPhase.SetupPhase2Graphics));
        if (!transferred)
            return;
        var screen = new ushort[BossBodySourceDefinitions.ScreenRows * BossBodySourceDefinitions.LayerColumns];
        for (int word = 0; word < screen.Length; word++)
            screen[word] = W(EnemyBg2StagingWram + word * sizeof(ushort));
        BossBodySheets sheets = runtime.BossArtwork.BodySheets ?? throw new InvalidOperationException("The runtime installs no boss body sheets.");
        // The installation's frames, which the runtime's installed Mother Brain body sheet is composed from.
        BossSourceArtwork bosses = InstalledFixtures.BossArtwork;
        BossBodySheet sheet = sheets.Of(BossBodyOwner.MotherBrain);
        var image = new BossBodyParts(BossBodyOwner.MotherBrain);
        BossBodyFrameSource? staged = BossBodySheetComposer.Frames(bosses, BossBodyOwner.MotherBrain).FirstOrDefault(frame =>
            frame.Cells.All(entry => entry.Key < screen.Length && screen[entry.Key] == entry.Value.PackedWord));
        if (staged is not null)
            image.Show(state.Body.DefinitionId, sheet.Frame(staged.Name));
        if (!BossBodyWords.Of(image.Publish(), BossBodyOwner.MotherBrain, bosses).AsSpan(0, screen.Length).SequenceEqual(screen))
            throw new InvalidDataException("The snapshot's staged Mother Brain body is not one installed body frame over her blank.");
        runtime.Enemies.Presentation.AttachBossBody(image);
    }

    /// <summary>The WRAM offset of the enemy BG2 staging image, <c>$7E:2000</c>.</summary>
    private const int EnemyBg2StagingWram = 0x2000;
}
