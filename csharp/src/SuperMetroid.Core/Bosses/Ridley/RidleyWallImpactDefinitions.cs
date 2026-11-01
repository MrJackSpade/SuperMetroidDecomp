namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Wall-impact request made by Ridley_Func_113.</summary>
internal static class RidleyWallImpactDefinitions
{
    /// <summary>The wall impact compares the larger absolute 8.8 velocity against 2.5 pixels/frame.</summary>
    public const int MinimumSpeed = 640;
    /// <summary>The wall impact's non-Norfair branch requests room-shake type 33.</summary>
    public const int EarthquakeType = 33;
    /// <summary>The wall impact initializes the quake to twelve frames.</summary>
    public const int Duration = 12;
}
