namespace SuperMetroid.Core.Rooms;

/// <summary>The three bank-$84 linked bomb-block restoration draw lists.</summary>
internal enum BombBlockRestoreDraw : ushort
{
    /// <summary>Horizontal parent and child restoration at $84:A4C7.</summary>
    Horizontal = 0xa4c7,
    /// <summary>Vertical parent and child restoration at $84:A4CF.</summary>
    Vertical = 0xa4cf,
    /// <summary>Four-block parent and child restoration at $84:A4D7.</summary>
    Square = 0xa4d7,
}
