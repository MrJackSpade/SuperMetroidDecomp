using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Verification.Areas.Escape.Artwork;

/// <summary>
/// Compares the escape timer's whole-frame draws with the OAM sprites the cartridge's timer routine emits:
/// every part must cover the same screen rectangle with the same characters, palette, priority and flips.
/// </summary>
internal static class EscapeTimerDrawChecks
{
    /// <summary>One sprite as both forms describe it: screen rectangle, OBJ character and attributes.</summary>
    private readonly record struct Sprite(Offset16<ScreenPixels> Position, int Size, int Character, int Palette, int Priority, bool FlipX, bool FlipY);

    /// <summary>Timer characters with blank pixels, for checks of geometry and bindings only.</summary>
    internal static SpriteArtwork BlankArtwork { get; } = new(new SpriteArtworkId(SpriteArtworkKind.EscapeTimerTiles),
        new byte[EscapeTimerTileAtlasFormat.TileCount * SpriteArtwork.CharacterByteCount]);

    /// <summary>Draws the timer into a fresh sprite frame and returns its finalized draws.</summary>
    internal static IReadOnlyList<SpriteDraw> Draw(EscapeTimer timer, EscapeTimerPresentation presentation,
        SpriteArtwork? artwork = null)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        EscapeTimerRenderer.Draw(timer, sprites, presentation, artwork ?? BlankArtwork);
        sprites.FinalizeFrame();
        return sprites.Draws;
    }

    /// <summary>Asserts that the draws stand for exactly the OAM sprites appended to <paramref name="native"/>, in order.</summary>
    internal static void AssertSameSprites(OamBuffer native, IReadOnlyList<SpriteDraw> draws, string message)
    {
        var expected = new List<Sprite>();
        for (int index = 0; index < native.AppendedSpriteCount; index++)
        {
            OamEntry entry = native.GetEntry(index);
            int size = entry.IsLarge ? 16 : 8;
            expected.Add(new(new((short)(entry.Position.X >= 256 ? entry.Position.X - 512 : entry.Position.X), (short)(entry.Position.Y >= 224 ? entry.Position.Y - 256 : entry.Position.Y)), size,
                entry.TileNumber, entry.Palette, entry.Priority, entry.FlipX, entry.FlipY));
        }
        var actual = new List<Sprite>();
        foreach (SpriteDraw draw in draws)
        {
            AssertEqual(SpriteSourceKind.EscapeTimer, draw.Source.Kind, message + ": draw source");
            for (int index = 0; index < draw.Frame.Parts.Length; index++)
            {
                SpriteScreenPart part = draw.Place(index);
                AssertEqual(part.Size.X, part.Size.Y, message + ": timer parts are square");
                actual.Add(new(part.Position, part.Size.X, part.Character + EscapeTimerPresentationDefinitions.FirstCharacter,
                    part.Palette, part.Priority, part.FlipX, part.FlipY));
            }
        }
        AssertEqual(expected.Count, actual.Count, message + ": sprite count");
        for (int index = 0; index < expected.Count; index++)
            AssertEqual(expected[index], actual[index], $"{message}: sprite {index}");
    }
}
