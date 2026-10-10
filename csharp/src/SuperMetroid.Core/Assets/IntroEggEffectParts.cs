using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native8C:8F7E..8FCA contains centered single-tile effects. Fragments
/// traverse a two-column/three-row atlas patch; slime follows a three-cell row
/// then continues down its first column. Shared OAM fields are calculated.</summary>
/// <param name="frame">Zero-based shell-fragment or slime frame to compose.</param>
internal sealed class IntroEggEffectParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Applies the single-tile provider for a matching shell-fragment or slime frame.</summary>
    /// <param name="pointer">The native frame pointer to match against the intro egg effect sequence.</param>
    /// <param name="supplied">The composition that receives the provider when the pointer matches.</param>
    /// <returns>The wrapped composition for a matching frame, or <paramref name="supplied"/> unchanged.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 11; frame++)
            if (IntroEggEffectSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroEggEffectParts(frame));
        return supplied;
    }
    /// <summary>Each intro egg effect frame contributes one centered sprite part.</summary>
    public int Count => 1;

    /// <summary>Gets the sole centered tile part represented by this effect frame.</summary>
    /// <param name="index">The part index; only zero is valid.</param>
    /// <returns>The part with this frame's atlas tile and OAM palette selection.</returns>
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
    /// <summary>Enumerates the single sprite part emitted for this frame.</summary>
    /// <returns>An enumerator containing the centered effect tile.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Defines the atlas origins used to traverse the intro egg fragment and slime tiles.</summary>
internal static class IntroEggEffectAtlas
{
    /// <summary>Tile$105, upper-left of the two-column shell-fragment patch.</summary>
    internal const int FragmentStart = 0x105;
    /// <summary>Tile$10D, moving slime and origin of the impact row/column traversal.</summary>
    internal const int SlimeStart = 0x10d;
}
