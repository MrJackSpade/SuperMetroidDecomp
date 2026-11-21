using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid ceiling-rock X positions and their production spawn cadence against native data.</summary>
internal static class KraidCeilingRockPositionsVerification
{
    /// <summary>Compares every ceiling rock's X with the cartridge's word for it.</summary>
    internal static void VerifyKraidCeilingRockCoordinates(CartridgeImportAddressSpace rom)
    {
        ushort Word(int a) => LittleEndianBytes.Word(KraidCeilingRockCoordinatesRomByte(rom, a), KraidCeilingRockCoordinatesRomByte(rom, a + 1));
        // The cartridge's table holds one X word per rock.
        IReadOnlyList<KraidCeilingRock> rocks = KraidCeilingRockDefinitions.Rocks;
        AssertEqual(9, rocks.Count, "Nine ceiling rocks");
        for (int rock = 0; rock < rocks.Count; rock++)
            AssertEqual(unchecked((short)Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + rock * 2)),
                rocks[rock].X, "Native ceiling rock X");
    }

    /// <summary>Reads one bank-$A7 coordinate byte solely for the native oracle.</summary>
    private static byte KraidCeilingRockCoordinatesRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Confirms each ceiling spawn advances to the next rock and pairs its native X coordinate with the PLM mutation while source-table reads are forbidden.</summary>
    internal static void VerifyKraidCeilingRockPositions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyKraidCeilingRockCoordinates), () => VerifyKraidCeilingRockCoordinates(rom));
        ushort Word(int a) => LittleEndianBytes.Word(KraidCeilingRockPositionsRomByte(rom, a), KraidCeilingRockPositionsRomByte(rom, a + 1));
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        // His setup reads whether his room's boss is defeated, which this room's is not, and writes the screen
        // scrolls his fight frames, which this room takes.
        enemies.VerificationServices().IsAreaBossDefeated ??= () => false;
        enemies.VerificationServices().SetRoomScrollState ??= (_, _) => { };
        KraidEnemyState state = enemies.PlaceKraid();
        enemies.VerificationServices().ReadRandom = () => 63;
        // Each growth update is his body's main AI, as the enemy pass runs it, in one frame of an open room.
        IEnemyAiFrame frame = PlacedEnemyAccess.AiFrame(RoomEnemyFrameFixtures.StandaloneSamus());
        var body = enemies.Places.InUpdateOrder[0];
        body.FrameCounter = 1; // Isolate the ceiling placement from the independent rising-rock cadence.
        // Every height his rising body passes through as it breaks the ceiling, and on past his room's two screens (#2299).
        for (int raw = 1; raw <= 1024; raw++)
        for (int index = 0; index < KraidCeilingRockDefinitions.Rocks.Count; index++)
        {
            foreach (var occupied in enemies.Projectiles.EnemyProjectiles) occupied.Delete();
            enemies.PassEvents.Begin();
            body.Position = body.Position.WithWhole(new(128, (ushort)raw));
            state.BodyState.NextCeilingRock = ListStep<KraidCeilingRock>.At(KraidCeilingRockDefinitions.Rocks, index);
            state.BodyState.GrowthPhase = KraidGrowthPhase.BreakCeilingPlatforms;
            body.RunMainAi(frame);
            int y = raw - 1;
            bool spawn = (y & 3) == 0;
            AssertEqual(y, body.Position.Y.Whole, "Ceiling break moves before cadence test");
            AssertEqual(spawn ? 1 : 0, enemies.LastEnemyFrame.All<KraidRoomChange>().Count(), "Ceiling PLM cadence");
            // The rock after the last is none, the place past the list's end.
            AssertEqual(index + (spawn ? 1 : 0), state.BodyState.NextCeilingRock?.Place ?? KraidCeilingRockDefinitions.Rocks.Count,
                "Ceiling rock advances only on emission");
            if (!spawn) continue;
            var rock = enemies.Projectiles.SpawnedInPlace(17);
            AssertEqual(Word(EnemyRomTablePointers.Kraid.CeilingRockXWords + index * 2), rock.Position.X.Whole, "Native ceiling X reaches actual projectile");
            AssertEqual((ushort)312, rock.Position.Y.Whole, "Ceiling projectile origin Y");
            AssertEqual((ushort)127, rock.Velocity.Y.RawValue, "Ceiling projectile retains current RNG velocity");
            AssertEqual(KraidCeilingRockDefinitions.Rocks[index].Crumble, enemies.LastEnemyFrame.All<KraidRoomChange>().First().Request, "Rock and ceiling mutation stay paired");
        }
        Console.WriteLine("Kraid ceiling rocks: all 9 rock positions and 9216 actual growth updates, at every height his rise passes, match with placement reads forbidden.");
    }

    /// <summary>Reads a retail placement byte for the production-spawn comparison.</summary>
    private static byte KraidCeilingRockPositionsRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

}
