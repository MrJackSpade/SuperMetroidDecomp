using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks Magdollite's extracted color cycle, its glow's cadence and that runtime reads no cartridge data.
/// </summary>
internal static class EnemyTileArtworkMagdollitePaletteVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Compares installed RGB5 colors with the cartridge, then exercises the room's lava glow.
    /// </summary>
    internal static void VerifyInstalledMagdollitePaletteCycle(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
    {
        MagdollitePaletteCycle native = stock.Enemies.MagdollitePaletteCycle ??
            throw new InvalidDataException("Installed enemy art has no Magdollite palette cycle.");
        for (int frame = 0; frame < MagdolliteShades.GlowFrames; frame++)
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
        // Exercise the room's production glow step. Forbid all bus reads: installed artwork must be sufficient on every tick.
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        // The glow names the frame every Magdollite shows (#1512); the frame's colours are the import's shade. A restarted
        // glow shows its first frame at once.
        enemies.MagdolliteBehavior.RestartGlow();
        AssertEqual(PictureShading.Shaded(MagdolliteShades.Glow(0)), enemies.MagdolliteBehavior.Glow,
            "a restarted Magdollite glow shows its first frame");
        Action step = enemies.MagdolliteBehavior.StepGlow;
        RoomEnemySlot untouched = enemies.SlotTable.Slots[0];
        int healthBefore = untouched.Health;
        ushort xBefore = untouched.Position.X.Whole;
        for (int tick = 0; tick < 32; tick++)
        {
            PictureShading before = enemies.MagdolliteBehavior.Glow;
            step();
            if ((tick + 1) % 8 != 0)
            {
                AssertEqual(before, enemies.MagdolliteBehavior.Glow,
                    $"Magdollite glow does not change before tick {tick + 1} reaches the step boundary");
            }
            else
            {
                int frame = (tick + 1) / 8 & (MagdolliteShades.GlowFrames - 1);
                AssertEqual(PictureShading.Shaded(MagdolliteShades.Glow(frame)), enemies.MagdolliteBehavior.Glow,
                    $"Magdollite glow tick {tick + 1} glow frame");
            }
        }
        AssertEqual(healthBefore, untouched.Health,
            "the Magdollite glow cycle does not change enemy health");
        AssertEqual(xBefore, untouched.Position.X.Whole,
            "the Magdollite glow cycle does not change enemy position");
        AssertEqual(8,
            enemies.MagdolliteBehavior.GlowStepCountdown,
            "the Magdollite glow keeps the eight-tick cadence");

        document.Frames[1][0] = original with { Red = 32 };
        AssertThrows<InvalidDataException>(() => MagdollitePaletteCycleFile.Read(new MemoryStream(MagdollitePaletteCycleFile.Write(document))),
            "Magdollite RGB5 channel outside five-bit precision is rejected");
        Console.WriteLine("  Magdollite colors: sixteen native RGB5 words, 32 glow ticks, channel rejection and no runtime ROM reads pass.");
    }
}
