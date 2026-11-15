namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// How a Kago bug moves itself (#1843): it waits, jumps and falls, and holds still while it lands or is shot.
/// </summary>
internal enum KagoBugMotion
{
    /// <summary>Holds a Kago bug for its idle time, then starts its jump.</summary>
    Waiting,

    /// <summary>Raises a jumping Kago bug.</summary>
    Jumping,

    /// <summary>Drops a Kago bug until it lands.</summary>
    Falling,
}
