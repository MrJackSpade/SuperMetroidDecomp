using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Hibashi;
using SuperMetroid.Verification.Harness;

/// <summary>Checks migrated Hibashi hitbox tables and both actor instruction streams against cartridge data and ROM-free production execution.</summary>
internal static class HibashiDefinitionsVerification
{
    /// <summary>Exercises all 22 rising hitbox frames, both instruction programs through their terminal sleeps, and compiled mechanics lookup without allocations.</summary>
    internal static void VerifyHibashiDefinitions(SuperMetroidAddressSpace rom)
    {
        const int yOffsetTable = 0xa68dbb;
        const int yRadiusTable = 0xa68de7;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewHibashiDefinitionGuard(new TestAddressSpace()));
        Action<RoomEnemySlot> initialize = enemies.HibashiBehavior.InitializeHibashi;
        Action<RoomEnemySlot, int> applyFrame = enemies.HibashiBehavior.ApplyHibashiActivityFrame;

        RoomEnemySlot graphics = enemies.SlotTable.Slots[0];
        graphics.Parameter2 = 0;
        graphics.Position = graphics.Position.WithWholeY(0x0200);
        initialize(graphics);
        RoomEnemySlot hitbox = enemies.SlotTable.Slots[1];
        hitbox.Parameter2 = 1;
        hitbox.Position = hitbox.Position.WithWholeY(graphics.Position.Y.Whole);
        initialize(hitbox);

        for (int frameIndex = 0; frameIndex < 22; frameIndex++)
        {
            HibashiActivityDefinition frame = HibashiDefinitions.ActivityFrame(frameIndex);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((yOffsetTable + frameIndex * 2)), frame.YOffset,
                $"Hibashi Y offset {frameIndex}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((yRadiusTable + frameIndex * 2)), frame.YRadius,
                $"Hibashi Y radius {frameIndex}");

