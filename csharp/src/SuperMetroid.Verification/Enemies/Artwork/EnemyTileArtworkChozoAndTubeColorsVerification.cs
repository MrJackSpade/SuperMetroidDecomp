using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Programs;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Chozo statue and n00b-tube palette bands against native RGB5 data and live CGRAM initialization.</summary>
internal static class EnemyTileArtworkChozoAndTubeColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks three editable color bands, persistent overrides, and real initializers while preserving native instructions and PLM requests.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyInstalledChozoAndTubeColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        ChozoAndTubeColorCatalog native = stock.ChozoAndTubeColors ??
            throw new InvalidDataException("Installed enemy art has no Chozo/tube colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(EnemySpeciesSourceDefinitions.ChozoAndTube.TubeCracksColors,
            native.ResolveTubeCracks, "tube cracks"));
        Suite(nameof(VerifyBand), () => VerifyBand(EnemySpeciesSourceDefinitions.ChozoAndTube.WreckedShipColors,
            native.ResolveWreckedShip, "Wrecked Ship Chozo"));
        Suite(nameof(VerifyBand), () => VerifyBand(EnemySpeciesSourceDefinitions.ChozoAndTube.LowerNorfairColors,
            native.ResolveLowerNorfair, "Lower Norfair Chozo"));

        string file = Path.Combine(stockDirectory, ChozoAndTubeColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        ChozoAndTubeColorDocument visual =
            JsonSerializer.Deserialize<ChozoAndTubeColorDocument>(stockJson,
                CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Chozo/tube color JSON is null.");
        visual.TubeCracks[5] = ChangeRed(visual.TubeCracks[5]);
        visual.WreckedShip[5] = ChangeRed(visual.WreckedShip[5]);
        visual.LowerNorfair[5] = ChangeRed(visual.LowerNorfair[5]);
        string overrides = Path.Combine(stockDirectory, "chozo-tube-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, ChozoAndTubeColorFormat.FileName);
        File.WriteAllBytes(overrideFile, ChozoAndTubeColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        ChozoAndTubeColorCatalog colors = edited.Enemies.ChozoAndTubeColors ??
            throw new InvalidDataException("Edited enemy art has no Chozo/tube colors.");
        CheckEditedBand(native.ResolveTubeCracks, colors.ResolveTubeCracks, "tube cracks");
        CheckEditedBand(native.ResolveWreckedShip, colors.ResolveWreckedShip,
            "Wrecked Ship Chozo");
        CheckEditedBand(native.ResolveLowerNorfair, colors.ResolveLowerNorfair,
            "Lower Norfair Chozo");
        AssertEqual(colors.ResolveWreckedShip(5),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Enemies.ChozoAndTubeColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .ChozoAndTubeColors.")).ResolveWreckedShip(5),
            "Chozo/tube override survives catalog reload");

        // Every live initializer must use the installed data, while variant selection,
        // instruction identity and native PLM requests remain engine-owned.
        Type type = typeof(RoomEnemySystem);
        var guarded = NewChozoAndTubePaletteReadGuard(rom);
        Suite(nameof(VerifyLive), () => VerifyLive(0, colors.ResolveTubeCracks,
            "n00b-tube cracks", (enemies, _) => enemies.ChozoStatueBehavior.InitializeN00bTubeCracks(), null));
        Suite(nameof(VerifyLive), () => VerifyLive(0, colors.ResolveWreckedShip,
            "Wrecked Ship Chozo", (enemies, statue) => enemies.ChozoStatueBehavior.InitializeChozoStatue(statue),
            ChozoStatueProgram.WreckedShipInitial));
        Suite(nameof(VerifyLive), () => VerifyLive(2, colors.ResolveLowerNorfair,
            "Lower Norfair Chozo", (enemies, statue) => enemies.ChozoStatueBehavior.InitializeChozoStatue(statue),
            ChozoStatueProgram.LowerNorfairInitial));

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Chozo/tube override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Chozo/tube color property fails loudly");
        visual.TubeCracks[5] = visual.TubeCracks[5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => ChozoAndTubeColorFile.Write(visual),
            "Chozo/tube RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Chozo/tube colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Chozo/tube colors: 96 native RGB5 words, all three live initializers, persistent overrides, ROM guard, and strict failures pass.");

        void VerifyBand(int source, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed {name} color {color} preserves native RGB5");
        }

        void VerifyLive(ushort variant, Func<int, RgbColor> resolve, string name,
            Action<RoomEnemySystem, RoomEnemySlot> initialize, ChozoStatueProgram? expectedProgram)
        {
            var cgram = new SamusColourRig();
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
            enemies.Presentation.BindColours(cgram.Gameplay);
            RoomEnemySlot statue = enemies.SlotTable.Slots[0];
            statue.SetSpawnParameter2(variant);
            initialize(enemies, statue);
            for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
                AssertEqual(resolve(color),
                    cgram.Colors[ChozoAndTubeColorTransfer.Destination + color],
                    $"live {name} CGRAM color {color}");
            if (expectedProgram is not null)
            {
                AssertEqual(AnimationProgramCursor<ChozoStatueProgram>.Start(expectedProgram.Value),
                    (statue.SpeciesState<ChozoStatueState>()
                        ?? throw new InvalidOperationException($"{name} has no statue state.")).Program,
                    $"{name} keeps native animation selection");
                AssertTrue(enemies.ChozoStatuePlmRequests.Count != 0,
                    $"{name} still publishes its native PLM request");
            }
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckEditedBand(Func<int, RgbColor> original,
            Func<int, RgbColor> changed, string name)
        {
            AssertTrue(original(5) != changed(5),
                $"{name} edit changes selected RGB5 word");
            for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
                if (color != 5)
                    AssertEqual(original(color), changed(color),
                        $"{name} edit leaves color {color} unchanged");
        }
    }

    /// <summary>Rejects reads of the migrated palette region during enemy initialization.</summary>
    private static CartridgeReadGuard NewChozoAndTubePaletteReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= EnemySpeciesSourceDefinitions.ChozoAndTube.TubeCracksColors and < (EnemySpeciesSourceDefinitions.ChozoAndTube.LowerNorfairColors + ChozoAndTubeColorDefinitions.ColorCount * sizeof(ushort)),
                address => $"Chozo/tube accessed migrated palette ROM ${address:X6}.");
}
