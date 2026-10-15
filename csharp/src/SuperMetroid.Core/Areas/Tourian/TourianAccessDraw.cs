namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The five bank-$84 Tourian access-floor draw lists, valued by native address.</summary>
internal enum TourianAccessDraw : ushort
{
    /// <summary><c>$84:9297</c>: clear one row after its crumble frames.</summary>
    EmptyRow = 0x9297,
    /// <summary><c>$84:92A3</c>: first visible crumble row.</summary>
    CrumbleFirst = 0x92a3,
    /// <summary><c>$84:92AF</c>: second visible crumble row.</summary>
    CrumbleSecond = 0x92af,
    /// <summary><c>$84:92BB</c>: third visible crumble row.</summary>
    CrumbleThird = 0x92bb,
    /// <summary><c>$84:92C7</c>: clear all six rows at once.</summary>
    Clear = 0x92c7,
}
