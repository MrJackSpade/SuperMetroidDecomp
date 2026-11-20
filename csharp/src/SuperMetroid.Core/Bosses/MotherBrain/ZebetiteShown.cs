namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What a Zebetite half shows (#2250, #2554): a standing barrier's chosen picture, or nothing for the barrier set up once
/// every Zebetite has fallen, which its setup deletes at once.
/// </summary>
internal abstract record ZebetiteShown
{
    /// <summary>Closes the hierarchy to the cases declared here.</summary>
    private ZebetiteShown()
    {
    }

    /// <summary>A standing barrier's half, showing <paramref name="Frame"/>.</summary>
    /// <param name="Frame">The picture its form and health choose.</param>
    internal sealed record Picture(EnemyFrameId Frame) : ZebetiteShown;

    /// <summary>The barrier set up once every Zebetite has fallen: deleted at its setup, it shows nothing.</summary>
    internal sealed record Destroyed : ZebetiteShown;

    /// <summary>The barrier set up once every Zebetite has fallen.</summary>
    internal static ZebetiteShown AllDestroyed { get; } = new Destroyed();
}
