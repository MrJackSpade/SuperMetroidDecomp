namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The draw each scripted room image draws.</summary>
internal static class ScriptedRoomPlmDrawDefinitions
{
    /// <summary>Each image's draw.</summary>
    internal static IReadOnlyDictionary<ScriptedRoomPlmFrame, PlmDraw> DrawLists { get; } = new Dictionary<ScriptedRoomPlmFrame, PlmDraw>
    {
        [ScriptedRoomPlmFrame.ElevatorPlatform0] = PlmDraw.Of(ElevatorPlatformDraw.First),
        [ScriptedRoomPlmFrame.ElevatorPlatform1] = PlmDraw.Of(ElevatorPlatformDraw.Second),
        [ScriptedRoomPlmFrame.ElevatorPlatform2] = PlmDraw.Of(ElevatorPlatformDraw.Third),
        [ScriptedRoomPlmFrame.MaridiaElevatube] = MaridiaElevatubePlmDefinitions.BlockDraw,
        [ScriptedRoomPlmFrame.OldTourianWallOpen] = OldTourianEscapeShaftWallPlmDefinitions.OpenWallDraw,
        [ScriptedRoomPlmFrame.EscapeAnimal0] = PlmDraw.Of(EscapeAnimalDraw.Frame0),
        [ScriptedRoomPlmFrame.EscapeAnimal1] = PlmDraw.Of(EscapeAnimalDraw.Frame1),
        [ScriptedRoomPlmFrame.EscapeAnimal2] = PlmDraw.Of(EscapeAnimalDraw.Frame2),
        [ScriptedRoomPlmFrame.EscapeAnimal3] = PlmDraw.Of(EscapeAnimalDraw.Blank),
        [ScriptedRoomPlmFrame.CrateriaPassage] = CrateriaMainstreetEscapePassagePlmDefinitions.ClearPairDraw,
        [ScriptedRoomPlmFrame.MotherBrainGateOpen] = PlmDraw.Of(MotherBrainEscapeGateDraw.Open),
        [ScriptedRoomPlmFrame.MotherBrainGateHalfClosed] = PlmDraw.Of(MotherBrainEscapeGateDraw.HalfClosed),
        [ScriptedRoomPlmFrame.MotherBrainGateClosed] = PlmDraw.Of(MotherBrainEscapeGateDraw.Closed),
        [ScriptedRoomPlmFrame.SingleBreakup0] = RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 0),
        [ScriptedRoomPlmFrame.SingleBreakup1] = RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 1),
        [ScriptedRoomPlmFrame.SingleBreakup2] = RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 2),
        [ScriptedRoomPlmFrame.SingleBreakup3] = RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 3),
        [ScriptedRoomPlmFrame.LowerNorfairChozoHand] = PlmDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand),
        [ScriptedRoomPlmFrame.ChozoSlopeCleared] = PlmDraw.Of(ChozoStatueDraw.ClearSlopeAccess),
        [ScriptedRoomPlmFrame.ChozoSlopeBlocked] = PlmDraw.Of(ChozoStatueDraw.BlockSlopeAccess),
    };
}
