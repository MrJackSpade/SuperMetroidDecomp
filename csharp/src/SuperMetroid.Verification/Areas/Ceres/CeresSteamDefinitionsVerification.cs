using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
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
            AssertEqual(CeresNativeWords.DecodeSteamAction(CartridgeImportSource.Require(rom).ReadCartridgeWord(functionTable + index * 2), "native Ceres steam function"),
                definition.Action,
                $"Ceres steam {variant} function");

            // The rotating-elevator vents ride the turn of the shaft they stand in, which only the shaft's room has (#2426).
            var shaft = new CeresElevatorShaftRoomMainState(new RoomMainCarryover());
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null),
                shaftTurn: shaft);
            enemies.VerificationServices().NextRandom = () => 0x001f;
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.SetSpawnParameter1(EnemyDefinitionId.CeresSteam, (ushort)index);
            place.Bind(EnemyDefinitionId.CeresSteam, place.Definition);
            enemies.CeresSteamBehavior.InitializeCeresSteam(place);

            AssertEqual(new AnimationPlayback<CeresSteamSegment>(CeresSteamAnimations.StartingWith(definition.Animation), AnimationPlace.StartOfAnimation), place.RequireSpeciesState<CeresSteamEnemyState>().Playback,
                $"production Ceres steam {variant} program");
            CeresSteamEnemyState steamState = place.SpeciesState<CeresSteamEnemyState>()
                ?? throw new InvalidOperationException("The initialized vent has no state.");
            AssertEqual<CeresSteamMotion>(definition.Action == CeresSteamAction.ApplyRotatingElevatorOffset
                    ? new CeresSteamMotion.RidesShaftTurn(shaft)
                    : CeresSteamMotion.HoldsItsPlace.Instance,
                steamState.Motion, $"production Ceres steam {variant} motion carries out its function");
            AssertEqual(32, steamState.UpdatesToActivation,
                $"production Ceres steam {variant} waits the drawn 32 updates, the one it appears on included");
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
