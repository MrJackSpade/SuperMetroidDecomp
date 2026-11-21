using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Compatibility;

/// <summary>Checks the Morph Ball eye's initial animations and how its eyelid and tracking animations run.</summary>
internal static class MorphBallEyeBehaviorVerification
{
    /// <summary>
    /// The real initializer starts a body's closed eyelid on its mounted side and each mount's directional pose; the
    /// eyelid closes over 8, 48 and 5 updates and then holds the closed pose; and a tracking eye (#2070) shows, on the same
    /// update its tracking reads Samus's angle, the aim frame of the sixteenth of the turn she lies in from it.
    /// </summary>
    internal static void VerifyMorphBallEyeBehavior()
    {
        foreach ((ushort parameter1, MorphBallEyeSegment closed) in new[]
                 {
                     new VerifyMorphBallEyeBehaviorCase(0, MorphBallEyeSegment.FacingRightClosed),
                     new VerifyMorphBallEyeBehaviorCase(1, MorphBallEyeSegment.FacingLeftClosed),
                 })
        {
            (_, _, MorphBallEyeEnemyState body) = Create(parameter1);
            AssertEqual(new AnimationPlayback<MorphBallEyeSegment>(MorphBallEyeAnimations.StartingWith(closed), AnimationPlace.StartOfAnimation), body.Playback, $"eye body parameter {parameter1} starts closed");
        }
        MorphBallEyeSegment[] mounts = [MorphBallEyeSegment.MountFacingLeft, MorphBallEyeSegment.MountFacingRight, MorphBallEyeSegment.MountFacingUp, MorphBallEyeSegment.MountFacingDown];
        for (ushort direction = 0; direction < mounts.Length; direction++)
        {
            RoomEnemyPlace mount = Place(parameter1: 0, parameter2: (ushort)(MorphBallEyeNativeValues.MountParameter | direction)).Place;
            AssertTrue(mount.Entity is MorphBallEyeMountEntity && mount.SpeciesState<MorphBallEyeEnemyState>() is null,
                $"eye mount direction {direction} is placed as a mount, which keeps only its own state (#2164)");
            AssertEqual(new AnimationPlayback<MorphBallEyeSegment>(MorphBallEyeAnimations.StartingWith(mounts[direction]), AnimationPlace.StartOfAnimation),
                mount.SpeciesState<MorphBallEyeMountState>()?.Playback, $"eye mount direction {direction}");
        }

        foreach (MorphBallEyeSegment closing in new[] { MorphBallEyeSegment.FacingRightDeactivating, MorphBallEyeSegment.FacingLeftDeactivating })
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, MorphBallEyeEnemyState state) = Create(parameter1: 0);
            state.Playback = new(MorphBallEyeAnimations.StartingWith(closing), AnimationPlace.StartOfAnimation);
            MorphBallEyeSegment closed = closing == MorphBallEyeSegment.FacingRightDeactivating ? MorphBallEyeSegment.FacingRightClosed : MorphBallEyeSegment.FacingLeftClosed;
            ushort shut = MorphBallEyeAnimationsConstants.Spritemaps(closed).Single();
            int[] holds = [8, 48, 5, 48];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. MorphBallEyeAnimationsConstants.Spritemaps(closing), shut], holds),
                EnemyInstructionRunner.ShownFrames(place, holds.Sum() + 1), $"eye {closing} closes and holds");
        }

        (RoomEnemySystem tracker, RoomEnemyPlace eye, MorphBallEyeEnemyState tracking) = Create(parameter1: 0);
        eye.Position = eye.Position.WithWhole(new(128, 128));
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        samus.CollectedItems = SamusEquipmentFlags.MorphBall;
        var frame = new ClockedEnemyAiFrame(RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Samus = samus }, new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
        IReadOnlyList<EnemyFrameId> aims = MorphBallEyeAnimations.AimFrames.InTurnOrder;
        for (int arc = 0; arc < aims.Count; arc++)
        {
            // Samus stands 48 pixels away along the middle of the arc, counted clockwise from straight up.
            double bearing = (arc + 0.5) * Math.Tau / aims.Count;
            samus.Position = FixedPosition<RoomPixels>.FromWhole(new(
                (ushort)(128 + (int)Math.Round(48 * Math.Sin(bearing))), (ushort)(128 - (int)Math.Round(48 * Math.Cos(bearing)))));
            tracking.Action = MorphBallEyeAction.Active;
            tracker.MorphBallEyeBehavior.RunMorphBallEyeMainAi(eye.HeldOccupant(), eye, frame);
            AssertEqual<EnemyFrameId?>(aims[arc], tracking.Aim, $"eye tracking Samus in arc {arc} aims along its pose");
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([MorphBallEyeFrameToolingDefinitions.Spritemap(aims[arc])], [1]),
                EnemyInstructionRunner.ShownFrames(eye, 1), $"eye tracking Samus in arc {arc} shows its pose on the same update");
        }
        Console.WriteLine("  Morph Ball eye behavior: initial eyelid and mount poses, both closings and the aim for every arc pass.");
    }

    /// <summary>
    /// The beam's half-width grows a quarter table unit faster each update, so its whole width reads 0, 0, 1, 2, 3 and
    /// then reaches four on the sixth update, which keeps the fraction and makes the beam full; a fading beam steps red
    /// and green one intensity per update toward their faded levels and is removed on the update after green arrives.
    /// </summary>
    internal static void VerifyMorphBallEyeBeamWideningAndFade()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        MorphBallEyeBeamState beam = enemies.MorphBallEyeBehavior.MorphBallEyeBeam;
        beam.Phase = MorphBallEyeBeamPhase.Widening;
        int[] widths = [0, 0, 1, 2, 3, 4];
        // #1684: every update against the 16.16 arithmetic the typed angles replace: the growth gains 16384/65536 of a
        // step, the width the growth, and the update whose whole width reaches four sets it to four, keeping its fraction.
        // A typed angle's 256ths of a step are the 16.16 value's 65536ths over 256: the 16.16 fraction's low byte stays zero.
        const int sixteenSixteenStep = 16384;
        int sixteenSixteenGrowth = 0;
        int sixteenSixteenWidth = 0;
        for (int update = 0; update < widths.Length; update++)
        {
            enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
            sixteenSixteenGrowth += sixteenSixteenStep;
            sixteenSixteenWidth += sixteenSixteenGrowth;
            if (sixteenSixteenWidth >> 16 >= 4)
                sixteenSixteenWidth = 4 << 16 | (sixteenSixteenWidth & 65535);
            AssertEqual(new TrigonometryAngle(widths[update]), beam.AngularWidth, $"eye beam whole width after widening update {update + 1}");
            AssertEqual(sixteenSixteenGrowth, beam.AngularHalfWidthGrowth.RawValue << 8, $"eye beam growth after widening update {update + 1} equals the 16.16 growth");
            AssertEqual(sixteenSixteenWidth, beam.AngularHalfWidth.RawValue << 8, $"eye beam half-width after widening update {update + 1} equals the 16.16 width");
            AssertEqual(update == widths.Length - 1 ? MorphBallEyeBeamPhase.Full : MorphBallEyeBeamPhase.Widening, beam.Phase,
                $"eye beam phase after widening update {update + 1}");
        }
        AssertEqual(RotationAngle.FromSteps(4, 64), beam.AngularHalfWidth, "a full beam keeps the fraction it overshot with");

        beam.Phase = MorphBallEyeBeamPhase.Deactivating;
        beam.Color = MorphBallEyeBeamDefinitions.InitialColor;
        int fadeUpdates = MorphBallEyeBeamDefinitions.InitialColor.FadeLevels.Green;
        for (int update = 1; update <= fadeUpdates; update++)
        {
            enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
            AssertEqual(Math.Max(MorphBallEyeBeamDefinitions.InitialColor.FadeLevels.Red - update, 0), global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeBeamState, global::SuperMetroid.Core.Assets.RgbColor>(beam, "Color").FadeLevels.Red,
                $"eye beam red intensity after fading update {update}");
            AssertEqual(MorphBallEyeBeamDefinitions.InitialColor.FadeLevels.Green - update, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeBeamState, global::SuperMetroid.Core.Assets.RgbColor>(beam, "Color").FadeLevels.Green, $"eye beam green intensity after fading update {update}");
            AssertEqual(0, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeBeamState, global::SuperMetroid.Core.Assets.RgbColor>(beam, "Color").FadeLevels.Blue, $"eye beam blue intensity after fading update {update}");
            AssertEqual(MorphBallEyeBeamPhase.Deactivating, beam.Phase, $"eye beam still fading after update {update}");
        }
        enemies.MorphBallEyeBehavior.StepMorphBallEyeBeam();
        AssertEqual(MorphBallEyeBeamPhase.Inactive, beam.Phase, "eye beam is removed on the update after green has faded");
        Console.WriteLine("  Morph Ball eye beam: six widening updates reach full width and a full fade removes the beam.");
    }

    /// <summary>
    /// #2342: the game's beam follows the eye listed right after the last beam mount placed, whichever eye opens (#1835);
    /// every room population that places the eye places one beam mount with one eye right after it, so that eye is the eye
    /// that opens, and the beam follows the eye that opened. Loading the Morph Ball room through the production loader and
    /// running its eye's opening requests the beam on that eye.
    /// </summary>
    internal static void VerifyMorphBallEyeBeamFollowsOpeningEye()
    {
        int rooms = 0;
        foreach (RoomEnemyPopulationDefinition population in RoomEnemyPopulationDefinitions.Definitions)
        {
            RoomEnemyPlacement[] placements = population.Placements.ToArray();
            int[] eyes = [.. Enumerable.Range(0, placements.Length).Where(index => placements[index].DefinitionId == EnemyDefinitionId.MorphBallEye)];
            if (eyes.Length == 0)
                continue;
            rooms++;
            AssertEqual(2, eyes.Length, $"{population.Name} places one beam mount and one eye");
            AssertEqual(eyes[0] + 1, eyes[1], $"{population.Name} lists its eye right after its beam mount");
            AssertTrue(placements[eyes[0]].SpawnSettings is MorphBallEyeMountSpawnSettings, $"{population.Name} lists its beam mount first");
            AssertTrue(placements[eyes[1]].SpawnSettings is MorphBallEyeBodySpawnSettings, $"{population.Name} lists its eye second");
        }
        AssertEqual(3, rooms, "the Morph Ball room, the Blue Brinstar energy tank room and the final missile room place the eye");

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => 1));
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(BrinstarRoomStateDefinitions.MorphBallDefault.EnemyPopulation, BgArtworkFixtures.For(new SnesVram()), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), RoomEnemyFrameFixtures.StandaloneSamus()));
        RoomEnemyPlace eye = enemies.Places.InUpdateOrder.Single(place => place.Entity is MorphBallEyeEntity);
        MorphBallEyeEnemyState state = eye.RequireSpeciesState<MorphBallEyeEnemyState>();
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        samus.CollectedItems = SamusEquipmentFlags.MorphBall;
        samus.Position = eye.Position;
        state.Action = MorphBallEyeAction.Activating;
        state.ActionTimer = UpdateCountdown.For(1);
        var frame = new ClockedEnemyAiFrame(RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Samus = samus }, new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
        enemies.MorphBallEyeBehavior.RunMorphBallEyeMainAi(eye.HeldOccupant(), eye, frame);
        AssertEqual(MorphBallEyeAction.Active, state.Action, "the eye has opened");
        AssertEqual(MorphBallEyeBeamPhase.PendingInitialization, enemies.MorphBallEyeBehavior.MorphBallEyeBeam.Phase, "the opened eye requests the beam");
        AssertTrue(ReferenceEquals(eye, enemies.MorphBallEyeBehavior.MorphBallEyeBeam.Body), "the beam follows the eye that opened");
        Console.WriteLine("  Morph Ball eye beam: every room places one mount and one eye, and the beam follows the eye that opened.");
    }

    /// <summary>An eye body after its real initializer.</summary>
    private static MorphBallEyeBehaviorCreateResult Create(ushort parameter1)
    {
        (RoomEnemySystem enemies, RoomEnemyPlace place) = Place(parameter1, parameter2: 0);
        AssertTrue(place.Entity is MorphBallEyeEntity && place.SpeciesState<MorphBallEyeMountState>() is null,
            "a placement whose part is the eye places the eye body, which keeps only its own state (#2164)");
        return new MorphBallEyeBehaviorCreateResult(enemies, place, place.SpeciesState<MorphBallEyeEnemyState>() ?? throw new InvalidOperationException("The eye has no state."));
    }

    /// <summary>
    /// An eye body or mount placed with the settings its parameter words decode to, recorded before its entity is bound as a
    /// room load records them, after its entity's real initializer.
    /// </summary>
    private static (RoomEnemySystem Enemies, RoomEnemyPlace Place) Place(ushort parameter1, ushort parameter2)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // A mount's beam follows the eye its room lists right after it (#1835).
        RoomEnemyPlace followed = enemies.Places.InUpdateOrder[1];
        followed.SetSpawnParameter1(EnemyDefinitionId.MorphBallEye, 0);
        followed.Bind(EnemyDefinitionId.MorphBallEye, followed.KeptDefinition);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.SetSpawnParameter1(EnemyDefinitionId.MorphBallEye, parameter1);
        place.SetSpawnParameter2(EnemyDefinitionId.MorphBallEye, parameter2);
        place.Bind(EnemyDefinitionId.MorphBallEye, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        ((IEnemyInitialization)(place.Entity ?? throw new InvalidOperationException("The eye bound no entity."))).Initialize();
        return (enemies, place);
    }

    /// <summary>Runs the Morph Ball eye behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--morph-ball-eye-instruction-program-definitions")]
    internal static int MorphBallEyeBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMorphBallEyeBehavior), () => VerifyMorphBallEyeBehavior());
        Suite(nameof(VerifyMorphBallEyeBeamWideningAndFade), () => VerifyMorphBallEyeBeamWideningAndFade());
        Suite(nameof(VerifyMorphBallEyeBeamFollowsOpeningEye), () => VerifyMorphBallEyeBeamFollowsOpeningEye());
        Suite(nameof(MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations), () => MorphBallEyeAnimationsVerification.VerifyMorphBallEyeAnimations(rom));
        return 0;
    }
}
