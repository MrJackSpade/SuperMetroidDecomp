using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Right-wrap atlas placement at $8C:BAA9. The ordered tile choices
/// remain supplied; coordinates, sizes and attributes follow the packed regions.</summary>
internal sealed class EndingLogoWrapParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly (int Tile, bool Cropped)[] selection;
    private EndingLogoWrapParts((int Tile, bool Cropped)[] selection) => this.selection = selection;

    internal static SpriteComposition CalculateIfMatching(SpriteComposition supplied)
    {
        var selection = new (int Tile, bool Cropped)[supplied.PartCount];
        for (int index = 0; index < selection.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int tile = part.Attributes.TileNumber;
            if (!TryPlace(tile, tile == 0x48 && !part.X.IsLarge, out _)) return supplied;
            selection[index] = (tile, tile == 0x48 && !part.X.IsLarge);
        }
        return supplied.CalculateIfMatching(new EndingLogoWrapParts(selection));
    }

    private static bool TryPlace(int tile, bool cropped, out CompiledSpritePart part)
    {
        int x, y;
        bool large = false, rotate = false;
        if (tile is 0x09 or 0x0b or 0x0d or 0x0f or 0x29 or 0x2b or 0x2c)
        {
            // Closing arc: a packed two-row region, half-turned into the lower edge.
            x = 8 * ((tile & 15) - 9) - 39;
            y = 8 * (tile / 16) - 57;
            large = tile is 0x09 or 0x0b or 0x0d or 0x29;
            rotate = true;
        }
        else if (tile is 0x48 or 0x49)
        {
            // Overlapping top caps and an eight-pixel crop of the left cap.
            x = cropped ? 8 : 16 + 8 * (tile - 0x48);
            y = cropped ? -24 : -32;
            large = !cropped;
        }
        else if (tile is 0x4c or 0x4e)
        {
            x = 8 * (tile - 0x4c); y = -16; large = true;
        }
        else if (tile is 0x68 or 0x69 or 0x78 or 0x79)
        {
            // Upper-right patch is a two-by-two small-tile rectangle.
            x = 32 + 8 * ((tile & 15) - 8);
            y = -16 + 8 * (tile / 16 - 6);
        }
        else if (tile is 0x6a or 0x6b)
        {
            x = -8 + 8 * (tile - 0x6a); y = 0;
        }
        else if (tile is 0x00 or 0x10)
        {
            x = 24; y = -8 + 8 * (tile / 16);
        }
        else
        {
            // Separately packed corners and vertical-edge endpoints.
            switch (tile)
            {
                case 0x5b: x = -8; y = -8; break; // left corner
                case 0x6c: x = 24; y = 8; break; // inner side junction
                case 0x20: x = 32; y = 24; large = true; break; // lower side endpoint
                case 0x6d: x = 32; y = 8; large = true; break; // outer side junction
                case 0x01: x = 32; y = -8; large = true; break; // upper side endpoint
                default: part = default; return false;
            }
        }
        int size = large ? 16 : 8;
        if (rotate) { x = -x - size; y = -y - size; }
        part = new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
            SnesObjAttributeWord.Create(tile, 0, 3, rotate
                ? SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical : 0), true);
        return true;
    }

    public int Count => selection.Length;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var chosen = selection[index];
            _ = TryPlace(chosen.Tile, chosen.Cropped, out CompiledSpritePart part);
            return part;
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
