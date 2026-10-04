using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Ending logo halves rotate by180 degrees; wrap stages reveal portions
/// of the complete right wrap. References preserve independently supplied artwork.</summary>
internal sealed class EndingLogoRelatedParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly SpriteComposition source;
    private readonly int offset;
    private readonly bool rotate;
    private EndingLogoRelatedParts(SpriteComposition source, int offset, int count, bool rotate)
    {
        this.source = source;
        this.offset = offset;
        Count = count;
        this.rotate = rotate;
    }

    internal static SpriteComposition CalculateIfMatching(int frame, SpriteComposition supplied,
        SpriteComposition upper, SpriteComposition completeRight)
    {
        if ((uint)frame >= 8) throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame is 0 or 4) return supplied;
        SpriteComposition source = frame == 1 ? upper : completeRight;
        if (source.PartCount != (frame == 1 ? 14 : 25)) return supplied;
        bool rotate = frame == 1 || frame >= 5;
        int stage = frame == 1 ? -1 : (frame - 2) % 3;
        int offset = stage is 0 or 1 ? 7 : 0;
        int count = stage switch { -1 => 14, 0 => 12, 1 => 18, _ => 25 };
        if (rotate)
            for (int index = offset; index < offset + count; index++)
            {
                CompiledSpritePart part = source.Part(index);
                int size = part.X.IsLarge ? 16 : 8;
                if (-part.X.SignedOffset - size is < -256 or > 255 ||
                    -unchecked((sbyte)part.Y) - size is < -128 or > 127) return supplied;
            }
        return supplied.CalculateIfMatching(new EndingLogoRelatedParts(source, offset, count, rotate));
    }

    public int Count { get; }
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            CompiledSpritePart part = source.Part(offset + index);
            if (!rotate) return part;
            int size = part.X.IsLarge ? 16 : 8;
            return new(SnesSpritemapXWord.Create(-part.X.SignedOffset - size, part.X.IsLarge),
                unchecked((byte)(-unchecked((sbyte)part.Y) - size)),
                SnesObjAttributeWord.Create(part.Attributes.TileNumber, part.Attributes.PaletteIndex,
                    part.Attributes.Priority, part.Attributes.FlipFlags ^
                    (SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical)), part.InheritPalette);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
