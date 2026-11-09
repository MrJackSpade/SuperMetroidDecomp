namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The draw each scripted room image draws.</summary>
internal static class ScriptedRoomObjectDrawDefinitions
{
    /// <summary>Each image's draw.</summary>
    internal static IReadOnlyDictionary<ScriptedRoomObjectFrame, RoomObjectDraw> DrawLists { get; } = new Dictionary<ScriptedRoomObjectFrame, RoomObjectDraw>
    {
        [ScriptedRoomObjectFrame.MaridiaElevatube] = MaridiaElevatubeObjectDefinitions.BlockDraw,
        [ScriptedRoomObjectFrame.OldTourianWallOpen] = OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw,
        [ScriptedRoomObjectFrame.EscapeAnimal0] = RoomObjectDraw.Of(EscapeAnimalDraw.Frame0),
        [ScriptedRoomObjectFrame.EscapeAnimal1] = RoomObjectDraw.Of(EscapeAnimalDraw.Frame1),
        [ScriptedRoomObjectFrame.EscapeAnimal2] = RoomObjectDraw.Of(EscapeAnimalDraw.Frame2),
        [ScriptedRoomObjectFrame.EscapeAnimal3] = RoomObjectDraw.Of(EscapeAnimalDraw.Blank),
        [ScriptedRoomObjectFrame.CrateriaPassage] = CrateriaMainstreetEscapePassageObjectDefinitions.ClearPairDraw,
        [ScriptedRoomObjectFrame.SingleBreakup0] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0),
        [ScriptedRoomObjectFrame.SingleBreakup1] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 1),
        [ScriptedRoomObjectFrame.SingleBreakup2] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 2),
        [ScriptedRoomObjectFrame.SingleBreakup3] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 3),
        [ScriptedRoomObjectFrame.LowerNorfairChozoHand] = RoomObjectDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand),
        [ScriptedRoomObjectFrame.ChozoSlopeCleared] = RoomObjectDraw.Of(ChozoStatueDraw.ClearSlopeAccess),
        [ScriptedRoomObjectFrame.ChozoSlopeBlocked] = RoomObjectDraw.Of(ChozoStatueDraw.BlockSlopeAccess),
    };
}
