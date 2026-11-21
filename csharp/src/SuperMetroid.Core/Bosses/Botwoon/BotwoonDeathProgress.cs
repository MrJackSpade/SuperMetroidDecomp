namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>How far Botwoon's death has gone (#2560), one value where he kept a fatally-hit and a body-death-started flag.</summary>
public enum BotwoonDeathProgress
{
    /// <summary>He has not taken his fatal hit.</summary>
    Alive,
    /// <summary>He took his fatal hit and keeps on his walk until his tail has next crossed out of the wall.</summary>
    AwaitingTailExit,
    /// <summary>His death has begun: each body segment stops following his head and starts its own delay before it falls.</summary>
    Dying,
}
