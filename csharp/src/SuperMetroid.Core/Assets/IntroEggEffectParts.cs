using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native8C:8F7E..8FCA contains centered single-tile effects. Fragments
/// traverse a two-column/three-row atlas patch; slime follows a three-cell row
/// then continues down its first column. Shared OAM fields are calculated.</summary>
internal sealed class IntroEggEffectParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses the calculated egg fragment or slime part only when it exactly matches the supplied OAM composition.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 11; frame++)
            if (IntroEggEffectSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroEggEffectParts(frame));
        return supplied;
    }
    /// <summary>Each egg fragment or slime frame contains one centered eight-pixel OBJ part.</summary>
    public int Count => 1;
    /// <summary>Selects a shell-fragment or slime tile by frame and centers its sprite four pixels about the origin.</summary>
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
    /// <summary>Yields the sole centered effect part.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
    /// <summary>Shares the one-part sequence with non-generic OAM consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Tile origins of the native shell-fragment patch and slime row in the intro egg effects sheet.</summary>
internal static class IntroEggEffectAtlas
{
    /// <summary>Tile$105, upper-left of the two-column shell-fragment patch.</summary>
    internal const int FragmentStart = 0x105;
    /// <summary>Tile$10D, moving slime and origin of the impact row/column traversal.</summary>
    internal const int SlimeStart = 0x10d;
}