            hitbox.Radius = hitbox.Radius.WithX(0);
            applyFrame(graphics, frameIndex);
            AssertEqual(unchecked((ushort)(0x0200 - frame.YOffset)), hitbox.Position.Y.Whole,
                $"Hibashi production Y position {frameIndex}");
            AssertEqual(frame.YRadius, hitbox.Radius.Y,
                $"Hibashi production Y radius {frameIndex}");
            AssertEqual(frameIndex, (enemies.HibashiBehavior.LastHibashiActivityFrameIndex ?? throw new InvalidOperationException("Verification requires enemies.HibashiBehavior.LastHibashiActivityFrameIndex.")),
                $"Hibashi production frame publication {frameIndex}");
            AssertEqual(frameIndex == 0 ? (ushort)8 : (ushort)0, hitbox.Radius.X,
                $"Hibashi production X radius {frameIndex}");
        }

        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(-1),
            "Hibashi negative activity index");
        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(22),
            "Hibashi activity index beyond authored table");
        AssertThrows<ArgumentOutOfRangeException>(() => applyFrame(graphics, 22),
            "Hibashi production activity index beyond authored table");

        var shownSpritemaps = new HashSet<ushort>();
        var programSystem = new RoomEnemySystem();
        programSystem.BindFixtureBus(rom);
        Action<RoomEnemySlot> initializeProgram = programSystem.HibashiBehavior.InitializeHibashi;

        RoomEnemySlot programGraphics = programSystem.SlotTable.Slots[0];
        programGraphics.EnemyDefinitionPointer = EnemyDefinitionId.Hibashi;
        programGraphics.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programGraphics.Parameter2 = 0;
        programGraphics.Position = programGraphics.Position.WithWhole(new(0x0180, 0x0200));
        initializeProgram(programGraphics);

        RoomEnemySlot programHitbox = programSystem.SlotTable.Slots[1];
        programHitbox.EnemyDefinitionPointer = EnemyDefinitionId.Hibashi;
        programHitbox.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programHitbox.Parameter2 = 1;
        programHitbox.Position = programHitbox.Position.WithWhole(programGraphics.Position.Whole);
        initializeProgram(programHitbox);

        // A second geyser erupting in lockstep: its eruption sound is queued in the same frame.
        RoomEnemySlot twinGraphics = programSystem.SlotTable.Slots[2];
        twinGraphics.EnemyDefinitionPointer = EnemyDefinitionId.Hibashi;
        twinGraphics.Definition = programGraphics.Definition;
        twinGraphics.Parameter2 = 0;
        twinGraphics.Position = twinGraphics.Position.WithWhole(new(448, 512));
        initializeProgram(twinGraphics);
        RoomEnemySlot twinHitbox = programSystem.SlotTable.Slots[3];
        twinHitbox.EnemyDefinitionPointer = EnemyDefinitionId.Hibashi;
        twinHitbox.Definition = programGraphics.Definition;
        twinHitbox.Parameter2 = 1;
        twinHitbox.Position = twinHitbox.Position.WithWhole(twinGraphics.Position.Whole);
        initializeProgram(twinHitbox);

        object?[] graphicsArguments =
            [programGraphics, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        object?[] hitboxArguments =
            [programHitbox, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        object?[] twinGraphicsArguments =
            [twinGraphics, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        object?[] twinHitboxArguments =
            [twinHitbox, null, null, Position16<RoomPixels>.Zero, SnesButton.None];

        // The visible stream needs 59 actor frames through its final cue; the margin proves both actors stay asleep.
        for (int frame = 0; frame < 70; frame++)
        {
            EnemyInstructionRunner.Process(programSystem, graphicsArguments);
            shownSpritemaps.Add(programGraphics.SpritemapPointer);
            EnemyInstructionRunner.Process(programSystem, hitboxArguments);
            shownSpritemaps.Add(programHitbox.SpritemapPointer);
            EnemyInstructionRunner.Process(programSystem, twinGraphicsArguments);
            EnemyInstructionRunner.Process(programSystem, twinHitboxArguments);
        }
        AssertTrue(shownSpritemaps.SetEquals(HibashiVisualDefinitionsTooling.Spritemaps()), "the Hibashi shows every authored pose");
        AssertEqual(HibashiAnimations.Spritemaps(HibashiProgram.Graphics).Last(), programGraphics.SpritemapPointer,
            "Hibashi finish retains the last graphics sprite");
        // $A6:8DAF queues per geyser: the contributed lowc movie's two geysers queue two (update 118,130).
        SoundEffectId eruption = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, HibashiDefinitions.EruptionSoundEffect);
        AssertEqual(2, programSystem.Audio.SoundRequests.Count(request => request.SoundEffect == eruption),
            "each Hibashi graphics program queues its eruption sound");
        AssertEqual(21, (programSystem.HibashiBehavior.LastHibashiActivityFrameIndex ?? throw new InvalidOperationException("Verification requires programSystem.HibashiBehavior.LastHibashiActivityFrameIndex.")),
            "Hibashi graphics program executes final activity callback");
        AssertEqual((ushort)1, (programSystem.HibashiStates[0] ?? throw new InvalidOperationException("Verification requires programSystem.HibashiStates[0].")).FinishedActivityFlag,
            "Hibashi graphics program publishes finished activity");
        AssertEqual((ushort)0, programHitbox.Radius.X,
            "Hibashi finished program clears hitbox X radius");
        AssertEqual((ushort)0, programHitbox.Radius.Y,
            "Hibashi finished program clears hitbox Y radius");
        AssertTrue(programGraphics.Properties.HasAny(EnemyProperties.Invisible),
            "Hibashi finished program hides graphics actor");
        AssertTrue(programHitbox.Properties.HasAny(EnemyProperties.IgnoreSamusCollision),
            "Hibashi finished program disables collision actor");
        HibashiEnemyState graphicsState = programSystem.HibashiStates[0] ?? throw new InvalidOperationException("The Hibashi graphics part has no state.");
        HibashiEnemyState hitboxState = programSystem.HibashiStates[1] ?? throw new InvalidOperationException("The Hibashi hitbox part has no state.");
        (AnimationProgramCursor<HibashiProgram> graphicsAsleep, AnimationProgramCursor<HibashiProgram> hitboxAsleep) = (graphicsState.Program, hitboxState.Program);
        EnemyInstructionRunner.Process(programSystem, graphicsArguments);
        EnemyInstructionRunner.Process(programSystem, hitboxArguments);
        AssertEqual(graphicsAsleep, graphicsState.Program, "Hibashi graphics program sleeps after finishing");
        AssertEqual(hitboxAsleep, hitboxState.Program, "Hibashi hitbox program sleeps on its one pose");
        AssertEqual(HibashiProgram.Graphics, graphicsState.Program.Program, "Hibashi graphics part runs the eruption");
        AssertEqual(HibashiProgram.Hitbox, hitboxState.Program.Program, "Hibashi hitbox part runs the collision-part animation");
        Suite(nameof(HibashiAnimationsVerification.VerifyHibashiAnimations), () => HibashiAnimationsVerification.VerifyHibashiAnimations(rom));

        Console.WriteLine(
            "Hibashi definitions: 44 physical words, all 22 real activity-frame hitboxes, and both typed programs' " +
            "eruption, cues and sleeps pass.");
    }

    /// <summary>Rejects fallback reads from the cartridge hitbox tables after their values have been compiled.</summary>
    private static CartridgeReadGuard NewHibashiDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(0xa68dbb, 0xa68e13 - 0xa68dbb,
            address => $"Hibashi attempted migrated hitbox read ${address:X6}.");

    /// <summary>Runs the Hibashi migration check against the local retail cartridge image.</summary>
    [VerificationCommand("--hibashi-instruction-mechanics")]
    internal static int HibashiInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyHibashiDefinitions), () => VerifyHibashiDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
