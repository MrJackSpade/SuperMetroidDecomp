using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Presentation identities for the endpoint and four timed rope records.</summary>
public static class GrappleSpriteDefinitions
{
    /// <summary>Editable JSON filename for Grapple's endpoint and four rope-segment OBJ attribute selections, excluding segment timing and beam mechanics.</summary>
    public const string FileName = "grapple-sprites.json";
    /// <summary>Supported revision of the endpoint-plus-four-segment visual attribute schema.</summary>
    public const int Version = 1;
    /// <summary>$94:B18D/B191/B195/B199: visual attribute words in the four timed segment records.</summary>
    public static SegmentAddressList SegmentAttributeAddresses => default;

    /// <summary>Four frames at $94:B18B-$B19A, each containing a duration and one attribute word.</summary>
    public const int SegmentFrameCount = 4;
    /// <summary>$94:B18D first OBJ tile: four consecutive rope-animation tiles $21-$24.</summary>
    private const int FirstSegmentTile = 0x21;
    /// <summary>$94:B18D-$B199 common OBJ palette five for rope segments.</summary>
    private const int SegmentPalette = 5;
    /// <summary>$94:B18D-$B199 common OBJ priority three for rope segments.</summary>
    private const int SegmentPriority = 3;

    /// <summary>The native rope appearance of a timed frame: characters $21-$24 with palette five and priority three.</summary>
    internal static SpritePart StockSegment(int frame) =>
        SpritePart.Single(FirstSegmentTile + frame, SegmentPalette, SegmentPriority);

    /// <summary>Calculated four-byte stride through the native segment attribute operands.</summary>
    public readonly struct SegmentAddressList : IReadOnlyList<int>
    {
        /// <summary>Four native visual attribute operands, one per timed rope record; not a count of drawn rope pieces.</summary>
        public int Count => SegmentFrameCount;
        /// <summary>Array-style alias of <see cref="Count"/> used to validate the four editable segment selections.</summary>
        public int Length => Count;
        /// <summary>Calculates the full bank-$94 address of a segment's OBJ attribute word, skipping the duration word in each four-byte record.</summary>
        /// <param name="index">Zero-based segment-animation frame, 0..3.</param>
        /// <returns>$94:B18D, $B191, $B195, or $B199; these are visual operands rather than timed-record starts.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0..3.</exception>
        public int this[int index] => (uint)index < SegmentFrameCount
            ? 0x94b18d + 4 * index : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Enumerates the four segment attribute addresses in native animation-record order without advancing animation.</summary>
        /// <returns>An enumerator yielding the same addresses as indexed access.</returns>
        public IEnumerator<int> GetEnumerator()
        {
            for (int index = 0; index < SegmentFrameCount; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same four address selections to non-generic collection consumers.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
