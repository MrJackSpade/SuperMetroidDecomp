using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Atlas packing and sprite geometry for subtitle arrows, delivered/examined baby and caret.</summary>
/// <param name="frame">Zero-based frame index: 0–2 select arrows, 3–5 delivered poses, 6–8 examined poses, and 9 the caret.</param>
internal sealed class IntroScientistParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Adds the matching intro-scientist sprite parts when the pointer selects one of their ten frames.</summary>
    /// <param name="pointer">Sprite pointer to match against the compiled frame definitions.</param>
    /// <param name="supplied">Composition accumulated from other matching sprite parts.</param>
    /// <returns>The supplied composition with the matching frame parts included, or unchanged when no frame matches.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 10; frame++)
            if (IntroScientistSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroScientistParts(frame));
        return supplied;
    }

    /// <summary>Gets the number of compiled parts for the selected arrow, baby, or caret frame.</summary>
    public int Count => frame < 3 ? 2 : frame < 6 ? 6 : 1;

    /// <summary>Gets one compiled sprite part in the selected frame's native OAM order.</summary>
    /// <param name="index">Zero-based part index within this frame.</param>
    /// <returns>The tile, position, size, and flip attributes for the part.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside this frame's part range.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large = false;
            SnesTileFlipFlags flips = 0;
            if (frame < 3)
            {
                x = -1 - 7 * index;
                y = 0;
                flips = index == 0 ? SnesTileFlipFlags.Horizontal : 0;
                tile = frame == 0 ? IntroScientistAtlas.InitialArrow
                    : IntroScientistAtlas.ArrowColumn + 16 * (frame - 1);
            }
            else if (frame < 6)
            {
                int pose = frame - 3;
                if (index < 3)
                {
                    x = 4 - 8 * index;
                    y = -12;
                    tile = IntroScientistAtlas.DeliveredTop + 3 * (pose % 2) + 16 * (pose / 2) + 2 - index;
                }
                else
                {
                    // Each lower 3x2 patch uses one 2x2 sprite and a two-sprite side column.
                    // The middle pose puts that column on the left, preserving native OAM order.
                    bool leftColumn = (pose & 1) != 0;
                    large = index == 5;
                    x = large ? -12 + (leftColumn ? 8 : 0) : leftColumn ? -12 : 4;
                    y = index == 3 ? 4 : -4;
                    int column = large ? (leftColumn ? 1 : 0) : leftColumn ? 0 : 2;
                    tile = IntroScientistAtlas.DeliveredLower + 3 * pose + column + (index == 3 ? 16 : 0);
                }
            }
            else if (frame < 9)
            {
                x = y = -8;
                large = true;
                tile = IntroScientistAtlas.ExaminedStart + 2 * (frame - 6);
            }
            else
            {
                x = 0;
                y = -1;
                tile = IntroScientistAtlas.Caret;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, flips), true);
        }
    }

    /// <summary>Enumerates the selected frame's compiled sprite parts in native OAM order.</summary>
    /// <returns>An enumerator over each part in the frame.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Tile indices for the intro scientist's arrows, baby poses, and blinking caret artwork.</summary>
internal static class IntroScientistAtlas
{
    /// <summary>Tile$1A8, first subtitle-arrow half in8C:8CCF.</summary>
    internal const int InitialArrow = 0x1a8;
    /// <summary>Tile$19F, top of the remaining two arrow drawings packed vertically.</summary>
    internal const int ArrowColumn = 0x19f;
    /// <summary>Tile$E9, start of three-wide top strips packed two per atlas row.</summary>
    internal const int DeliveredTop = 0xe9;
    /// <summary>Tile$E0, start of three adjacent three-by-two lower baby patches.</summary>
    internal const int DeliveredLower = 0xe0;
    /// <summary>Tile$199, first of three adjacent sixteen-pixel examined baby drawings.</summary>
    internal const int ExaminedStart = 0x199;
    /// <summary>Tile$FC, blinking typewriter block selected by8C:8D68.</summary>
    internal const int Caret = 0xfc;
}
