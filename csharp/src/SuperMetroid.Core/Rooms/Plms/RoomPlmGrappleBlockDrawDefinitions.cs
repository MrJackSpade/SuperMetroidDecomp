using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The five one-block draw lists used by breakable-Grapple-block PLMs. These are
/// complete native level words, not merely tile references: the initial frame
/// installs Grapple collision and the later frames install air collision.
/// </summary>
internal static class RoomPlmGrappleBlockDrawDefinitions
{
    /// <summary>A single Grapple-block draw with the level word it installs.</summary>
    internal readonly record struct DrawList(GrappleBlockDraw Frame, RoomLevelWord LevelWord)
    {
        /// <summary>The one-block run writes along its row.</summary>
        internal const PlmRunDirection Direction = PlmRunDirection.Horizontal;
        /// <summary>The run writes its one level word, followed by signed zero X/Y termination.</summary>
        internal const int Count = 1;
        /// <summary>The one-block record leaves the next draw position in its current column.</summary>
        internal const sbyte NextX = 0;
        /// <summary>The one-block record leaves the next draw position in its current row.</summary>
        internal const sbyte NextY = 0;
    }

    /// <summary>The value each key selects; LevelWord looks it up.</summary>
    internal static readonly IReadOnlyDictionary<GrappleBlockDraw, RoomLevelWord> LevelWords = new Dictionary<GrappleBlockDraw, RoomLevelWord>
    {
        [GrappleBlockDraw.Intact] = RoomLevelWord.Create(0x0b7, LevelBlockFlipFlags.None, RoomCollisionType.GrappleBlock),
        [GrappleBlockDraw.Break0] = RoomLevelWord.Create(0x053, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Break1] = RoomLevelWord.Create(0x054, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Break2] = RoomLevelWord.Create(0x055, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        [GrappleBlockDraw.Blank] = RoomLevelWord.Create(0x0ff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
    };
}
