using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Metroids;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Audio;

/// <summary>Checks compiled Metroid escape movement and random cry words against retail sources.</summary>
internal static class MetroidBehaviorDefinitionsVerification
{
    /// <summary>Compares source tables and exercises the production escape and cry consumers with those reads forbidden.</summary>
    internal static void VerifyMetroidBehaviorDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort frame = 0; frame < 4; frame++)
        {
            Offset16<RoomPixels> actual =
                MetroidReactions.EscapeDisplacement(frame);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3ea3f + frame * 2))), actual.X,
                $"Metroid escape X displacement {frame}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3ea47 + frame * 2))), actual.Y,
                $"Metroid escape Y displacement {frame}");
        }

        for (ushort index = 0; index < 8; index++)
        {
            SoundEffectId cry = MetroidReactions.RandomCrySoundEffect(index);
            AssertEqual(SoundEffectCommands.Decode(cry.Library, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3ead6 + index * 2))),
                cry,
                $"Metroid random cry {index}");
        }

        Suite(nameof(VerifyMetroidEscapeConsumer), () => VerifyMetroidEscapeConsumer());
        Suite(nameof(VerifyMetroidCryConsumer), () => VerifyMetroidCryConsumer(rom));
        Console.WriteLine(
            "Metroid behavior definitions: eight displacement words, eight cry words, all 65,536 escape selectors and all eight production instruction selections pass with source tables forbidden.");
    }

    /// <summary>Checks every ushort escape timer for wrapped displacement, countdown, and homing handoff.</summary>
    private static void VerifyMetroidEscapeConsumer()
    {
        // One system serves every timer: the escape step is static and keeps no per-call state.

        // An escape starts at its duration and ends on the step that reaches zero, so every count from one is swept.
        for (int timer = 1; timer <= short.MaxValue; timer++)
        {
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0001, 0xffff)),
            };
            var state = new MetroidEnemyState(
                new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyA, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard),
                new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyB, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
            {
                EscapeTimer = timer,
                Function = MetroidAiFunction.PowerBombEscape,
                Playback = new(MetroidAnimations.StartingWith(MetroidProgram.DrainingSamus), AnimationPlace.StartOfAnimation),
            };

            Offset16<RoomPixels> expected =
                MetroidReactions.EscapeDisplacement((ushort)timer);
            MetroidBehavior.RunMetroidPowerBombEscape(slot, state);
            AssertEqual(unchecked((ushort)(1 + expected.X)), slot.Position.X.Whole,
                $"production Metroid escape X timer {timer}");
            AssertEqual(unchecked((ushort)(0xffff + expected.Y)), slot.Position.Y.Whole,
                $"production Metroid escape Y timer {timer}");
            AssertEqual(timer - 1, state.EscapeTimer,
                $"production Metroid escape countdown {timer}");
            AssertEqual(timer == 1 ? MetroidAiFunction.Homing : MetroidAiFunction.PowerBombEscape,
                state.Function,
                $"production Metroid escape function {timer}");
            AssertEqual(
                new AnimationPlayback<MetroidProgram>(
                    MetroidAnimations.StartingWith(timer == 1 ? MetroidProgram.ChasingSamus : MetroidProgram.DrainingSamus),
                    AnimationPlace.StartOfAnimation),
                state.Playback,
                $"production Metroid escape animation {timer}");
        }
    }

    /// <summary>Checks all eight random cry selections through the real instruction interpreter.</summary>
    private static void VerifyMetroidCryConsumer(SuperMetroidAddressSpace rom)
    {
        var guarded = NewMetroidBehaviorReadGuard(rom);

        for (ushort random = 0; random < 8; random++)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            ushort selectedRandom = random;
            enemies.VerificationServices().NextRandom = () => selectedRandom;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Metroid, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.FrameHold = WrappingCountdown.Start(1);
            // Start at the chasing loop's random-cry cue, after its twenty pulse poses.
            enemies.SlotTable.Slots[0].SetSpeciesState(new MetroidEnemyState(new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyA, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard), new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyB, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
            {
                Playback = new(MetroidAnimations.StartingWith(MetroidProgram.ChasingSamus),
                    AnimationSegments<MetroidProgram, MetroidFrame, MetroidCue>.PlaceOf(MetroidAnimations.StartingWith(MetroidProgram.ChasingSamus),
                        MetroidProgram.ChasingSamus,
                        MetroidAnimations.Segments.Beats(MetroidProgram.ChasingSamus).ToList().FindIndex(beat => beat is AnimationBeat<MetroidFrame, MetroidCue>.Event))),
            });

            EnemyInstructionInterpreter.RunsAnimationScript(slot, null, null, Position16<RoomPixels>.Zero, 0);

            AssertEqual((SoundEffectId?)MetroidReactions.RandomCrySoundEffect(random),
                enemies.Audio.LatestSound,
                $"production Metroid random cry {random}");
            AssertEqual(16, slot.FrameHold.RawValue,
                $"production Metroid script timer {random}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa3e9d1), slot.SpritemapPointer,
                $"production Metroid script spritemap {random}");
            // The cue ends the pass; the loop shows its first pose and stands before its second.
            AssertEqual(new AnimationPlayback<MetroidProgram>(MetroidAnimations.StartingWith(MetroidProgram.ChasingSamus), new AnimationPlace(0, 1)),
                (enemies.SlotTable.Slots[0].EntityState<MetroidEntity, MetroidEnemyState>() ?? throw new InvalidOperationException("The Metroid has no state.")).Playback,
                $"production Metroid script continuation {random}");
        }
    }

    /// <summary>Forbids reads from converted Metroid displacement and cry tables.</summary>
    private static CartridgeReadGuard NewMetroidBehaviorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa3ea3f and < 0xa3ea4f or >= 0xa3ead6 and < 0xa3eae6,
                address => $"Metroid behavior attempted migrated table read ${address:X6}.");
}
