using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours each tileset's gameplay message boxes (#1392): one image of each message's text in each
/// form the message box shows it (each button a panel can name, each save choice), painted over the HUD's characters in
/// the tileset's standard BG3 colours with the two colours a message box installs.
/// </summary>
internal static class MessageBoxSheetComposer
{
    /// <summary>The buttons a panel can name for its action.</summary>
    private static readonly SnesButton[] BindableButtons =
        [SnesButton.A, SnesButton.B, SnesButton.X, SnesButton.Y, SnesButton.Select, SnesButton.L, SnesButton.R];

    /// <summary>Every form of every message the message box shows, by image name: the words of each.</summary>
    internal static IReadOnlyDictionary<string, ImmutableArray<ushort>> Messages(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        var messages = new SortedDictionary<string, ImmutableArray<ushort>>(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (GameplayMessageId message in GameplayMessageBoxState.OrdinaryMessages)
        {
            foreach (SnesButton shoot in BindableButtons)
            foreach (SnesButton run in BindableButtons)
            {
                var box = new GameplayMessageBoxState();
                box.BindPresentation(inputs.Maps.GameplayMessageTitles, inputs.Maps.GameplayMessagePanels, inputs.Maps.GameplayMessageNotices);
                box.Begin(message, SnesButton.None, shoot, run);
                foreach (ImmutableArray<ushort> words in box.Forms())
                {
                    if (seen.Add(Convert.ToHexString(System.Runtime.InteropServices.MemoryMarshal.AsBytes(words.AsSpan()))))
                        messages.Add($"{message}.{seen.Count}", words);
                }
            }
        }
        return messages;
    }

    /// <summary>Composes <paramref name="tileset"/>'s message box images of <paramref name="messages"/>.</summary>
    [PpuLayoutBoundary("codec: the HUD characters a message's tile words name")]
    internal static ComposedSpriteSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset,
        IReadOnlyDictionary<string, ImmutableArray<ushort>> messages)
    {
        Ensure.NotNull(inputs);
        Bgr555[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).PackedColours.Span);
        colours[GameplayMessageRomData.Palette.TemporaryLightIndex] = GameplayMessageRomData.Palette.TemporaryLightColor;
        colours[GameplayMessageRomData.Palette.TemporaryDarkIndex] = GameplayMessageRomData.Palette.TemporaryDarkColor;
        int width = GameplayMessageRomData.Layout.TilemapWidth;
        int rows = messages.Values.Sum(words => words.Length / width);
        var canvas = new RoomTileCanvas(width * RoomTileCanvas.CharacterPixels, rows * RoomTileCanvas.CharacterPixels);
        ReadOnlySpan<byte> characters = inputs.Hud.Bytes.Span;
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int top = 0;
        foreach ((string name, ImmutableArray<ushort> words) in messages)
        {
            for (int index = 0; index < words.Length; index++)
            {
                var word = new SnesBgTilemapWord(words[index]);
                canvas.PaintTwoBppTile(index % width * RoomTileCanvas.CharacterPixels, (top + index / width) * RoomTileCanvas.CharacterPixels, word,
                    characters.Slice(word.CharacterIndex * RoomTileCanvas.TwoBppCharacterBytes, RoomTileCanvas.TwoBppCharacterBytes), colours);
            }
            int messageRows = words.Length / width;
            frames.Add(name, TileWordSheetComposer.Frame(canvas, top, messageRows, TileWordLayer.Bg3));
            top += messageRows;
        }
        return canvas.ToSheet(colours, frames, SpriteImageColours.TwoBppBackground);
    }
}
