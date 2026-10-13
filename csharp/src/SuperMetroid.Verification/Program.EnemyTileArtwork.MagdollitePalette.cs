using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>
/// Checks that Magdollite's extracted color cycle can be edited without changing its draw-hook cadence or reading cartridge data at runtime.
/// </summary>
internal static class EnemyTileArtworkMagdollitePaletteVerification
{
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
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
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
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        MagdollitePaletteCycle cycle = edited.MagdollitePaletteCycle ??
            throw new InvalidDataException("Edited enemy art has no Magdollite palette cycle.");
        AssertTrue(cycle.Resolve(1, 0) != native.Resolve(1, 0),
            "Magdollite color override changes the selected RGB5 word");
        AssertEqual(cycle.Resolve(1, 0),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .MagdollitePaletteCycle!.Resolve(1, 0),
            "Magdollite color override persists across catalog reload");
        for (int frame = 0; frame < MagdollitePaletteRomData.FrameCount; frame++)
        for (int color = 0; color < MagdollitePaletteRomData.AnimatedColorCount; color++)
            if (frame != 1 || color != 0)
                AssertEqual(native.Resolve(frame, color), cycle.Resolve(frame, color),
                    $"Magdollite color edit leaves frame {frame} color {color} unchanged");

        // Exercise the actual global graphics-drawn hook, not a palette helper.
        // Forbid all bus reads: installed artwork must be sufficient on every tick.
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        var cgram = new SnesCgram();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.BindFixtureBus(CartridgeReadGuard.Unbacked()
                .ForbidAll(address => $"Installed Magdollite palette hook read ROM ${address:X6}.")
                .RejectWrites(address => $"Installed Magdollite palette hook wrote ROM ${address:X6}."));
        enemies.Presentation._cgram = cgram;
        enemies.MagdolliteBehavior._magdollitePaletteAnimationInstalled = true;
        enemies.MagdolliteBehavior._magdollitePaletteAnimationTimer = NativeCountdown.Start((ushort)8);
        enemies.MagdolliteBehavior._magdollitePaletteAnimationIndex = (ushort)0;
        enemies.MagdolliteBehavior._magdollitePaletteBaseByteOffset = (ushort)0x100;
        Action step = enemies.MagdolliteBehavior.StepMagdollitePaletteAnimation;
        const int destination = (0x100 >> 1) + MagdollitePaletteRomData.FirstAnimatedColor;
        RoomEnemySlot untouched = enemies.SlotTable.Slots[0];
        ushort healthBefore = untouched.Health;
        ushort xBefore = untouched.Position.X.Whole;
        for (int tick = 0; tick < 32; tick++)
        {
            Bgr555[] before = cgram.Colors.Slice(destination,
                MagdollitePaletteRomData.AnimatedColorCount).ToArray();
            step();
            int frame = (tick + 1) / 8 & (MagdollitePaletteRomData.FrameCount - 1);
            if ((tick + 1) % 8 != 0)
            {
                AssertTrue(before.AsSpan().SequenceEqual(cgram.Colors.Slice(destination,
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
            (ushort)enemies.MagdolliteBehavior._magdollitePaletteAnimationTimer.RawValue,
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
