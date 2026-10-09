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
    /// <summary>$AA:E954/E959 and following poses: adjacent foot glyphs $170/$171.</summary>
    private const int FootLeftTile = 0x170, FootRightTile = FootLeftTile + 1, TilePixels = 8;
    /// <summary>$AA:E959/E9C4/EA34/EAA4: chosen left-foot origins -31/-28/-14/-7.</summary>
    private const int TransferFootX = -31, EarlySupportFootX = -28,
        LateSupportFootX = -14, PushOffFootX = -7;

    /// <summary>Returns the native left-foot X origin for one of the four stride phases.</summary>
    /// <param name="phase">Zero-based transfer, early support, late support, or push-off phase.</param>
    /// <returns>The signed offset from the Chozo pose origin in pixels.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside zero through three.</exception>
    internal static int SupportFootX(int phase) => phase switch
    {
        0 => TransferFootX,
        1 => EarlySupportFootX,
        2 => LateSupportFootX,
        3 => PushOffFootX,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };

    /// <summary>Computes the bank-$AA spritemap pointer for one of the eight four-pose stride compositions.</summary>
    /// <param name="pose">Zero-based pose identity in the full stride sequence.</param>
    /// <returns>The native ROM address used to identify this artwork composition.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is outside zero through seven.</exception>
    internal static int NativePoseIdentity(int pose)
    {
        if ((uint)pose >= 8) throw new ArgumentOutOfRangeException(nameof(pose));
        int firstBytes = 2 + FirstPartCount * 5;
        int otherBytes = 2 + OtherPartCount * 5;
        int phase = pose % 4;
        return FirstPose + pose / 4 * (firstBytes + 3 * otherBytes) +
            (phase == 0 ? 0 : firstBytes + (phase - 1) * otherBytes);
    }

    /// <summary>Wraps recognized stock stride art so its two foot tiles follow the authored support origins.</summary>
    /// <param name="identity">Native spritemap address used to identify one of the eight stride poses.</param>
    /// <param name="supplied">Installed artwork parts; returned unchanged when the pose or foot layout does not match.</param>
    /// <returns>A foot-adjusting view for a matching stock pose, otherwise the original supplied parts.</returns>
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
            if (part.Attributes.TileNumber == FootLeftTile)
            {
                if (left >= 0 || part.X.SignedOffset != SupportFootX(phase)) return supplied;
                left = index;
            }
            else if (part.Attributes.TileNumber == FootRightTile)
            {
                if (right >= 0 || part.X.SignedOffset != SupportFootX(phase) + TilePixels) return supplied;
                right = index;
            }
        }
        if (left < 0 || right < 0) return supplied;
        return new FootParts(supplied, phase, left, right);
    }

    /// <summary>Captured non-position attributes of a support-foot glyph, allowing its X coordinate to be rebuilt per phase.</summary>
    /// <param name="XFlags">Raw X-word bits outside the nine-bit signed screen coordinate.</param>
    /// <param name="Y">Original vertical coordinate retained for every stride phase.</param>
    /// <param name="Attributes">Tile, palette, priority, and size attributes preserved from the supplied artwork.</param>
    private readonly record struct FootAppearance(ushort XFlags, byte Y, SnesObjAttributeWord Attributes)
    {
        /// <summary>Captures a foot part's immutable Y and attribute data plus X-word flags.</summary>
        /// <param name="part">Supplied artwork part identified as one of the two foot glyphs.</param>
        /// <returns>A snapshot that can recreate the part at a phase-specific X origin.</returns>
        internal static FootAppearance From(EnemySpritemapPart part) =>
            new((ushort)(part.X.Raw & ~0x1ff), part.Y, part.Attributes);

        /// <summary>Recreates the foot glyph at a new nine-bit X coordinate while preserving its other data.</summary>
        /// <param name="x">Signed pixel offset encoded into the low nine bits.</param>
        /// <returns>The adjusted artwork part.</returns>
        internal EnemySpritemapPart At(int x) =>
            new(new SnesSpritemapXWord((ushort)(XFlags | (x & 0x1ff))), Y, Attributes);
    }

    /// <summary>Indexed view over supplied stride artwork that substitutes only the two validated foot glyph X positions.</summary>
    private sealed class FootParts : EnemySpritemapParts
    {
        /// <summary>All supplied artwork parts except the two foot glyphs, kept in their original order.</summary>
        private readonly EnemySpritemapPart[] otherParts;
        /// <summary>Captured left and right foot parts, retaining their distinct tile attributes.</summary>
        private readonly FootAppearance left, right;
        /// <summary>Stride phase and source indices used to place the captured foot parts in the original sequence.</summary>
        private readonly int phase, leftIndex, rightIndex;

        /// <summary>Captures the foot glyphs and other parts so phase-specific indexing can return adjusted artwork.</summary>
        /// <param name="supplied">Original spritemap parts containing the validated foot tiles.</param>
        /// <param name="phase">Stride phase selecting the shared left-foot origin.</param>
        /// <param name="leftIndex">Original part index occupied by the left-foot tile.</param>
        /// <param name="rightIndex">Original part index occupied by the right-foot tile.</param>
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
        /// <summary>Number of parts in the original composition, including both substituted feet.</summary>
        public override int Count => otherParts.Length + 2;

        /// <summary>Returns an original non-foot part or a foot part positioned for the selected stride phase.</summary>
        /// <param name="index">Zero-based position in the original spritemap part order.</param>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the composition.</exception>
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
