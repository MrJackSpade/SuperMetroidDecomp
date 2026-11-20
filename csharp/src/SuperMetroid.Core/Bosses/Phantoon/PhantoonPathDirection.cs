namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Which way Phantoon runs his figure-eight path (#2471, <c>Places</c>).</summary>
public enum PhantoonPathDirection
{
    /// <summary>On to each next place of the path, past its last to its first.</summary>
    Forwards,

    /// <summary>Back to each place before, past its first to its last.</summary>
    Backwards,
}
