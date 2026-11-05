using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks that Magdollite's extracted color cycle can be edited without changing its draw-hook cadence or reading cartridge data at runtime.
/// </summary>
internal static class EnemyTileArtworkMagdollitePaletteVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Compares installed RGB5 colors with the cartridge, then exercises an override through the room enemy draw hook and CGRAM.
    /// </summary>
    internal static void VerifyInstalledMagdollitePaletteCycle(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        MagdollitePaletteCycle native = stock.Enemies.MagdollitePaletteCycle ??
            throw new InvalidDataException("Installed enemy art has no Magdollite palette cycle.");
        for (int frame = 0; frame < MagdollitePaletteDefinitions.FrameCount; frame++)
        for (int color = 0; color < MagdollitePaletteCycle.AnimatedColorCount; color++)
        {
            int address = EnemySpeciesSourceDefinitions.Magdollite.CycleColors +
                (frame * EnemySpeciesSourceDefinitions.Magdollite.RowColors +
                 MagdollitePaletteCycle.FirstAnimatedColor + color) * sizeof(ushort);
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address),
                native.Resolve(frame, color),
                $"Magdollite palette frame {frame} color {color} preserves native RGB5");
        }

        string file = Path.Combine(stockDirectory, MagdollitePaletteCycleFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        MagdollitePaletteCycleDocument document =
            JsonSerializer.Deserialize<MagdollitePaletteCycleDocument>(stockJson,
                CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Magdollite palette JSON is null.");
        PaletteRgb5 original = document.Frames[1][0];
        // Exercise the actual global graphics-drawn hook, not a palette helper.
        // Forbid all bus reads: installed artwork must be sufficient on every tick.
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        // The hook names the glow frame every Magdollite shows (#1512); the frame's colours are the import's shade.
        enemies.MagdolliteBehavior.MagdollitePaletteAnimationInstalled = true;
        enemies.MagdolliteBehavior.MagdollitePaletteAnimationTimer = WrappingCountdown.Start(8);
        enemies.MagdolliteBehavior.MagdollitePaletteFrame = 0;
        Action step = enemies.MagdolliteBehavior.StepMagdollitePaletteAnimation;
        RoomEnemySlot untouched = enemies.SlotTable.Slots[0];
        int healthBefore = untouched.Health;
        ushort xBefore = untouched.Position.X.Whole;
        for (int tick = 0; tick < 32; tick++)
        {
            PictureShading before = enemies.MagdolliteBehavior.MagdolliteGlow;
            step();
            if ((tick + 1) % 8 != 0)
            {
                AssertEqual(before, enemies.MagdolliteBehavior.MagdolliteGlow,
                    $"Magdollite glow does not change before tick {tick + 1} reaches the hook boundary");
            }
            else
            {
                int frame = (tick + 1) / 8 & (MagdollitePaletteDefinitions.FrameCount - 1);
                AssertEqual(PictureShading.Shaded(MagdolliteShades.Glow(frame)), enemies.MagdolliteBehavior.MagdolliteGlow,
                    $"Magdollite draw-hook tick {tick + 1} glow frame");
            }
        }
        AssertEqual(healthBefore, untouched.Health,
            "the Magdollite glow cycle does not change enemy health");
        AssertEqual(xBefore, untouched.Position.X.Whole,
            "the Magdollite glow cycle does not change enemy position");
        AssertEqual((ushort)8,
            enemies.MagdolliteBehavior.MagdollitePaletteAnimationTimer.RawValue,
            "the Magdollite glow keeps the eight-tick cadence");

        document.Frames[1][0] = original with { Red = 32 };
        AssertThrows<InvalidDataException>(() => MagdollitePaletteCycleFile.Read(new MemoryStream(MagdollitePaletteCycleFile.Write(document))),
            "Magdollite RGB5 channel outside five-bit precision is rejected");
        Console.WriteLine("  Magdollite colors: sixteen native RGB5 words, 32 draw-hook ticks, channel rejection and no runtime ROM reads pass.");
    }
}
