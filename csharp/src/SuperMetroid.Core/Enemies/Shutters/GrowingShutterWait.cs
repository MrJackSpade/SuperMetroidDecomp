namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for before it grows (#2560): a timer with its updates, or Samus coming near.</summary>
public abstract partial record GrowingShutterWait
{
    /// <summary>The shutter grows once a timer runs out.</summary>
    /// <param name="Updates">The updates it waits.</param>
    public sealed partial record Timer(int Updates) : GrowingShutterWait;

    /// <summary>The shutter grows once Samus comes near.</summary>
    /// <param name="Distance">The horizontal distance in pixels Samus must come within.</param>
    public sealed partial record SamusNear(int Distance) : GrowingShutterWait;
}
