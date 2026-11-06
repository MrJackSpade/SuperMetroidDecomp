using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Zebetite's eight-frame two-color pulse against native
/// CGRAM updates while preserving its runtime gates and counter.</summary>
internal static class EnemyTileArtworkZebetiteColorsVerification
{
    /// <summary>The colour the Zebetites' pulse starts at: the second enemy row's colour twelve.</summary>
    private const int PulseFirstColour = 0xac;

    /// <summary>Compares live pulse frames with cartridge colors, then checks the engine gates and malformed asset rejection.</summary>
    internal static void VerifyInstalledZebetiteColors(ISnesAddressSpace bus,
        string stockDirectory, InstalledEnemyArtwork stock)
    {
        if (stock.Bosses.ZebetiteColors is null)
            throw new InvalidDataException("Installed enemy artwork lacks Zebetite pulse colors.");
        SnesCgram nativeCgram = SeedColors();
        ushort nativeCounter = 0;
        CartridgeReadGuard guard = new CartridgeReadGuard(bus).Forbid(
            address => address is >= MotherBrainSourceDefinitions.Zebetite.PulseColors and
                < (MotherBrainSourceDefinitions.Zebetite.PulseColors +
                    ZebetiteColorFormat.FrameCount * ZebetiteColorFormat.ColorsPerFrame *
                    sizeof(ushort)),
            address => $"Installed Zebetite pulse read migrated ROM color ${address:X6}.");
        RoomEnemySystem installed = Create(guard, stock);
        for (int call = 0; call < ZebetiteColorFormat.FrameCount * 2; call++)
        {
            ReferenceCycle(nativeCgram, ref nativeCounter);
            Cycle(installed);
            AssertEqual((call + 1) % ZebetiteDefinitions.PaletteCycleLength,
                Counter(installed),
                "Installed Zebetite pulse retains physical-slot-zero counter and wrap");
            AssertEqual(nativeCounter, Counter(installed),
                "Installed Zebetite pulse keeps native frame selection");
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(Shown(installed, stock))),
                "Installed Zebetite pulse preserves full native CGRAM and neighbors");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Installed Zebetite pulse never reads migrated ROM colors");

        byte[] stockBytes = File.ReadAllBytes(Path.Combine(stockDirectory, ZebetiteColorFile.FileName));
        RoomEnemySystem selected = Create(guard, stock);
        selected.ZebetiteBehavior.PaletteChangeNumber = 1;
        int counterBeforeGate = Counter(selected);
        Cycle(selected);
        AssertEqual(counterBeforeGate, Counter(selected),
            "Zebetite palette-change gate remains an engine-owned condition");
        selected.ZebetiteBehavior.PaletteChangeNumber = 0;
        (selected.SlotTable.Slots[0].SpeciesState<ZebetiteEnemyState>() ?? throw new InvalidOperationException("Verification requires the primary Zebetite state.")).IsSecondaryHalf = true;
        Cycle(selected);
        AssertEqual(counterBeforeGate, Counter(selected),
            "Zebetite secondary-half gate remains an engine-owned condition");

        var invalid = RequiredJson.Parse(stockBytes);
        invalid.Required("frames").Required(0).Required(0)["green"] = 32;
        AssertThrows<InvalidDataException>(() => ZebetiteColorFile.Read(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Zebetite RGB5 component rejected");
        Console.WriteLine("  Zebetite colors: 16 native RGB5 words, sixteen live pulse calls, full CGRAM, ROM guard, gates and channel rejection pass.");

        static RoomEnemySystem Create(
            ISnesAddressSpace source, InstalledEnemyArtwork artwork)
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
            RoomEnemySlot primarySlot = enemies.SlotTable.Slots[0];
            primarySlot.SetSpawnSettings(new ZebetiteSpawnSettings(ZebetiteHalf.Primary));
            var primary = new ZebetiteEnemyState(primarySlot.Spawn.Population);
            primarySlot.SetSpeciesState(primary, primary.ClearWithRecord);
            return enemies;
        }

        // The import's shade of pulse frame <frame> among <artwork>'s Zebetite shades (#1512).
        static SheetShade Pulse(InstalledEnemyArtwork artwork, int frame) =>
            MotherBrainSheetShades.Zebetites(artwork.Bosses).Single(shade => shade.Shade == ZebetiteShades.Pulse(frame));

        // The colours the Zebetites show over the seeded colours: the pulse frame last shown, as the import pre-coloured it.
        static SnesCgram Shown(RoomEnemySystem enemies, InstalledEnemyArtwork artwork)
        {
            SnesCgram cgram = SeedColors();
            if (enemies.ZebetiteBehavior.PulseShown is int frame)
            {
                SheetShade pulse = Pulse(artwork, frame);
                for (int index = 0; index < pulse.Inks.Length; index++)
                    cgram.SetColor(BossShadeColours.Sprites + pulse.Inks[index], pulse.Colours[index]);
            }
            return cgram;
        }

        static int Counter(RoomEnemySystem enemies) => enemies.ZebetiteBehavior.SharedPaletteCycle;

        // Independent $A6:FD5E pulse: advance modulo eight, copy two raw colors.
        void ReferenceCycle(SnesCgram cgram, ref ushort counter)
        {
            counter = (ushort)((counter + 1) & 7);
            for (int color = 0; color < 2; color++)
            {
                int address = 0xa6fd87 + counter * 4 + color * 2;
                cgram.SetColor(PulseFirstColour + color,
                    RgbColor.FromWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(address)));
            }
        }

        static SnesCgram SeedColors()
        {
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, RgbColor.FromWord((ushort)(color * 31)));
            return cgram;
        }

        static void Cycle(RoomEnemySystem enemies)
        {
            enemies.ZebetiteBehavior.CycleZebetitePalette(enemies.SlotTable.Slots[0]);
        }
    }
}
