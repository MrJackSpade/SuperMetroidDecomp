using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Ceres;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Ceres steam selector definitions against native words and actual enemy initialization.</summary>
internal static class CeresSteamDefinitionsVerification
{
    /// <summary>Checks all six instruction/function pairs while forbidding migrated selector reads in production.</summary>
    internal static void VerifyCeresSteamDefinitions(CartridgeImportAddressSpace rom)
    {
        const int instructionTable = 0xa6eff5;
        const int functionTable = 0xa6f001;

        var guarded = NewCeresSteamDefinitionReadGuard(rom);
        // The vents' ways in the order of the cartridge's tables, which a vent's first spawn parameter indexes.
        CeresSteamVariant[] variants = [CeresSteamVariant.Up, CeresSteamVariant.Left, CeresSteamVariant.Down, CeresSteamVariant.Right, CeresSteamVariant.RotatingElevatorLeft, CeresSteamVariant.RotatingElevatorRight];
        for (int index = 0; index < variants.Length; index++)
        {
            CeresSteamVariant variant = variants[index];
            CeresSteamInitialization definition =
                CeresSteamInitializations.Initialization(variant);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(CeresSteamNativeListAddresses.Entries, definition.Animation),
                $"Ceres steam {variant} instruction list");
            AssertEqual(CeresNativeWords.SteamActions.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(functionTable + index * 2), "native Ceres steam function"),
                definition.Action,
                $"Ceres steam {variant} function");

            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => 0x001f;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.SetSpawnParameter1(EnemyDefinitionId.Steam, (ushort)index);
            slot.Bind(EnemyDefinitionId.Steam, slot.Definition);
            enemies.CeresSteamBehavior.InitializeCeresSteam(slot);

            AssertEqual(new AnimationPlayback<CeresSteamSegment>(CeresSteamAnimations.StartingWith(definition.Animation), AnimationPlace.StartOfAnimation), slot.RequireSpeciesState<CeresSteamEnemyState>().Playback,
                $"production Ceres steam {variant} program");
            CeresSteamEnemyState steamState = slot.SpeciesState<CeresSteamEnemyState>()
                ?? throw new InvalidOperationException("The initialized vent has no state.");
            AssertEqual<CeresSteamAction?>(definition.Action, steamState.Action,
                $"production Ceres steam {variant} function");
            AssertEqual(WrappingCountdown.Start(0x0020), steamState.ActivationDelay,
                $"production Ceres steam {variant} activation timer");
        }

        AssertThrows<InvalidDataException>(
            () => CeresSteamInitializations.Initialization((CeresSteamVariant)6),
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
