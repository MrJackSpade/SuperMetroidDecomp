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

    /// <summary>The slope shape written at the first slope block.</summary>
    public static readonly RoomBlockBehavior FirstSlopeBehavior = RoomBlockBehavior.Sloping(new SlopeGeometry(18, BlockMirroring.None));

    /// <summary>The slope shape written at the adjacent slope block.</summary>
    public static readonly RoomBlockBehavior SecondSlopeBehavior = RoomBlockBehavior.Sloping(new SlopeGeometry(19, BlockMirroring.None));

    /// <summary>The special-solid behaviour of the Wrecked Ship hand's block in that area's behaviour table.</summary>
    public static readonly RoomBlockBehavior WreckedShipHandBehavior = RoomBlockBehavior.Reacting(SpecialBlockReaction.WreckedShipChozoHand);

    /// <summary>The special-solid behaviour of the Lower Norfair hand's block in Norfair's behaviour table.</summary>
    public static readonly RoomBlockBehavior LowerNorfairHandBehavior = RoomBlockBehavior.Reacting(SpecialBlockReaction.BrinstarSlowPermanentSpeedBlockOrNorfairChozoHand);

    /// <summary>
    /// The block the crumbling plug writes when the hand's trigger spawns it: its setup never chooses one, so it writes
    /// what the room loader's last draw left behind, block 416 mirrored both ways, as air.
    /// </summary>
    /// <remarks>
    /// Gameplay terrain, not a picture (#1580): nothing pictures it, and when room objects run, the plug draws its first
    /// breakup image over it in the room-object pass later in the same update. Until then it is read as terrain: its
    /// air by Samus's and her shots' collision, and its block number and mirroring by a Wave shot's scan past the room's
    /// ceiling (<see cref="Compatibility.StraddledTerrain{TTerrain}"/>), whose straddled terrain takes its collision kind from them
    /// and keeps the block it read as the block to restore.
    /// </remarks>
    public static readonly RoomTerrainBlock HandCrumbleSetupBlock =
        RoomTerrainBlock.Of(416, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air);

    /// <summary>The block the crumbling plug writes when the walking statue's footstep spawns it: block zero, unmirrored, as air.</summary>
    /// <remarks>
    /// Gameplay terrain, not a picture (#1580): its setup never chooses a block, so nothing pictures it. The statue's
    /// spawn is applied right after the enemy pass, and when room objects run the plug draws its first breakup image over
    /// it in the room-object pass later in the same update. Until then it is read as terrain: its air by Samus's and her
    /// shots' collision, and its block number and mirroring by a Wave shot's scan past the room's ceiling
    /// (<see cref="Compatibility.StraddledTerrain{TTerrain}"/>).
    /// </remarks>
    public static readonly RoomTerrainBlock FootstepCrumbleSetupBlock =
        RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air);

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
