using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.YappingMaws;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Yapping Maw's typed attack and cooldown programs.</summary>
internal static class YappingMawInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data for the Yapping Maw instruction check.</summary>
    internal static void VerifyYappingMawInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs all attack and cooldown entries, checking sound, held-Samus offsets, and initial facing selection.</summary>
    internal static void VerifyYappingMawInstructionProgramDefinitions(
        CartridgeImportAddressSpace rom)
    {
        for (int direction = 0; direction < 8; direction++)
        {
            YappingMawSegment entry = YappingMawBites.AttackForDirection(YappingMawAnimationDefinitions.Aims.At(direction));
            (RoomEnemySystem enemies, RoomEnemyPlace place, YappingMawEnemyState state) =
                NewYappingMawInstructionSystem(entry);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(YappingMawAnimations.Segments, entry, YappingMawFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(YappingMawAnimations.Segments, entry),
                    () => enemies.YappingMawBehavior.AdvanceYappingMawTypedAnimation(place, state)),
                $"Yapping Maw attack direction {direction} loop");
            AssertEqual<SoundEffectId?>(EnemySound.YappingMaw, enemies.Audio.LatestSound,
                $"Yapping Maw attack direction {direction} publishes its sound");
        }

        Cooldown[] cooldowns =
        [
            new Cooldown(YappingMawSegment.CooldownUpRight, YappingMawSegment.ChewingUp, 6, 8, -8, "up-right"),
            new Cooldown(YappingMawSegment.CooldownUp, YappingMawSegment.ChewingUp, 5, 0, -16, "up"),
            new Cooldown(YappingMawSegment.CooldownUpLeft, YappingMawSegment.ChewingUp, 6, -8, -8, "up-left"),
            new Cooldown(YappingMawSegment.CooldownDownRight, YappingMawSegment.ChewingDown, 6, 8, 8, "down-right"),
            new Cooldown(YappingMawSegment.CooldownDown, YappingMawSegment.ChewingDown, 5, 0, 16, "down"),
            new Cooldown(YappingMawSegment.CooldownDownLeft, YappingMawSegment.ChewingDown, 6, -8, 8, "down-left"),
        ];
        foreach ((YappingMawSegment entry, YappingMawSegment loop, int calls, short x, short y, string name) in cooldowns)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, YappingMawEnemyState state) =
                NewYappingMawInstructionSystem(entry);
            RunYappingMawInstructionFrames(enemies, place, state, count: 1);
            AssertEqual(x, state.HeldSamusOffset.X,
                $"Yapping Maw {name} callback X offset");
            AssertEqual(y, state.HeldSamusOffset.Y,
                $"Yapping Maw {name} callback Y offset");
            RunYappingMawInstructionFrames(enemies, place, state, count: calls - 1);
            AssertEqual(YappingMawAnimationsConstants.Spritemaps(loop).First(), place.SpritemapPointer,
                $"Yapping Maw {name} cooldown loops back to its first pose");
            AssertEqual<SoundEffectId?>(EnemySound.YappingMaw, enemies.Audio.LatestSound,
                $"Yapping Maw {name} cooldown publishes its sound");
        }

        Suite(nameof(VerifyYappingMawInitializerSelections), () => VerifyYappingMawInitializerSelections());
        Suite(nameof(VerifyYappingMawTargeting), () => VerifyYappingMawTargeting());
        Suite(nameof(YappingMawAnimationsVerification.VerifyYappingMawAnimations), () => YappingMawAnimationsVerification.VerifyYappingMawAnimations(rom));

        Console.WriteLine(
            "Yapping Maw: all eight attack loops, six cooldown programs with their held-Samus offsets and sounds, " +
            "and the initial facing selection pass.");
    }

    /// <summary>Checks native parameter-two facing selection and linked body projectile setup.</summary>
    private static void VerifyYappingMawInitializerSelections(
)
    {
        foreach ((ushort parameter2, YappingMawSegment expected, string name) in new[]
        {
            new VerifyYappingMawInitializerSelectionsCase(0, YappingMawSegment.AttackingDown, "alternate/down"),
            new VerifyYappingMawInitializerSelectionsCase(1, YappingMawSegment.AttackingUp, "ordinary/up"),
        })
        {
            (_, _, YappingMawEnemyState state) = PlaceYappingMaw(128, parameter2);
            AssertEqual(new AnimationPlayback<YappingMawSegment>(YappingMawAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation), state.Playback,
                $"Yapping Maw {name} initializer program");
            AssertEqual(4, state.Links.Count,
                $"Yapping Maw {name} initializer body links");
            AssertTrue(state.RootEffectActor.IsActive,
                $"Yapping Maw {name} initializer root sprite object");
        }
    }

    /// <summary>
    /// Drives the production main AI with Samus 300 pixels right of the root, a distance the targeting helper reports as
    /// that axis's magnitude at angle zero. The activation distance is compared strictly: 301 attacks, 300 does not. An
    /// attack caps the distance at 64, and the next update's extension setup halves it into the radius.
    /// The setup then turns the aim angle around the turn: the curl is a quarter turn minus the aim, the bend mirrors in
    /// the curl's second half-turn, and the eight sectors are centred on their directions, so an aim of 240 wraps to
    /// sector zero.
    /// </summary>
    private static void VerifyYappingMawTargeting()
    {
        foreach ((int activationDistance, bool attacks) in new[] { new VerifyYappingMawTargetingCase(301, true), new VerifyYappingMawTargetingCase(300, false) })
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, YappingMawEnemyState state) = NewInitializedYappingMaw(activationDistance);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
            samus.Position = samus.Position.WithWhole(new(128 + 300, 128));
            enemies.YappingMawBehavior.RunYappingMawMainAi(place.HeldOccupant().SpawnSettings<YappingMawSpawnSettings>(), place, state, Frame(samus));
            string context = $"Yapping Maw activation distance {activationDistance}";
            AssertEqual(attacks ? YappingMawAction.BeginExtension : YappingMawAction.WaitingForSamus, state.Action, $"{context} attack decision");
            AssertEqual(attacks ? YappingMawAttackGeometry.MaximumTargetDistance : 300, state.DistanceToSamus, $"{context} target distance");
            if (!attacks)
                continue;

            AssertEqual(TrigonometryAngle.QuarterTurn, state.AimAngle, $"{context} aims straight along X");
            enemies.YappingMawBehavior.RunYappingMawMainAi(place.HeldOccupant().SpawnSettings<YappingMawSpawnSettings>(), place, state, Frame(samus));
            AssertEqual(YappingMawAction.ExtendingOrRetracting, state.Action, $"{context} begins extending");
            AssertEqual(32, state.SegmentRadius, $"{context} radius is half the capped distance");
            AssertEqual(RotationAngle.QuarterTurn, state.CurlAngle, $"{context} curl is a quarter turn minus the aim");
        }

        foreach ((int aim, int curl, bool mirrored, int direction) in new[] { new YappingMawInstructionProgramDefinitionsVerifyYappingMawTargetingCase(240, 80, false, 0), new YappingMawInstructionProgramDefinitionsVerifyYappingMawTargetingCase(200, 120, false, 6), new YappingMawInstructionProgramDefinitionsVerifyYappingMawTargetingCase(150, 170, true, 5) })
        {
            (_, RoomEnemyPlace place, YappingMawEnemyState state) = NewInitializedYappingMaw(301);
            state.DistanceToSamus = YappingMawAttackGeometry.MaximumTargetDistance;
            state.AimAngle = AimAngleConstants.FromClockwiseSteps(aim);
            YappingMawBehavior.BeginYappingMawExtension(place, state);
            AssertEqual(RotationAngle.FromSteps(checked((byte)curl)), state.CurlAngle, $"Yapping Maw aim {aim} curl wraps around the turn");
            AssertEqual(mirrored, state.MirroredBend, $"Yapping Maw aim {aim} bend");
            AssertEqual(YappingMawAnimationDefinitions.Aims.At(direction), state.Aim, $"Yapping Maw aim {aim} sector");
        }

        // Every aim falls in the sector it lies nearest: the aim turned on half a sector, around the turn, in eighths.
        for (int aim = 0; aim <= byte.MaxValue; aim++)
            AssertEqual(YappingMawAnimationDefinitions.Aims.At(((aim + 16) & 255) / 32), YappingMawBehavior.AimOf(AimAngleConstants.FromClockwiseSteps(aim)), $"Yapping Maw aim {aim} sector");

        static ClockedEnemyAiFrame Frame(SamusState samus) => new(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
    }

    /// <summary>A Yapping Maw rooted at (128, 128) through the production initializer, with the given activation distance.</summary>
    internal static NewInitializedYappingMawResult NewInitializedYappingMaw(int activationDistance) =>
        PlaceYappingMaw(checked((ushort)activationDistance), 1);

    /// <summary>
    /// A Yapping Maw rooted at (128, 128) whose population entry's parameters are <paramref name="parameter1"/> and
    /// <paramref name="parameter2"/>, under the blank definition, set up as its room's population sets it up (#2554): its
    /// setup builds the state its main AI and animation run on.
    /// </summary>
    private static NewInitializedYappingMawResult PlaceYappingMaw(int parameter1, int parameter2)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace authored = enemies.Places.InUpdateOrder[0];
        authored.Bind(EnemyDefinitionId.YappingMaw, authored.KeptDefinition);
        authored.Bind(authored.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        authored.Position = authored.Position.WithWhole(new(128, 128));
        authored.SetSpawnParameter1(EnemyDefinitionId.YappingMaw, parameter1);
        authored.SetSpawnParameter2(EnemyDefinitionId.YappingMaw, parameter2);
        RoomEnemyPlace place = enemies.LoadAuthoredPopulation()[0];
        return new NewInitializedYappingMawResult(enemies, place, place.RequireSpeciesState<YappingMawEnemyState>());
    }

    /// <summary>
    /// A set-up Yapping Maw, its program then <paramref name="entry"/> from its start, which the Maw's own steps start
    /// (its attacks on reaching Samus, its cooldowns as an attack ends), stepping on its next update.
    /// </summary>
    private static NewInitializedYappingMawResult NewYappingMawInstructionSystem(
            YappingMawSegment entry)
    {
        (RoomEnemySystem enemies, RoomEnemyPlace place, YappingMawEnemyState state) = PlaceYappingMaw(128, 1);
        state.Playback = new(YappingMawAnimations.StartingWith(entry), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        return new NewInitializedYappingMawResult(enemies, place, state);
    }

    /// <summary>Forces each requested instruction call through the production enemy interpreter.</summary>
    private static void RunYappingMawInstructionFrames(RoomEnemySystem enemies,
        RoomEnemyPlace place,
        YappingMawEnemyState state,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            place.AnimationClock.StepOnNextUpdate();
            enemies.YappingMawBehavior.AdvanceYappingMawTypedAnimation(place, state);
        }
    }

    /// <summary>Runs the retail-backed Yapping Maw instruction migration command.</summary>
    [VerificationCommand("--yapping-maw-instruction-program-definitions")]
    internal static int YappingMawInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions());
        return 0;
    }
}
