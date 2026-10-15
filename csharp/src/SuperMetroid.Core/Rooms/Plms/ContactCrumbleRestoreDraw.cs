namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The three bank-$84 linked contact-crumble restoration draw lists.</summary>
internal enum ContactCrumbleRestoreDraw : ushort
{
    /// <summary>Horizontal parent and child restoration at $84:A4A1.</summary>
    Horizontal = 0xa4a1,
    /// <summary>Vertical parent and child restoration at $84:A4A9.</summary>
    Vertical = 0xa4a9,
    /// <summary>Four-block parent and children restoration at $84:A4B1.</summary>
    Square = 0xa4b1,
}
