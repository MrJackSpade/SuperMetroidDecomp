using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingExplosionSpriteDefinitions.Pose;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A472/A4B0/A516: glow and two supernova compositions mirror one
/// supplied quadrant about the actor origin. Reflection subtracts sprite size and
/// toggles the corresponding flip. Native quadrant order starts top-right for glow
/// and supernova one, top-left for supernova two, then proceeds clockwise.</summary>
internal sealed class EndingExplosionQuadrantParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly CompiledSpritePart[] basis;
    private readonly int firstQuadrant;
    private EndingExplosionQuadrantParts(CompiledSpritePart[] basis, int firstQuadrant)
    {
        this.basis = basis;
        this.firstQuadrant = firstQuadrant;
    }

    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        int count, first;
        if (pointer == EndingExplosionSpriteDefinitions.Pointer(Pose.Glow)) { count = 3; first = 0; }
        else if (pointer == EndingExplosionSpriteDefinitions.Pointer(Pose.SupernovaFirst)) { count = 5; first = 0; }
        else if (pointer == EndingExplosionSpriteDefinitions.Pointer(Pose.SupernovaSecond)) { count = 5; first = 3; }
        else return supplied;
        if (supplied.PartCount != count * 4) return supplied;
        var basis = new CompiledSpritePart[count];
        for (int index = 0; index < count; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int size = part.X.IsLarge ? 16 : 8;
            // Arbitrary edits outside the signed coordinate mirror domain stay explicit.
            if (-part.X.SignedOffset - size is < -256 or > 255 ||
                -unchecked((sbyte)part.Y) - size is < -128 or > 127) return supplied;
            basis[index] = part;
        }
        return supplied.CalculateIfMatching(new EndingExplosionQuadrantParts(basis, first));
    }

    public int Count => basis.Length * 4;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int quadrant = (firstQuadrant + index / basis.Length) % 4;
            bool reflectX = (quadrant < 2) != (firstQuadrant < 2);
            bool reflectY = quadrant is 1 or 2;
            CompiledSpritePart part = basis[index % basis.Length];
            int size = part.X.IsLarge ? 16 : 8;
            int x = part.X.SignedOffset, y = unchecked((sbyte)part.Y);
            SnesTileFlipFlags flips = part.Attributes.FlipFlags ^
                (reflectX ? SnesTileFlipFlags.Horizontal : 0) ^ (reflectY ? SnesTileFlipFlags.Vertical : 0);
            return new(SnesSpritemapXWord.Create(reflectX ? -x - size : x, part.X.IsLarge),
                unchecked((byte)(reflectY ? -y - size : y)),
                SnesObjAttributeWord.Create(part.Attributes.TileNumber, part.Attributes.PaletteIndex, part.Attributes.Priority, flips),
                part.InheritPalette);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
