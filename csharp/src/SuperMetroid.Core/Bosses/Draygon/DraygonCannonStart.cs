namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The state a wall cannon is set up in when its room loads.</summary>
internal enum DraygonCannonStart
{
    /// <summary>The cannon is whole: its program begins at its first step.</summary>
    Whole,

    /// <summary>The cannon is already destroyed: its program begins at the destruction step.</summary>
    Destroyed,
}
