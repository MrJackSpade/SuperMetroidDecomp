using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification's native view of Samus body artwork: the bank-$92 address of each body DMA definition record and
/// frame selector, as the cartridge's pointer tables store them, with each pose's frame list laid out by the cartridge's
/// pose numbers.
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

    /// <summary>
    /// Each pose with a record's first frame selector, from the cartridge's frame-list pointer table
    /// <paramref name="pointers"/>, which holds one pointer per pose-table record in record order.
    /// </summary>
    internal static Dictionary<SamusPoseId, int> Frames(IReadOnlyList<ushort> pointers) =>
        SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose => Frame(pointers[SamusPoseNumbers.Number(pose)]));

    /// <summary>Native views of the body's tables.</summary>
    extension(SamusBodySources body)
    {
        /// <summary>The bank-$92 address of each upper-body definition group.</summary>
        internal ushort[] TopSetPointers => [.. body.TopSetStarts.ToArray().Select(RecordPointer)];

        /// <summary>The bank-$92 address of each lower-body definition group.</summary>
        internal ushort[] BottomSetPointers => [.. body.BottomSetStarts.ToArray().Select(RecordPointer)];

        /// <summary>The bank-$92 address of each pose with a record's frame list.</summary>
        internal IReadOnlyDictionary<SamusPoseId, ushort> PosePointers =>
            body.PoseFirstFrames.ToDictionary(pose => pose.Key, pose => FramePointer(pose.Value));

        /// <summary>The SNES address of the record a native body set and position select.</summary>
        internal int DefinitionAddress(SamusBodyHalf half, int set, int position) =>
            0x920000 | RecordPointer(body.DefinitionRecord(half, set, position));
    }

    /// <summary>Where the cartridge stores the body frame selectors.</summary>
    extension(SamusBodySources)
    {
        /// <summary>$92:DB48: the SNES address of the first frame selector.</summary>
        internal static int FirstFrameAddress => SamusSourceDefinitions.BodyFrames.Value;

        /// <summary>The bank-$92 address of the first frame selector.</summary>
        internal static int FirstFrameOffset => SamusSourceDefinitions.FirstBodyFrame;
    }
}
