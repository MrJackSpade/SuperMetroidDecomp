using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Hibashi;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Samus;

/// <summary>Checks migrated Hibashi hitbox tables and both actor instruction streams against cartridge data and ROM-free production execution.</summary>
internal static class HibashiDefinitionsVerification
{
    /// <summary>Exercises all 22 rising hitbox frames, both instruction programs through their terminal sleeps, and compiled mechanics lookup without allocations.</summary>
    internal static void VerifyHibashiDefinitions(CartridgeImportAddressSpace rom)
    {
        const int yOffsetTable = 0xa68dbb;
        const int yRadiusTable = 0xa68de7;

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The room's population load sets up the flame and then its collision part (#2554).
        IReadOnlyList<RoomEnemyPlace> geyser = enemies.LoadPopulation(
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Hibashi, new HibashiFlameSpawnSettings(InactiveTime: 0), new(0, 0x0200)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Hibashi, new HibashiCollisionPartSpawnSettings(), new(0, 0x0200)));
        RoomEnemyPlace graphics = geyser[0];
        RoomEnemyPlace hitbox = geyser[1];

        HibashiEruptionStep[] steps = HibashiAnimationsToolingConstants.EruptionSteps();
        AssertEqual(22, steps.Length, "Hibashi eruption steps");
        // The flame erupts as its animation plays: each of its steps raises its hitbox to the step's height and gives it
        // the step's half-height, the first its half-width too. Each step is observed on the update it moves the hitbox.
        // The flame's AI starts the eruption on the update its first wait runs out (#2340).
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        for (int update = 0; update < HibashiDefinitions.FirstWait; update++)
            graphics.RunMainAi(samus);
        object?[] eruptionUpdate = [graphics, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        for (int frameIndex = 0; frameIndex < steps.Length; frameIndex++)
        {
            HibashiEruptionStep frame = steps[frameIndex];
            AssertEqual(rom.ReadCartridgeWord((yOffsetTable + frameIndex * 2)), frame.YOffset,
                $"Hibashi Y offset {frameIndex}");
            AssertEqual(rom.ReadCartridgeWord((yRadiusTable + frameIndex * 2)), frame.YRadius,
                $"Hibashi Y radius {frameIndex}");

            hitbox.Radius = hitbox.Radius.WithX(0);
            ushort before = hitbox.Position.Y.Whole;
            for (int update = 0; update < 64 && hitbox.Position.Y.Whole == before; update++)
                EnemyInstructionRunner.Process(eruptionUpdate);
            AssertEqual(unchecked((ushort)(0x0200 - frame.YOffset)), hitbox.Position.Y.Whole,
                $"Hibashi production Y position {frameIndex}");
            AssertEqual(frame.YRadius, hitbox.Radius.Y,
                $"Hibashi production Y radius {frameIndex}");
            AssertEqual(frameIndex == 0 ? (ushort)8 : (ushort)0, hitbox.Radius.X,
                $"Hibashi production X radius {frameIndex}");
        }


        var shownSpritemaps = new HashSet<ushort>();
        var programSystem = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The room's population load sets up two geysers, each its flame and then its collision part; the second erupts in
        // lockstep with the first, so its eruption sound is queued in the same frame.
        IReadOnlyList<RoomEnemyPlace> geysers = programSystem.LoadPopulation(
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Hibashi, new HibashiFlameSpawnSettings(InactiveTime: 0), new(0x0180, 0x0200)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Hibashi, new HibashiCollisionPartSpawnSettings(), new(0x0180, 0x0200)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Hibashi, new HibashiFlameSpawnSettings(InactiveTime: 0), new(448, 512)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Hibashi, new HibashiCollisionPartSpawnSettings(), new(448, 512)));
        RoomEnemyPlace programGraphics = geysers[0];
        RoomEnemyPlace programHitbox = geysers[1];
        RoomEnemyPlace twinGraphics = geysers[2];
        RoomEnemyPlace twinHitbox = geysers[3];

        object?[] graphicsArguments =
            [programGraphics, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        object?[] hitboxArguments =
            [programHitbox, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        object?[] twinGraphicsArguments =
            [twinGraphics, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        object?[] twinHitboxArguments =
            [twinHitbox, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];

        // Both flames erupt, as their AI starts the eruption it plays when their first wait runs out (#2439).
        for (int update = 0; update < HibashiDefinitions.FirstWait; update++)
        {
            programGraphics.RunMainAi(samus);
            twinGraphics.RunMainAi(samus);
        }

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
        AssertEqual(HibashiAnimationsTooling.Spritemaps(HibashiSegment.Flame).Last(), programGraphics.SpritemapPointer,
            "Hibashi finish retains the last graphics sprite");
        // $A6:8DAF queues per geyser: the contributed lowc movie's two geysers queue two (update 118,130).
        SoundEffectId eruption = HibashiDefinitions.EruptionSoundEffect;
        AssertEqual(2, programSystem.Audio.SoundRequests.Count(request => request.SoundEffect == eruption),
            "each Hibashi graphics program queues its eruption sound");
        AssertEqual((ushort)0, programHitbox.Radius.X,
            "Hibashi finished program clears hitbox X radius");
        AssertEqual((ushort)0, programHitbox.Radius.Y,
            "Hibashi finished program clears hitbox Y radius");
        AssertTrue(programGraphics.Presence.HasAny(EnemyPresence.Hidden),
            "Hibashi finished program hides graphics actor");
        AssertTrue(programHitbox.Contact.HasAny(EnemyContact.IgnoresSamus),
            "Hibashi finished program disables collision actor");
        // Both programs sleep: a further update shows the same pose with the same time left.
        (ushort Spritemap, int Hold) graphicsAsleep = (programGraphics.SpritemapPointer, programGraphics.AnimationClock.Hold);
        (ushort Spritemap, int Hold) hitboxAsleep = (programHitbox.SpritemapPointer, programHitbox.AnimationClock.Hold);
        EnemyInstructionRunner.Process(graphicsArguments);
        EnemyInstructionRunner.Process(hitboxArguments);
        AssertEqual(graphicsAsleep, (programGraphics.SpritemapPointer, programGraphics.AnimationClock.Hold), "Hibashi graphics program sleeps after finishing");
        AssertEqual(hitboxAsleep, (programHitbox.SpritemapPointer, programHitbox.AnimationClock.Hold), "Hibashi hitbox program sleeps on its one pose");
        AssertTrue(HibashiAnimationsTooling.Spritemaps(HibashiSegment.Hitbox).Contains(programHitbox.SpritemapPointer),
            "Hibashi hitbox part shows the collision-part animation's pose");
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
