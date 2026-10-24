using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification's native view of Samus body artwork: the bank-$92 address of each body DMA definition record and
/// frame selector, as the cartridge's pointer tables store them.
/// </summary>
internal static class SamusBodyNative
{
    /// <summary>The bank-$92 address of body definition <paramref name="record"/>.</summary>
    internal static ushort RecordPointer(int record) =>
        checked((ushort)(SamusSourceDefinitions.FirstBodyDefinition + record * SamusSourceDefinitions.BodyDefinitionBytes));

    /// <summary>The body definition record stored at bank-$92 <paramref name="pointer"/>.</summary>
    internal static int Record(int pointer) =>
        (pointer & 0xffff) >= SamusSourceDefinitions.FirstBodyDefinition && ((pointer & 0xffff) - SamusSourceDefinitions.FirstBodyDefinition) % SamusSourceDefinitions.BodyDefinitionBytes == 0
            ? ((pointer & 0xffff) - SamusSourceDefinitions.FirstBodyDefinition) / SamusSourceDefinitions.BodyDefinitionBytes
            : throw new InvalidDataException($"${pointer:X4} is not a Samus body definition record.");

    /// <summary>The bank-$92 address of frame selector <paramref name="frame"/>.</summary>
    internal static ushort FramePointer(int frame) => checked((ushort)(SamusSourceDefinitions.FirstBodyFrame + frame * 4));

    /// <summary>The frame selector stored at bank-$92 <paramref name="pointer"/>.</summary>
    internal static int Frame(int pointer) =>
        (pointer & 0xffff) >= SamusSourceDefinitions.FirstBodyFrame && ((pointer & 0xffff) - SamusSourceDefinitions.FirstBodyFrame) % 4 == 0
            ? ((pointer & 0xffff) - SamusSourceDefinitions.FirstBodyFrame) / 4
            : throw new InvalidDataException($"${pointer:X4} is not a Samus body frame selector.");

    /// <summary>The records stored at <paramref name="pointers"/>.</summary>
    internal static int[] Records(IEnumerable<ushort> pointers) => [.. pointers.Select(pointer => Record(pointer))];

    /// <summary>The frame selectors stored at <paramref name="pointers"/>.</summary>
    internal static int[] Frames(IEnumerable<ushort> pointers) => [.. pointers.Select(pointer => Frame(pointer))];

    /// <summary>Native views of the body catalog.</summary>
    extension(SamusBodyArtworkCatalog body)
    {
        /// <summary>The bank-$92 address of each upper-body definition group.</summary>
        internal ushort[] TopSetPointers => [.. body.TopSetStarts.ToArray().Select(RecordPointer)];

        /// <summary>The bank-$92 address of each lower-body definition group.</summary>
        internal ushort[] BottomSetPointers => [.. body.BottomSetStarts.ToArray().Select(RecordPointer)];

        /// <summary>The bank-$92 address of each pose's frame list.</summary>
        internal ushort[] PosePointers => [.. body.PoseFirstFrames.ToArray().Select(FramePointer)];

        /// <summary>The SNES address of the record a native body set and position select.</summary>
        internal int DefinitionAddress(SamusBodyHalf half, int set, int position) =>
            0x920000 | RecordPointer(body.DefinitionRecord(half, set, position));
    }

    /// <summary>Where the cartridge stores the body frame selectors.</summary>
    extension(SamusBodyArtworkCatalog)
    {
        /// <summary>$92:DB48: the SNES address of the first frame selector.</summary>
        internal static int FirstFrameAddress => SamusSourceDefinitions.BodyFrames.Value;

        /// <summary>The bank-$92 address of the first frame selector.</summary>
        internal static int FirstFrameOffset => SamusSourceDefinitions.FirstBodyFrame;
    }

    /// <summary>Native views of the shown body definitions.</summary>
    extension(SamusBodyFrameState frames)
    {
        /// <summary>The SNES address of the shown top-half definition record.</summary>
        internal int TopDefinitionAddress => 0x920000 | RecordPointer(frames.Shown.Top ??
            throw new InvalidOperationException("No top-half definition is shown."));

        /// <summary>The SNES address of the shown bottom-half definition record, or null before any frame named one.</summary>
        internal int? BottomDefinitionAddress => frames.Shown.Bottom is int bottom ? 0x920000 | RecordPointer(bottom) : null;
    }
}
