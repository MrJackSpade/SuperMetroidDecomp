namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's movement behaviour: how his body travels each update. Each member names that behaviour.</summary>
public enum BotwoonMovement
{
    /// <summary>Heads straight for his target hole until he has crossed into or out of the wall.</summary>
    MoveTowardHole,
    /// <summary>Walks his authored path, as many steps an update as his speed stage takes, until he walks off its end.</summary>
    FollowAuthoredPath,
    /// <summary>
    /// He has crossed into or out of the wall, or walked off his path's end: he stands still until his next update, which
    /// chooses what follows instead of moving.
    /// </summary>
    Finished,
}
