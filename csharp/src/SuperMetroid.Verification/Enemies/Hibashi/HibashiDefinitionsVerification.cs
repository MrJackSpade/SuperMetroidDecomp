using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Hibashi;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks migrated Hibashi hitbox tables and both actor instruction streams against cartridge data and ROM-free production execution.</summary>
internal static class HibashiDefinitionsVerification
{
    /// <summary>Exercises all 22 rising hitbox frames, both instruction programs through their terminal sleeps, and compiled mechanics lookup without allocations.</summary>
    internal static void VerifyHibashiDefinitions(CartridgeImportAddressSpace rom)
    {
        const int yOffsetTable = 0xa68dbb;
        const int yRadiusTable = 0xa68de7;

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = enemies.HibashiBehavior.InitializeHibashi;
        Action<RoomEnemySlot, int> applyFrame = enemies.HibashiBehavior.ApplyHibashiActivityFrame;

        RoomEnemySlot graphics = enemies.SlotTable.Slots[0];
        graphics.Bind(EnemyDefinitionId.Hibashi, graphics.Definition);
        graphics.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 0);
        graphics.Position = graphics.Position.WithWholeY(0x0200);
        initialize(graphics);
        RoomEnemySlot hitbox = enemies.SlotTable.Slots[1];
        hitbox.Bind(EnemyDefinitionId.Hibashi, hitbox.Definition);
        hitbox.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 1);
        hitbox.Position = hitbox.Position.WithWholeY(graphics.Position.Y.Whole);
        initialize(hitbox);

        for (int frameIndex = 0; frameIndex < 22; frameIndex++)
        {
            HibashiActivityDefinition frame = HibashiEruption.ActivityFrame(frameIndex);
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
            AssertEqual(frameIndex == 0 ? (ushort)8 : (ushort)0, hitbox.Radius.X,
                $"Hibashi production X radius {frameIndex}");
        }

        AssertThrows<ArgumentOutOfRangeException>(() => HibashiEruption.ActivityFrame(-1),
            "Hibashi negative activity index");
        AssertThrows<ArgumentOutOfRangeException>(() => HibashiEruption.ActivityFrame(22),
            "Hibashi activity index beyond authored table");
        AssertThrows<ArgumentOutOfRangeException>(() => applyFrame(graphics, 22),
            "Hibashi production activity index beyond authored table");

        var shownSpritemaps = new HashSet<ushort>();
        var programSystem = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initializeProgram = programSystem.HibashiBehavior.InitializeHibashi;

        RoomEnemySlot programGraphics = programSystem.SlotTable.Slots[0];
        programGraphics.Bind(EnemyDefinitionId.Hibashi, programGraphics.Definition);
        programGraphics.Bind(programGraphics.DefinitionId, default(RoomEnemyDefinition));
        programGraphics.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 0);
        programGraphics.Position = programGraphics.Position.WithWhole(new(0x0180, 0x0200));
        initializeProgram(programGraphics);

        RoomEnemySlot programHitbox = programSystem.SlotTable.Slots[1];
        programHitbox.Bind(EnemyDefinitionId.Hibashi, programHitbox.Definition);
        programHitbox.Bind(programHitbox.DefinitionId, default(RoomEnemyDefinition));
        programHitbox.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 1);
        programHitbox.Position = programHitbox.Position.WithWhole(programGraphics.Position.Whole);
        initializeProgram(programHitbox);

        // A second geyser erupting in lockstep: its eruption sound is queued in the same frame.
        RoomEnemySlot twinGraphics = programSystem.SlotTable.Slots[2];
        twinGraphics.Bind(EnemyDefinitionId.Hibashi, twinGraphics.Definition);
        twinGraphics.Bind(twinGraphics.DefinitionId, programGraphics.Definition);
        twinGraphics.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 0);
        twinGraphics.Position = twinGraphics.Position.WithWhole(new(448, 512));
        initializeProgram(twinGraphics);
        RoomEnemySlot twinHitbox = programSystem.SlotTable.Slots[3];
        twinHitbox.Bind(EnemyDefinitionId.Hibashi, twinHitbox.Definition);
        twinHitbox.Bind(twinHitbox.DefinitionId, programGraphics.Definition);
        twinHitbox.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 1);
        twinHitbox.Position = twinHitbox.Position.WithWhole(twinGraphics.Position.Whole);
        initializeProgram(twinHitbox);

        object?[] graphicsArguments =
            [programGraphics, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
        object?[] hitboxArguments =
            [programHitbox, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
        object?[] twinGraphicsArguments =
            [twinGraphics, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
        object?[] twinHitboxArguments =
            [twinHitbox, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];

        // The visible stream needs 59 actor frames through its final cue; the margin proves both actors stay asleep.
        for (int frame = 0; frame < 70; frame++)
        {
            EnemyInstructionRunner.Process(graphicsArguments);
            shownSpritemaps.Add(programGraphics.SpritemapPointer);
            EnemyInstructionRunner.Process(hitboxArguments);
            shownSpritemaps.Add(programHitbox.SpritemapPointer);
            EnemyInstructionRunner.Process(twinGraphicsArguments);
            EnemyInstructionRunner.Process(twinHitboxArguments);
        }
        AssertTrue(shownSpritemaps.SetEquals(HibashiVisualDefinitionsTooling.Spritemaps()), "the Hibashi shows every authored pose");
        AssertEqual(HibashiAnimationsTooling.Spritemaps(HibashiSegment.Graphics).Last(), programGraphics.SpritemapPointer,
            "Hibashi finish retains the last graphics sprite");
        // $A6:8DAF queues per geyser: the contributed lowc movie's two geysers queue two (update 118,130).
        SoundEffectId eruption = HibashiDefinitions.EruptionSoundEffect;
        AssertEqual(2, programSystem.Audio.SoundRequests.Count(request => request.SoundEffect == eruption),
            "each Hibashi graphics program queues its eruption sound");
        AssertTrue((programSystem.HibashiStates[0] ?? throw new InvalidOperationException("Verification requires programSystem.HibashiStates[0].")).ActivityFinished,
            "Hibashi graphics program publishes finished activity");
        AssertEqual((ushort)0, programHitbox.Radius.X,
            "Hibashi finished program clears hitbox X radius");
        AssertEqual((ushort)0, programHitbox.Radius.Y,
            "Hibashi finished program clears hitbox Y radius");
        AssertTrue(programGraphics.Presence.HasAny(EnemyPresence.Hidden),
            "Hibashi finished program hides graphics actor");
        AssertTrue(programHitbox.Contact.HasAny(EnemyContact.IgnoresSamus),
            "Hibashi finished program disables collision actor");
        HibashiEnemyState graphicsState = programSystem.HibashiStates[0] ?? throw new InvalidOperationException("The Hibashi graphics part has no state.");
        HibashiEnemyState hitboxState = programSystem.HibashiStates[1] ?? throw new InvalidOperationException("The Hibashi hitbox part has no state.");
        AnimationPlayback<HibashiSegment> graphicsAsleep = graphicsState.Playback;
        AnimationPlayback<HibashiSegment> hitboxAsleep = hitboxState.Playback;
        EnemyInstructionRunner.Process(graphicsArguments);
        EnemyInstructionRunner.Process(hitboxArguments);
        AssertEqual(graphicsAsleep, graphicsState.Playback, "Hibashi graphics program sleeps after finishing");
        AssertEqual(hitboxAsleep, hitboxState.Playback, "Hibashi hitbox program sleeps on its one pose");
        AssertEqual(HibashiSegment.Graphics, graphicsState.Playback.Animation.Phases[graphicsState.Playback.Place.Phase].Segment, "Hibashi graphics part runs the eruption");
        AssertEqual(HibashiSegment.Hitbox, hitboxState.Playback.Animation.Phases[hitboxState.Playback.Place.Phase].Segment, "Hibashi hitbox part runs the collision-part animation");
        Suite(nameof(HibashiAnimationsVerification.VerifyHibashiAnimations), () => HibashiAnimationsVerification.VerifyHibashiAnimations(rom));

        Console.WriteLine(
            "Hibashi definitions: 44 physical words, all 22 real activity-frame hitboxes, and both typed programs' " +
            "eruption, cues and sleeps pass.");
    }


    /// <summary>Runs the Hibashi migration check against the local retail cartridge image.</summary>
    [VerificationCommand("--hibashi-instruction-mechanics")]
    internal static int HibashiInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyHibashiDefinitions), () => VerifyHibashiDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
