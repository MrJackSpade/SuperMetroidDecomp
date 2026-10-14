using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Metroid escape movement and random cry words against retail sources.</summary>
internal static class MetroidBehaviorDefinitionsVerification
{
    /// <summary>Compares source tables and exercises the production escape and cry consumers with those reads forbidden.</summary>
    internal static void VerifyMetroidBehaviorDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort frame = 0; frame < 4; frame++)
        {
            Offset16<RoomPixels> actual =
                MetroidBehaviorDefinitions.EscapeDisplacement(frame);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3ea3f + frame * 2))), actual.X,
                $"Metroid escape X displacement {frame}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3ea47 + frame * 2))), actual.Y,
                $"Metroid escape Y displacement {frame}");
        }

        for (ushort index = 0; index < 8; index++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3ead6 + index * 2)),
                MetroidBehaviorDefinitions.RandomCrySoundEffect(index),
                $"Metroid random cry {index}");
        }

        Suite(nameof(VerifyMetroidEscapeConsumer), () => VerifyMetroidEscapeConsumer(rom));
        Suite(nameof(VerifyMetroidCryConsumer), () => VerifyMetroidCryConsumer(rom));
        Console.WriteLine(
            "Metroid behavior definitions: eight displacement words, eight cry words, all 65,536 escape selectors and all eight production instruction selections pass with source tables forbidden.");
    }

    /// <summary>Checks every ushort escape timer for wrapped displacement, countdown, and homing handoff.</summary>
    private static void VerifyMetroidEscapeConsumer(SuperMetroidAddressSpace rom)
    {
        var guarded = NewMetroidBehaviorReadGuard(rom);
        // One system serves every timer: the escape step is static and keeps no per-call state.
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guarded);

        for (int timer = 0; timer <= ushort.MaxValue; timer++)
        {
            var slot = new RoomEnemySlot(0)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0001, 0xffff)),
            };
            var state = new MetroidEnemyState(
                slot,
                new RoomSpriteObjectSlot(0),
                new RoomSpriteObjectSlot(1))
            {
                EscapeTimer = NativeCountdown.FromRaw((ushort)timer),
                Function = MetroidAiFunction.PowerBombEscape,
                Program = AnimationProgramCursor<MetroidProgram>.Start(MetroidProgram.DrainingSamus),
            };

            Offset16<RoomPixels> expected =
                MetroidBehaviorDefinitions.EscapeDisplacement((ushort)timer);
            MetroidBehavior.RunMetroidPowerBombEscape(slot, state);
            AssertEqual(unchecked((ushort)(1 + expected.X)), slot.Position.X.Whole,
                $"production Metroid escape X timer {timer}");
            AssertEqual(unchecked((ushort)(0xffff + expected.Y)), slot.Position.Y.Whole,
                $"production Metroid escape Y timer {timer}");
            AssertEqual(unchecked((ushort)(timer - 1)), state.EscapeTimer.RawValue,
                $"production Metroid escape countdown {timer}");
            AssertEqual(timer == 1 ? MetroidAiFunction.Homing : MetroidAiFunction.PowerBombEscape,
                state.Function,
                $"production Metroid escape function {timer}");
            AssertEqual(
                AnimationProgramCursor<MetroidProgram>.Start(timer == 1 ? MetroidProgram.ChasingSamus : MetroidProgram.DrainingSamus),
                state.Program,
                $"production Metroid escape animation {timer}");
        }
    }

    /// <summary>Checks all eight random cry selections through the real instruction interpreter.</summary>
    private static void VerifyMetroidCryConsumer(SuperMetroidAddressSpace rom)
    {
        var guarded = NewMetroidBehaviorReadGuard(rom);

        for (ushort random = 0; random < 8; random++)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            ushort selectedRandom = random;
            enemies.VerificationServices().NextRandom = ((Func<ushort>)(() => selectedRandom));
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Metroid;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.InstructionTimer = NativeCountdown.Start(1);
            // Start at the chasing loop's random-cry cue, after its twenty pulse poses.
            enemies.MetroidBehavior._metroidStates[0] = new MetroidEnemyState(slot, new RoomSpriteObjectSlot(0), new RoomSpriteObjectSlot(1))
            {
                Program = new AnimationProgramCursor<MetroidProgram>(MetroidProgram.ChasingSamus,
                    MetroidAnimations.Programs.Steps(MetroidProgram.ChasingSamus).ToList().FindIndex(step => step.Kind == AnimationProgramStepKind.Cue)),
            };

            enemies.Instructions.ProcessInstructions(slot, null, null, Position16<RoomPixels>.Zero, (ushort)0);

            AssertEqual(MetroidBehaviorDefinitions.RandomCrySoundEffect(random),
                enemies.MetroidBehavior.LastMetroidSoundEffectLibrary2 ?? ushort.MaxValue,
                $"production Metroid random cry {random}");
            AssertEqual(16, slot.InstructionTimer.RawValue,
                $"production Metroid script timer {random}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa3e9d1), slot.SpritemapPointer,
                $"production Metroid script spritemap {random}");
            AssertEqual(new AnimationProgramCursor<MetroidProgram>(MetroidProgram.ChasingSamus, 1),
                (enemies.MetroidBehavior._metroidStates[0] ?? throw new InvalidOperationException("The Metroid has no state.")).Program,
                $"production Metroid script continuation {random}");
        }
    }

    /// <summary>Forbids reads from converted Metroid displacement and cry tables.</summary>
    private static CartridgeReadGuard NewMetroidBehaviorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa3ea3f and < 0xa3ea4f or >= 0xa3ead6 and < 0xa3eae6,
                address => $"Metroid behavior attempted migrated table read ${address:X6}.");
}
