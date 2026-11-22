namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>When a Yapping Maw may grab, how far its tongue curls and how long it holds after retracting (#2156).</summary>
internal static partial class YappingMawAttackDefinitions
{
    /// <summary>How near Samus may be, in pixels and exclusive, for the Maw to hold off its attack and keep refreshing its grab cooldown.</summary>
    internal const int GrabSafetyDistance = 32;

    /// <summary>The links of the Maw's tongue, root to mouth (#2162).</summary>
    internal const int TongueLinks = 4;

    /// <summary>The largest curl the tongue's four links take (#2563).</summary>
    internal static YappingMawCurl LargestCurl { get; } = YappingMawCurl.FromWholeUnits(128);

    /// <summary>The curl, in whole angle units, per angle step each link of the chain bends by: the bend step is a quarter of the curl.</summary>
    internal const int CurlPerBendStep = 4;

    /// <summary>
    /// How long the Maw holds off grabbing once its grab cooldown starts, in updates: it may grab from the update after
    /// them (<see cref="Compatibility.YappingMawGrabCooldown"/>).
    /// </summary>
    internal const int GrabCooldown = 48;

    /// <summary>
    /// How long after the tongue fully retracts the Maw holds before it releases Samus's input, in updates: 65, the last
    /// the one it releases her on.
    /// </summary>
    internal const int RetractedHold = 65;

    /// <summary>How far from the Maw its root sits, on the side away from its attack, in pixels.</summary>
    internal const int RootDistance = 8;
}
