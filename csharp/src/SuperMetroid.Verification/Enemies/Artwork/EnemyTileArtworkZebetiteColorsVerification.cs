using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
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

    /// <summary>Compares live pulse frames with cartridge colors, then checks
    /// one edited channel, override persistence, and malformed asset rejection.</summary>
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

        string stockPath = Path.Combine(stockDirectory, ZebetiteColorFile.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "zebetite-color-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, ZebetiteColorFile.FileName);
        JsonNode editedDocument = RequiredJson.Parse(stockBytes);
        JsonNode red = editedDocument.Required("frames").Required(3).Required(1).Required("red");
        editedDocument.Required("frames").Required(3).Required(1)["red"] = red.GetValue<int>() ^ 1;
        byte[] selectedBytes = System.Text.Encoding.UTF8.GetBytes(editedDocument.ToJsonString());
        File.WriteAllBytes(overridePath, selectedBytes);
        InstalledEnemyArtwork edited = stock with { Bosses = stock.Bosses with { ZebetiteColors = EnemyTileArtworkFiles.Open(stockDirectory, overrides).ZebetiteColors() } };
        RoomEnemySystem selected = Create(guard, edited);
        SnesCgram controlCgram = SeedColors();
        ushort controlCounter = 0;
        for (int call = 0; call < 2; call++)
        {
            Cycle(selected);
            ReferenceCycle(controlCgram, ref controlCounter);
            AssertTrue(new CgramPalettes(controlCgram).ImageEquals(new CgramPalettes(Shown(selected, edited))),
                "Zebetite edit leaves preceding pulse frames unchanged");
        }
        Cycle(selected);
        ReferenceCycle(controlCgram, ref controlCounter);
        SnesCgram selectedCgram = Shown(selected, edited);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            AssertEqual((ushort)(controlCgram.Colors[color].ToWord() ^
                (color == PulseFirstColour + 1 ? 1 : 0)),
                selectedCgram.Colors[color],
                "Zebetite visual edit changes only one color channel at the selected frame");
        AssertEqual(controlCounter, Counter(selected),
            "Zebetite color edit does not change animation counter");
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

        EnemyTileStockReExtraction.ReExtractStock(bus, stockDirectory);
        AssertTrue(selectedBytes.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Stock re-extraction preserves the Zebetite color override");
        InstalledEnemyArtwork reloaded = stock with { Bosses = stock.Bosses with { ZebetiteColors = EnemyTileArtworkFiles.Open(stockDirectory, overrides).ZebetiteColors() } };
        AssertEqual(Pulse(edited, 3).Colours[1], Pulse(reloaded, 3).Colours[1],
            "Reload retains edited Zebetite pulse color");

        File.WriteAllText(overridePath, "broken");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(stockDirectory, overrides).ZebetiteColors(),
            "Corrupt Zebetite color override fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1}");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(stockDirectory, overrides).ZebetiteColors(),
            "Duplicate Zebetite color property fails loudly");
        var invalid = RequiredJson.Parse(stockBytes);
        invalid.Required("frames").Required(0).Required(0)["green"] = 32;
        AssertThrows<InvalidDataException>(() => ZebetiteColorFile.Read(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Zebetite RGB5 component rejected");
        File.Delete(overridePath);
        File.WriteAllText(stockPath, "broken stock");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(stockDirectory, null).ZebetiteColors(),
            "Corrupt stock Zebetite colors fail manifest validation");
        File.WriteAllBytes(stockPath, stockBytes);
        Console.WriteLine("  Zebetite colors: 16 native RGB5 words, sixteen live pulse calls, full CGRAM, ROM guard, isolated edit, gates, persistence and strict failures pass.");

        static RoomEnemySystem Create(
            ISnesAddressSpace source, InstalledEnemyArtwork artwork)
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
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
