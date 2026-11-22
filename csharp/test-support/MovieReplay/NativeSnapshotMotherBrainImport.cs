using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses;
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
        state.PhaseTimer.SetCartridgeCount(CoordinateAxis.AsSigned(W(MotherBrainMemory.BodyFunctionTimer)));
        ushort flash = W(MotherBrainMemory.RoomPaletteAnimationScript);
        // The recorded hold counts the updates the flash's image has shown; the image moves on once the count has reached
        // its two updates, so it has the updates up to its two and one more left (#2404).
        state.RoomFlash = flash == 0 ? null
            : MotherBrainRoomFlashAnimationsConstants.ShowingFor(
                MotherBrainRoomFlashImage.All[MotherBrainNativeListAddresses.RoomFlashRecords.Single(record => unchecked((ushort)record.Record) == flash).Frame],
                MotherBrainRoomFlashDefinitions.FlashImageTicks - W(MotherBrainMemory.RoomPaletteFrameHold) + 1);
        // The sub-function word is shared: outside the tube collapse it holds the death-explosion interval timer
        // (imported below), so only a collapse routine pointer decodes to a collapse function.
        if (MotherBrainNativeListAddresses.DecodeTubeCollapse(W(MotherBrainMemory.BrainSubFunction)) is { } collapse)
            state.TubeCollapse = collapse;
        state.TubeCollapseTimer.SetCartridgeCount(CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunctionTimer)));
        StepBurstsToRecordedPlace(state.Bursts, CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunctionTimer)));
        state.FakeDeathExplosionTimer.SetCartridgeCount(CoordinateAxis.AsSigned(W(MotherBrainMemory.FakeDeathExplosionTimer)));
        // The recorded place is the position the last explosion took, counted in the order the game stepped back through
        // them; the positions are listed in the order they burst, so the next is the one after it there.
        int lastExplosion = W(MotherBrainMemory.FakeDeathExplosionIndex);
        int explosions = MotherBrainTubeCollapseDefinitions.FakeDeathExplosionPositions.Count;
        state.NextFakeDeathExplosion = ListStep<Position16<RoomPixels>>.At(MotherBrainTubeCollapseDefinitions.FakeDeathExplosionPositions,
            (explosions - lastExplosion) % explosions);
        // The native palette words name rows: the neck draws in her brain's row, and her brain in it or, once her death
        // copied her brain's colours there, in the corpse's (#1512).
        int neckRow = SnesObjAttributeWordConstants.FromPaletteBits(W(MotherBrainMemory.NeckPaletteIndex)).PaletteIndex;
        int brainRow = SnesObjAttributeWordConstants.FromPaletteBits(W(MotherBrainMemory.BrainPaletteIndex)).PaletteIndex;
        if (neckRow != NativeBrainRow || brainRow is not (NativeBrainRow or NativeCorpseRow))
            throw new InvalidDataException($"Mother Brain's neck row {neckRow} and brain row {brainRow} are not rows her encounter draws in.");
        if (brainRow == NativeCorpseRow)
            state.Colours.DetachBrain();
        ushort brain = W(MotherBrainMemory.BrainAnimationScript);
        // Bit 15 clear means the brain list is not live. A pointer at a record showing the picture before it again stands
        // in the beat that shows both, after the updates its earlier records show (#2318).
        var shownAgain = new Dictionary<ushort, NativeFrameShownAgain>();
        Dictionary<MotherBrainHeadSegment, NativeEnemyList<MotherBrainHeadSegment>> head = MotherBrainNativeListAddresses.CompareHead(cartridge, shownAgain);
        NativeFrameShownAgain beat = shownAgain.TryGetValue(brain, out NativeFrameShownAgain again) ? again : new NativeFrameShownAgain(brain, 0);
        ImportBrainAnimation(state.Brain, unchecked((short)brain) >= 0 ? null : BrainPlaybackAt(head, beat.FirstRecord),
            beat.UpdatesBefore + W(MotherBrainMemory.BrainFrameHold));
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
        // A falling tube keeps its state to itself (#2554): the family import refuses a snapshot taken while one falls.
        state.BrainShake.ShakeFor(W(MotherBrainMemory.BrainMainShakeTimer));
        state.DeleteTurretsAndRinkas = W(MotherBrainMemory.DeleteTurretsAndRinkas) != 0;
        state.NextRainbowImage = ListStep<MotherBrainRainbowImage>.At(MotherBrainRainbowImage.All,
            DecodeNextRainbowFrame(W(MotherBrainMemory.RainbowPaletteIndex)));
        // What her head's main last drew is not imported: her head's next main, which runs before her next draw, decides it.
        // Each joint's distance is the neck's own; only where each lies is imported.
        for (int joint = 0; joint < state.Neck.Joints.Count; joint++)
        {
            int record = MotherBrainMemory.NeckSegments + joint * 6;
            state.Neck.Joints[joint].Position = new(W(record), W(record + 2));
        }
        state.LowerNeckAngle = RotationAngleConstants.FromRaw(W(MotherBrainMemory.LowerNeckAngle));
        state.UpperNeckAngle = RotationAngleConstants.FromRaw(W(MotherBrainMemory.UpperNeckAngle));
        state.NeckTurnPerUpdate = RotationAngleConstants.FromRaw(W(MotherBrainMemory.NeckAngleDelta));
        state.NeckMovementEnabled = W(MotherBrainMemory.NeckMovementEnabled) != 0;
        state.LowerNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.LowerNeckMovementIndex), "Mother Brain lower neck movement index");
        state.UpperNeckMovement = MotherBrainNativeWords.NeckMovements.Decode(
            W(MotherBrainMemory.UpperNeckMovementIndex), "Mother Brain upper neck movement index");
        // While she waits before her pause the cartridge counts the wait by the leg-loading record it has reached, its body
        // timer already holding the pause; the port's phase timer holds the wait and starts the pause as it runs out (#2469).
        if (state.Stage == MotherBrainBodyStage.FakeDeathAscent && state.AscentPhase == MotherBrainAscentPhase.WaitBeforePause)
            state.PhaseTimer.SetCartridgeCount(
                MotherBrainWaitDefinitions.BeforePauseUpdates - 1 - DecodeLegTilePage(W(MotherBrainMemory.SpriteTileTransferEntry)));
        // The cartridge counts every fade but her fake death's greying on one word; each fade plays on its own (#2251,
        // #2404). The one her phase runs resumes from the recorded count and phase timer; every other starts over when its
        // phase starts it.
        if (RunningFade(state) is { } running)
            ResumeFade(running, W(MotherBrainMemory.GreyTransitionCounter), state.PhaseTimer.CartridgeCount);
        state.DroolGenerationEnabled = W(MotherBrainMemory.DroolGeneration) != 0;
        // The recorded place is the mouth point the last drool took; the points are listed from the one a fresh encounter's
        // first drool takes, so the next point sits at the recorded place there.
        state.NextDroolMouthPoint = ListStep<Offset16<RoomPixels>>.At(MotherBrainFightDefinitions.DroolMouthOffsets,
            W(MotherBrainMemory.DroolProjectileParameter));
        state.SmallPurpleBreathGenerationEnabled = W(MotherBrainMemory.SmallPurpleBreathGeneration) != 0;
        state.SmallPurpleBreathActive = W(MotherBrainMemory.SmallPurpleBreathActive) != 0;
        state.WalkCounter = SuperMetroid.Core.Compatibility.MotherBrainWalkCreditConstants.FromNativeWord(W(MotherBrainMemory.WalkCounter));
        state.AttackPhase = MotherBrainNativeWords.AttackPhases.Decode(W(MotherBrainMemory.AttackPhase), "Mother Brain attack phase");
        // The cooldown's word holds the updates it lasts.
        state.AttackCooldown.SetCartridgeCount(W(MotherBrainMemory.BodyAttackCooldown) - 1);
        // Her bomb count is the bombs counted at their spawn and not yet deleted (#2553); the import places none of her
        // bombs, so it imports only a snapshot whose native count is zero, and refuses any other.
        int nativeBombs = W(MotherBrainMemory.BombCounter);
        if (nativeBombs != state.BombCounter)
            throw new InvalidDataException(
                $"The native snapshot counts {nativeBombs} live Mother Brain bombs, but the import places none of her bombs.");
        state.BodyTargetXPosition = W(MotherBrainMemory.BodyFunctionTimer);
        state.HandBeamPhase = MotherBrainNativeWords.HandBeamPhases.Decode(W(MotherBrainMemory.DeathBeamAttackPhase), "Mother Brain hand-beam phase");
        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromStoredParts(
            W(MotherBrainMemory.DeathBeamNext + 2), W(MotherBrainMemory.DeathBeamNext),
            W(MotherBrainMemory.DeathBeamNext + 6), W(MotherBrainMemory.DeathBeamNext + 4));
        state.HandBeamNextVelocity = new PixelVelocity<RoomPixels>(
            new SignedEightEight(W(MotherBrainMemory.DeathBeamNext + 8)),
            new SignedEightEight(W(MotherBrainMemory.DeathBeamNext + 10)));
        // The game reads only the low byte of each aim angle, the byte angle it stored.
        state.HandBeamNextAngle = new TrigonometryAngle(W(MotherBrainMemory.DeathBeamNext + 12) & byte.MaxValue);
        state.OnionRingsTargetAngle = new TrigonometryAngle(W(MotherBrainMemory.OnionRingsTargetAngle) & byte.MaxValue);
        if (state.RainbowBeamSequence is { } lateFight)
            ImportNativeMotherBrainRainbowSequence(lateFight, W);
    }

    /// <summary>The fade her recorded phase runs, which the cartridge's fade count and phase timer describe; null in any other phase.</summary>
    private static MotherBrainFadeProgress? RunningFade(MotherBrainEnemyState state)
    {
        if (state.Stage == MotherBrainBodyStage.FakeDeathAscent && state.AscentPhase == MotherBrainAscentPhase.TransitionFromGray)
            return state.FromFakeDeathGrey;
        if (state.RainbowBeamSequence is { State: { Stage: MotherBrainLateFightStage.DrainedByBaby, DrainedPhase: MotherBrainDrainedPhase.TransitionToGrey } })
            return state.DrainedGreying;
        if (state.RainbowBeamSequence is { State: { Stage: MotherBrainLateFightStage.Reviving, RevivalPhase: MotherBrainRevivalPhase.TransitionFromGrey } })
            return state.RevivalFromGrey;
        if (state.RainbowBeamSequence is { State: { Stage: MotherBrainLateFightStage.Dying, DeathPhase: MotherBrainDeathPhase.FadeOutBody } })
            return state.DeathBodyFade;
        if (state.RainbowBeamSequence is { State: { Stage: MotherBrainLateFightStage.Dying, DeathPhase: MotherBrainDeathPhase.FadeToGrey } })
            return state.CorpseGreying;
        return null;
    }

    /// <summary>
    /// Resumes <paramref name="fade"/> where the cartridge's count of its steps, <paramref name="steps"/>, and its phase
    /// timer, <paramref name="timer"/>, leave it: the cartridge steps the fade as the timer runs below zero, so the next step
    /// comes the timer's count and one more updates on, and at once once it has run out. Before its first step the fade
    /// waits that long for its first image; after it, the image last shown holds that long. The fade's own start and steps
    /// bring it there (#2471): started to show its first image on its first step, it shows each image after the one before
    /// has held its updates, and the image last shown has held all but the updates it has left.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The count is past the fade's last image, which ends its phase, or the hold left is longer than an image's.
    /// </exception>
    private static void ResumeFade(MotherBrainFadeProgress fade, int steps, int timer)
    {
        int updates = Math.Max(timer + 1, 1);
        if (steps == 0)
        {
            fade.StartAfter(updates);
            return;
        }
        if (steps > fade.Fade.Images.Count)
            throw new InvalidDataException($"Mother Brain's recorded fade count {steps} is past the last of the {fade.Fade.Images.Count} images of {fade.Fade}, whose phase has ended.");
        if (updates > fade.Fade.ImageUpdates)
            throw new InvalidDataException($"Mother Brain's recorded fade hold of {updates} updates is longer than the {fade.Fade.ImageUpdates} each image of {fade.Fade} holds.");
        fade.StartAfter(1);
        int stepsToResume = 1 + (steps - 1) * fade.Fade.ImageUpdates + fade.Fade.ImageUpdates - updates;
        for (int step = 0; step < stepsToResume; step++)
            _ = fade.Step();
    }

    /// <summary>
    /// Imports the brain animation from the playback resting on the beat the recorded pointer stands at and the recorded
    /// count of draws the frame there has shown (#2317), bringing her brain there as the game does: its animation started,
    /// then played on draw by draw. A frame shows on with the updates its count leaves it, the playback past it; a held
    /// frame shows for good; a cue, which only a start leaves the pointer on, plays at the next draw.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The recorded brain list is unset, which only a brain her head's initialization has not started yet has, and the room
    /// load initialized her head; or a recorded cue stands anywhere but at the start of its animation.
    /// </exception>
    private static void ImportBrainAnimation(MotherBrainBrainAnimation brain, AnimationPlayback<MotherBrainHeadSegment>? resting, int drawsShown)
    {
        if (resting is not { } playback)
            throw new InvalidDataException("The recorded brain list is unset, as only a Mother Brain whose head has not been initialized has it; the room load initialized hers.");
        AnimationPhase<MotherBrainHeadSegment> phase = playback.Animation.Phases[playback.Place.Phase];
        switch (MotherBrainAnimations.HeadSegments.Beats(phase.Segment)[phase.FirstBeat + playback.Place.Beat])
        {
            case AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame frame:
                PlayBrainTo(brain, playback.Animation, new AnimationPlace(playback.Place.Phase, playback.Place.Beat + 1).WithPlaysLeft(playback.Place.PlaysLeft));
                // The frame moves on at the draw its count passes its list's duration: its updates less the draws shown.
                brain.Clock.HoldFor(frame.Duration - drawsShown + 1);
                return;
            case AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.HeldFrame:
                PlayBrainTo(brain, playback.Animation, AnimationPlace.HoldingIn(playback.Place.Phase, phase.BeatCount));
                return;
            default:
                // A start leaves the pointer on its opening cue, and shows the picture after that cue until the cue plays.
                if (playback.Place is not { Phase: 0, Beat: 0 })
                    throw new InvalidDataException($"The recorded brain list stands on a cue at {playback.Place}, which only the start of an animation leaves it on.");
                brain.Start(playback.Animation);
                return;
        }
    }

    /// <summary>
    /// Starts her brain on <paramref name="animation"/> as the game starts it and plays it on, draw by draw with each draw
    /// moving it to its next picture, until it stands at <paramref name="target"/>, just past the picture it shows there. The
    /// importer carries out none of the cues the brain passes on the way: they belong to the updates before the snapshot.
    /// </summary>
    /// <exception cref="InvalidDataException">Playing the animation from its start never brings it to <paramref name="target"/>.</exception>
    private static void PlayBrainTo(MotherBrainBrainAnimation brain, Animation<MotherBrainHeadSegment> animation, AnimationPlace target)
    {
        int steps = PicturesTo(animation, target);
        brain.Start(animation);
        int shownAtStart = MotherBrainBrainCadence.ShowsOpeningPictureAtStart(MotherBrainAnimations.HeadSegments.OpensWithFrame(animation)) ? 1 : 0;
        for (int step = shownAtStart; step < steps; step++)
        {
            brain.Clock.StepOnNextUpdate();
            _ = brain.AdvanceAtDraw(timeIsFrozen: false, IgnoredCue);
        }
    }

    /// <summary>
    /// How many pictures <paramref name="animation"/> shows from its start until it stands at <paramref name="target"/>,
    /// played as the brain plays it with none of its cues carried out.
    /// </summary>
    /// <exception cref="InvalidDataException">The animation comes back to a place it stood at, or holds, before it reaches <paramref name="target"/>.</exception>
    private static int PicturesTo(Animation<MotherBrainHeadSegment> animation, AnimationPlace target)
    {
        var passed = new HashSet<AnimationPlace>();
        AnimationPlace place = AnimationPlace.StartOfAnimation;
        for (int pictures = 1; ; pictures++)
        {
            _ = MotherBrainAnimations.HeadSegments.PlayToNextFrame(ref animation, ref place, IgnoredCue);
            if (place == target)
                return pictures;
            if (!passed.Add(place))
                throw new InvalidDataException($"Mother Brain's brain animation {animation} never stands at {target} played from its start.");
        }
    }

    /// <summary>The importer's reaction to a brain cue it plays past: none, the animation playing straight on.</summary>
    private static AnimationContinuation<MotherBrainHeadSegment>? IgnoredCue(MotherBrainHeadCue cue) => null;

    /// <summary>
    /// The brain playback the recorded brain list pointer <paramref name="pointer"/> stands for. The native pointer rests
    /// on the beat it plays next, so a pointer at a list's entry rests on that list's first beat, entered.
    /// </summary>
    private static AnimationPlayback<MotherBrainHeadSegment> BrainPlaybackAt(IReadOnlyDictionary<MotherBrainHeadSegment, NativeEnemyList<MotherBrainHeadSegment>> head, ushort pointer)
    {
        AnimationPlayback<MotherBrainHeadSegment> playback = NativeEnemyAnimationOracle.PlaybackAt(head,
            MotherBrainNativeListAddresses.HeadEntries, MotherBrainAnimations.HeadStartingAt, pointer);
        return playback.Place.HasNotEntered ? playback with { Place = AnimationPlace.StartOf(playback.Place.Phase) } : playback;
    }

    /// <summary>
    /// Rebuilds the rest of the rainbow-beam sequence that runs phase two from the beam onward, which the recorded body
    /// phase began in its stage. It runs over the imported encounter, whose neck and flags were imported above (#2209), so
    /// only what the late fight alone keeps is imported here.
    /// </summary>
    private static void ImportNativeMotherBrainRainbowSequence(MotherBrainRainbowBeamAttackSequence sequence, Func<int, ushort> W)
    {
        // The beam's width and its sound attempts are read only while its beam holds Samus, and the beam's start sets both
        // (#2553); the import places no beam, so it refuses a snapshot taken while one holds her and imports neither.
        if (sequence.RainbowAttack.BeamActive)
            throw new InvalidDataException("The native snapshot holds Mother Brain's rainbow beam on Samus, which the import does not place.");
        // The cartridge keeps the brain's fall speed in the body's phase timer.
        sequence.State.BrainFallSpeed = new SignedEightEight(W(MotherBrainMemory.BodyFunctionTimer));
        sequence.State.RainbowBeamSoundPlaying = W(MotherBrainMemory.RainbowBeamSoundPlaying) != 0;
        sequence.State.HealthColoursEnabled = W(MotherBrainMemory.HealthBasedPaletteHandling) != 0;
        sequence.State.Phase2CorpseState = DecodePhase2CorpseState(W(MotherBrainMemory.Phase2CorpseState));
        sequence.Revival.BabyMetroidAttackCounter = W(MotherBrainMemory.BabyMetroidAttackCounter);
        // Once the sequence runs, the sub-function word holds the bursts' interval; their place was imported with the
        // encounter, whose cycle the sequence shares.
        // The cartridge's interval runs below zero on the update a burst is due: a count at zero or below is due next.
        StartBurstInterval(sequence.Encounter.Bursts, Math.Max(0, CoordinateAxis.AsSigned(W(MotherBrainMemory.BrainSubFunction))) + 1);
    }

    /// <summary>
    /// Brings the encounter's burst cycle, fresh from the room's load, to the place the snapshot records the last burst took
    /// (#2553), as her ascent's dust bursts brought it there: each burst steps the cycle back one place from the last,
    /// continuing from the list's last place once it passes the first, so a fresh cycle stands at the recorded place after
    /// as many bursts as that place lies short of the list's length.
    /// </summary>
    /// <exception cref="InvalidDataException">The recorded place lies outside her ascent's dust positions.</exception>
    private static void StepBurstsToRecordedPlace(SuperMetroid.Core.Compatibility.MotherBrainBurstCycle bursts, int place)
    {
        IReadOnlyList<Position16<RoomPixels>> dust = MotherBrainAscentDustDefinitions.DustPositions;
        if (place < 0 || place >= dust.Count)
            throw new InvalidDataException($"The native snapshot's burst place {place} lies outside Mother Brain's {dust.Count} ascent dust positions.");
        for (int burst = 0; burst < (dust.Count - place) % dust.Count; burst++)
            _ = bursts.NextOf(dust);
    }

    /// <summary>
    /// Brings the encounter's burst cycle, due at once as the room's load leaves it, to a wait of <paramref name="updates"/>
    /// before its next burst (#2553), as a burst due now restarts the wait with its interval and the update it falls due on.
    /// </summary>
    private static void StartBurstInterval(SuperMetroid.Core.Compatibility.MotherBrainBurstCycle bursts, int updates)
    {
        if (updates == 1)
            return;
        if (!bursts.IntervalRunsOut(updates - 1))
            throw new InvalidDataException("Mother Brain's burst cycle is not due at once as the room's load leaves it.");
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
        MotherBrainDeathPhase.ShowsEscapeSubtitle,
        MotherBrainDeathPhase.TypeOutZebesEscapeText,
        MotherBrainDeathPhase.EscapeDoorBursts,
        MotherBrainDeathPhase.BlowUpEscapeDoor,
        MotherBrainDeathPhase.KeepEarthquakeGoing,
    ];

    /// <summary>
    /// Where her body is placed at the imported form and body phase of <paramref name="state"/> (#1949, #2031): unplaced
    /// until her rise places it, with her from then on, and at the room's corner once her escape's start has shown the
    /// warning there, as the port's rise and escape start place it.
    /// </summary>
    internal static MotherBrainBodyPlacement BodyPlacementOf(MotherBrainEnemyState state) =>
        state.RainbowBeamSequence is { State: { Stage: MotherBrainLateFightStage.Dying, DeathPhase: var dying } } && EscapeWarningPhases.Contains(dying)
            ? MotherBrainBodyPlacement.AtRoomCorner
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
        // The installation's frames, which the runtime's installed Mother Brain body sheet is composed from.
        BossSourceArtwork bosses = InstalledFixtures.BossArtwork;
        BossBodySheet sheet = runtime.BossArtwork.Body(BossBodyOwner.MotherBrain);
        var image = new BossBodyParts(BossBodyOwner.MotherBrain);
        BossBodyFrameSource? staged = BossBodySheetComposer.Frames(bosses, BossBodyOwner.MotherBrain).FirstOrDefault(frame =>
            frame.Cells.All(entry => entry.Key < screen.Length && screen[entry.Key] == entry.Value.PackedWord));
        if (staged is not null)
            image.Show(state.Body.HeldOccupant().DefinitionId, sheet.Frame(staged.Name));
        if (!BossBodyWords.Of(image.Publish(), BossBodyOwner.MotherBrain, bosses).AsSpan(0, screen.Length).SequenceEqual(screen))
            throw new InvalidDataException("The snapshot's staged Mother Brain body is not one installed body frame over her blank.");
        runtime.Enemies.Presentation.AttachBossBody(UnplacedBossBody.Showing(image));
    }

    /// <summary>The WRAM offset of the enemy BG2 staging image, <c>$7E:2000</c>.</summary>
    private const int EnemyBg2StagingWram = 0x2000;
}
