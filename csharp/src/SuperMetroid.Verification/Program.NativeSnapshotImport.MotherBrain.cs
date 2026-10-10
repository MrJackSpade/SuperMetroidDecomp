using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Imports Mother Brain's shared encounter state (enemy records $EC7F/$EC3F) from the
    /// native body/brain slot words and her extended WRAM ($7E:7800-$787B, $7E:8000-$8069).
    /// Called only when the room loaded her; the physical slot words were imported first.
    /// </summary>
    private static void ImportNativeMotherBrain(SuperMetroidRuntime runtime, Func<int, ushort> W)
    {
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain ?? throw new InvalidDataException(
            "The native snapshot holds Mother Brain, but the room load did not create her.");
        state.Form = W(MotherBrainMemory.Form);
        state.Pose = (MotherBrainBodyPose)W(MotherBrainMemory.Pose);
        state.HitboxesEnabled = W(MotherBrainMemory.HitboxesEnabled);
        state.Function = (MotherBrainBodyFunction)W(MotherBrainMemory.BodyFunction);
        state.BrainFunction = (MotherBrainBrainFunction)W(MotherBrainMemory.BrainFunction);
        state.FunctionTimer = W(MotherBrainMemory.BodyFunctionTimer);
        state.RoomPaletteInstructionPointer = W(MotherBrainMemory.RoomPaletteInstructionList);
        state.RoomPaletteInstructionTimer = W(MotherBrainMemory.RoomPaletteInstructionTimer);
        state.TubeCollapseFunction = (MotherBrainTubeCollapseFunction)W(MotherBrainMemory.BrainSubFunction);
        state.TubeCollapseTimer = W(MotherBrainMemory.BrainSubFunctionTimer);
        state.BodySubFunctionTimer = W(MotherBrainMemory.BrainSubFunctionTimer);
        state.FakeDeathExplosionTimer = W(MotherBrainMemory.FakeDeathExplosionTimer);
        state.FakeDeathExplosionIndex = W(MotherBrainMemory.FakeDeathExplosionIndex);
        state.EnableUnpauseHook = W(MotherBrainMemory.EnableUnpauseHook) != 0;
        state.NeckPaletteIndex = W(MotherBrainMemory.NeckPaletteIndex);
        state.BrainPaletteIndex = W(MotherBrainMemory.BrainPaletteIndex);
        state.BrainInstructionPointer = W(MotherBrainMemory.BrainInstructionList);
        state.BrainInstructionTimer = W(MotherBrainMemory.BrainInstructionTimer);
        state.BrainMainShakeTimer = W(MotherBrainMemory.BrainMainShakeTimer);
        state.DeleteTurretsAndRinkas = W(MotherBrainMemory.DeleteTurretsAndRinkas) != 0;
        state.RainbowPaletteCursor = W(MotherBrainMemory.RainbowPaletteIndex);
        state.DrawNeck = state.BrainFunction == MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn;
        state.NeckSegment0Distance = W(MotherBrainMemory.NeckSegments + 4);
        state.NeckSegment1Distance = W(MotherBrainMemory.NeckSegments + 10);
        state.NeckSegment2Distance = W(MotherBrainMemory.NeckSegments + 16);
        state.NeckSegment3Distance = W(MotherBrainMemory.NeckSegments + 22);
        state.NeckSegment4Distance = W(MotherBrainMemory.NeckSegments + 28);
        MotherBrainNeckPoint Segment(int index) => new(
            W(MotherBrainMemory.NeckSegments + index * 6), W(MotherBrainMemory.NeckSegments + index * 6 + 2));
        state.NeckSegment0 = Segment(0);
        state.NeckSegment1 = Segment(1);
        state.NeckSegment2 = Segment(2);
        state.NeckSegment3 = Segment(3);
        state.NeckSegment4 = Segment(4);
        state.LowerNeckAngle = W(MotherBrainMemory.LowerNeckAngle);
        state.UpperNeckAngle = W(MotherBrainMemory.UpperNeckAngle);
        state.NeckAngleDelta = W(MotherBrainMemory.NeckAngleDelta);
        state.NeckMovementEnabled = W(MotherBrainMemory.NeckMovementEnabled) != 0;
        state.LowerNeckMovementIndex = W(MotherBrainMemory.LowerNeckMovementIndex);
        state.UpperNeckMovementIndex = W(MotherBrainMemory.UpperNeckMovementIndex);
        state.SpriteTileTransferEntryPointer = W(MotherBrainMemory.SpriteTileTransferEntry);
        state.GrayTransitionCounter = W(MotherBrainMemory.GreyTransitionCounter);
        state.BrainPaletteHandlingEnabled = W(MotherBrainMemory.BrainPaletteHandling) != 0;
        state.DroolGenerationEnabled = W(MotherBrainMemory.DroolGeneration) != 0;
        state.DroolProjectileParameter = W(MotherBrainMemory.DroolProjectileParameter);
        state.SmallPurpleBreathGenerationEnabled = W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0;
        state.SmallPurpleBreathActive = W(MotherBrainMemory.SmallPurpleBreathActive) != 0;
        state.WalkCounter = W(MotherBrainMemory.WalkCounter);
        state.AttackPhase = (MotherBrainAttackPhase)W(MotherBrainMemory.AttackPhase);
        state.AttackCooldown = W(MotherBrainMemory.BodyAttackCooldown);
        state.BombCounter = W(MotherBrainMemory.BombCounter);
        state.BodyTargetXPosition = W(MotherBrainMemory.BodyFunctionTimer);
        state.HandBeamPhase = (MotherBrainHandBeamPhase)W(MotherBrainMemory.DeathBeamAttackPhase);
        state.HandBeamNextXSubposition = W(MotherBrainMemory.DeathBeamNext);
        state.HandBeamNextXPosition = W(MotherBrainMemory.DeathBeamNext + 2);
        state.HandBeamNextYSubposition = W(MotherBrainMemory.DeathBeamNext + 4);
        state.HandBeamNextYPosition = W(MotherBrainMemory.DeathBeamNext + 6);
        state.HandBeamNextXVelocity = W(MotherBrainMemory.DeathBeamNext + 8);
        state.HandBeamNextYVelocity = W(MotherBrainMemory.DeathBeamNext + 10);
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
                phase => (MotherBrainBodyFunction)PrivateState.InvokeStatic(typeof(RoomEnemySystem),
                    "MapLiveMotherBrainRainbowFunction", phase)!,
                phase => phase));

    /// <summary>
    /// Rebuilds the rainbow-beam sequence that owns phase two from the beam onward. The
    /// sequence keeps its own copies of native words; live actor words are re-synchronized
    /// before each step, so only the sequence-owned words are imported here.
    /// </summary>
    private static void ImportNativeMotherBrainRainbowSequence(SuperMetroidRuntime runtime, MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackPhase phase, Func<int, ushort> W)
    {
        var artwork = PrivateState.Property<SuperMetroid.Core.Assets.RoomCharacterAtlas>(runtime.Enemies, "MotherBrainCorpseArtwork");
        var sequence = new MotherBrainRainbowBeamAttackSequence(artwork);
        void Set(string property, object value) => PrivateState.SetProperty(sequence, property, value);
        Set(nameof(sequence.Phase), phase);
        Set(nameof(sequence.HeadInstructionTimer), W(MotherBrainMemory.BrainInstructionTimer));
        Set(nameof(sequence.HeadInstructionPointer), W(MotherBrainMemory.BrainInstructionList));
        Set(nameof(sequence.FunctionTimer), W(MotherBrainMemory.BodyFunctionTimer));
        Set(nameof(sequence.AngularWidth), W(MotherBrainMemory.RainbowBeamAngularWidth));
        Set(nameof(sequence.SoundQueueCount), W(MotherBrainMemory.RainbowBeamSoundQueueCount));
        Set(nameof(sequence.RainbowBeamSoundPlaying), W(MotherBrainMemory.RainbowBeamSoundPlaying) != 0);
        Set(nameof(sequence.BrainMainShakeTimer), W(MotherBrainMemory.BrainMainShakeTimer));
        Set(nameof(sequence.BrainPaletteHandlingEnabled), W(MotherBrainMemory.BrainPaletteHandling) != 0);
        Set(nameof(sequence.HealthBasedPaletteHandlingEnabled), W(MotherBrainMemory.HealthBasedPaletteHandling) != 0);
        Set(nameof(sequence.DroolGenerationEnabled), W(MotherBrainMemory.DroolGeneration) != 0);
        Set(nameof(sequence.SmallPurpleBreathGenerationEnabled), W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0);
        Set(nameof(sequence.GreyTransitionCounter), W(MotherBrainMemory.GreyTransitionCounter));
        Set(nameof(sequence.Phase2CorpseState), W(MotherBrainMemory.Phase2CorpseState));
        Set(nameof(sequence.BabyMetroidAttackCounter), W(MotherBrainMemory.BabyMetroidAttackCounter));
        Set(nameof(sequence.BrainPaletteIndex), W(MotherBrainMemory.BrainPaletteIndex));
        Set(nameof(sequence.MotherBrainUnpauseHookEnabled), W(MotherBrainMemory.EnableUnpauseHook) != 0);
        Set(nameof(sequence.RainbowBeamPaletteAnimationIndex), W(MotherBrainMemory.RainbowPaletteIndex));
        Set(nameof(sequence.DeathExplosionIntervalTimer), W(MotherBrainMemory.BrainSubFunction));
        Set(nameof(sequence.DeathAndEscapeExplosionIndex), W(MotherBrainMemory.BrainSubFunctionTimer));
        Set(nameof(sequence.LowerNeckAngle), W(MotherBrainMemory.LowerNeckAngle));
        Set(nameof(sequence.UpperNeckAngle), W(MotherBrainMemory.UpperNeckAngle));
        Set(nameof(sequence.NeckAngleDelta), W(MotherBrainMemory.NeckAngleDelta));
        Set(nameof(sequence.NeckMovementEnabled), W(MotherBrainMemory.NeckMovementEnabled));
        Set(nameof(sequence.LowerNeckMovementIndex), W(MotherBrainMemory.LowerNeckMovementIndex));
        Set(nameof(sequence.UpperNeckMovementIndex), W(MotherBrainMemory.UpperNeckMovementIndex));
        state.RainbowBeamSequence = sequence;
    }
}

