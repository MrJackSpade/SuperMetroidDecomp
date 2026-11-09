namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The state a wall cannon is set up in when its room loads.</summary>
internal enum DraygonCannonStart
{
    /// <summary>The cannon is whole: it shows its shield on its first update.</summary>
    Whole,

    /// <summary>The cannon is already destroyed: it destroys its terrain and starts smoking on its first update.</summary>
    Destroyed,
}
