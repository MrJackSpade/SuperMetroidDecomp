using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEnemyPickupDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            var kind = (EnemyPickupKind)index;
            EnemyPickupAnimationDefinition animation = EnemyPickupDefinitions.Animation(kind);
            AssertEqual(index * 2, animation.TableOffset,
                $"enemy pickup table offset {kind}");
            AssertEqual(ReadEnemyPickupDefinitionWord(rom, 0x86ef04 + index * 2),
                animation.InstructionList,
                $"enemy pickup instruction {kind}");
        }

        MethodInfo begin = typeof(RoomEnemySystem).GetMethod(
            "BeginEnemyPickup", BindingFlags.Static | BindingFlags.NonPublic)!;
        for (int index = 1; index <= 5; index++)
        {
            var kind = (EnemyPickupKind)index;
            var projectile = new RoomEnemyProjectileSlot(index)
            {
                Variable0 = 0xffff,
                Variable1 = 0xffff,
                InstructionPointer = 0xffff,
                InstructionTimer = 0xffff,
                PreInstruction = 0xffff,
                PersistsOnSamusContact = true,
            };
            begin.Invoke(null, [projectile, kind]);
            EnemyPickupAnimationDefinition expected = EnemyPickupDefinitions.Animation(kind);
            AssertEqual(expected.TableOffset, projectile.Variable0,
                $"production pickup table offset {kind}");
            AssertEqual(expected.InstructionList, projectile.InstructionPointer,
                $"production pickup instruction {kind}");
            AssertEqual(1, projectile.InstructionTimer,
                $"production pickup instruction timer {kind}");
            AssertEqual(400, projectile.Variable1,
                $"production pickup lifetime {kind}");
            AssertEqual(EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_Pickup,
                projectile.PreInstruction,
                $"production pickup pre-instruction {kind}");
            AssertTrue(!projectile.PersistsOnSamusContact,
                $"production pickup contact persistence {kind}");
        }

        AssertThrows<InvalidDataException>(
            () => EnemyPickupDefinitions.Animation(EnemyPickupKind.NoDrop),
            "no-drop pickup has no animation entry");
        Console.WriteLine(
            "Enemy pickup definitions: six native selectors and all five live pickup initialization paths pass without the pointer table.");
    }

    private static ushort ReadEnemyPickupDefinitionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
