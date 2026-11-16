namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>When a Yapping Maw may grab, how far its tongue curls and how long it holds after retracting (#2156).</summary>
internal static class YappingMawAttackDefinitions
{
    /// <summary>How near Samus may be, in pixels and exclusive, for the Maw to hold off its attack and keep refreshing its grab cooldown.</summary>
    internal const int GrabSafetyDistance = 32;

    /// <summary>The links of the Maw's tongue, root to mouth (#2162).</summary>
    internal const int TongueLinks = 4;

    /// <summary>The largest curl the tongue's four links take, in whole angle units.</summary>
    internal const int LargestCurl = 128;

    /// <summary>The curl, in whole angle units, per angle step each link of the chain bends by: the bend step is a quarter of the curl.</summary>
    internal const int CurlPerBendStep = 4;

    /// <summary>How long after it releases Samus the Maw waits before it can grab again, in updates.</summary>
    internal const int GrabCooldown = 48;

    /// <summary>How long after the tongue fully retracts the Maw holds before it releases Samus's input, in updates.</summary>
    internal const int RetractedHold = 64;

    /// <summary>How far from the Maw its root sits, on the side away from its attack, in pixels.</summary>
    internal const int RootDistance = 8;
}
