using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculate the two matching seven-part middle bands of8C:9558.
/// The remaining36 parts are separately unresolved and stay supplied.</summary>
internal sealed class ZebesPlanetBandParts(CompiledSpritePart[] remaining) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != CeresDestructionSpriteDefinitions.Planet || supplied.PartCount != 50) return supplied;
        var remaining = new CompiledSpritePart[36];
        for (int index = 0; index < remaining.Length; index++)
            remaining[index] = supplied.Part(index < 19 ? index : index + 14);
        return supplied.CalculateIfMatching(new ZebesPlanetBandParts(remaining));
    }
    public int Count => 50;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (index < 19 || index >= 33) return remaining[index < 19 ? index : index - 14];
            int band = (index - 19) / 7;
            int column = Math.Min(2 * (6 - (index - 19) % 7), 11);
            int tile = (band == 0 ? ZebesPlanetBandAtlas.Lower : ZebesPlanetBandAtlas.Upper) + column;
            return new(SnesSpritemapXWord.Create(-64 + 8 * column, true), unchecked((byte)(-16 * band)),
                SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal static class ZebesPlanetBandAtlas
{
    /// <summary>Tile60, left edge of the planet band at relative Y=0 in8C:9558.</summary>
    internal const int Lower = 0x60;
    /// <summary>Tile43, left edge of the planet band at relative Y=-16 in8C:9558.</summary>
    internal const int Upper = 0x43;
}