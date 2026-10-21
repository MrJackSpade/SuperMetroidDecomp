namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The three linked contact-crumble restoration draw lists.</summary>
internal enum ContactCrumbleRestoreDraw
{
    /// <summary>Horizontal parent and child restoration.</summary>
    Horizontal,
    /// <summary>Vertical parent and child restoration.</summary>
    Vertical,
    /// <summary>Four-block parent and children restoration.</summary>
    Square,
}
