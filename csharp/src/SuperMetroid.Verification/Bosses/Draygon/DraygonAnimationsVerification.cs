using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// #1283, #1600: Draygon's typed body, eye, tail, arms and projectile animations against the retail cartridge. Each
/// segment must match its native bank-$A5 or bank-$86 list beat by beat and each animation the lists' control flow, and
/// the real runner must match them update by update: timers, frames, the loop counter, cues, part starts and deletion.
/// </summary>
internal static class DraygonAnimationsVerification
{
    /// <summary>
    /// Every part segment's beats equal its native list and every animation follows the lists' control flow; the runner
    /// then plays the reset that starts all four parts and the dying sequence, with its counted explosions, beside the
    /// native list.
    /// </summary>
    internal static void VerifyDraygonAnimations(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<DraygonSegment>().Length, Entries.Length, "every typed Draygon program has a native entry");
        for (int index = 0; index < Entries.Length; index++)
            AssertEqual((DraygonSegment)index, Entries[index].Program, "the Draygon programs are declared in authored order");
        AssertTrue(EntriesAscend, "native Draygon entries ascend");

        var drawn = new HashSet<ushort>();
        Dictionary<DraygonSegment, NativeEnemyList<DraygonSegment>> lists =
            Entries.ToDictionary(item => item.Program, item => ReadList(cartridge, item.Program, item.Entry, drawn));
        NativeEnemyAnimationOracle.CompareAnimations(lists, DraygonAnimations.StartingAt);
        int beatCount = lists.Values.Sum(list => list.BeatAddresses.Length);
        AssertEqual(DraygonFrameToolingDefinitions.FrameCount, drawn.Count, "the Draygon segments draw every catalog frame");
        for (int index = 0; index < DraygonFrameToolingDefinitions.FrameCount; index++)
            AssertTrue(drawn.Contains(DraygonFrameToolingDefinitions.Spritemap(DraygonFrameToolingDefinitions.CartridgeOrder[index])), $"Draygon catalog frame {index} is drawn by a native list");

