using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Immutable stock support-foot geometry shared by Chozo pose matching and carry motion.
/// The four chosen origins and all other artwork inputs remain required under Stream3's
/// EnemySpritemapCatalog.frames; this is not an artwork-retention exception.
/// </summary>
internal static class ChozoStrideGeometryDefinitions
{
    /// <summary>$AA:E943, Spritemaps_Chozo_4: first of eight stride compositions.</summary>
    private const int FirstPose = 0xaae943;
    /// <summary>$AA:E943/EAFE: the first pose of each four-pose stride has21 five-byte parts.</summary>
    private const int FirstPartCount = 21;
    /// <summary>$AA:E9AE/EA1E/EA8E and EB69/EBD9/EC49: other stride poses have22parts.</summary>
    private const int OtherPartCount = 22;
    /// <summary>Width of one OBJ glyph, the distance from the left foot to the adjacent right foot.</summary>
    private const int TilePixels = 8;

    /// <summary>$AA:E954/E959 and following poses: the adjacent support-foot glyphs.</summary>
    private enum FootGlyph
    {
        /// <summary>OBJ tile $170, the left half of the support foot.</summary>
        Left = 0x170,
        /// <summary>OBJ tile $171, the right half of the support foot.</summary>
        Right = 0x171,
    }
    /// <summary>$AA:E959/E9C4/EA34/EAA4: chosen left-foot origins -31/-28/-14/-7.</summary>
    private const int TransferFootX = -31, EarlySupportFootX = -28,
        LateSupportFootX = -14, PushOffFootX = -7;

    /// <summary>support foot x.</summary>
    internal static int SupportFootX(int phase) => phase switch
    {
        0 => TransferFootX,
        1 => EarlySupportFootX,
        2 => LateSupportFootX,
        3 => PushOffFootX,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };

    /// <summary>native pose identity.</summary>
    internal static int NativePoseIdentity(int pose)
    {
        if ((uint)pose >= 8) throw new ArgumentOutOfRangeException(nameof(pose));
        int firstBytes = 2 + FirstPartCount * 5;
        int otherBytes = 2 + OtherPartCount * 5;
        int phase = pose % 4;
        return FirstPose + pose / 4 * (firstBytes + 3 * otherBytes) +
            (phase == 0 ? 0 : firstBytes + (phase - 1) * otherBytes);
    }

    /// <summary>compile.</summary>
    internal static EnemySpritemapParts Compile(int identity, EnemySpritemapParts supplied)
    {
        int pose = 0;
        while (pose < 8 && NativePoseIdentity(pose) != identity) pose++;
        if (pose == 8) return supplied;
        int phase = pose % 4;
        int left = -1, right = -1;
        for (int index = 0; index < supplied.Count; index++)
        {
            EnemySpritemapPart part = supplied[index];
            if (!Enum.IsDefined((FootGlyph)part.Attributes.TileNumber))
                continue;
            switch ((FootGlyph)part.Attributes.TileNumber)
            {
                case FootGlyph.Left:
                    if (left >= 0 || part.X.SignedOffset != SupportFootX(phase)) return supplied;
                    left = index;
                    break;
                case FootGlyph.Right:
                    if (right >= 0 || part.X.SignedOffset != SupportFootX(phase) + TilePixels) return supplied;
                    right = index;
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Undefined {nameof(FootGlyph)} {part.Attributes.TileNumber}.");
            }
        }
        if (left < 0 || right < 0) return supplied;
        return new FootParts(supplied, phase, left, right);
    }

    /// <summary>Represents foot appearance.</summary>
    private readonly record struct FootAppearance(ushort XFlags, byte Y, SnesObjAttributeWord Attributes)
    {
        /// <summary>from.</summary>
        internal static FootAppearance From(EnemySpritemapPart part) =>
            new((ushort)(part.X.Raw & ~0x1ff), part.Y, part.Attributes);
        /// <summary>at.</summary>
        internal EnemySpritemapPart At(int x) =>
            new(new SnesSpritemapXWord((ushort)(XFlags | (x & 0x1ff))), Y, Attributes);
    }

    /// <summary>Represents foot parts.</summary>
    private sealed class FootParts : EnemySpritemapParts
    {
        /// <summary>Stores other parts.</summary>
        private readonly EnemySpritemapPart[] otherParts;
        /// <summary>Stores left.</summary>
        private readonly FootAppearance left, right;
        /// <summary>Stores phase.</summary>
        private readonly int phase, leftIndex, rightIndex;

        /// <summary>Initializes a new foot parts instance.</summary>
        internal FootParts(EnemySpritemapParts supplied, int phase, int leftIndex, int rightIndex)
        {
            this.phase = phase;
            this.leftIndex = leftIndex;
            this.rightIndex = rightIndex;
            left = FootAppearance.From(supplied[leftIndex]);
            right = FootAppearance.From(supplied[rightIndex]);
            otherParts = new EnemySpritemapPart[supplied.Count - 2];
            int output = 0;
            for (int index = 0; index < supplied.Count; index++)
                if (index != leftIndex && index != rightIndex) otherParts[output++] = supplied[index];
        }
        /// <summary>Gets the number of items in this collection.</summary>
        public override int Count => otherParts.Length + 2;
        /// <summary>Accesses a value by index or range.</summary>
        public override EnemySpritemapPart this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index == leftIndex) return left.At(SupportFootX(phase));
                if (index == rightIndex) return right.At(SupportFootX(phase) + TilePixels);
                return otherParts[index - (index > leftIndex ? 1 : 0) - (index > rightIndex ? 1 : 0)];
            }
        }
    }
}