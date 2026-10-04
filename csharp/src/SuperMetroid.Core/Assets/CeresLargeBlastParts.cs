using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Four single-sprite Ceres blasts: centered X=-8 and Y=-10-2*frame.
/// Size16, priority3, no flips and inherited palette are common to all four records.
/// The supplied atlas selection remains a separately unresolved mapping.</summary>
internal sealed class CeresLargeBlastParts(int frame, int tile) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (supplied.PartCount != 1) return supplied;
        for (int frame = 0; frame < 4; frame++)
            if (pointer == CeresDestructionSpriteDefinitions.LargeFrame(frame))
                return supplied.CalculateIfMatching(new CeresLargeBlastParts(frame, supplied.Part(0).Attributes.TileNumber));
        return supplied;
    }
    public int Count => 1;
    public CompiledSpritePart this[int index] => index == 0
        ? new(SnesSpritemapXWord.Create(-8, true), unchecked((byte)(-10 - 2 * frame)),
            SnesObjAttributeWord.Create(tile, 0, 3, 0), true)
        : throw new ArgumentOutOfRangeException(nameof(index));
    public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}