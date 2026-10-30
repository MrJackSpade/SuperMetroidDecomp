namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for before it grows (#1471).</summary>
public enum GrowingShutterWait
{
    /// <summary>It waits for its timer.</summary>
    Timer,

    /// <summary>It waits for Samus to come near.</summary>
    SamusNear,
}
