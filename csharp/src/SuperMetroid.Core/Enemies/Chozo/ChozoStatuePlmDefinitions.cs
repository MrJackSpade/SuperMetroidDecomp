using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Fixed PLM header/list identities published by the two Chozo-statue actors.</summary>
internal static class ChozoStatuePlmDefinitions
{
    /// <summary>The sleeping Lower Norfair hand and its initial animation.</summary>
    public static ChozoStatuePlmDefinition LowerNorfairHand =>
        new(PlmHeaderId.LowerNorfairChozoHand, PlmProgramStart.Typed(ScriptedRoomPlmProgram.LowerNorfairChozoHand));

    /// <summary>The Wrecked Ship hand and its delete animation.</summary>
    public static ChozoStatuePlmDefinition WreckedShipHand =>
        new(PlmHeaderId.WreckedShipChozoHand, PlmProgramStart.Delete);

    /// <summary>The clear-slope-access PLM and its program.</summary>
    public static ChozoStatuePlmDefinition ClearSlopeAccess =>
        new(PlmHeaderId.ClearSlopeAccessForWreckedShipChozo, PlmProgramStart.Typed(ScriptedRoomPlmProgram.ChozoClearSlope));

    /// <summary>The block-slope-access PLM and its program.</summary>
    public static ChozoStatuePlmDefinition BlockSlopeAccess =>
        new(PlmHeaderId.BlockSlopeAccessForWreckedShipChozo, PlmProgramStart.Typed(ScriptedRoomPlmProgram.ChozoBlockSlope));

    /// <summary>The crumbling Lower Norfair plug and its draw program.</summary>
    public static ChozoStatuePlmDefinition CrumblePlug =>
        new(PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug, PlmProgramStart.Typed(ScriptedRoomPlmProgram.ChozoCrumblePlug));
    /// <summary>The five supported headers' definitions; no other data is interpreted.</summary>
    internal static readonly Dictionary<PlmHeaderId, ChozoStatuePlmDefinition> ByHeader = new()
    {
        [PlmHeaderId.LowerNorfairChozoHand] = LowerNorfairHand,
        [PlmHeaderId.WreckedShipChozoHand] = WreckedShipHand,
        [PlmHeaderId.ClearSlopeAccessForWreckedShipChozo] = ClearSlopeAccess,
        [PlmHeaderId.BlockSlopeAccessForWreckedShipChozo] = BlockSlopeAccess,
        [PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug] = CrumblePlug,
    };
}
