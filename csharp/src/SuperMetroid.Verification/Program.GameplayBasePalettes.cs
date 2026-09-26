using System.Text.Json;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGameplayBasePalettes(string sourceRom)
    {
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "gameplay-base-palettes-" + Guid.NewGuid().ToString("N")));
        string stock = Path.Combine(directory, "stock");
        string overrides = Path.Combine(directory, "overrides");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
        try
        {
            GameplayBasePaletteFiles.Extract(bus, stock, SupportedCartridge.Sha256);
            GameplayBasePaletteCatalog baseline = GameplayBasePaletteFiles.Load(stock, overrides);
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                AssertEqual(ReadWord(bus, GameplayBasePaletteFormat.InitialSourceAddress + color * 2),
                    baseline.Initial[color], $"installed starting CGRAM color {color}");
            for (int color = 0; color < GameplayBasePaletteFormat.SpriteColorCount; color++)
                AssertEqual(ReadWord(bus, GameplayBasePaletteFormat.CommonSpriteSourceAddress + color * 2),
                    baseline.CommonSprites[color], $"installed shared OBJ color {color}");

            var guarded = new FrontendCartridgeReadGuard(
                SuperMetroidAddressSpace.LoadRetailRom(sourceRom));
            var runtime = new SuperMetroidRuntime(guarded, initialPaletteArt: baseline);
            AssertTrue(runtime.Cgram.Colors.SequenceEqual(baseline.Initial.ToArray()),
                "runtime construction consumes installed starting colors without cartridge reads");
            runtime.Cgram.SetColor(128, 0);
            runtime.Cgram.SetColor(208, 0);
            // Exercise the production room-load restore, not just the catalog helpers.
            MethodInfo restore = typeof(SuperMetroidRuntime).GetMethod("LoadGameplaySpritePalettes",
                BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException(
                    "Runtime room-entry palette restore was not found.");
            restore.Invoke(runtime, null);
            for (int color = 0; color < GameplayBasePaletteFormat.SpriteColorCount; color++)
            {
                AssertEqual(baseline.CommonSprites[color], runtime.Cgram.Colors[128 + color],
                    $"room-entry shared OBJ color {color}");
                AssertEqual(baseline.Initial[208 + color], runtime.Cgram.Colors[208 + color],
                    $"room-entry enemy-projectile OBJ color {color}");
            }

            var document = JsonSerializer.Deserialize<GameplayBasePaletteDocument>(
                File.ReadAllBytes(Path.Combine(stock, GameplayBasePaletteFormat.ArtworkFileName)),
                GameplayBasePaletteFormat.JsonOptions) ?? throw new InvalidDataException(
                    "Stock gameplay base palette document is null.");
            document.Initial[1] = document.Initial[1] with { Red =
                document.Initial[1].Red == 31 ? 30 : 31 };
            document.CommonSprites[1] = document.CommonSprites[1] with { Blue =
                document.CommonSprites[1].Blue == 31 ? 30 : 31 };
            Directory.CreateDirectory(overrides);
            string overridePath = Path.Combine(overrides, GameplayBasePaletteFormat.ArtworkFileName);
            File.WriteAllBytes(overridePath, GameplayBasePaletteCatalog.Write(document));
            GameplayBasePaletteCatalog edited = GameplayBasePaletteFiles.Load(stock, overrides);
            var editedRuntime = new SuperMetroidRuntime(guarded, initialPaletteArt: edited);
            AssertEqual(edited.Initial[1], editedRuntime.Cgram.Colors[1],
                "edited starting color reaches live CGRAM");
            AssertTrue(editedRuntime.Cgram.Colors[1] != runtime.Cgram.Colors[1],
                "edited starting color differs from stock");
            restore.Invoke(editedRuntime, null);
            AssertEqual(edited.CommonSprites[1], editedRuntime.Cgram.Colors[129],
                "edited shared sprite color reaches room-entry CGRAM");

            document.Initial[1] = document.Initial[1] with { Red = 32 };
            File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
                GameplayBasePaletteFormat.JsonOptions));
            AssertThrows<InvalidDataException>(() => GameplayBasePaletteFiles.Load(stock, overrides),
                "invalid RGB5 override fails loudly");
            File.Delete(overridePath);
            GameplayBasePaletteCatalog restored = GameplayBasePaletteFiles.Load(stock, overrides);
            AssertTrue(restored.Initial.SequenceEqual(baseline.Initial) &&
                restored.CommonSprites.SequenceEqual(baseline.CommonSprites),
                "removing override restores stock palette");
            Console.WriteLine("Gameplay base palettes: 256+16 cartridge colors, guarded runtime construction, room-entry restores, override and malformed data pass.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }

        static ushort ReadWord(ISnesAddressSpace addressSpace, int address) =>
            (ushort)(addressSpace.ReadByte(address) | addressSpace.ReadByte(address + 1) << 8);
    }
}
