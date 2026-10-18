namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The bank-$84 draw list each scripted room image draws.</summary>
internal static class ScriptedRoomPlmDrawDefinitions
{
    /// <summary>Each image's draw list.</summary>
    internal static IReadOnlyDictionary<ScriptedRoomPlmFrame, ushort> DrawLists { get; } = new Dictionary<ScriptedRoomPlmFrame, ushort>
    {
        [ScriptedRoomPlmFrame.ElevatorPlatform0] = 0xaa97,
        [ScriptedRoomPlmFrame.ElevatorPlatform1] = 0xaaaf,
        [ScriptedRoomPlmFrame.ElevatorPlatform2] = 0xaac7,
        [ScriptedRoomPlmFrame.MaridiaElevatube] = 0x9367,
        [ScriptedRoomPlmFrame.OldTourianWallOpen] = 0x9283,
        [ScriptedRoomPlmFrame.EscapeAnimal0] = 0x925b,
        [ScriptedRoomPlmFrame.EscapeAnimal1] = 0x9265,
        [ScriptedRoomPlmFrame.EscapeAnimal2] = 0x926f,
        [ScriptedRoomPlmFrame.EscapeAnimal3] = 0x9279,
        [ScriptedRoomPlmFrame.CrateriaPassage] = 0x9253,
        [ScriptedRoomPlmFrame.MotherBrainGateOpen] = 0x9473,
        [ScriptedRoomPlmFrame.MotherBrainGateHalfClosed] = 0x947f,
        [ScriptedRoomPlmFrame.MotherBrainGateClosed] = 0x948b,
        [ScriptedRoomPlmFrame.SingleBreakup0] = 0xa345,
        [ScriptedRoomPlmFrame.SingleBreakup1] = 0xa34b,
        [ScriptedRoomPlmFrame.SingleBreakup2] = 0xa351,
        [ScriptedRoomPlmFrame.SingleBreakup3] = 0xa357,
        [ScriptedRoomPlmFrame.LowerNorfairChozoHand] = 0xa2b5,
        [ScriptedRoomPlmFrame.ChozoSlopeCleared] = 0x9cc5,
        [ScriptedRoomPlmFrame.ChozoSlopeBlocked] = 0x9d0f,
    };
}
