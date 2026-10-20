namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Whether a non-gameplay frame runs HDMA objects.</summary>
public enum HdmaObjectProcessing
{
    /// <summary>HDMA objects do not run.</summary>
    Skipped,

    /// <summary>HDMA objects run.</summary>
    Run,
}
