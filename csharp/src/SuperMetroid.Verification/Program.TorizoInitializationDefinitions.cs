using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SharedSlopeHeightsVerification;

internal static class TorizoInitializationDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo initialization")]
    internal static void VerifyCompiledTorizoInitialization(SuperMetroidAddressSpace rom)
    {
        for (int variant = 0; variant < 2; variant++)
        {
            ushort[] native = [CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeXPositions + variant * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeYPositions + variant * 2),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeInstructionLists + variant * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakePropertyMasks + variant * 2),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeXRadii + variant * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeYRadii + variant * 2)];
            var compiled = variant == 0 ? TorizoInitializationDefinitions.Bomb : TorizoInitializationDefinitions.Golden;
            ushort[] actual = [compiled.Position.X, compiled.Position.Y, compiled.Instruction, compiled.PropertyMask, compiled.Radius.X, compiled.Radius.Y];
            AssertTrue(native.AsSpan().SequenceEqual(actual), "Every Torizo initialization record word matches ROM");

            var enemies = new RoomEnemySystem();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, NewSlopeHeightNoReadBus());
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
            bool defeated = false;
            enemies.VerificationServices().IsAreaTorizoDefeated = (Func<bool>)(() => defeated);
            var initializeWithInput = typeof(RoomEnemySystem).GetMethod("InitializeBombTorizo", flags)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState?, ushort>>(enemies);
            void initialize(RoomEnemySlot slot) => initializeWithInput(slot, null, 0);
            var actor = enemies.Slots[0];
            actor.EnemyDefinitionPointer = variant == 0 ? EnemyDefinitionId.BombTorizo : EnemyDefinitionId.GoldenTorizo;
            for (int properties = 0; properties <= ushort.MaxValue; properties++)
            {
                actor.Properties = (ushort)properties;
                actor.ExtraProperties = (ushort)~properties;
                actor.Radius = new(0xffff, 0xffff);
                actor.Position = new(new(0xffff, 0x1234), new(0xffff, 0xabcd));
                actor.Health = 0x3456;
                actor.CurrentInstruction = actor.InstructionTimer = actor.Timer = actor.PaletteIndex = 0xffff;
                initialize(actor);
                AssertEqual(native[0], actor.Position.X.Whole, "Real Torizo initial X");
                AssertEqual(native[1], actor.Position.Y.Whole, "Real Torizo initial Y");
                AssertEqual(native[2], actor.CurrentInstruction, "Real Torizo initial instruction selection");
                AssertEqual((ushort)(properties | native[3]), actor.Properties, "All population property bits preserved by native OR");
                AssertEqual(native[4], actor.Radius.X, "Real Torizo X hitbox radius");
                AssertEqual(native[5], actor.Radius.Y, "Real Torizo Y hitbox radius");
                AssertEqual((ushort)((ushort)~properties | (ushort)EnemyExtraProperties.UsesExtendedSpritemap), actor.ExtraProperties, "Extended map flag preserves population extra properties");
                AssertEqual((ushort)0x1234, actor.Position.X.Fraction, "Torizo initialization preserves X fraction");
                AssertEqual((ushort)0xabcd, actor.Position.Y.Fraction, "Torizo initialization preserves Y fraction");
                AssertEqual((ushort)0x3456, actor.Health, "Torizo initialization preserves population health");
                AssertEqual((ushort)1, actor.InstructionTimer, "Initial instruction timer");
                AssertEqual((ushort)0, actor.Timer, "Initial actor timer");
                AssertEqual((ushort)0, actor.PaletteIndex, "Initial palette index");
                var state = variant == 0 ? enemies.BombTorizo! : enemies.GoldenTorizo!;
                AssertEqual((ushort)0, state.Velocity.X.RawValue, "Initial horizontal velocity");
                AssertEqual((ushort)0x100, state.Velocity.Y.RawValue, "Initial falling velocity");
            }
            defeated = true;
            actor.Properties = 0x1234;
            actor.Position = actor.Position.WithWhole(new(0x1111, 0x2222));
            actor.CurrentInstruction = 0x3333;
            initialize(actor);
            AssertEqual((ushort)(0x1234 | (ushort)EnemyProperties.Deleted), actor.Properties, "Defeated Torizo is deleted before initialization writes");
            AssertEqual((ushort)0x1111, actor.Position.X.Whole, "Defeated Torizo preserves X");
            AssertEqual((ushort)0x2222, actor.Position.Y.Whole, "Defeated Torizo preserves Y");
            AssertEqual((ushort)0x3333, actor.CurrentInstruction, "Defeated Torizo preserves instruction");
        }
        Console.WriteLine("Torizo initialization: all 12 native words, 131072 real live initializations and both defeated early returns pass without ROM reads.");
    }
}
