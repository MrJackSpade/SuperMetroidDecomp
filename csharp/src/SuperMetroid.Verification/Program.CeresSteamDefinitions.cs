using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents ceres steam definitions verification.</summary>
internal static class CeresSteamDefinitionsVerification
{
    /// <summary>Verifies ceres steam definitions.</summary>
    internal static void VerifyCeresSteamDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa6eff5;
        const int functionTable = 0xa6f001;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

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
            enemies.BindFixtureBus(guarded);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x001f);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = (ushort)variant;
            enemies.CeresSteamBehavior.InitializeCeresSteam(slot);

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

    /// <summary>Creates ceres steam definition read guard.</summary>
    private static CartridgeReadGuard NewCeresSteamDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6eff5 and < 0xa6f00d,
                address => $"Ceres steam attempted migrated selector read ${address:X6}.");
}
