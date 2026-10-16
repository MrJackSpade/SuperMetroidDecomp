using SuperMetroid.Core.Programs;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Fx;
using SuperMetroid.Verification.Enemies;

/// <summary>Checks Ceres steam selector definitions against native words and actual enemy initialization.</summary>
internal static class CeresSteamDefinitionsVerification
{
    /// <summary>Checks all six instruction/function pairs while forbidding migrated selector reads in production.</summary>
    internal static void VerifyCeresSteamDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa6eff5;
        const int functionTable = 0xa6f001;

        var guarded = NewCeresSteamDefinitionReadGuard(rom);
        for (int index = 0; index < 6; index++)
        {
            var variant = (CeresSteamVariant)index;
            CeresSteamInitialization definition =
                CeresSteamDefinitions.Initialization(variant);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + index * 2)),
                NativeEnemyProgramOracle.EntryOf(CeresSteamNativeListAddresses.Entries, definition.Program),
                $"Ceres steam {variant} instruction list");
            AssertEqual((CeresSteamFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord((functionTable + index * 2)),
                definition.Function,
                $"Ceres steam {variant} function");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x001f);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = (ushort)variant;
            slot.Bind(EnemyDefinitionId.Steam, slot.Definition);
            enemies.CeresSteamBehavior.InitializeCeresSteam(slot);

            AssertEqual(AnimationProgramCursor<CeresSteamProgram>.Start(definition.Program), enemies.CeresSteamPrograms[slot.SlotIndex],
                $"production Ceres steam {variant} program");
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

    /// <summary>Rejects cartridge reads across both native Ceres steam selector tables.</summary>
    private static CartridgeReadGuard NewCeresSteamDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6eff5 and < 0xa6f00d,
                address => $"Ceres steam attempted migrated selector read ${address:X6}.");
}
