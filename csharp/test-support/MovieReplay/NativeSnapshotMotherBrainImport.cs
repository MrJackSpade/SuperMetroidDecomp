using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
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
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The native snapshot holds Mother Brain, but the room load did not create her.");
        state.Form = DecodeForm(W(MotherBrainMemory.Form));
        ImportMotherBrainBodyImage(runtime, state, W);
        state.Pose = (MotherBrainBodyPose)W(MotherBrainMemory.Pose);
        state.HitboxesEnabled = MotherBrainHitboxPartsCodec.FromNativeWord(W(MotherBrainMemory.HitboxesEnabled));
        state.Function = MotherBrainNativeWords.MotherBrainBodyFunctions.Decode(W(MotherBrainMemory.BodyFunction), "recorded MotherBrainBodyFunction");
        state.BrainFunction = MotherBrainNativeListAddresses.NativeBrainFunctions[W(MotherBrainMemory.BrainFunction)];
        state.FunctionTimer = CoordinateAxis.AsSigned(W(MotherBrainMemory.BodyFunctionTimer));
        ushort flash = W(MotherBrainMemory.RoomPaletteAnimationScript);
        state.RoomFlashFrame = flash == 0 ? null
            : MotherBrainNativeListAddresses.RoomFlashRecords.Single(record => unchecked((ushort)record.Record) == flash).Frame;
        state.RoomFlashTimer = W(MotherBrainMemory.RoomPaletteFrameHold);
        // The sub-function word is shared: outside the tube collapse it holds the death-explosion interval timer
        // (imported below), so only a collapse routine pointer decodes to a collapse function.
        if (MotherBrainNativeListAddresses.NativeTubeCollapseFunctions.TryGetValue(W(MotherBrainMemory.BrainSubFunction), out MotherBrainTubeCollapseFunction collapse))
            state.TubeCollapseFunction = collapse;
        state.TubeCollapseTimer = CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunctionTimer));
        state.BodySubFunctionTimer = CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunctionTimer));
        state.FakeDeathExplosionTimer = CoordinateAxis.AsSigned(W(MotherBrainMemory.FakeDeathExplosionTimer));
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
        Dictionary<MotherBrainBodyProgram, NativeEnemyList<MotherBrainBodyProgram>> body = MotherBrainNativeListAddresses.CompareBody(cartridge);
        // Each record's instruction-list word is read from the snapshot; the port keeps the typed playback it stands for.
        ushort AnimationScript(RoomEnemySlot slot) => W(NativeSnapshotMemory.EnemyBase +
            SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(slot.SlotIndex) + MovieDesyncMemory.EnemyAnimationScriptOffset);
        AnimationPlayback<MotherBrainBodyProgram> BodyPlaybackAt(RoomEnemySlot slot) => NativeEnemyAnimationOracle.PlaybackAt(
            body, MotherBrainNativeListAddresses.BodyEntries, MotherBrainAnimations.BodyStartingAt, AnimationScript(slot));
        state.BodyPlayback = BodyPlaybackAt(state.Body);
        state.HeadSlotPlayback = BodyPlaybackAt(state.Head);
        Dictionary<MotherBrainTubePiece, NativeEnemyList<MotherBrainTubePiece>> tubes = MotherBrainNativeListAddresses.CompareTubes(cartridge);
        foreach (RoomEnemySlot tube in runtime.Enemies.SlotTable.Slots.Where(slot => slot.DefinitionId == EnemyDefinitionId.MotherBrainTubes))
        {
            tube.RequireSpeciesState<MotherBrainFallingTubeState>().Playback = NativeEnemyAnimationOracle.PlaybackAt(
                tubes, MotherBrainNativeListAddresses.TubeEntries, MotherBrainTubeAnimations.StartingAt, AnimationScript(tube));
        }
        state.BrainMainShakeTimer = W(MotherBrainMemory.BrainMainShakeTimer);
        state.DeleteTurretsAndRinkas = W(MotherBrainMemory.DeleteTurretsAndRinkas) != 0;
        state.NextRainbowFrame = DecodeNextRainbowFrame(W(MotherBrainMemory.RainbowPaletteIndex));
        state.DrawNeck = state.BrainFunction == MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn;
        for (int joint = 0; joint < MotherBrainNeckJointDefinitions.Count; joint++)
        {
            int record = MotherBrainMemory.NeckSegments + joint * 6;
            state.NeckJoints[joint] = new(W(record), W(record + 2));
            state.NeckJointDistances[joint] = W(record + 4);
        }
        state.LowerNeckAngle = RotationAngle.FromRaw(W(MotherBrainMemory.LowerNeckAngle));
        state.UpperNeckAngle = RotationAngle.FromRaw(W(MotherBrainMemory.UpperNeckAngle));
        state.NeckAngleDelta = W(MotherBrainMemory.NeckAngleDelta);
        state.NeckMovementEnabled = W(MotherBrainMemory.NeckMovementEnabled) != 0;
        state.LowerNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.LowerNeckMovementIndex), "Mother Brain lower neck movement index");
        state.UpperNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.UpperNeckMovementIndex), "Mother Brain upper neck movement index");
        state.StandingBodyReadyingElapsed = DecodeLegTilePage(W(MotherBrainMemory.SpriteTileTransferEntry));
        state.GrayTransitionCounter = W(MotherBrainMemory.GreyTransitionCounter);
        state.DroolGenerationEnabled = W(MotherBrainMemory.DroolGeneration) != 0;
        state.DroolProjectileParameter = W(MotherBrainMemory.DroolProjectileParameter);
        state.SmallPurpleBreathGenerationEnabled = W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0;
        state.SmallPurpleBreathActive = W(MotherBrainMemory.SmallPurpleBreathActive) != 0;
        state.WalkCounter = MotherBrainWalkCreditConstants.FromNativeWord(W(MotherBrainMemory.WalkCounter));
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
        state.RainbowBeamAngularWidth = RotationAngle.FromRaw(W(MotherBrainMemory.RainbowBeamAngularWidth));
        if (RainbowSequenceFunctions.Contains(state.Function))
            ImportNativeMotherBrainRainbowSequence(state, state.Function, W);
    }

    /// <summary>
    /// The brain playback the recorded brain list pointer <paramref name="pointer"/> stands for. The native pointer rests
    /// on the beat it plays next, so a pointer at a list's entry rests on that list's first beat, entered.
    /// </summary>
    private static AnimationPlayback<MotherBrainHeadProgram> BrainPlaybackAt(IImportCartridgeSource cartridge, ushort pointer)
    {
        AnimationPlayback<MotherBrainHeadProgram> playback = NativeEnemyAnimationOracle.PlaybackAt(MotherBrainNativeListAddresses.CompareHead(cartridge),
            MotherBrainNativeListAddresses.HeadEntries, MotherBrainAnimations.HeadStartingAt, pointer);
        return playback.Place.Beat == AnimationPlace.NotEntered ? playback with { Place = AnimationPlace.StartOf(playback.Place.Phase) } : playback;
    }

    /// <summary>The body functions the live rainbow sequence runs, which a recorded function in this set resumes.</summary>
    private static readonly HashSet<MotherBrainBodyFunction> RainbowSequenceFunctions =
    [
        MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck,
        MotherBrainBodyFunction.SecondPhaseRainbowStartCharging,
        MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck,
        MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge,
        MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown,
        MotherBrainBodyFunction.SecondPhaseRainbowStartFiring,
        MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall,
        MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay,
        MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus,
        MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus,
        MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring,
        MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall,
        MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand,
        MotherBrainBodyFunction.SecondPhaseRainbowLowerHead,
        MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction,
        MotherBrainBodyFunction.SecondPhaseFinishSamusOff,
        MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp,
        MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire,
        MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam,
        MotherBrainBodyFunction.SecondPhaseFinishSamusOffReadyBaby,
        MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam,
        MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey,
        MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey,
        MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey,
        MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife,
        MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey,
        MotherBrainBodyFunction.SecondPhaseReviveWakeUp,
        MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch,
        MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby,
        MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath,
        MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath,
        MotherBrainBodyFunction.SecondPhaseMurderBabyAttack,
        MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown,
        MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack,
        MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack,
        MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding,
        MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance,
        MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting,
        MotherBrainBodyFunction.ThirdPhaseFightingMain,
        MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown,
        MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom,
        MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding,
        MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom,
        MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects,
        MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut,
        MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody,
        MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions,
        MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation,
        MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround,
        MotherBrainBodyFunction.ThirdPhaseDeathReadyCorpse,
        MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey,
        MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey,
        MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver,
        MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway,
        MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay,
        MotherBrainBodyFunction.ThirdPhaseDeathReadyEscapeWarning,
        MotherBrainBodyFunction.ThirdPhaseDeathStartEscape,
        MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle,
        MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText,
        MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer,
        MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor,
        MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing,
    ];

    /// <summary>
    /// Rebuilds the rainbow-beam sequence that owns phase two from the beam onward. The
    /// sequence keeps its own copies of native words; live actor words are re-synchronized
    /// before each step, so only the sequence-owned words are imported here.
    /// </summary>
    private static void ImportNativeMotherBrainRainbowSequence(MotherBrainEnemyState state,
        MotherBrainBodyFunction phase, Func<int, ushort> W)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        sequence.State.Phase = phase;
        sequence.State.FunctionTimer = CoordinateAxis.AsSigned(W(MotherBrainMemory.BodyFunctionTimer));
        sequence.RainbowAttack.AngularWidth = RotationAngle.FromRaw(W(MotherBrainMemory.RainbowBeamAngularWidth));
        sequence.RainbowAttack.SoundQueueCount = CoordinateAxis.AsSigned(W(MotherBrainMemory.RainbowBeamSoundQueueCount));
        sequence.State.RainbowBeamSoundPlaying = W(MotherBrainMemory.RainbowBeamSoundPlaying) != 0;
        sequence.State.BrainMainShakeTimer = W(MotherBrainMemory.BrainMainShakeTimer);
        sequence.State.HealthColoursEnabled = W(MotherBrainMemory.HealthBasedPaletteHandling) != 0;
        sequence.State.DroolGenerationEnabled = W(MotherBrainMemory.DroolGeneration) != 0;
        sequence.State.SmallPurpleBreathGenerationEnabled = W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0;
        sequence.State.GreyTransitionCounter = W(MotherBrainMemory.GreyTransitionCounter);
        sequence.State.Phase2CorpseState = DecodePhase2CorpseState(W(MotherBrainMemory.Phase2CorpseState));
        sequence.Revival.BabyMetroidAttackCounter = W(MotherBrainMemory.BabyMetroidAttackCounter);
        sequence.ThirdPhaseDeath.DeathExplosionIntervalTimer = CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunction));
        sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex = CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunctionTimer));
        sequence.LowerNeckAngle = RotationAngle.FromRaw(W(MotherBrainMemory.LowerNeckAngle));
        sequence.UpperNeckAngle = RotationAngle.FromRaw(W(MotherBrainMemory.UpperNeckAngle));
        sequence.State.NeckAngleDelta = W(MotherBrainMemory.NeckAngleDelta);
        sequence.State.NeckMovementEnabled = W(MotherBrainMemory.NeckMovementEnabled) != 0;
        sequence.State.LowerNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.LowerNeckMovementIndex), "Mother Brain lower neck movement index");
        sequence.State.UpperNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.UpperNeckMovementIndex), "Mother Brain upper neck movement index");
        state.RainbowBeamSequence = sequence;
    }

    /// <summary>Decodes the snapshot's form word, rejecting a stage the encounter does not have.</summary>
    internal static MotherBrainForm DecodeForm(ushort word) =>
        ClosedEnumWordDecoding.Decode<MotherBrainForm>(word, "Mother Brain form");

    /// <summary>Decodes the snapshot's corpse-handshake word, rejecting a value outside living, corpse and revived.</summary>
    internal static MotherBrainPhase2CorpseState DecodePhase2CorpseState(ushort word) =>
        ClosedEnumWordDecoding.Decode<MotherBrainPhase2CorpseState>(word, "Mother Brain phase-two corpse state");

    /// <summary>
    /// Decodes the snapshot's two-bytes-per-entry rainbow palette list offset into the next rainbow frame. The offset
    /// one past the last frame is the list's terminator, which the next palette step reads as a return to the first
    /// frame; an odd offset or one beyond the terminator is not a state the game produces.
    /// </summary>
    internal static int DecodeNextRainbowFrame(ushort word) =>
        word % 2 == 0 && word / 2 <= MotherBrainRainbowPaletteFormat.RainbowFrameCount
            ? word / 2 % MotherBrainRainbowPaletteFormat.RainbowFrameCount
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
        var function = MotherBrainNativeWords.MotherBrainBodyFunctions.Decode(W(MotherBrainMemory.BodyFunction), "recorded MotherBrainBodyFunction");
        bool transferred = state.Form >= MotherBrainForm.SecondPhase || (state.Form == MotherBrainForm.FakeDeath && function is
            MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain or MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense or
            MotherBrainBodyFunction.FakeDeathAscentPrepareForRising or MotherBrainBodyFunction.FakeDeathAscentReadyStandingBody or
            MotherBrainBodyFunction.FakeDeathAscentContinuePausing or MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake or
            MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain or MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched or
            MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray);
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
