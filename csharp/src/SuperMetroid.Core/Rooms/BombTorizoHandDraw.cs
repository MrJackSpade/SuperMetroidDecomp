namespace SuperMetroid.Core.Rooms;

/// <summary>The two bank-$84 draw lists of Bomb Torizo's hand PLM, valued by native address.</summary>
internal enum BombTorizoHandDraw : ushort
{
    /// <summary>Intact Chozo hand and surrounding stone at $84:9877.</summary>
    Intact = 0x9877,
    /// <summary>Final five-run cleared-hand area at $84:989D.</summary>
    Cleared = 0x989d,
}
