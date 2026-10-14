namespace SuperMetroid.Core.Game;

/// <summary>The five bank-$A9 <c>CorpseRottingTileRowOffsets</c> tables, valued by native address.</summary>
internal enum CorpseRotationTable : ushort
{
    /// <summary>CorpseRottingTileRowOffsets.Torizo at $A9:E226; ten tiles per row.</summary>
    Torizo = 0xe226,
    /// <summary>CorpseRottingTileRowOffsets.Sidehopper at $A9:E240; five tiles per row.</summary>
    Sidehopper = 0xe240,
    /// <summary>CorpseRottingTileRowOffsets.Zoomer at $A9:E24C; three tiles per row.</summary>
    Zoomer = 0xe24c,
    /// <summary>CorpseRottingTileRowOffsets.Ripper at $A9:E252; three tiles per row.</summary>
    Ripper = 0xe252,
    /// <summary>CorpseRottingTileRowOffsets.Skree at $A9:E258; two tiles per row.</summary>
    Skree = 0xe258,
}
