using SuperMetroid.Core.Assets;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Animals.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Dachora's default, speed, and shine palette frames against installed art and native selectors.</summary>
internal static class EnemyTileArtworkDachoraColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises installed RGB5 colors through live CGRAM updates while preserving phase and next-frame selection.</summary>
    internal static void VerifyInstalledDachoraColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        DachoraColorCatalog native = stock.Enemies.DachoraColors ??
            throw new InvalidDataException("Installed enemy art has no Dachora colors.");
        Suite(nameof(VerifyFrame), () => VerifyFrame(DachoraColourPhase.Default, 0));
        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        {
            VerifyFrame(DachoraColourPhase.Speed, frame);
            VerifyFrame(DachoraColourPhase.Shine, frame);
            AssertEqual(unchecked((ushort)DachoraColorSources.Source(
                    DachoraColourPhase.Speed, frame)),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    DachoraColorSources.SpeedPointerTable + frame * sizeof(ushort)),
                $"native Dachora speed selector {frame}");
            AssertEqual(unchecked((ushort)DachoraColorSources.Source(
                    DachoraColourPhase.Shine, frame)),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    DachoraColorSources.ShinePointerTable + frame * sizeof(ushort)),
                $"native Dachora shine selector {frame}");
        }

        string file = Path.Combine(stockDirectory, DachoraColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        DachoraColorDocument visual = JsonSerializer.Deserialize<DachoraColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Dachora color JSON is null.");

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => address is (>= EnemySpeciesSourceDefinitions.Dachora.DefaultColors and
                    < EnemySpeciesSourceDefinitions.Dachora.ShineColors +
                        (DachoraColorDefinitions.AnimatedFrameCount * EnemySpeciesSourceDefinitions.Dachora.ImageBytes))
                or (>= DachoraColorSources.SpeedPointerTable and
                    < DachoraColorSources.SpeedPointerTable + (DachoraColorDefinitions.AnimatedFrameCount * sizeof(ushort)))
                or (>= DachoraColorSources.ShinePointerTable and
                    < DachoraColorSources.ShinePointerTable + (DachoraColorDefinitions.AnimatedFrameCount * sizeof(ushort))),
            address => $"Live Dachora read migrated color/selector ROM ${address:X6}.");
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        Type type = typeof(RoomEnemySystem);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(96, 96));
        slot.Radius = new(8, 8);
        var state = new DachoraEnemyState();
        DachoraBehavior.ShowDachoraColours(slot, DachoraColourPhase.Default, 0);
        CheckLive(DachoraColourPhase.Default, 0);

        RoomLevelData empty = VerificationHelpersVerification.CreateEmptyRoom(new(16, 16));
        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        {
            state.Speed = SignedSixteenSixteen.FromParts(8, 0);
            state.SetGlowCycle(frame, ticks: 1);
            enemies.DachoraBehavior.AccelerateDachora(slot, state, empty);
            CheckLive(DachoraColourPhase.Speed, frame);
        }

        for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
        {
            state.SetGlowCycle(frame, ticks: 0);
            DachoraBehavior.StepDachoraShineGlow(slot, state);
            CheckLive(DachoraColourPhase.Shine, frame);
            AssertEqual((frame + 1) % 4, state.GlowFrame,
                $"Dachora shine frame {frame} retains native next-frame selection");
            AssertEqual(0, state.GlowTicks,
                $"Dachora shine frame {frame} keeps no speed ticks");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Dachora color consumers avoid RGB5 and selector ROM reads");

        visual.Speed[0][5] = visual.Speed[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => DachoraColorFile.Read(new MemoryStream(DachoraColorFile.Write(visual))),
            "Dachora RGB5 channel outside five-bit precision is rejected");
        Console.WriteLine("  Dachora colors: 144 native RGB5 words, eight compiled selector pointers, live speed/shine phase selection, ROM guard and strict failures pass.");

        void VerifyFrame(DachoraColourPhase phase, int frame)
        {
            int source = DachoraColorSources.Source(phase, frame);
            for (int color = 0; color < DachoraColorFormat.ColorsPerFrame; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * sizeof(ushort)),
                    native.Resolve(phase, frame, color),
                    $"installed Dachora {phase} frame {frame} color {color}");
        }

        void CheckLive(DachoraColourPhase phase, int frame)
        {
            // The Dachora shows its phase's frame as the shade its draws publish (#1512); the colours are the import's.
            AssertEqual(PictureShading.Shaded(DachoraShades.Of(phase, frame)), slot.Shading,
                $"live Dachora {phase} frame {frame} shade");
        }
    }
}
