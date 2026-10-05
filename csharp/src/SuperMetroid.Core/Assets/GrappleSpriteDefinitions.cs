using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Presentation identities for the endpoint and four timed rope records.</summary>
public static class GrappleSpriteDefinitions
{
    public const string FileName = "grapple-sprites.json";
    public const int Version = 1;
    /// <summary>$94:B13D: immediate endpoint attributes in DrawGrappleBeamEnd_NotConnected.</summary>
    public const int EndpointAttributeAddress = 0x94b13d;
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

    internal static ushort StockSegment(int frame) => SnesObjAttributeWord.Create(
        FirstSegmentTile + frame, SegmentPalette, SegmentPriority, default).Raw;

    /// <summary>Calculated four-byte stride through the native segment attribute operands.</summary>
    public readonly struct SegmentAddressList : IReadOnlyList<int>
    {
        public int Count => SegmentFrameCount;
        public int Length => Count;
        public int this[int index] => (uint)index < SegmentFrameCount
            ? 0x94b18d + 4 * index : throw new IndexOutOfRangeException();
        public IEnumerator<int> GetEnumerator()
        {
            for (int index = 0; index < SegmentFrameCount; index++)
                yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
