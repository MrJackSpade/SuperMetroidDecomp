using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.GameOver;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend;

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
        Ensure.NotNull(bus);
        VerifyCompiledAnimation(bus);

        ushort[] tilemap = new ushort[GameOverPageDefinitions.TilemapCellCount];
        Array.Fill<ushort>(tilemap, GameOverPageDefinitions.BlankTile);
        foreach (GameOverTextStream stream in GameOverTexts.All)
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
            [GameOverPresentationNames.BabyFrameName(GameOverBabyFrame.Closed)] =
                MenuSpriteExtractor.Read(bus,
                    GameOverBabyAnimationDefinitions.NativeSpritemap(GameOverBabyFrame.Closed)),
            [GameOverPresentationNames.BabyFrameName(GameOverBabyFrame.Middle)] =
                MenuSpriteExtractor.Read(bus,
                    GameOverBabyAnimationDefinitions.NativeSpritemap(GameOverBabyFrame.Middle)),
            [GameOverPresentationNames.BabyFrameName(GameOverBabyFrame.Open)] =
                MenuSpriteExtractor.Read(bus,
                    GameOverBabyAnimationDefinitions.NativeSpritemap(GameOverBabyFrame.Open)),
            [GameOverPresentationDefinitions.EggFrame] =
                MenuSpriteExtractor.Read(bus, GameOverSourceDefinitions.EggSpritemap),
        };
        for (int frame = 0; frame < GameOverPresentationDefinitions.CursorFrameCount; frame++)
        {
            sprites.Add(GameOverPresentationNames.CursorFrameName(frame),
                MenuSpriteExtractor.Read(bus, MenuMissileSpritemaps.SpritemapId(frame)));
        }

        var palettes = new Dictionary<string, ushort[]>(StringComparer.Ordinal);
        foreach (GameOverBabyColouring palette in Enum.GetValues<GameOverBabyColouring>())
        {
            var colors = new ushort[GameOverPresentationFormat.BabyPaletteColorCount];
            int pointer = GameOverBabyAnimationDefinitions.NativePalettePointer(palette);
            for (int color = 0; color < colors.Length; color++)
                colors[color] = ReadBank82Word(unchecked((ushort)(pointer + color * 2)));
            palettes.Add(GameOverBabyPaletteNames.BabyPaletteName(palette), colors);
        }

        using var output = new MemoryStream();
        GameOverSourcePresentation.Write(output, new()
        {
            Version = GameOverPresentationFormat.Version,
            Tilemap = cells,
            Sprites = sprites,
            BabyPalettes = palettes,
            BabyAnchor = new(GameOverSourceDefinitions.BabyX, GameOverSourceDefinitions.BabyY),
            CursorX = GameOverSourceDefinitions.MissileX,
            YesCursorY = GameOverSourceDefinitions.YesMissileY,
            NoCursorY = GameOverSourceDefinitions.NoMissileY,
            BabyPalette = GameOverSourceDefinitions.BabyPalette,
            EggPalette = GameOverSourceDefinitions.EggPalette,
            CursorPalette = MenuRenderDefinitions.ObjectPalette,
            CursorFrameDuration = MenuAnimationTimings.MissileFrameDuration(bus),
        });
        return output.ToArray();

        void LoadText(GameOverTextStream stream)
        {
            // The cartridge's load immediates name each element's first cell as a byte offset into the page's rows.
            int destinationByteOffset = (stream.Origin.Y * GameOverPageDefinitions.TilemapWidth + stream.Origin.X) * sizeof(ushort);
            int initialColumn = destinationByteOffset;
            int sourceAddress = GameOverSourceDefinitions.TextBank | GameOverTextSources.Pointer(stream.Element);
            while (true)
            {
                int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), sourceAddress);
                sourceAddress = GameOverSourceDefinitions.TextBank | (SnesAddressMath.OffsetInBank(sourceAddress + 2));
                if (GameOverSourceDefinitions.TextControls.TryGetValue(word, out GameOverTextControl control))
                {
                    switch (control)
                    {
                        case GameOverTextControl.End:
                            return;
                        case GameOverTextControl.NextLine:
                            initialColumn += GameOverSourceDefinitions.TextRowBytes;
                            destinationByteOffset = initialColumn;
                            continue;
                        default:
                            throw new InvalidOperationException($"Undefined {nameof(GameOverTextControl)} {control}.");
                    }
                }

                int wordIndex = destinationByteOffset / sizeof(ushort);
                if ((uint)wordIndex >= tilemap.Length)
                    throw new InvalidDataException(
                        $"Game-over text {stream.Description} escaped its 32x32 tilemap.");
                tilemap[wordIndex] = checked((ushort)word);
                destinationByteOffset += sizeof(ushort);
            }
        }

        ushort ReadBank82Word(int pointer) =>
            RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), GameOverSourceDefinitions.SpriteBank | pointer);
    }

    /// <summary>Compares each look of the Baby's animation, its duration, sprite and colours, its cries and its loop with the cartridge.</summary>
    /// <remarks>
    /// Walks the cartridge's record stream from its first record alongside the animation's beats: each six-byte frame
    /// record is followed by the next record, or by a two-byte cry callback and then the next record, and the last by the
    /// restart marker.
    /// </remarks>
    private static void VerifyCompiledAnimation(ISnesAddressSpace bus)
    {
        const int FrameRecordBytes = 6;
        const int CryCallbackBytes = 2;
        int pointer = GameOverBabyAnimationDefinitions.FirstPointer;
        IReadOnlyList<AnimationBeat<GameOverBabyLook, GameOverBabySound>> beats = GameOverBabyAnimation.Segments.Beats(GameOverBabySegment.Crying);
        if (GameOverBabyAnimation.Playing.End != AnimationEnd.LoopsFrom(0))
            throw new InvalidDataException("The game-over Baby's animation does not start again after its last look.");
        for (int beat = 0; beat < beats.Count; beat++)
        {
            if (beats[beat] is not AnimationBeat<GameOverBabyLook, GameOverBabySound>.Frame look)
                throw new InvalidDataException($"Game-over Baby beat {beat} is a cry that follows no look.");
            int address = GameOverSourceDefinitions.SpriteBank | pointer;
            int duration = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            int sprite = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2);
            int palette = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 4);
            if (duration != look.Duration ||
                sprite != GameOverBabyAnimationDefinitions.NativeSpritemap(look.Shown.Frame) ||
                palette != GameOverBabyAnimationDefinitions.NativePalettePointer(look.Shown.Colours))
                throw new InvalidDataException(
                    $"Game-over Baby look $82:{pointer:X4} does not match the cartridge.");

            int nextWord = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 6);
            pointer += FrameRecordBytes;
            if (beat + 1 == beats.Count)
            {
                if (nextWord != GameOverSourceDefinitions.BabyAnimationEnd)
                    throw new InvalidDataException(
                        $"Game-over Baby look $82:{pointer - FrameRecordBytes:X4} lacks its native end marker.");
            }
            else if (beats[beat + 1] is AnimationBeat<GameOverBabyLook, GameOverBabySound>.Event cry)
            {
                if (nextWord != GameOverBabyAnimationDefinitions.NativeSoundOpcode(cry.Raised))
                    throw new InvalidDataException(
                        $"Game-over Baby cry at $82:{pointer:X4} does not match the cartridge.");
                pointer += CryCallbackBytes;
                beat++;
            }
        }
    }
}
