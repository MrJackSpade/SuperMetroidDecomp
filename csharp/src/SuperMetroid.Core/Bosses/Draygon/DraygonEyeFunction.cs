namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What the Draygon eye does each update; each member names the routine it replaced (#1413: the values are arbitrary).</summary>
internal enum DraygonEyeFunction
{
    /// <summary><c>RTS_A5804B</c>: the eye is inert.</summary>
    Inert = 1,

    /// <summary>Draygon eye behaviour: facing left.</summary>
    FacingLeft,

    /// <summary>Draygon eye behaviour: facing right.</summary>
    FacingRight,
}
