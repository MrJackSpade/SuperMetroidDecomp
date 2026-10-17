using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Imports the game-over screen's presentation and verifies the compiled Baby animation
/// dispatcher against the selected cartridge revision.
/// </summary>
[CpuBusBoundary("codec: ROM game-over screen tilemaps, palettes and Baby animation dispatcher → game-over presentation")]
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
        Ensure.NotNull(bus);
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
                sourceAddress = GameOverRomData.TextBank | (SnesAddressMath.OffsetInBank(sourceAddress + 2));
                if (Enum.IsDefined((GameOverTextControl)word))
                {
                    switch ((GameOverTextControl)word)
                    {
                        case GameOverTextControl.End:
                            return;
                        case GameOverTextControl.NextLine:
                            initialColumn += GameOverRomData.TilemapRowByteCount;
                            destinationByteOffset = initialColumn;
                            continue;
                        default:
                            throw new InvalidOperationException($"Undefined {nameof(GameOverTextControl)} {word}.");
                    }
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

    /// <summary>Compares each Baby pose's duration, sprite, palette, sound handoff, and restart marker with the compiled dispatcher.</summary>
    /// <remarks>
    /// Walks the cartridge's record stream from its first record alongside the compiled steps in play order: each
    /// six-byte frame record is followed by the next record, or by a two-byte cry callback and then the next record.
    /// </remarks>
    private static void VerifyCompiledAnimation(ISnesAddressSpace bus)
    {
        const int FrameRecordBytes = 6;
        const int CryCallbackBytes = 2;
        int pointer = GameOverBabyAnimationDefinitions.FirstPointer;
        foreach (GameOverBabyInstruction instruction in GameOverBabyAnimation.All)
        {
            int address = GameOverRomData.SpriteBank | pointer;
            ushort duration = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            ushort sprite = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2);
            ushort palette = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 4);
            if (duration != instruction.Duration ||
                sprite != GameOverBabyAnimationDefinitions.NativeSpritemap(instruction.Frame) ||
                palette != GameOverBabyAnimationDefinitions.NativePalettePointer(instruction.Palette))
                throw new InvalidDataException(
                    $"Compiled game-over Baby instruction $82:{pointer:X4} does not match the cartridge.");

            ushort nextWord = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 6);
            if (instruction.RestartAfter)
            {
                if (nextWord != GameOverRomData.BabyAnimation.End)
                    throw new InvalidDataException(
                        $"Game-over Baby instruction $82:{pointer:X4} lacks its native end marker.");
            }
            else if (instruction.SoundAfter != GameOverBabySound.None)
            {
                if (nextWord != GameOverBabyAnimationDefinitions.NativeSoundOpcode(instruction.SoundAfter) ||
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 8) !=
                    GameOverBabyAnimation.Get(instruction.NextStep).Duration)
                    throw new InvalidDataException(
                        $"Game-over Baby sound handoff at $82:{pointer:X4} does not match the cartridge.");
            }
            else if (nextWord != GameOverBabyAnimation.Get(instruction.NextStep).Duration)
            {
                throw new InvalidDataException(
                    $"Game-over Baby next duration at $82:{pointer:X4} does not match the cartridge.");
            }
            pointer += FrameRecordBytes + (instruction.SoundAfter == GameOverBabySound.None ? 0 : CryCallbackBytes);
        }
    }
}
