using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Fixed PLM header/list identities published by the two Chozo-statue actors.</summary>
internal static class ChozoStatuePlmDefinitions
{
    /// <summary><c>$84:D6D6/$D13F</c>, sleeping Lower Norfair hand and its initial list.</summary>
    public static ChozoStatuePlmDefinition LowerNorfairHand =>
        new(PlmHeaderId.LowerNorfairChozoHand, PlmProgramStart.Native(ChozoStatuePlmProgramDefinitions.LowerNorfairHandStart));

    /// <summary><c>$84:D6EE/$AAE3</c>, Wrecked Ship hand and its delete list.</summary>
    public static ChozoStatuePlmDefinition WreckedShipHand =>
        new(PlmHeaderId.WreckedShipChozoHand, PlmProgramStart.Delete);

    /// <summary><c>$84:D6F8/$D3CF</c>, clear-slope-access header and program.</summary>
    public static ChozoStatuePlmDefinition ClearSlopeAccess =>
        new(PlmHeaderId.ClearSlopeAccessForWreckedShipChozo, PlmProgramStart.Native(ChozoStatuePlmProgramDefinitions.ClearSlopeStart));

    /// <summary><c>$84:D6FC/$D3EC</c>, block-slope-access header and program.</summary>
    public static ChozoStatuePlmDefinition BlockSlopeAccess =>
        new(PlmHeaderId.BlockSlopeAccessForWreckedShipChozo, PlmProgramStart.Native(ChozoStatuePlmProgramDefinitions.BlockSlopeStart));

    /// <summary><c>$84:D113/$D0F6</c>, crumbling Lower Norfair plug and its draw program.</summary>
    public static ChozoStatuePlmDefinition CrumblePlug =>
        new(PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug, PlmProgramStart.Native(ChozoStatuePlmProgramDefinitions.CrumblePlugStart));
    /// <summary>Resolves a supported header without interpreting adjacent bank-$84 data.</summary>
    public static ChozoStatuePlmDefinition Resolve(PlmHeaderId headerPointer) => headerPointer switch
    {
        PlmHeaderId.LowerNorfairChozoHand => LowerNorfairHand,
        PlmHeaderId.WreckedShipChozoHand => WreckedShipHand,
        PlmHeaderId.ClearSlopeAccessForWreckedShipChozo => ClearSlopeAccess,
        PlmHeaderId.BlockSlopeAccessForWreckedShipChozo => BlockSlopeAccess,
        PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug => CrumblePlug,
        _ => throw new InvalidDataException($"Unsupported Chozo PLM ${(int)headerPointer:X4}."),
    };
}
