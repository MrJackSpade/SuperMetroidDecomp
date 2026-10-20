using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The five one-block draw lists used by breakable-Grapple-block PLMs. These are
/// complete native level words, not merely tile references: the initial frame
/// installs Grapple collision and the later frames install air collision.
/// </summary>
internal static class RoomPlmGrappleBlockDrawDefinitions
{
    /// <summary>Initial Grapple collision/appearance, $84:A4F9.</summary>
    internal const ushort Grapple = 0xa4f9;

    /// <summary>A single Grapple-block PLM draw record carrying both its native pointer and installed level word.</summary>
    internal readonly record struct DrawList(ushort Pointer, ushort LevelWord)
    {
        /// <summary>Native one-word row, followed by signed zero X/Y termination.</summary>
        internal const ushort DirectionAndCount = 1;
        /// <summary>The one-block record leaves the next draw position in its current column.</summary>
        internal const sbyte NextX = 0;
        /// <summary>The one-block record leaves the next draw position in its current row.</summary>
        internal const sbyte NextY = 0;
    }

    /// <summary>Enumerates the initial Grapple block, three breakup frames, and blank air in native order.</summary>
    internal static IEnumerable<DrawList> All
    {
        get
        {
            // Five adjacent six-byte records in the original publication order.
            for (int frame = 0; frame < LevelWords.Length; frame++)
            {
                TryGet((ushort)(Grapple + RecordBytes * frame), out DrawList draw);
                yield return draw;
            }
        }
    }

    /// <summary>Six-byte native length of each one-block draw record.</summary>
    private const int RecordBytes = 6;
    /// <summary>
    /// Installed level words of the five adjacent records in native order: the Grapple block, the three
    /// consecutive breakup tiles as air, and blank air.
    /// </summary>
    private static readonly RoomLevelWord[] LevelWords =
    [
        RoomLevelWord.Create(0x0b7, LevelBlockFlipFlags.None, RoomCollisionType.GrappleBlock),
        RoomLevelWord.Create(0x053, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        RoomLevelWord.Create(0x054, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        RoomLevelWord.Create(0x055, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        RoomLevelWord.Create(0x0ff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
    ];

    /// <summary>Resolves only the five compiled draw-list pointers; unknown pointers produce a default record.</summary>
    internal static bool TryGet(ushort pointer, out DrawList definition)
    {
        int relative = pointer - Grapple;
        if (relative < 0 || relative % RecordBytes != 0 || relative / RecordBytes >= LevelWords.Length)
        {
            definition = default;
            return false;
        }
        definition = new(pointer, LevelWords[relative / RecordBytes].Raw);
        return true;
    }
}
