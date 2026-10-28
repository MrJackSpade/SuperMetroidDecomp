using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Fixed PLM header/list identities published by the two Chozo-statue actors.</summary>
internal static class ChozoStatuePlmDefinitions
{
    /// <summary><c>$84:D6D6/$D13F</c>, sleeping Lower Norfair hand and its initial list.</summary>
    public static ChozoStatuePlmDefinition LowerNorfairHand =>
        new(PlmHeaderId.LowerNorfairChozoHand, PlmProgramStart.Typed(ScriptedRoomPlmProgram.LowerNorfairChozoHand));

    /// <summary><c>$84:D6EE/$AAE3</c>, Wrecked Ship hand and its delete list.</summary>
    public static ChozoStatuePlmDefinition WreckedShipHand =>
        new(PlmHeaderId.WreckedShipChozoHand, PlmProgramStart.Delete);

    /// <summary><c>$84:D6F8/$D3CF</c>, clear-slope-access header and program.</summary>
    public static ChozoStatuePlmDefinition ClearSlopeAccess =>
        new(PlmHeaderId.ClearSlopeAccessForWreckedShipChozo, PlmProgramStart.Typed(ScriptedRoomPlmProgram.ChozoClearSlope));

    /// <summary><c>$84:D6FC/$D3EC</c>, block-slope-access header and program.</summary>
    public static ChozoStatuePlmDefinition BlockSlopeAccess =>
        new(PlmHeaderId.BlockSlopeAccessForWreckedShipChozo, PlmProgramStart.Typed(ScriptedRoomPlmProgram.ChozoBlockSlope));

    /// <summary><c>$84:D113/$D0F6</c>, crumbling Lower Norfair plug and its draw program.</summary>
    public static ChozoStatuePlmDefinition CrumblePlug =>
        new(PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug, PlmProgramStart.Typed(ScriptedRoomPlmProgram.ChozoCrumblePlug));
    /// <summary>The five supported headers' definitions; no other bank-$84 data is interpreted.</summary>
    internal static readonly Dictionary<PlmHeaderId, ChozoStatuePlmDefinition> ByHeader = new()
    {
        [PlmHeaderId.LowerNorfairChozoHand] = LowerNorfairHand,
        [PlmHeaderId.WreckedShipChozoHand] = WreckedShipHand,
        [PlmHeaderId.ClearSlopeAccessForWreckedShipChozo] = ClearSlopeAccess,
        [PlmHeaderId.BlockSlopeAccessForWreckedShipChozo] = BlockSlopeAccess,
        [PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug] = CrumblePlug,
    };
}