/// <summary>
/// Native WRAM identities of Mother Brain's encounter state, from the body ($0FA8) and brain
/// ($0FE8) slot variables and her extended enemy RAM.
/// </summary>
internal static class MotherBrainMemory
{
    /// <summary>$0FA8: body function pointer.</summary>
    public const int BodyFunction = 0x0fa8;
    /// <summary>$0FB2: body function timer, also the body's target X during walks.</summary>
    public const int BodyFunctionTimer = 0x0fb2;
    /// <summary>$0FB4: attack cooldown, also the rainbow beam's explosion index.</summary>
    public const int BodyAttackCooldown = 0x0fb4;
    /// <summary>$0FE8: brain function pointer.</summary>
    public const int BrainFunction = 0x0fe8;
    /// <summary>$0FF0: brain sub-function (tube collapse), also the death explosion interval timer.</summary>
    public const int BrainSubFunction = 0x0ff0;
    /// <summary>$0FF2: brain sub-function timer, also the body sub-function timer and death explosion index.</summary>
    public const int BrainSubFunctionTimer = 0x0ff2;
    /// <summary>$7E:7800: form (glass, fake death, phase two, three).</summary>
    public const int Form = 0x7800;
    /// <summary>$7E:7804: body pose written by body bytecode.</summary>
    public const int Pose = 0x7804;
    /// <summary>$7E:7808: hitboxes-enabled word.</summary>
    public const int HitboxesEnabled = 0x7808;
    /// <summary>$7E:780E: walk counter.</summary>
    public const int WalkCounter = 0x780e;
    /// <summary>$7E:7818/$781A: neck and brain palette indices.</summary>
    public const int NeckPaletteIndex = 0x7818, BrainPaletteIndex = 0x781a;
    /// <summary>$7E:781C/$781E: room palette instruction list and timer.</summary>
    public const int RoomPaletteInstructionList = 0x781c, RoomPaletteInstructionTimer = 0x781e;
    /// <summary>$7E:782E: death (hand) beam attack phase.</summary>
    public const int DeathBeamAttackPhase = 0x782e;
    /// <summary>$7E:7830: attack phase.</summary>
    public const int AttackPhase = 0x7830;
    /// <summary>$7E:7834: onion rings target angle.</summary>
    public const int OnionRingsTargetAngle = 0x7834;
    /// <summary>$7E:783A: delete-turrets-and-Rinkas flag.</summary>
    public const int DeleteTurretsAndRinkas = 0x783a;
    /// <summary>$7E:7826: Baby Metroid attack counter.</summary>
    public const int BabyMetroidAttackCounter = 0x7826;
    /// <summary>$7E:782A/$782C: rainbow beam sound queue countdown and playing flag.</summary>
    public const int RainbowBeamSoundQueueCount = 0x782a, RainbowBeamSoundPlaying = 0x782c;
    /// <summary>$7E:783E: phase-two corpse state.</summary>
    public const int Phase2CorpseState = 0x783e;
    /// <summary>$7E:7862: health-based palette handling flag.</summary>
    public const int HealthBasedPaletteHandling = 0x7862;
    /// <summary>$7E:7840: brain main shake timer.</summary>
    public const int BrainMainShakeTimer = 0x7840;
    /// <summary>$7E:7842: rainbow beam palette animation index.</summary>
    public const int RainbowPaletteIndex = 0x7842;
    /// <summary>$7E:7844: enable-unpause-hook flag.</summary>
    public const int EnableUnpauseHook = 0x7844;
    /// <summary>$7E:784A: bomb counter.</summary>
    public const int BombCounter = 0x784a;
    /// <summary>$7E:7860/$7864/$7866/$7868/$786A: brain palette, drool and purple breath flags.</summary>
    public const int BrainPaletteHandling = 0x7860, DroolGeneration = 0x7864, DroolProjectileParameter = 0x7866,
        SmallPurpleBreathGeneration = 0x7868, SmallPurpleBreathActive = 0x786a;
    /// <summary>$7E:8000/$8002: brain instruction timer and list pointer.</summary>
    public const int BrainInstructionTimer = 0x8000, BrainInstructionList = 0x8002;
    /// <summary>$7E:8004: sprite tile transfer entry pointer.</summary>
    public const int SpriteTileTransferEntry = 0x8004;
    /// <summary>$7E:8006-$8013: death beam next X/Y sub-position and position, velocities, angle.</summary>
    public const int DeathBeamNext = 0x8006;
    /// <summary>$7E:8026: rainbow beam angular width.</summary>
    public const int RainbowBeamAngularWidth = 0x8026;
    /// <summary>$7E:802E: grey transition counter.</summary>
    public const int GreyTransitionCounter = 0x802e;
    /// <summary>$7E:8030/$8032: fake death explosion timer and index.</summary>
    public const int FakeDeathExplosionTimer = 0x8030, FakeDeathExplosionIndex = 0x8032;
    /// <summary>$7E:8040/$8042: lower and upper neck angles.</summary>
    public const int LowerNeckAngle = 0x8040, UpperNeckAngle = 0x8042;
    /// <summary>$7E:8044: five neck segments, each X, Y and distance.</summary>
    public const int NeckSegments = 0x8044;
    /// <summary>$7E:8062-$8069: neck movement enable, movement indices and angle delta.</summary>
    public const int NeckMovementEnabled = 0x8062, LowerNeckMovementIndex = 0x8064,
        UpperNeckMovementIndex = 0x8066, NeckAngleDelta = 0x8068;
}
