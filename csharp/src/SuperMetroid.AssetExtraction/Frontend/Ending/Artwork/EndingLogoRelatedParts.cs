using System.Collections;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Ending logo halves rotate by180 degrees; wrap stages reveal portions
/// of the complete right wrap. References preserve independently supplied artwork.</summary>
internal sealed class EndingLogoRelatedParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Supplied upper or complete-right logo composition whose selected parts are reused.</summary>
    private readonly SpriteComposition source;
    /// <summary>First source part included at this wrap stage.</summary>
    private readonly int offset;
    /// <summary>Whether this stage rotates selected parts 180 degrees around their OAM origins.</summary>
    private readonly bool rotate;
    /// <summary>Records the source slice and rotation only after its calculated composition matches supplied artwork.</summary>
    private EndingLogoRelatedParts(SpriteComposition source, int offset, int count, bool rotate)
    {
        this.source = source;
        this.offset = offset;
        Count = count;
        this.rotate = rotate;
    }

    /// <summary>Recognizes a logo wrap stage and substitutes a calculated source slice only when every supplied OAM part matches.</summary>
    internal static SpriteComposition CalculateIfMatching(int frame, SpriteComposition supplied,
        SpriteComposition upper, SpriteComposition completeRight)
    {
        if ((uint)frame >= 8) throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame is 0 or 4) return supplied;
        SpriteComposition source = frame == 1 ? upper : completeRight;
        if (source.PartCount != (frame == 1 ? 14 : 25)) return supplied;
        bool rotate = frame is 1 or >= 5;
        int stage = frame == 1 ? -1 : (frame - 2) % 3;
        int offset = stage is 0 or 1 ? 7 : 0;
        int count = stage switch { -1 => 14, 0 => 12, 1 => 18, _ => 25 };
        if (rotate)
            for (int index = offset; index < offset + count; index++)
            {
                CompiledSpritePart part = source.Part(index);
                int size = part.Part.Large ? 16 : 8;
                if (-part.Part.Offset.X - size is < -256 or > 255 ||
                    -part.Part.Offset.Y - size is < -128 or > 127) return supplied;
            }
        return supplied.CalculateIfMatching(new EndingLogoRelatedParts(source, offset, count, rotate));
    }

    /// <summary>Number of source parts visible at this wrap stage.</summary>
    public int Count { get; }
    /// <summary>Returns a retained source part or its 180-degree reflected position and flip flags.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            CompiledSpritePart part = source.Part(offset + index);
            if (!rotate) return part;
            int size = part.Part.Large ? 16 : 8;
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-part.Part.Offset.X - size, -part.Part.Offset.Y - size), part.Part.Large,
                part.Part.Character, part.Part.Palette, part.Part.Priority, part.Part.Flip ^
                    (TileFlip.Horizontal | TileFlip.Vertical)), part.InheritPalette);
        }
    }
    /// <summary>Yields the selected logo parts in their authored OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Preserves authored logo-part order for non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
