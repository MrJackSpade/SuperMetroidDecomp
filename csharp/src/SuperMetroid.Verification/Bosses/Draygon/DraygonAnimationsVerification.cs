using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
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
        // A copy of a segment's list continuing elsewhere (#2399: the whip the final whips count) is a repeat of its own.
        foreach (DraygonNativeListAddressesEntry copy in Copies)
            lists[copy.Program] = lists[copy.Program] with { Repeats = [.. lists[copy.Program].Repeats, ReadList(cartridge, copy.Program, copy.Entry, drawn)] };
        // An animation started through a counter preamble (#1938) starts at its counted list, which the shared comparison
        // takes for a body no animation starts in; it is compared below from the preamble's count.
        // The body's dying cry is no animation of its own (#2175): the body's dying runs on into it, and no owner starts it.
        // Nor are the runs that follow a counted run (#2296): the dying's paralysis and its looping burst, the eye's dying
        // stare and the tail's release of Samus, which the animations play on into.
        NativeEnemyAnimationOracle.CompareAnimations(lists,
            DraygonAnimations.StartingAt.Where(pair => !CounterPreambles.Any(preamble => ReferenceEquals(preamble.Animation, pair.Value)))
                .ToDictionary(pair => pair.Key, pair => pair.Value),
            onlyRunOnInto: new HashSet<DraygonSegment>
            {
                DraygonSegment.BodyDyingCry, DraygonSegment.BodyDyingParalysis, DraygonSegment.BodyDyingExplosionLoop,
                DraygonSegment.EyeFacingLeftDyingStare, DraygonSegment.EyeFacingRightDyingStare,
                DraygonSegment.TailFacingLeftReleaseSamus, DraygonSegment.TailFacingRightReleaseSamus,
            });
        foreach (NativeCountedEntry<DraygonSegment> preamble in CounterPreambles)
        {
            int address = (BodyBankNumber << 16) | preamble.Address;
            AssertEqual(NativeEnemyListDefinitions.WordKind.SetCounter, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address)),
                $"Draygon {preamble.Name}'s preamble loads the counter");
            ushort counted = (ushort)(preamble.Address + 4);
            DraygonSegment into = ProgramAt(counted) ?? throw new InvalidDataException($"Draygon {preamble.Name}'s preamble runs on into no segment's list.");
            int plays = cartridge.ReadCartridgeWord(address + 2);
            (List<AnimationPhase<DraygonSegment>> phases, AnimationEnd end) =
                NativeEnemyAnimationOracle.Linearize(lists, into, plays, counted);
            AssertTrue(preamble.Animation.Phases.SequenceEqual(phases), $"Draygon {preamble.Name}'s phases follow its preamble and native list");
            AssertEqual(end, preamble.Animation.End, $"Draygon {preamble.Name}'s end follows its native list");
        }
        int beatCount = lists.Values.Sum(list => list.BeatAddresses.Length);
        AssertEqual(DraygonFrameToolingDefinitionsConstants.FrameCount, drawn.Count, "the Draygon segments draw every catalog frame");
        for (int index = 0; index < DraygonFrameToolingDefinitionsConstants.FrameCount; index++)
            AssertTrue(drawn.Contains(DraygonFrameToolingDefinitions.RootAt(index)), $"Draygon catalog frame {index} is drawn by a native list");

        VerifyReset();
        VerifyGoopPassResetStartsTheArms();
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
        IReadOnlyList<AnimationBeat<EnemyFrameId, DraygonCue>> beats = DraygonAnimations.Segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<DraygonSegment>>();
        // The beat the reading stood before at each place it reached, the HUD-split callbacks it skips included (#2166).
        var beatAt = new Dictionary<ushort, int>();
        // The updates of the last frame beat the native list has shown so far (#2573): the idle eye shows each picture once
        // for its whole duration where the list shows it twice in a row, so a frame the list shows again straight after
        // adds its duration to the same beat.
        int shownOfLastFrame = 0;
        ushort cursor = entry;
        while (true)
        {
            int beat = addresses.Count;
            string at = $"Draygon {segment} beat {beat}";
            beatAt.TryAdd(cursor, beat);
            if (cursor != entry && ProgramAt(cursor) is { } next)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, next, null, cursor, TargetEntry: cursor));
                return Finish();
            }

            cursor = SkipHudSplitCallbacks(cartridge, cursor);
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    if (beat > 0 && beats[beat - 1] is AnimationBeat<EnemyFrameId, DraygonCue>.Frame showing && shownOfLastFrame < showing.Duration)
                    {
                        AssertTrue(DraygonFrameToolingDefinitionsConstants2.Spritemaps(showing.Shown).Contains(operand),
                            $"{at}: the list shows {showing.Shown} again for the rest of its beat");
                        shownOfLastFrame += word;
                        AssertTrue(shownOfLastFrame <= showing.Duration, $"{at}: the list's showings of {showing.Shown} fill its beat");
                        drawn.Add(operand);
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    }
                    var frame = AssertIs<AnimationBeat<EnemyFrameId, DraygonCue>.Frame>(Beat(beat, at), $"{at}: frame");
                    // A beat may show its picture over two consecutive showings of the list's (#2573).
                    AssertTrue(word == frame.Duration || (word < frame.Duration && segment is DraygonSegment.EyeFacingLeftIdle or DraygonSegment.EyeFacingRightIdle),
                        $"{at}: duration {word}, beat {frame.Duration}");
                    shownOfLastFrame = word;
                    // The shut jaw shows from two roots (#2268): the list's operand is one of the shown frame's.
                    AssertTrue(DraygonFrameToolingDefinitionsConstants2.Spritemaps(frame.Shown).Contains(operand), $"{at}: spritemap ${operand:X4} shows {frame.Shown}");
                    drawn.Add(operand);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    if (operand != entry && beatAt.TryGetValue(operand, out int loopBeat) && loopBeat > 0)
                    {
                        // A goto back to a beat its own list already showed (#2166) loops the list from there: the dying
                        // explosions' lead-in plays once before their loop.
                        controls.Insert(controls.FindLastIndex(control => control.AfterBeat <= loopBeat) + 1,
                            new(NativeEnemyControlKind.LoopStart, loopBeat, segment, null, operand));
                        controls.Add(new(NativeEnemyControlKind.Goto, beat, segment, null, cursor, loopBeat, entry));
                        return Finish();
                    }
                    controls.Add(new(NativeEnemyControlKind.Goto, beat,
                        ProgramAt(operand) ?? throw new InvalidDataException($"{at}: ${operand:X4} is no segment's native entry."), null, cursor, TargetEntry: operand));
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
                    controls.Add(new(NativeEnemyControlKind.Repeat, beat, segment, null, cursor, TargetEntry: operand));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Wait:
                    var pause = AssertIs<AnimationBeat<EnemyFrameId, DraygonCue>.Pause>(Beat(beat, at), $"{at}: wait");
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
                    var raised = AssertIs<AnimationBeat<EnemyFrameId, DraygonCue>.Event>(Beat(beat, at), $"{at}: callback");
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
            return new NativeEnemyList<DraygonSegment>([.. addresses], [.. controls], entry);
        }

        AnimationBeat<EnemyFrameId, DraygonCue> Beat(int beat, string at) => beat < beats.Count
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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.DraygonBody, body.KeptDefinition);
        body.Position = body.Position.WithWhole(new(256, 384));
        var state = new DraygonEnemyState(body.HeldOccupant(), enemies.Places.InUpdateOrder[1], enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3]);
        body.SetSpeciesState(state);
        state.Eye.Bind(EnemyDefinitionId.DraygonEye, state.Eye.KeptDefinition);
        state.Eye.SetSpeciesState(state.EyePart);
        state.Tail.Bind(EnemyDefinitionId.DraygonTail, state.Tail.KeptDefinition);
        state.Tail.SetSpeciesState(state.TailPart);
        state.Arms.Bind(EnemyDefinitionId.DraygonArms, state.Arms.KeptDefinition);
        state.Arms.SetSpeciesState(state.ArmsPart);
        foreach (DraygonPart part in new[] { state.BodyPart, state.EyePart, state.TailPart, state.ArmsPart })
            part.SetAnimation(DraygonSegment.Rest);
        enemies.VerificationServices().NextRandom = () => 4660;
        enemies.VerificationServices().ReadRandom = () => 4660;
        return new NewDraygonResult(enemies, state);
    }

    /// <summary>The animation starting at <paramref name="segment"/>, before its first play.</summary>
    private static AnimationPlayback<DraygonSegment> Started(DraygonSegment segment) =>
        new(DraygonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>Runs one update of <paramref name="part"/> through the production interpreter.</summary>
    private static void Pass(RoomEnemyPlace part) =>
        EnemyAnimationPlayer.AdvanceAnimation(part, samus: RoomEnemyFrameFixtures.StandaloneSamus(), level: RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, controllerInput: 0);

    /// <summary>
    /// The body's reset starts all four parts in the same update, gives the eye its tracking function and the
    /// HUD-draw interrupt, and keeps the body itself in the reset, whose own frame replaces its started program.
    /// </summary>
    private static void VerifyReset()
    {
        (_, DraygonEnemyState state) = NewDraygon();
        state.BodyPart.SetAnimation(DraygonSegment.BodyFacingRightReset);
        state.Body.AnimationClock.StepOnNextUpdate();
        Pass(state.Body);
        AssertEqual(Started(DraygonSegment.EyeFacingRightIdle), state.EyePart.Playback, "the reset starts the eye");
        AssertEqual(Started(DraygonSegment.TailFacingRightIdle), state.TailPart.Playback, "the reset starts the tail");
        AssertEqual(Started(DraygonSegment.ArmsFacingRightIdle), state.ArmsPart.Playback, "the reset starts the arms");
        foreach (RoomEnemyPlace part in new[] { state.Eye, state.Tail, state.Arms })
            AssertEqual(1, part.AnimationClock.Hold, "the reset's started parts run on their next update");
        AssertEqual(new AnimationPlayback<DraygonSegment>(DraygonAnimations.StartingWith(DraygonSegment.BodyFacingRightReset), new AnimationPlace(0, 3)),
            state.BodyPart.Playback, "the body's own frame keeps it in the reset, after its frame");
        AssertEqual(DraygonFrameToolingDefinitions.Spritemap(EnemyFrameId.DraygonBodyFacingRightJawShut), state.Body.SpritemapPointer, "the reset shows the right-facing body frame");
        AssertEqual<DraygonEyeAction?>(DraygonEyeAction.Tracking, state.EyeTracking.Action, "the reset gives the eye its tracking function");
        AssertEqual<HorizontalDirection?>(HorizontalDirection.Right, state.EyeTracking.Facing, "the reset's tracking eye faces right (#2165)");
    }

    /// <summary>
    /// #2221: a goop pass's setup leaves the arms' animation to the body's reset, which starts them facing the pass's way in
    /// the same update, before the arms' own update: the setup's set-up leaves the arms as they were, and the reset's start
    /// is the first animation they play.
    /// </summary>
    private static void VerifyGoopPassResetStartsTheArms()
    {
        foreach (HorizontalDirection direction in new[] { HorizontalDirection.Left, HorizontalDirection.Right })
        {
            (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
            enemies.DraygonBehavior.Goop.SetupDraygonGoopPass(state, direction, RoomEnemyFrameFixtures.StandaloneSamus(), frameCounter: 0);
            AssertEqual(Started(DraygonSegment.Rest), state.ArmsPart.Playback, $"a {direction} goop pass's setup leaves the arms' animation");
            Pass(state.Body);
            AssertEqual(Started(direction == HorizontalDirection.Right ? DraygonSegment.ArmsFacingRightIdle : DraygonSegment.ArmsFacingLeftIdle),
                state.ArmsPart.Playback, $"the {direction} goop pass's reset starts the arms before they play");
        }
    }

    /// <summary>
    /// The body's dying, played here on the arms, runs on into its dying explosions: the cry, the intangible body, eight counted
    /// explosion rounds with their waits, the paralysis of the tail, then the explosion loop, update by update beside
    /// the native list, including the plays the counted rounds have left, which the arms' timer word counts (#2006).
    /// </summary>
    private static void VerifyDying(IImportCartridgeSource cartridge)
    {
        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        RoomEnemyPlace arms = state.Arms;
        state.ArmsPart.SetAnimation(DraygonSegment.BodyFacingLeftDying);
        arms.AnimationClock.StepOnNextUpdate();
        var native = new NativeEnemyListReader(BodyBankNumber, Entry(DraygonSegment.BodyFacingLeftDying), 1, OperandWords);
        bool paralysed = false;
        for (int update = 0; update < 400; update++)
        {
            enemies.Audio.BeginFrame();
            Pass(arms);
            List<NativeEnemyListDefinitions.Callback> callbacks = native.Step(cartridge);
            string at = $"Draygon dying update {update}";
            SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertClockHold(native.Timer, arms.AnimationClock, at);
            AssertEqual(native.Spritemap, arms.SpritemapPointer, $"{at}: frame");
            AssertEqual(native.NewFrame, arms.AnimationClock.ShowsNewFrame, $"{at}: new-frame flag");
            if (NativeEnemyAnimationOracle.NativePlaysLeft(state.ArmsPart.Playback) is { } playsLeft)
                AssertEqual<int>(native.Counter, playsLeft, $"{at}: plays left");
            DraygonCue[] cues = [.. callbacks.Where(callback => !SelectsHudSplit(callback.Word)).Select(callback => CueOf(callback.Word, callback.Operands))];
            AssertEqual(EnemyAudioQueueAccess.SoundList(cues.OfType<DraygonCue.QueueSound>().Select(sound => sound.Sound)),
                enemies.Audio.QueuedSoundList, $"{at}: every sound the native list queues, in its order");
            if (cues.Any(cue => cue is DraygonCue.ParalyseTailAndArms))
            {
                paralysed = true;
                AssertEqual(Started(DraygonSegment.Rest), state.TailPart.Playback, $"{at}: the tail sleeps");
                AnimationPlayback<DraygonSegment> armsPlayback = state.ArmsPart.Playback;
                AnimationPhase<DraygonSegment> running = armsPlayback.Animation.Phases[armsPlayback.Place.Phase];
                AssertEqual((DraygonSegment.BodyDyingExplosionLoop, 0), (running.Segment, running.FirstBeat),
                    $"{at}: the running arms keep their own program, its explosions' loop, as the native final store replaces the paralysis");
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
        NativeEnemyAnimationOracle.CompareAnimations(projectileLists, DraygonProjectileAnimations.StartingAt,
            onlyRunOnInto: new HashSet<DraygonProjectileSegment> { DraygonProjectileSegment.WallTurretFlight });

        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        enemies.DraygonBehavior.Parts.SpawnDraygonGoop(state, HorizontalDirection.Right);
        EnemyProjectile goop = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.DraygonGoop);
        AssertTrue(goop.Animation is DraygonProjectileAnimation { PlayingSegment: DraygonProjectileSegment.Goop },
            "the real goop producer starts the typed goop loop");
        goop.StartAnimation(new DraygonProjectileAnimation(enemies.DraygonBehavior.Shots.Goop, DraygonProjectileSegment.GoopTouch));
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        enemies.Projectiles.AdvanceEnemyProjectileAnimations(goop, samus, Position16<RoomPixels>.Zero, shortCounter: SuperMetroid.Core.Compatibility.ShortFrameCount.Zero);
        AssertTrue(goop.IsMovingBy(enemies.DraygonBehavior.Shots.Goop.GoopStuckToSamus), "the goop touch sticks the goop to Samus");
        AssertEqual(EnemyProjectileProgramFrameSources.Operand(EnemyProjectileFrameId.DraygonGoopSpiky), goop.PresentationOperandAddress,
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
        IReadOnlyList<AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>> beats = DraygonProjectileAnimations.Segments.Beats(segment);
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
                var frame = AssertIs<AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>.Frame>(Beat(beat, at), $"{at}: frame");
                AssertEqual(word, frame.Duration, $"{at}: duration");
                // An operand that stores a pose's picture again shows that pose (#2268).
                AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyProjectileProgramFrameOperands.OwnOperand(unchecked((ushort)(cursor + 2))),
                    EnemyProjectileProgramFrameSources.Operand(frame.Shown), $"{at}: frame operand");
                addresses.Add(cursor);
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (IsProjectileGoTo(word))
            {
                if (operand != entry && addresses.IndexOf(operand) is var loopBeat and > 0)
                {
                    // A goto back to a beat its own list already showed (#2166) loops the list from there: the wall
                    // turret shot's bloom and firing play once before its flight loops.
                    controls.Insert(controls.FindLastIndex(control => control.AfterBeat <= loopBeat) + 1,
                        new(NativeEnemyControlKind.LoopStart, loopBeat, segment, null, operand));
                    controls.Add(new(NativeEnemyControlKind.Goto, beat, segment, null, cursor, loopBeat));
                    return Finish();
                }
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
            var raised = AssertIs<AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>.Event>(Beat(beat, at), $"{at}: callback");
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

        AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue> Beat(int beat, string at) => beat < beats.Count
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
