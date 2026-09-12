using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledTorizoInitialization(SuperMetroidAddressSpace rom)
    {
        for (int variant = 0; variant < 2; variant++)
        {
            ushort Word(int address) => (ushort)(rom.ReadByte(address + variant * 2) | rom.ReadByte(address + variant * 2 + 1) << 8);
            ushort[] native = [Word(EnemyRomTablePointers.Torizo.WakeXPositions), Word(EnemyRomTablePointers.Torizo.WakeYPositions),
                Word(EnemyRomTablePointers.Torizo.WakeInstructionLists), Word(EnemyRomTablePointers.Torizo.WakePropertyMasks),
                Word(EnemyRomTablePointers.Torizo.WakeXRadii), Word(EnemyRomTablePointers.Torizo.WakeYRadii)];
            var compiled = variant == 0 ? TorizoInitializationDefinitions.Bomb : TorizoInitializationDefinitions.Golden;
            ushort[] actual = [compiled.X, compiled.Y, compiled.Instruction, compiled.PropertyMask, compiled.XRadius, compiled.YRadius];
            AssertTrue(native.AsSpan().SequenceEqual(actual), "Every Torizo initialization record word matches ROM");

            var enemies = new RoomEnemySystem();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
            bool defeated = false;
            typeof(RoomEnemySystem).GetField("_isAreaTorizoDefeated", flags)!.SetValue(enemies, (Func<bool>)(() => defeated));
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBombTorizo", flags)!.CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var actor = enemies.Slots[0];
            actor.EnemyDefinitionPointer = variant == 0 ? RoomEnemySystem.BombTorizoDefinition : RoomEnemySystem.GoldenTorizoDefinition;
            for (int properties = 0; properties <= ushort.MaxValue; properties++)
            {
                actor.Properties = (ushort)properties;
                actor.ExtraProperties = (ushort)~properties;
                actor.XPosition = actor.YPosition = actor.XRadius = actor.YRadius = 0xffff;
                actor.XSubposition = 0x1234;
                actor.YSubposition = 0xabcd;
                actor.Health = 0x3456;
                actor.CurrentInstruction = actor.InstructionTimer = actor.Timer = actor.PaletteIndex = 0xffff;
                initialize(actor);
                AssertEqual(native[0], actor.XPosition, "Real Torizo initial X");
                AssertEqual(native[1], actor.YPosition, "Real Torizo initial Y");
                AssertEqual(native[2], actor.CurrentInstruction, "Real Torizo initial instruction selection");
                AssertEqual((ushort)(properties | native[3]), actor.Properties, "All population property bits preserved by native OR");
                AssertEqual(native[4], actor.XRadius, "Real Torizo X hitbox radius");
                AssertEqual(native[5], actor.YRadius, "Real Torizo Y hitbox radius");
                AssertEqual((ushort)((ushort)~properties | (ushort)EnemyExtraProperties.UsesExtendedSpritemap), actor.ExtraProperties, "Extended map flag preserves population extra properties");
                AssertEqual((ushort)0x1234, actor.XSubposition, "Torizo initialization preserves X fraction");
                AssertEqual((ushort)0xabcd, actor.YSubposition, "Torizo initialization preserves Y fraction");
                AssertEqual((ushort)0x3456, actor.Health, "Torizo initialization preserves population health");
                AssertEqual((ushort)1, actor.InstructionTimer, "Initial instruction timer");
                AssertEqual((ushort)0, actor.Timer, "Initial actor timer");
                AssertEqual((ushort)0, actor.PaletteIndex, "Initial palette index");
                var state = variant == 0 ? enemies.BombTorizo! : enemies.GoldenTorizo!;
                AssertEqual((ushort)0, state.HorizontalVelocity, "Initial horizontal velocity");
                AssertEqual((ushort)0x100, state.VerticalVelocity, "Initial falling velocity");
            }
            defeated = true;
            actor.Properties = 0x1234;
            actor.XPosition = 0x1111;
            actor.YPosition = 0x2222;
            actor.CurrentInstruction = 0x3333;
            initialize(actor);
            AssertEqual((ushort)(0x1234 | (ushort)EnemyProperties.Deleted), actor.Properties, "Defeated Torizo is deleted before initialization writes");
            AssertEqual((ushort)0x1111, actor.XPosition, "Defeated Torizo preserves X");
            AssertEqual((ushort)0x2222, actor.YPosition, "Defeated Torizo preserves Y");
            AssertEqual((ushort)0x3333, actor.CurrentInstruction, "Defeated Torizo preserves instruction");
        }
        Console.WriteLine("Torizo initialization: all 12 native words, 131072 real live initializations and both defeated early returns pass without ROM reads.");
    }
}
