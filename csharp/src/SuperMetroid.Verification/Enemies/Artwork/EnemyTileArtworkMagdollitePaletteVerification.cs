using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.World;

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
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the installed Magdollite palette cycle")]
    internal static void VerifyInstalledMagdollitePaletteCycle(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        MagdollitePaletteCycle native = stock.MagdollitePaletteCycle ??
            throw new InvalidDataException("Installed enemy art has no Magdollite palette cycle.");
        for (int frame = 0; frame < MagdollitePaletteRomData.FrameCount; frame++)
        for (int color = 0; color < MagdollitePaletteRomData.AnimatedColorCount; color++)
        {
            int address = MagdollitePaletteRomData.Source +
                (frame * MagdollitePaletteRomData.SourceColorsPerFrame +
                 MagdollitePaletteRomData.FirstAnimatedColor + color) * sizeof(ushort);
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
        document.Frames[1][0] = original with
        {
            Blue = original.Blue == 31 ? 30 : original.Blue + 1,
        };
        string overrides = Path.Combine(stockDirectory, "magdollite-cycle-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, MagdollitePaletteCycleFormat.FileName);
        File.WriteAllBytes(overrideFile, MagdollitePaletteCycle.Write(document));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        MagdollitePaletteCycle cycle = edited.Enemies.MagdollitePaletteCycle ??
            throw new InvalidDataException("Edited enemy art has no Magdollite palette cycle.");
        AssertTrue(cycle.Resolve(1, 0) != native.Resolve(1, 0),
            "Magdollite color override changes the selected RGB5 word");
        AssertEqual(cycle.Resolve(1, 0),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Enemies.MagdollitePaletteCycle ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .MagdollitePaletteCycle.")).Resolve(1, 0),
            "Magdollite color override persists across catalog reload");
        for (int frame = 0; frame < MagdollitePaletteRomData.FrameCount; frame++)
        for (int color = 0; color < MagdollitePaletteRomData.AnimatedColorCount; color++)
            if (frame != 1 || color != 0)
                AssertEqual(native.Resolve(frame, color), cycle.Resolve(frame, color),
                    $"Magdollite color edit leaves frame {frame} color {color} unchanged");

        // Exercise the actual global graphics-drawn hook, not a palette helper.
        // Forbid all bus reads: installed artwork must be sufficient on every tick.
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        var cgram = new SamusColourRig();
        enemies.Presentation.BindCgram(cgram.Gameplay);
        enemies.MagdolliteBehavior.MagdollitePaletteAnimationInstalled = true;
        enemies.MagdolliteBehavior.MagdollitePaletteAnimationTimer = WrappingCountdown.Start(8);
        enemies.MagdolliteBehavior.MagdollitePaletteFrame = 0;
        enemies.MagdolliteBehavior.MagdollitePaletteBaseColor = 0x80;
        Action step = enemies.MagdolliteBehavior.StepMagdollitePaletteAnimation;
        const int destination = 0x80 + MagdollitePaletteRomData.FirstAnimatedColor;
        RoomEnemySlot untouched = enemies.SlotTable.Slots[0];
        int healthBefore = untouched.Health;
        ushort xBefore = untouched.Position.X.Whole;
        for (int tick = 0; tick < 32; tick++)
        {
            Bgr555[] before = cgram.Colors.Slice(destination,
                MagdollitePaletteRomData.AnimatedColorCount).ToArray();
            step();
            int frame = (tick + 1) / 8 & (MagdollitePaletteRomData.FrameCount - 1);
            if ((tick + 1) % 8 != 0)
            {
                AssertTrue(before.SequenceEqual(cgram.Colors.Slice(destination,
                        MagdollitePaletteRomData.AnimatedColorCount)),
                    $"Magdollite colors do not change before tick {tick + 1} reaches the hook boundary");
            }
            else
            {
                frame = (tick + 1) / 8 & (MagdollitePaletteRomData.FrameCount - 1);
                for (int color = 0; color < MagdollitePaletteRomData.AnimatedColorCount; color++)
                    AssertEqual(cycle.Resolve(frame, color), cgram.Colors[destination + color],
                        $"Magdollite draw-hook tick {tick + 1} color {color}");
            }
        }
        AssertEqual(healthBefore, untouched.Health,
            "Magdollite visual-cycle edit does not change enemy health");
        AssertEqual(xBefore, untouched.Position.X.Whole,
            "Magdollite visual-cycle edit does not change enemy position");
        AssertEqual((ushort)8,
            enemies.MagdolliteBehavior.MagdollitePaletteAnimationTimer.RawValue,
            "Magdollite color edits preserve the eight-tick cadence");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Magdollite color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1,\"frames\":[]}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Magdollite color property fails loudly");
        document.Frames[1][0] = original with { Red = 32 };
        AssertThrows<InvalidDataException>(() => MagdollitePaletteCycle.Write(document),
            "Magdollite RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);

        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Magdollite colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Magdollite colors: sixteen native RGB5 words, 32 draw-hook ticks, edited CGRAM, persistent override, strict failures and no runtime ROM reads pass.");
    }
}
