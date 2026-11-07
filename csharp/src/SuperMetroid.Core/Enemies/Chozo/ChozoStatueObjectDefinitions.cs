using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Fixed PLM header/list identities published by the two Chozo-statue actors.</summary>
internal static class ChozoStatueObjectDefinitions
{
    /// <summary>The sleeping Lower Norfair hand and its initial animation.</summary>
    public static ChozoStatueObjectDefinition LowerNorfairHand =>
        new(RoomObjectHeaderId.LowerNorfairChozoHand, RoomObjectProgramStart.Typed(ScriptedRoomObjectProgram.LowerNorfairChozoHand));

    /// <summary>The Wrecked Ship hand and its delete animation.</summary>
    public static ChozoStatueObjectDefinition WreckedShipHand =>
        new(RoomObjectHeaderId.WreckedShipChozoHand, RoomObjectProgramStart.Delete);

    /// <summary>The clear-slope-access PLM and its program.</summary>
    public static ChozoStatueObjectDefinition ClearSlopeAccess =>
        new(RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo, RoomObjectProgramStart.Typed(ScriptedRoomObjectProgram.ChozoClearSlope));

    /// <summary>The block-slope-access PLM and its program.</summary>
    public static ChozoStatueObjectDefinition BlockSlopeAccess =>
        new(RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo, RoomObjectProgramStart.Typed(ScriptedRoomObjectProgram.ChozoBlockSlope));

    /// <summary>The crumbling Lower Norfair plug and its draw program.</summary>
    public static ChozoStatueObjectDefinition CrumblePlug =>
        new(RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug, RoomObjectProgramStart.Typed(ScriptedRoomObjectProgram.ChozoCrumblePlug));
    /// <summary>The five supported headers' definitions; no other data is interpreted.</summary>
    internal static readonly Dictionary<RoomObjectHeaderId, ChozoStatueObjectDefinition> ByHeader = new()
    {
        [RoomObjectHeaderId.LowerNorfairChozoHand] = LowerNorfairHand,
        [RoomObjectHeaderId.WreckedShipChozoHand] = WreckedShipHand,
        [RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo] = ClearSlopeAccess,
        [RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo] = BlockSlopeAccess,
        [RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug] = CrumblePlug,
    };
}
