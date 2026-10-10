using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Visual OAM compositions selected by the SR388 egg and confused-baby lists.</summary>
internal static class IntroDiscoveryActorSpriteDefinitions
{
    /// <summary>$8C:8D6F, first egg composition.</summary>
    internal const ushort EggStart = 0x8d6f;
    /// <summary>$8C:8FCB, first confused-baby composition.</summary>
    internal const ushort BabyStart = 0x8fcb;
    /// <summary>$8C:909D, Ceres large asteroids reused by the second confused-baby list.
    /// The published asset key remains hatched-baby.</summary>
    internal const ushort BabyLarge = 0x909d;

    /// <summary>Six-part intact record, eight nine-part cracking records, then seven three-part remnants.</summary>
    internal static ushort EggFramePointer(int frame)
    {
        if ((uint)frame >= 16) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(EggStart + (frame > 0 ? 32 : 0) + 47 * Math.Clamp(frame - 1, 0, 8) + 17 * Math.Max(frame - 9, 0));
    }

    /// <summary>Three consecutive one-part confused-baby records, seven bytes each.</summary>
    internal static ushort BabyFramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(BabyStart + 7 * frame);
    }

    /// <summary>
    /// Provides the ordered set of egg, confused-baby, and reused large-baby OAM compositions.
    /// </summary>
    internal static IReadOnlyList<IntroDiscoveryActorSpriteFrameDefinition> Frames { get; } = new FrameList();

    /// <summary>
    /// Computes each visual definition from its ordinal without storing a separate frame array.
    /// </summary>
    private sealed class FrameList : IReadOnlyList<IntroDiscoveryActorSpriteFrameDefinition>
    {
        /// <summary>
        /// Gets the number of egg and confused-baby visual definitions in the ordered list.
        /// </summary>
        public int Count => 20;

        /// <summary>
        /// Gets a frame definition by list order, mapping the ordinal to its ROM pointer, asset name, and stock part count.
        /// </summary>
        /// <param name="index">The zero-based position in the combined visual-definition list.</param>
        /// <returns>The visual definition represented by that position.</returns>
        public IntroDiscoveryActorSpriteFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index == 19) return new(BabyLarge, "hatched-baby", 19);
                if (index >= 16) return new(BabyFramePointer(index - 16), $"confused-baby-{index - 15}", 1);
                string name = index == 0 ? "egg-intact" : index == 8 ? "egg-hatched"
                    : index < 8 ? $"egg-crack-{index}" : $"egg-remnant-{index - 8}";
                return new(EggFramePointer(index), name, index == 0 ? 6 : index < 9 ? 9 : 3);
            }
        }
        /// <summary>
        /// Enumerates all egg and confused-baby definitions in their published list order.
        /// </summary>
        /// <returns>An enumerator over the computed frame definitions.</returns>
        public IEnumerator<IntroDiscoveryActorSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>
/// Identifies one intro-discovery actor sprite composition and the number of stock OAM parts it contains.
/// </summary>
/// <param name="Pointer">The bank-relative ROM pointer to the composition's spritemap data.</param>
/// <param name="Name">The asset key used to identify this composition in installed presentation data.</param>
/// <param name="StockPartCount">The number of sprite parts authored in the stock composition.</param>
internal readonly record struct IntroDiscoveryActorSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
