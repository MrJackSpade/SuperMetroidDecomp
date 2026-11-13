namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What the Draygon eye does each update (#1413: the values are arbitrary).</summary>
internal enum DraygonEyeFunction
{
    /// <summary>The eye is inert.</summary>
    Inert = 1,

    /// <summary>Draygon eye behaviour: facing left.</summary>
    FacingLeft,

    /// <summary>Draygon eye behaviour: facing right.</summary>
    FacingRight,
}
