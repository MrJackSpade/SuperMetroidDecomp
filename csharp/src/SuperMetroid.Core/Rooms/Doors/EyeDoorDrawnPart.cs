namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What one eye-door draw writes: one of the door's components, or the clear of the whole door.</summary>
public enum EyeDoorDrawnPart
{
    /// <summary>The eye, a column of blocks.</summary>
    Eye,

    /// <summary>The door's middle.</summary>
    Door,

    /// <summary>The door's bottom.</summary>
    Bottom,

    /// <summary>The clear of the whole door's four blocks, a column, once the eye opens.</summary>
    WholeDoorClear,
}
