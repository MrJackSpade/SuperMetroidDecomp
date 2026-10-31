using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
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
        SuperMetroidAddressSpace rom)
    {
        for (int direction = 0; direction < 8; direction++)
        {
            YappingMawProgram entry = YappingMawBites.AttackForDirection(YappingMawAnimationDefinitions.Aims.At(direction));
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                NewYappingMawInstructionSystem(entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(YappingMawAnimations.Programs, entry, YappingMawFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(YappingMawAnimations.Programs, entry)),
                $"Yapping Maw attack direction {direction} loop");
            AssertEqual<SoundEffectId?>(Library2Sound.YappingMaw, enemies.Audio.PublishedSoundFrom(EnemySoundSource.YappingMaw),
                $"Yapping Maw attack direction {direction} publishes its sound");
        }

        Cooldown[] cooldowns =
        [
            new Cooldown(YappingMawProgram.CooldownUpRight, YappingMawProgram.CooldownUpLoop, 6, 8, -8, "up-right"),
            new Cooldown(YappingMawProgram.CooldownUp, YappingMawProgram.CooldownUpLoop, 5, 0, -16, "up"),
            new Cooldown(YappingMawProgram.CooldownUpLeft, YappingMawProgram.CooldownUpLeftLoop, 6, -8, -8, "up-left"),
            new Cooldown(YappingMawProgram.CooldownDownRight, YappingMawProgram.CooldownDownLoop, 6, 8, 8, "down-right"),
            new Cooldown(YappingMawProgram.CooldownDown, YappingMawProgram.CooldownDownLoop, 5, 0, 16, "down"),
            new Cooldown(YappingMawProgram.CooldownDownLeft, YappingMawProgram.CooldownDownLeftLoop, 6, -8, 8, "down-left"),
        ];
        foreach ((YappingMawProgram entry, YappingMawProgram loop, int calls, short x, short y, string name) in cooldowns)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) =
                NewYappingMawInstructionSystem(entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            RunYappingMawInstructionFrames(arguments, slot, count: 1);
            AssertEqual(x, state.HeldSamusOffset.X,
                $"Yapping Maw {name} callback X offset");
            AssertEqual(y, state.HeldSamusOffset.Y,
                $"Yapping Maw {name} callback Y offset");
            RunYappingMawInstructionFrames(arguments,
                slot,
                count: calls - 1);
            AssertEqual(YappingMawAnimationsConstants.Spritemaps(loop).First(), slot.SpritemapPointer,
                $"Yapping Maw {name} cooldown loops back to its first pose");
            AssertEqual<SoundEffectId?>(Library2Sound.YappingMaw, enemies.Audio.PublishedSoundFrom(EnemySoundSource.YappingMaw),
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
        foreach ((ushort parameter2, YappingMawProgram expected, string name) in new[]
        {
            new VerifyYappingMawInitializerSelectionsCase(0, YappingMawProgram.AttackingDown, "alternate/down"),
            new VerifyYappingMawInitializerSelectionsCase(1, YappingMawProgram.AttackingUp, "ordinary/up"),
        })
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.YappingMaw, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.Position = slot.Position.WithWhole(new(128, 128));
            slot.SetSpawnParameter1(EnemyDefinitionId.YappingMaw, 128);
            slot.SetSpawnParameter2(EnemyDefinitionId.YappingMaw, parameter2);
            enemies.YappingMawBehavior.InitializeYappingMaw(slot);
            YappingMawEnemyState state = enemies.YappingMawStates[0] ??
                throw new InvalidOperationException(
                    $"Yapping Maw {name} initializer did not publish typed state.");
            AssertEqual(AnimationProgramCursor<YappingMawProgram>.Start(expected), state.Program,
                $"Yapping Maw {name} initializer program");
            AssertEqual(4, state.BodyProjectiles.Count(projectile => projectile is not null),
                $"Yapping Maw {name} initializer body links");
            AssertTrue(state.RootSpriteObject is not null,
                $"Yapping Maw {name} initializer root sprite object");
        }
    }

    /// <summary>
    /// Drives the production main AI with Samus 300 pixels right of the root, a distance the targeting helper reports as
    /// that axis's magnitude at angle zero. The activation distance is compared by the sign of the sixteen-bit
    /// difference: 301 attacks, 300 does not (strict), and 40000 does not either, because the wrapped difference is
    /// positive. An attack caps the distance at 64, and the next update's extension setup halves it into the radius.
    /// The setup then turns the aim angle around the turn: the curl is a quarter turn minus the aim, the bend mirrors in
    /// the curl's second half-turn, and the eight sectors are centred on their directions, so an aim of 240 wraps to
    /// sector zero.
    /// </summary>
    private static void VerifyYappingMawTargeting()
    {
        foreach ((int activationDistance, bool attacks) in new[] { new VerifyYappingMawTargetingCase(301, true), new VerifyYappingMawTargetingCase(300, false), new VerifyYappingMawTargetingCase(40000, false) })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) = NewInitializedYappingMaw(activationDistance);
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence());
            samus.Position = samus.Position.WithWhole(new(128 + 300, 128));
            enemies.YappingMawBehavior.RunYappingMawMainAi(slot, Frame(samus));
            string context = $"Yapping Maw activation distance {activationDistance}";
            AssertEqual(attacks ? YappingMawAiFunction.BeginExtension : YappingMawAiFunction.WaitingForSamus, state.Function, $"{context} attack decision");
            AssertEqual(attacks ? YappingMawAttackGeometry.MaximumTargetDistance : 300, state.DistanceToSamus, $"{context} target distance");
            if (!attacks)
                continue;

            AssertEqual(default(Bearing), state.AimAngle, $"{context} aims straight along X");
            enemies.YappingMawBehavior.RunYappingMawMainAi(slot, Frame(samus));
            AssertEqual(YappingMawAiFunction.ExtendingOrRetracting, state.Function, $"{context} begins extending");
            AssertEqual(32, state.SegmentRadius, $"{context} radius is half the capped distance");
            AssertEqual(RotationAngle.QuarterTurn, state.CurlAngle, $"{context} curl is a quarter turn minus the aim");
        }

        foreach ((int aim, int curl, bool mirrored, int direction) in new[] { new YappingMawInstructionProgramDefinitionsVerifyYappingMawTargetingCase(240, 80, false, 0), new YappingMawInstructionProgramDefinitionsVerifyYappingMawTargetingCase(200, 120, false, 6), new YappingMawInstructionProgramDefinitionsVerifyYappingMawTargetingCase(150, 170, true, 5) })
        {
            (_, RoomEnemySlot slot, YappingMawEnemyState state) = NewInitializedYappingMaw(301);
            state.DistanceToSamus = YappingMawAttackGeometry.MaximumTargetDistance;
            state.AimAngle = new Bearing(checked((byte)aim));
            YappingMawBehavior.BeginYappingMawExtension(slot, state);
            AssertEqual(RotationAngle.FromTableIndex(checked((byte)curl)), state.CurlAngle, $"Yapping Maw aim {aim} curl wraps around the turn");
            AssertEqual(mirrored, state.MirroredBend, $"Yapping Maw aim {aim} bend");
            AssertEqual(YappingMawAnimationDefinitions.Aims.At(direction), state.Aim, $"Yapping Maw aim {aim} sector");
        }

        static ClockedEnemyAiFrame Frame(SamusState samus) => new(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
            new FrameCounters(0));
    }

    /// <summary>A Yapping Maw rooted at (128, 128) through the production initializer, with the given activation distance.</summary>
    internal static NewInitializedYappingMawResult NewInitializedYappingMaw(int activationDistance)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.YappingMaw, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.Position = slot.Position.WithWhole(new(128, 128));
        slot.SetSpawnParameter1(EnemyDefinitionId.YappingMaw, checked((ushort)activationDistance));
        slot.SetSpawnParameter2(EnemyDefinitionId.YappingMaw, 1);
        enemies.YappingMawBehavior.InitializeYappingMaw(slot);
        YappingMawEnemyState state = enemies.YappingMawStates[0] ?? throw new InvalidOperationException("Yapping Maw has no state.");
        return new NewInitializedYappingMawResult(enemies, slot, state);
    }

    /// <summary>Creates a Yapping Maw slot running a selected program with typed state.</summary>
    private static NewInitializedYappingMawResult NewYappingMawInstructionSystem(
            YappingMawProgram entry)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.YappingMaw, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        var state = new YappingMawEnemyState(slot) { Program = AnimationProgramCursor<YappingMawProgram>.Start(entry) };
        slot.SetSpeciesState(state);
        return new NewInitializedYappingMawResult(enemies, slot, state);
    }

    /// <summary>Forces each requested instruction call through the production enemy interpreter.</summary>
    private static void RunYappingMawInstructionFrames(object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = WrappingCountdown.Start(1);
            EnemyInstructionRunner.Process(arguments);
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
