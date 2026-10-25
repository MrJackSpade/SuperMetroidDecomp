namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon's colours land in the colour memory, which only verification reads.</summary>
internal static class PhantoonColorDestinations
{
    /// <summary>The first colour the Wrecked Ship's power-on fade writes.</summary>
    public const int PowerOnDestination = 0;

    /// <summary>The first colour of Phantoon's sixteen-colour body palette.</summary>
    public const int BodyDestination = 112;
}