        VerifyReset();
        VerifyDying(cartridge);
        Console.WriteLine($"  Draygon animations: {Entries.Length} segments and {beatCount} beats match bank $A5, and every animation its lists' control flow; the reset and dying sequence match the runner.");
    }

    /// <summary>
    /// Reads one segment's native list, comparing each of its beats with the segment's, and returns each beat's native
    /// address and the list's control instructions (sleeps, stops, gotos, counter loads, counted repeats and runs on into
    /// the next authored list), which <see cref="NativeEnemyAnimationOracle.CompareAnimations"/> matches with the animations.
    /// </summary>
    private static NativeEnemyList<DraygonSegment> ReadList(IImportCartridgeSource cartridge, DraygonSegment segment, ushort entry, HashSet<ushort> drawn)
    {
        IReadOnlyList<AnimationBeat<DraygonFrame, DraygonCue>> beats = DraygonAnimations.Segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<DraygonSegment>>();
        ushort cursor = entry;
        while (true)
        {
            int beat = addresses.Count;
            string at = $"Draygon {segment} beat {beat}";
            if (cursor != entry && ProgramAt(cursor) is { } next)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, next, null, cursor));
                return Finish();
            }

            cursor = SkipHudSplitCallbacks(cartridge, cursor);
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var frame = AssertIs<AnimationBeat<DraygonFrame, DraygonCue>.Frame>(Beat(beat, at), $"{at}: frame");
                    AssertEqual(word, frame.Duration, $"{at}: duration");
                    AssertEqual(operand, DraygonFrameToolingDefinitions.Spritemap(frame.Shown), $"{at}: spritemap");
                    drawn.Add(operand);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    controls.Add(new(NativeEnemyControlKind.Goto, beat,
                        ProgramAt(operand) ?? throw new InvalidDataException($"{at}: ${operand:X4} is no segment's native entry."), null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat, null, null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Stop:
                    controls.Add(new(NativeEnemyControlKind.Stop, beat, null, null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                    controls.Add(new(NativeEnemyControlKind.SetCounter, beat, null, operand, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                    AssertEqual<DraygonSegment?>(segment, ProgramAt(operand), $"{at}: a counted repeat replays its own list");
                    controls.Add(new(NativeEnemyControlKind.Repeat, beat, segment, null, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Wait:
                    var pause = AssertIs<AnimationBeat<DraygonFrame, DraygonCue>.Pause>(Beat(beat, at), $"{at}: wait");
                    AssertEqual(operand, pause.Duration, $"{at}: wait length");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    int count = OperandWords(word);
                    var operands = new ushort[count];
                    for (int operandIndex = 0; operandIndex < count; operandIndex++)
                        operands[operandIndex] = cartridge.ReadCartridgeWord(address + 2 + 2 * operandIndex);
                    var raised = AssertIs<AnimationBeat<DraygonFrame, DraygonCue>.Event>(Beat(beat, at), $"{at}: callback");
                    AssertEqual(CueOf(word, operands), raised.Raised, $"{at}: cue");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2 + 2 * count));
                    continue;
                }
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }

        NativeEnemyList<DraygonSegment> Finish()
        {
            AssertEqual(beats.Count, addresses.Count, $"Draygon {segment}: its native list holds each of its beats");
            return new NativeEnemyList<DraygonSegment>([.. addresses], [.. controls]);
        }

        AnimationBeat<DraygonFrame, DraygonCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Draygon {segment}.");
    }

    /// <summary>
    /// Steps past native callbacks that only select the screen split drawing the HUD; the display draws the HUD on
    /// its own, so the typed programs carry no step for them.
    /// </summary>
    private static ushort SkipHudSplitCallbacks(IImportCartridgeSource cartridge, ushort cursor)
    {
        while (SelectsHudSplit(cartridge.ReadCartridgeWord((BodyBankNumber << 16) | cursor)))
            cursor = unchecked((ushort)(cursor + 2));
        return cursor;
    }

    /// <summary>Creates Draygon's four linked parts in a fresh enemy system.</summary>
    private static NewDraygonResult NewDraygon()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.DraygonBody, body.Definition);
        body.Position = body.Position.WithWhole(new(256, 384));
        var state = new DraygonEnemyState(body)
        {
            Eye = enemies.SlotTable.Slots[1],
            Tail = enemies.SlotTable.Slots[2],
            Arms = enemies.SlotTable.Slots[3],
        };
        state.Eye.Bind(EnemyDefinitionId.DraygonEye, state.Eye.Definition);
        var eyeState = new DraygonEyeState();
        state.Eye.SetSpeciesState(eyeState);
        state.Tail.Bind(EnemyDefinitionId.DraygonTail, state.Tail.Definition);
        state.Arms.Bind(EnemyDefinitionId.DraygonArms, state.Arms.Definition);
        foreach (RoomEnemySlot part in new[] { body, state.Eye, state.Tail, state.Arms })
            state.StartAnimation(part, DraygonSegment.Sleep);
        enemies.DraygonBehavior.Parts.Draygon = state;
        enemies.VerificationServices().NextRandom = () => 4660;
        enemies.VerificationServices().ReadRandom = () => 4660;
        return new NewDraygonResult(enemies, state);
    }

    /// <summary>The animation starting at <paramref name="segment"/>, before its first play.</summary>
    private static AnimationPlayback<DraygonSegment> Started(DraygonSegment segment) =>
        new(DraygonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>Runs one update of <paramref name="part"/> through the production interpreter.</summary>
    private static void Pass(RoomEnemySlot part) =>
        EnemyAnimationPlayer.AdvanceAnimation(part, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);

    /// <summary>
    /// The body's reset starts all four parts in the same update, gives the eye its tracking function and the
    /// HUD-draw interrupt, and keeps the body itself in the reset, whose own frame replaces its started program.
    /// </summary>
    private static void VerifyReset()
    {
        (_, DraygonEnemyState state) = NewDraygon();
        state.StartAnimation(state.Body, DraygonSegment.BodyFacingRightReset);
        state.Body.AnimationClock.StepOnNextUpdate();
        Pass(state.Body);
        AssertEqual(Started(DraygonSegment.EyeFacingRightIdle), state.Playback(state.Eye), "the reset starts the eye");
        AssertEqual(Started(DraygonSegment.TailFacingRightIdle), state.Playback(state.Tail), "the reset starts the tail");
        AssertEqual(Started(DraygonSegment.ArmsFacingRightIdle), state.Playback(state.Arms), "the reset starts the arms");
        foreach (RoomEnemySlot part in new[] { state.Eye, state.Tail, state.Arms })
            AssertEqual((ushort)1, part.AnimationClock.Hold.RawValue, "the reset's started parts run on their next update");
        AssertEqual(new AnimationPlayback<DraygonSegment>(DraygonAnimations.StartingWith(DraygonSegment.BodyFacingRightReset), new AnimationPlace(0, 3)),
            state.Playback(state.Body), "the body's own frame keeps it in the reset, after its frame");
        AssertEqual(DraygonFrameToolingDefinitions.Spritemap(DraygonFrame.BodyFacingRightReset), state.Body.SpritemapPointer, "the reset shows the right-facing body frame");
        AssertEqual<DraygonEyeAction?>(DraygonEyeAction.FacingRight, state.Eye.SpeciesState<DraygonEyeState>()?.Action, "the reset gives the eye its tracking function");
    }

    /// <summary>
    /// The arms' dying program runs on into the body's dying explosions: the cry, the intangible body, eight counted
    /// explosion rounds with their waits, the paralysis of the tail, then the explosion loop, update by update beside
    /// the native list, including the loop counter in the arms' timer word.
    /// </summary>
    private static void VerifyDying(IImportCartridgeSource cartridge)
    {
        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        RoomEnemySlot arms = state.Arms;
        state.StartAnimation(arms, DraygonSegment.ArmsFacingLeftDying);
        arms.AnimationClock.StepOnNextUpdate();
        var native = new NativeEnemyListReader(BodyBankNumber, Entry(DraygonSegment.ArmsFacingLeftDying), 1, OperandWords);
        bool paralysed = false;
        for (int update = 0; update < 400; update++)
        {
            enemies.Audio.BeginFrame();
            Pass(arms);
            List<NativeEnemyListDefinitions.Callback> callbacks = native.Step(cartridge);
            string at = $"Draygon dying update {update}";
            AssertEqual(native.Timer, arms.AnimationClock.Hold.RawValue, $"{at}: timer");
            AssertEqual(native.Spritemap, arms.SpritemapPointer, $"{at}: frame");
            AssertEqual(native.NewFrame, arms.AnimationClock.ShowsNewFrame, $"{at}: new-frame flag");
            AssertEqual(native.Counter, arms.AnimationClock.Repeats.RawValue, $"{at}: loop counter");
            DraygonCue[] cues = [.. callbacks.Where(callback => !SelectsHudSplit(callback.Word)).Select(callback => CueOf(callback.Word, callback.Operands))];
            AssertEqual(EnemyAudioQueueAccess.SoundList(cues.OfType<DraygonCue.QueueSound>().Select(sound => sound.Sound)),
                enemies.Audio.QueuedSoundList, $"{at}: every sound the native list queues, in its order");
            if (cues.Any(cue => cue is DraygonCue.ParalyseTailAndArms))
            {
                paralysed = true;
                AssertEqual(Started(DraygonSegment.Sleep), state.Playback(state.Tail), $"{at}: the tail sleeps");
                AnimationPlayback<DraygonSegment> armsPlayback = state.Playback(arms);
                AssertEqual(DraygonSegment.BodyDyingExplosionsLoop, armsPlayback.Animation.Phases[armsPlayback.Place.Phase].Segment,
                    $"{at}: the running arms keep their own program, as the native final store replaces the paralysis");
            }
        }
        AssertTrue(paralysed, "the dying sequence reaches the paralysis");
        AssertTrue(state.Body.Contact.HasAny(EnemyContact.IgnoresSamus), "the dying sequence makes the body intangible");
    }

    /// <summary>
    /// Every projectile segment's beats equal its native bank-$86 list and every projectile animation follows the lists'
    /// control flow, and the goop touch sticks the goop to Samus and shows its first frame in the same update.
    /// </summary>
    internal static void VerifyDraygonProjectileAnimations(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<DraygonProjectileSegment>().Length, ProjectileEntries.Length, "every projectile segment has a native entry");
        Dictionary<DraygonProjectileSegment, NativeEnemyList<DraygonProjectileSegment>> projectileLists =
            ProjectileEntries.ToDictionary(item => item.Program, item => ReadProjectileList(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(projectileLists, DraygonProjectileAnimations.StartingAt);
        AssertEqual(DraygonProjectileVisualDefinitions.ProjectileFrameCount, DraygonProjectileVisualDefinitions.ProjectileFrames.Count,
            "the projectile catalog lists every frame");

        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        enemies.DraygonBehavior.Parts.SpawnDraygonGoop(state, movingRight: true);
        EnemyProjectile goop = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.DraygonGoop);
        AssertTrue(goop.Animation is DraygonProjectileAnimation { PlayingSegment: DraygonProjectileSegment.Goop },
            "the real goop producer starts the typed goop loop");
        goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileSegment.GoopTouch));
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        enemies.Projectiles.AdvanceEnemyProjectileAnimations(goop, samus, Position16<RoomPixels>.Zero, shortCounter: 0);
        AssertEqual(EnemyProjectileFrameBehavior.DraygonGoopStuckToSamus, goop.FrameBehavior, "the goop touch sticks the goop to Samus");
        AssertEqual(EnemyProjectileProgramFrameSources.Operand(DraygonProjectileVisualDefinitions.ShownProjectileFrames[new(DraygonProjectileFrameKind.Goop, 0)]), goop.PresentationOperandAddress,
            "the goop touch shows the goop's first frame in the same update");
        AssertEqual((ushort)10, goop.FrameHold.RawValue, "the goop's first frame lasts ten updates");
        Console.WriteLine("  Draygon projectile programs: goop, burst, touch, bloom and flight match bank $86; the touch sticks the goop at once.");
    }

    /// <summary>
    /// Reads one projectile segment's native bank-$86 list, comparing each of its beats with the segment's, and returns
    /// its beats' addresses and control instructions; a goto into the shared deletion list continues with its deletion,
    /// after which the animation holds.
    /// </summary>
    private static NativeEnemyList<DraygonProjectileSegment> ReadProjectileList(IImportCartridgeSource cartridge, DraygonProjectileSegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>> beats = DraygonProjectileAnimations.Segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<DraygonProjectileSegment>>();
        ushort cursor = entry;
        while (true)
        {
            int beat = addresses.Count;
            string at = $"Draygon projectile {segment} beat {beat}";
            if (cursor != entry && ProjectileProgramAt(cursor) is { } next)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, next, null, cursor));
                return Finish();
            }
            ushort word = cartridge.ReadCartridgeWord(ProjectileBank | cursor);
            ushort operand = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var frame = AssertIs<AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>.Frame>(Beat(beat, at), $"{at}: frame");
                AssertEqual(word, frame.Duration, $"{at}: duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), EnemyProjectileProgramFrameSources.Operand(DraygonProjectileVisualDefinitions.ShownProjectileFrames[frame.Shown]), $"{at}: frame operand");
                addresses.Add(cursor);
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (IsProjectileGoTo(word))
            {
                if (ProjectileProgramAt(operand) is { } target)
                {
                    controls.Add(new(NativeEnemyControlKind.Goto, beat, target, null, cursor));
                    return Finish();
                }
                // A goto into the shared deletion list continues with its delete instruction.
                cursor = operand;
                continue;
            }
            AssertTrue(ProjectileCallbacks.TryGetValue(word, out DraygonProjectileCue cue), $"{at}: native callback ${word:X4} is known");
            var raised = AssertIs<AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>.Event>(Beat(beat, at), $"{at}: callback");
            AssertEqual(cue, raised.Raised, $"{at}: cue");
            addresses.Add(cursor);
            cursor = unchecked((ushort)(cursor + 2));
            if (cue == DraygonProjectileCue.Delete)
            {
                // Nothing follows the deletion: the animation holds there.
                controls.Add(new(NativeEnemyControlKind.Sleep, beat + 1, null, null, cursor));
                return Finish();
            }
        }

        NativeEnemyList<DraygonProjectileSegment> Finish()
        {
            AssertEqual(beats.Count, addresses.Count, $"Draygon projectile {segment}: its native list holds each of its beats");
            return new NativeEnemyList<DraygonProjectileSegment>([.. addresses], [.. controls]);
        }

        AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Draygon projectile {segment}.");
    }

    /// <summary>Runs Draygon's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--draygon-instruction-mechanics")]
    internal static int DraygonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDraygonAnimations), () => VerifyDraygonAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Draygon's projectile program parity check against the repository cartridge.</summary>
    [VerificationCommand("--draygon-projectile-instruction-mechanics")]
    internal static int DraygonProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDraygonProjectileAnimations), () => VerifyDraygonProjectileAnimations(LoadRepositoryRom()));
        return 0;
    }
}
