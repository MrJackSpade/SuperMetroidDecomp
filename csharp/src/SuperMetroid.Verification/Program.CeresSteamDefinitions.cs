using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class CeresSteamDefinitionsVerification
{
    internal static void VerifyCeresSteamDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa6eff5;
        const int functionTable = 0xa6f001;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeCeresSteam", flags)!;

        var guarded = NewCeresSteamDefinitionReadGuard(rom);
        for (int index = 0; index < 6; index++)
        {
            var variant = (CeresSteamVariant)index;
            CeresSteamInitialization definition =
                CeresSteamDefinitions.Initialization(variant);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + index * 2)),
                definition.InstructionList,
                $"Ceres steam {variant} instruction list");
            AssertEqual((CeresSteamFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord((functionTable + index * 2)),
                definition.Function,
                $"Ceres steam {variant} function");

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x001f);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = (ushort)variant;
            initialize.Invoke(enemies, [slot]);

            AssertEqual(definition.InstructionList, slot.CurrentInstruction,
                $"production Ceres steam {variant} instruction list");
            AssertEqual(definition.Function, (CeresSteamFunction)slot.VariableA,
                $"production Ceres steam {variant} function");
            AssertEqual(0x0020, slot.VariableD,
                $"production Ceres steam {variant} activation timer");
        }

        AssertThrows<InvalidDataException>(
            () => CeresSteamDefinitions.Initialization((CeresSteamVariant)6),
            "Ceres steam variant past tables");
        Console.WriteLine(
            "Ceres steam definitions: twelve native words and all six production initialization paths pass with both selector tables forbidden.");
    }

    private static CartridgeReadGuard NewCeresSteamDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6eff5 and < 0xa6f00d,
                address => $"Ceres steam attempted migrated selector read ${address:X6}.");
}
