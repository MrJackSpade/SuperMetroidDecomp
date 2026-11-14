using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The room blocks, block behaviours and heights the Chozo statues' hands, slopes and acid use.</summary>
internal static class ChozoStatueRoomDefinitions
{
    /// <summary>The base height the Wrecked Ship statue's draining acid settles at, in room pixels.</summary>
    public const int LoweredAcidY = 722;

    /// <summary>
    /// The first of the two slope blocks the Wrecked Ship statue opens and closes in the Bowling Alley; the second is the
    /// block to its right, on the same row.
    /// </summary>
    public static readonly Position16<RoomBlocks> FirstSlopeBlock = new(36, 29);

    /// <summary>The slope written at the first slope block: a slope rising one pixel a column.</summary>
    public static readonly RoomBlockBehavior FirstSlopeBehavior = RoomBlockBehavior.Sloping(new SlopeGeometry(SlopeShapeDefinitions.UnitRise, BlockMirroring.None));

    /// <summary>The slope written at the adjacent slope block: a flat surface at the block's top.</summary>
    public static readonly RoomBlockBehavior SecondSlopeBehavior = RoomBlockBehavior.Sloping(new SlopeGeometry(SlopeShapeDefinitions.Solid, BlockMirroring.None));

    /// <summary>The behaviour of the Wrecked Ship hand's special block: the Wrecked Ship Chozo hand.</summary>
    public static readonly RoomBlockBehavior WreckedShipHandBehavior = RoomBlockBehavior.Reacting(SpecialBlockReaction.WreckedShipChozoHand);

    /// <summary>The behaviour of the Lower Norfair hand's special block: the Lower Norfair Chozo hand.</summary>
    public static readonly RoomBlockBehavior LowerNorfairHandBehavior = RoomBlockBehavior.Reacting(SpecialBlockReaction.LowerNorfairChozoHand);

    /// <summary>The column of the Power Bomb block whose breaking open triggers the Lower Norfair hand.</summary>
    public const int LowerNorfairTriggerX = 4;

    /// <summary>The row of the Power Bomb block whose breaking open triggers the Lower Norfair hand.</summary>
    public const int LowerNorfairTriggerY = 8;

    /// <summary>The Wrecked Ship hand's trigger block, where the hand room object is also placed.</summary>
    public static readonly Position16<RoomBlocks> WreckedShipHandBlock = new(74, 23);

    /// <summary>The block where the Wrecked Ship statue's slope-access room objects are placed.</summary>
    public static readonly Position16<RoomBlocks> WreckedShipSlopeAccessBlock = new(23, 29);

    /// <summary>The block where the Lower Norfair hand and room-plug room objects are placed.</summary>
    public static readonly Position16<RoomBlocks> LowerNorfairHandBlock = new(12, 29);

    /// <summary>
    /// The Bowling Alley screens the Wrecked Ship statue's waking opens to the camera, in the order it opens them: the second
    /// and third screens of the room's second row.
    /// </summary>
    public static readonly IReadOnlyList<Position16<RoomScreens>> WakingOpenScreens = [new(1, 1), new(2, 1)];

    /// <summary>
    /// The Bowling Alley screens the Wrecked Ship statue's waking then opens with the camera aligned to their top, in the
    /// order it opens them: the second and third screens of the room's third row, below the open ones.
    /// </summary>
    public static readonly IReadOnlyList<Position16<RoomScreens>> WakingTopAlignedScreens = [new(1, 2), new(2, 2)];

    /// <summary>
    /// The Bowling Alley screens the Wrecked Ship statue's release closes, in the order it closes them: the first five
    /// screens of the room's second row.
    /// </summary>
    public static readonly IReadOnlyList<Position16<RoomScreens>> ReleaseClosedScreens = [new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1)];

    /// <summary>The Bowling Alley screen the Wrecked Ship statue's release then opens with the camera aligned to its top: the third row's second screen.</summary>
    public static readonly Position16<RoomScreens> ReleaseTopAlignedScreen = new(1, 2);

    /// <summary>The Bowling Alley screen the Wrecked Ship statue's release closes last: the third row's third screen.</summary>
    public static readonly Position16<RoomScreens> ReleaseClosedLastScreen = new(2, 2);
}
