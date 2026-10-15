namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The bank-$84 draw lists owned by the translated Chozo statue PLMs.</summary>
internal enum ChozoStatueDraw : ushort
{
    /// <summary>Wrecked Ship statue's clear-slope-access draw, $84:9CC5.</summary>
    ClearSlopeAccess = 0x9cc5,
    /// <summary>Wrecked Ship statue's block-slope-access draw, $84:9D0F.</summary>
    BlockSlopeAccess = 0x9d0f,
    /// <summary>Blank Lower Norfair hand after the acid-lowered event, $84:A2B5.</summary>
    LowerNorfairClearedHand = 0xa2b5,
}
