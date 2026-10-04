using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native8C:8F7E..8FCA contains centered single-tile effects. Fragments
/// traverse a two-column/three-row atlas patch; slime follows a three-cell row
/// then continues down its first column. Shared OAM fields are calculated.</summary>
internal sealed class IntroEggEffectParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 11; frame++)
            if (IntroEggEffectSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroEggEffectParts(frame));
        return supplied;
    }
    public int Count => 1;
    public CompiledSpritePart this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(index, 0);
            int tile;
            if (frame < 6) tile = IntroEggEffectAtlas.FragmentStart + 16 * (frame / 2) + frame % 2;
            else
            {
                int slime = frame - 6;
                tile = IntroEggEffectAtlas.SlimeStart + (slime < 3 ? slime : 16 * (slime - 2));
            }
            return new(SnesSpritemapXWord.Create(-4, false), unchecked((byte)-4),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class IntroEggEffectAtlas
{
    /// <summary>Tile$105, upper-left of the two-column shell-fragment patch.</summary>
    internal const int FragmentStart = 0x105;
    /// <summary>Tile$10D, moving slime and origin of the impact row/column traversal.</summary>
    internal const int SlimeStart = 0x10d;
}
