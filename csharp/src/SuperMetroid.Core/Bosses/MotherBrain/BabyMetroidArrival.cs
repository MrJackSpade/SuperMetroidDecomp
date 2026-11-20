namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Whether the Baby Metroid of Mother Brain's late fight has arrived (#2471, #2554): awaited until the final beam's spawn
/// gives it its place in the room's enemies, then the arrived Baby, built with that place.
/// </summary>
public abstract record BabyMetroidArrival
{
    /// <summary>Closes the hierarchy to the cases declared here.</summary>
    private BabyMetroidArrival()
    {
    }

    /// <summary>The Baby has not arrived: the final beam has not spawned it.</summary>
    public sealed record Awaited : BabyMetroidArrival;

    /// <summary>The Baby has arrived as <paramref name="Baby"/>, in its place in the room's enemies.</summary>
    /// <param name="Baby">The arrived Baby.</param>
    public sealed record Arrived(BabyMetroidCutsceneState Baby) : BabyMetroidArrival;

    /// <summary>The Baby the final beam has not spawned yet.</summary>
    public static BabyMetroidArrival NotYet { get; } = new Awaited();
}
