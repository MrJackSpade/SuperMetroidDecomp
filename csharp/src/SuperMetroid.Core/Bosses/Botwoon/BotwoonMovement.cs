namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's movement behaviour: how his body travels each update. Each member names that behaviour (#2554).</summary>
internal abstract record BotwoonMovement
{
    /// <summary>Only the behaviours below are choices.</summary>
    private BotwoonMovement()
    {
    }

    /// <summary>Heads straight for his target hole until he has crossed into or out of the wall.</summary>
    internal sealed record MoveTowardHole : BotwoonMovement;

    /// <summary>Walks <paramref name="Path"/>, as many steps an update as his speed stage takes, until he walks off its end.</summary>
    /// <param name="Path">The authored path he walks.</param>
    internal sealed record FollowAuthoredPath(BotwoonPathDefinition Path) : BotwoonMovement;

    /// <summary>
    /// He has crossed into or out of the wall, or walked off his path's end: he stands still until his next update, which
    /// chooses what follows instead of moving.
    /// </summary>
    internal sealed record Finished : BotwoonMovement;
}
