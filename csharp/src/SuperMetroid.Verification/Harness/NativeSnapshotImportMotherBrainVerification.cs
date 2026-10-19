using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Maps Mother Brain's native body, brain, neck, and rainbow-sequence WRAM into a loaded encounter.</summary>
internal static class NativeSnapshotImportMotherBrainVerification
{
    /// <summary>
    /// Imports Mother Brain's shared encounter state (enemy records $EC7F/$EC3F) from the
    /// native body/brain slot words and her extended WRAM ($7E:7800-$787B, $7E:8000-$8069).
    /// Called only when the room loaded her; the physical slot words were imported first.
    /// Native list pointers become typed program cursors (#641) through the ROM oracle's walk of each list.
    /// </summary>
    internal static void ImportNativeMotherBrain(SuperMetroidRuntime runtime, IImportCartridgeSource cartridge, Func<int, ushort> W)
    {
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The native snapshot holds Mother Brain, but the room load did not create her.");
        state.Form = DecodeForm(W(MotherBrainMemory.Form));
        state.Pose = (MotherBrainBodyPose)W(MotherBrainMemory.Pose);
        state.HitboxesEnabled = MotherBrainHitboxPartsCodec.FromNativeWord(W(MotherBrainMemory.HitboxesEnabled));
        state.Function = (MotherBrainBodyFunction)W(MotherBrainMemory.BodyFunction);
        state.BrainFunction = MotherBrainNativeListAddresses.NativeBrainFunctions[W(MotherBrainMemory.BrainFunction)];
        state.FunctionTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.BodyFunctionTimer));
        ushort flash = W(MotherBrainMemory.RoomPaletteInstructionList);
        state.RoomFlashFrame = flash == 0 ? null
            : MotherBrainNativeListAddresses.RoomFlashRecords.Single(record => unchecked((ushort)record.Record) == flash).Frame;
        state.RoomFlashTimer = W(MotherBrainMemory.RoomPaletteInstructionTimer);
        // The sub-function word is shared: outside the tube collapse it holds the death-explosion interval timer
        // (imported below), so only a collapse routine pointer decodes to a collapse function.
        if (MotherBrainNativeListAddresses.NativeTubeCollapseFunctions.TryGetValue(W(MotherBrainMemory.BrainSubFunction), out MotherBrainTubeCollapseFunction collapse))
            state.TubeCollapseFunction = collapse;
        state.TubeCollapseTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.BrainSubFunctionTimer));
        state.BodySubFunctionTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.BrainSubFunctionTimer));
        state.FakeDeathExplosionTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.FakeDeathExplosionTimer));
        state.FakeDeathExplosionIndex = W(MotherBrainMemory.FakeDeathExplosionIndex);
        state.EnableUnpauseHook = W(MotherBrainMemory.EnableUnpauseHook) != 0;
        state.NeckPalette = SnesObjAttributeWord.FromPaletteBits(W(MotherBrainMemory.NeckPaletteIndex));
        state.BrainPalette = SnesObjAttributeWord.FromPaletteBits(W(MotherBrainMemory.BrainPaletteIndex));
        ushort brain = W(MotherBrainMemory.BrainInstructionList);
        // Bit 15 clear means the brain list is not live.
        state.BrainCursor = unchecked((short)brain) >= 0 ? null : MotherBrainNativeListAddresses.CompareHead(cartridge)[brain];
        state.BrainFrameTimer = W(MotherBrainMemory.BrainInstructionTimer);
        Dictionary<ushort, AnimationProgramCursor<MotherBrainBodyProgram>> body = MotherBrainNativeListAddresses.CompareBody(cartridge);
        state.BodyCursor = body[checked((ushort)state.Body.CurrentInstruction)];
        state.HeadSlotCursor = body[checked((ushort)state.Head.CurrentInstruction)];
        Dictionary<ushort, AnimationProgramCursor<MotherBrainTubePiece>> tubes = MotherBrainNativeListAddresses.CompareTubes(cartridge);
        foreach (RoomEnemySlot tube in runtime.Enemies.SlotTable.Slots.Where(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes))
            runtime.Enemies.MotherBrainBehavior.Extensions[tube.SlotIndex].FallingTubeCursor = tubes[checked((ushort)tube.CurrentInstruction)];
        state.BrainMainShakeTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.BrainMainShakeTimer));
        state.DeleteTurretsAndRinkas = W(MotherBrainMemory.DeleteTurretsAndRinkas) != 0;
        state.RainbowPaletteCursor = W(MotherBrainMemory.RainbowPaletteIndex);
        state.DrawNeck = state.BrainFunction == MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn;
        state.NeckSegment0Distance = W(MotherBrainMemory.NeckSegments + 4);
        state.NeckSegment1Distance = W(MotherBrainMemory.NeckSegments + 10);
        state.NeckSegment2Distance = W(MotherBrainMemory.NeckSegments + 16);
        state.NeckSegment3Distance = W(MotherBrainMemory.NeckSegments + 22);
        state.NeckSegment4Distance = W(MotherBrainMemory.NeckSegments + 28);
        Position16<RoomPixels> Segment(int index) => new(
            W(MotherBrainMemory.NeckSegments + index * 6), W(MotherBrainMemory.NeckSegments + index * 6 + 2));
        state.NeckSegment0 = Segment(0);
        state.NeckSegment1 = Segment(1);
        state.NeckSegment2 = Segment(2);
        state.NeckSegment3 = Segment(3);
        state.NeckSegment4 = Segment(4);
        state.LowerNeckAngle = SnesAngle.FromRaw(W(MotherBrainMemory.LowerNeckAngle));
        state.UpperNeckAngle = SnesAngle.FromRaw(W(MotherBrainMemory.UpperNeckAngle));
        state.NeckAngleDelta = W(MotherBrainMemory.NeckAngleDelta);
        state.NeckMovementEnabled = W(MotherBrainMemory.NeckMovementEnabled) != 0;
        state.LowerNeckMovementIndex = ClosedNativeWords.Decode<MotherBrainNeckMovement>(
            W(MotherBrainMemory.LowerNeckMovementIndex), "Mother Brain lower neck movement index");
        state.UpperNeckMovementIndex = ClosedNativeWords.Decode<MotherBrainNeckMovement>(
            W(MotherBrainMemory.UpperNeckMovementIndex), "Mother Brain upper neck movement index");
        state.NextLegTilePage = DecodeLegTilePage(W(MotherBrainMemory.SpriteTileTransferEntry));
        state.GrayTransitionCounter = W(MotherBrainMemory.GreyTransitionCounter);
        state.BrainPaletteHandlingEnabled = W(MotherBrainMemory.BrainPaletteHandling) != 0;
        state.DroolGenerationEnabled = W(MotherBrainMemory.DroolGeneration) != 0;
        state.DroolProjectileParameter = W(MotherBrainMemory.DroolProjectileParameter);
        state.SmallPurpleBreathGenerationEnabled = W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0;
        state.SmallPurpleBreathActive = W(MotherBrainMemory.SmallPurpleBreathActive) != 0;
        state.WalkCounter = MotherBrainWalkCreditConstants.FromNativeWord(W(MotherBrainMemory.WalkCounter));
        state.AttackPhase = (MotherBrainAttackPhase)W(MotherBrainMemory.AttackPhase);
        state.AttackCooldown = NativeCountdown.FromRaw(W(MotherBrainMemory.BodyAttackCooldown));
        state.BombCounter = W(MotherBrainMemory.BombCounter);
        state.BodyTargetXPosition = W(MotherBrainMemory.BodyFunctionTimer);
        state.HandBeamPhase = (MotherBrainHandBeamPhase)W(MotherBrainMemory.DeathBeamAttackPhase);
        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromWords(
            W(MotherBrainMemory.DeathBeamNext + 2), W(MotherBrainMemory.DeathBeamNext),
            W(MotherBrainMemory.DeathBeamNext + 6), W(MotherBrainMemory.DeathBeamNext + 4));
        state.HandBeamNextVelocity = new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(W(MotherBrainMemory.DeathBeamNext + 8)),
            new SnesSignedEightEight(W(MotherBrainMemory.DeathBeamNext + 10)));
        state.HandBeamNextAngle = W(MotherBrainMemory.DeathBeamNext + 12);
        state.OnionRingsTargetAngle = W(MotherBrainMemory.OnionRingsTargetAngle);
        state.RainbowBeamAngularWidth = W(MotherBrainMemory.RainbowBeamAngularWidth);
        if (RainbowPhaseOfFunction.Value.TryGetValue(state.Function, out MotherBrainRainbowBeamAttackPhase phase))
            ImportNativeMotherBrainRainbowSequence(runtime, state, phase, W);
    }

    /// <summary>
    /// The live rainbow sequence's phase for each body function it publishes, inverted from
    /// the room adapter's own map. Only <see cref="MotherBrainRainbowBeamAttackPhase.Inactive"/>
    /// publishes no function.
    /// </summary>
    private static readonly Lazy<Dictionary<MotherBrainBodyFunction, MotherBrainRainbowBeamAttackPhase>> RainbowPhaseOfFunction =
        new(() => Enum.GetValues<MotherBrainRainbowBeamAttackPhase>()
            .Where(phase => phase != MotherBrainRainbowBeamAttackPhase.Inactive)
            .ToDictionary(
                phase => MotherBrainBehavior.MapLiveMotherBrainRainbowFunction(phase),
                phase => phase));

    /// <summary>
    /// Rebuilds the rainbow-beam sequence that owns phase two from the beam onward. The
    /// sequence keeps its own copies of native words; live actor words are re-synchronized
    /// before each step, so only the sequence-owned words are imported here.
    /// </summary>
    private static void ImportNativeMotherBrainRainbowSequence(SuperMetroidRuntime runtime, MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackPhase phase, Func<int, ushort> W)
    {
        var artwork = runtime.Enemies.Room.MotherBrainCorpseArtwork;
        var sequence = new MotherBrainRainbowBeamAttackSequence(artwork);
        sequence.Phase = phase;
        sequence.FunctionTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.BodyFunctionTimer));
        sequence.AngularWidth = W(MotherBrainMemory.RainbowBeamAngularWidth);
        sequence.SoundQueueCount = NativeCountdown.FromRaw(W(MotherBrainMemory.RainbowBeamSoundQueueCount));
        sequence.RainbowBeamSoundPlaying = W(MotherBrainMemory.RainbowBeamSoundPlaying) != 0;
        sequence.BrainMainShakeTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.BrainMainShakeTimer));
        sequence.BrainPaletteHandlingEnabled = W(MotherBrainMemory.BrainPaletteHandling) != 0;
        sequence.HealthBasedPaletteHandlingEnabled = W(MotherBrainMemory.HealthBasedPaletteHandling) != 0;
        sequence.DroolGenerationEnabled = W(MotherBrainMemory.DroolGeneration) != 0;
        sequence.SmallPurpleBreathGenerationEnabled = W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0;
        sequence.GreyTransitionCounter = W(MotherBrainMemory.GreyTransitionCounter);
        sequence.Phase2CorpseState = DecodePhase2CorpseState(W(MotherBrainMemory.Phase2CorpseState));
        sequence.BabyMetroidAttackCounter = W(MotherBrainMemory.BabyMetroidAttackCounter);
        sequence.BrainPalette = SnesObjAttributeWord.FromPaletteBits(W(MotherBrainMemory.BrainPaletteIndex));
        sequence.MotherBrainUnpauseHookEnabled = W(MotherBrainMemory.EnableUnpauseHook) != 0;
        sequence.RainbowBeamPaletteAnimationIndex = W(MotherBrainMemory.RainbowPaletteIndex);
        sequence.DeathExplosionIntervalTimer = NativeCountdown.FromRaw(W(MotherBrainMemory.BrainSubFunction));
        sequence.DeathAndEscapeExplosionIndex = NativeCountdown.FromRaw(W(MotherBrainMemory.BrainSubFunctionTimer));
        sequence.LowerNeckAngle = SnesAngle.FromRaw(W(MotherBrainMemory.LowerNeckAngle));
        sequence.UpperNeckAngle = SnesAngle.FromRaw(W(MotherBrainMemory.UpperNeckAngle));
        sequence.NeckAngleDelta = W(MotherBrainMemory.NeckAngleDelta);
        sequence.NeckMovementEnabled = W(MotherBrainMemory.NeckMovementEnabled) != 0;
        sequence.LowerNeckMovementIndex = ClosedNativeWords.Decode<MotherBrainNeckMovement>(
            W(MotherBrainMemory.LowerNeckMovementIndex), "Mother Brain lower neck movement index");
        sequence.UpperNeckMovementIndex = ClosedNativeWords.Decode<MotherBrainNeckMovement>(
            W(MotherBrainMemory.UpperNeckMovementIndex), "Mother Brain upper neck movement index");
        state.RainbowBeamSequence = sequence;
    }

    /// <summary>Decodes the snapshot's form word, rejecting a stage the encounter does not have.</summary>
    internal static MotherBrainForm DecodeForm(ushort word) =>
        ClosedNativeWords.Decode<MotherBrainForm>(word, "Mother Brain form");

    /// <summary>Decodes the snapshot's corpse-handshake word, rejecting a value outside living, corpse and revived.</summary>
    internal static MotherBrainPhase2CorpseState DecodePhase2CorpseState(ushort word) =>
        ClosedNativeWords.Decode<MotherBrainPhase2CorpseState>(word, "Mother Brain phase-two corpse state");

    /// <summary>
    /// Decodes the snapshot's leg-transfer list pointer into the next leg-artwork page. A zero pointer is a list that
    /// is not running, which starts from the first page; any other pointer must name one of the twelve records.
    /// </summary>
    internal static int DecodeLegTilePage(ushort word) =>
        word == 0 ? 0 : MotherBrainLegTileTransferDefinitionsConstants.IndexOf(word);
}
