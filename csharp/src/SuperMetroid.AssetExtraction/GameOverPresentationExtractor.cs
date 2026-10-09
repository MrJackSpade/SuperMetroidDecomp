using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Imports the game-over screen's presentation and verifies the compiled Baby animation
/// dispatcher against the selected cartridge revision.
/// </summary>
public static class GameOverPresentationExtractor
{
    /// <summary>Imports game-over text, Baby and egg artwork, cursor frames, and Baby palettes after verifying the compiled native animation sequence.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying game-over text, spritemaps, palettes, and Baby instruction records.</param>
    /// <returns>New UTF-8 JSON bytes containing the BG tilemap, visual frames, raw palette words, pixel-space anchors, palette selections, and cursor-frame duration.</returns>
    /// <remarks>Checks Baby frame durations, spritemap and palette pointers, sound handoffs, and restart markers; those dispatcher mechanics remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">The native animation differs from its compiled definitions, text escapes the 32x32 page, or presentation validation fails.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        VerifyCompiledAnimation(bus);

        ushort[] tilemap = new ushort[GameOverPresentationDefinitions.TilemapCellCount];
        Array.Fill(tilemap, GameOverRomData.BlankTile.Raw);
        foreach (GameOverTextStream stream in GameOverRomData.Text.All)
            LoadText(stream);

        var cells = new MapPresentationCell[tilemap.Length];
        for (int index = 0; index < tilemap.Length; index++)
        {
            var word = new SnesBgTilemapWord(tilemap[index]);
            cells[index] = new()
            {
                TileColumn = word.CharacterIndex % MapTileAtlasFormat.TileColumns,
                TileRow = word.CharacterIndex / MapTileAtlasFormat.TileColumns,
                Palette = word.PaletteIndex,
                Priority = word.HasPriority,
                FlipX = word.FlipHorizontally,
                FlipY = word.FlipVertically,
            };
        }

