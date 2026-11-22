using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
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
            AssertEqual(rom.ReadCartridgeWord((instructionTable + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(CeresSteamNativeListAddresses.EntriesOf(definition.Way), CeresSteamSegment.Dormant),
                $"Ceres steam {variant} instruction list");
            AssertEqual(CeresNativeWords.DecodeSteamAction(rom.ReadCartridgeWord(functionTable + index * 2), "native Ceres steam function"),
                definition.Action,
                $"Ceres steam {variant} function");

            // The rotating-elevator vents ride the turn of the shaft they stand in, which only the shaft's room has (#2426).
            var shaft = new CeresElevatorShaftRoomMainState(new RoomMainHandOvers().ShaftTurnToDebris);
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null),
                shaftTurn: shaft);
            enemies.VerificationServices().NextRandom = () => 0x001f;
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.SetSpawnParameter1(EnemyDefinitionId.SteamVent, (ushort)index);
            place.Bind(EnemyDefinitionId.SteamVent, place.KeptDefinition);
            ((IEnemyInitialization)(place.Entity ?? throw new InvalidOperationException("The vent bound no entity."))).Initialize();

            // The vent's motion carries out its function: a vent riding the shaft's turn is drawn where the turn carries the
            // point it stands on, and one holding its place is drawn where it stands.
            SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
            Position16<RoomPixels> standing = place.Position.Whole;
            place.RunMainAi(samus);
            AssertEqual(definition.Action == CeresSteamAction.ApplyRotatingElevatorOffset
                    ? shaft.Transform.Transform(standing) - standing
                    : default,
                place.DrawOffset, $"production Ceres steam {variant} motion carries out its function");

            // The dormant program hides the vent for the drawn 32 updates and shows the puff's first picture on the next.
            bool Hidden() => place.Presence.HasAny(EnemyPresence.Hidden) && place.Contact.HasAny(EnemyContact.IgnoresSamus);
            for (int update = 1; update <= 32; update++)
            {
                place.AdvanceAnimation(samus);
                AssertTrue(Hidden(), $"production Ceres steam {variant} stays hidden on dormant update {update}");
            }
            place.AdvanceAnimation(samus);
            AssertTrue(!Hidden(), $"production Ceres steam {variant} appears after the drawn 32 updates");
            AssertEqual<EnemyFrameId?>(definition.Way.Segments.Beats(CeresSteamSegment.Puffing)[0].ShownFrame, place.Frame,
                $"production Ceres steam {variant} program puffs as it appears");
        }

        Console.WriteLine(
            "Ceres steam definitions: twelve native words and all six production initialization paths pass with both selector tables forbidden.");
    }

    /// <summary>Rejects cartridge reads across both native Ceres steam selector tables.</summary>
    private static CartridgeReadGuard NewCeresSteamDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6eff5 and < 0xa6f00d,
                address => $"Ceres steam attempted migrated selector read ${address:X6}.");
}