        var sprites = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [GameOverPresentationDefinitions.BabyFrameName(GameOverBabyFrame.Closed)] =
                MenuSpriteExtractor.Read(bus,
                    GameOverBabyAnimationDefinitions.NativeSpritemap(GameOverBabyFrame.Closed)),
            [GameOverPresentationDefinitions.BabyFrameName(GameOverBabyFrame.Middle)] =
                MenuSpriteExtractor.Read(bus,
                    GameOverBabyAnimationDefinitions.NativeSpritemap(GameOverBabyFrame.Middle)),
            [GameOverPresentationDefinitions.BabyFrameName(GameOverBabyFrame.Open)] =
                MenuSpriteExtractor.Read(bus,
                    GameOverBabyAnimationDefinitions.NativeSpritemap(GameOverBabyFrame.Open)),
            [GameOverPresentationDefinitions.EggFrame] =
                MenuSpriteExtractor.Read(bus, GameOverRomData.Sprites.EggSpritemap),
        };
        for (int frame = 0; frame < GameOverPresentationDefinitions.CursorFrameCount; frame++)
        {
            sprites.Add(GameOverPresentationDefinitions.CursorFrameName(frame),
                MenuSpriteExtractor.Read(bus, MenuMissileAnimationDefinitions.SpritemapId(frame)));
        }

        var palettes = new Dictionary<string, ushort[]>(StringComparer.Ordinal);
        foreach (GameOverBabyPalette palette in Enum.GetValues<GameOverBabyPalette>())
        {
            var colors = new ushort[GameOverRomData.BabyAnimation.PaletteColorCount];
            ushort pointer = GameOverBabyAnimationDefinitions.NativePalettePointer(palette);
            for (int color = 0; color < colors.Length; color++)
                colors[color] = ReadBank82Word(unchecked((ushort)(pointer + color * 2)));
            palettes.Add(GameOverPresentationDefinitions.BabyPaletteName(palette), colors);
        }

        using var output = new MemoryStream();
        GameOverPresentation.Write(output, new()
        {
            Version = GameOverPresentationDefinitions.Version,
            Tilemap = cells,
            Sprites = sprites,
            BabyPalettes = palettes,
            BabyAnchor = new(GameOverRomData.Sprites.BabyX, GameOverRomData.Sprites.BabyY),
            CursorX = GameOverRomData.Sprites.MissileX,
            YesCursorY = GameOverRomData.Sprites.YesMissileY,
            NoCursorY = GameOverRomData.Sprites.NoMissileY,
            BabyPalette = GameOverRomData.Sprites.BabyPalette.PaletteIndex,
            EggPalette = GameOverRomData.Sprites.EggPalette.PaletteIndex,
            CursorPalette = MenuPpuState.ObjectPaletteBits >> 9,
            CursorFrameDuration = MenuMissileAnimationDefinitions.FrameDuration,
        });
        return output.ToArray();

        void LoadText(GameOverTextStream stream)
        {
            int destinationByteOffset = stream.DestinationByteOffset;
            int initialColumn = destinationByteOffset;
            int sourceAddress = GameOverRomData.TextBank | stream.SourcePointer;
            while (true)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), sourceAddress);
                sourceAddress = GameOverRomData.TextBank | ((sourceAddress + 2) & 0xffff);
                if (word == GameOverRomData.TextEnd)
                    return;
                if (word == GameOverRomData.TextNextLine)
                {
                    initialColumn += GameOverRomData.TilemapRowByteCount;
                    destinationByteOffset = initialColumn;
                    continue;
                }

                int wordIndex = destinationByteOffset / sizeof(ushort);
                if ((uint)wordIndex >= tilemap.Length)
                    throw new InvalidDataException(
                        $"Game-over text {stream.Description} escaped its 32x32 tilemap.");
                tilemap[wordIndex] = word;
                destinationByteOffset += sizeof(ushort);
            }
        }

        ushort ReadBank82Word(ushort pointer) =>
            RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GameOverRomData.SpriteBank | pointer);
    }

    /// <summary>Proves that the compiled Baby Metroid frame, sound, and restart chain still matches its bank-$82 instruction stream.</summary>
    /// <param name="bus">Import address space containing the native game-over animation instructions.</param>
    private static void VerifyCompiledAnimation(ISnesAddressSpace bus)
    {
        foreach (GameOverBabyInstruction instruction in GameOverBabyAnimationDefinitions.All)
        {
            int address = GameOverRomData.SpriteBank | instruction.Pointer;
            ushort duration = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            ushort sprite = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2);
            ushort palette = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 4);
            if (duration != instruction.Duration ||
                sprite != GameOverBabyAnimationDefinitions.NativeSpritemap(instruction.Frame) ||
                palette != GameOverBabyAnimationDefinitions.NativePalettePointer(instruction.Palette))
                throw new InvalidDataException(
                    $"Compiled game-over Baby instruction $82:{instruction.Pointer:X4} does not match the cartridge.");

            ushort nextWord = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 6);
            if (instruction.RestartAfter)
            {
                if (nextWord != GameOverRomData.BabyAnimation.End)
                    throw new InvalidDataException(
                        $"Game-over Baby instruction $82:{instruction.Pointer:X4} lacks its native end marker.");
            }
            else if (instruction.SoundAfter != GameOverBabySound.None)
            {
                if (nextWord != GameOverBabyAnimationDefinitions.NativeSoundOpcode(instruction.SoundAfter) ||
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 8) !=
                    GameOverBabyAnimationDefinitions.Get(instruction.NextPointer).Duration)
                    throw new InvalidDataException(
                        $"Game-over Baby sound handoff at $82:{instruction.Pointer:X4} does not match the cartridge.");
            }
            else if (nextWord != GameOverBabyAnimationDefinitions.Get(instruction.NextPointer).Duration)
            {
                throw new InvalidDataException(
                    $"Game-over Baby next duration at $82:{instruction.Pointer:X4} does not match the cartridge.");
            }
        }
    }
}
