namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Which half of a Zebetite generation an enemy is.</summary>
internal enum ZebetiteHalf
{
    /// <summary>The primary Zebetite, spawned for every generation.</summary>
    Primary,

    /// <summary>The linked second half of a paired generation.</summary>
    Linked,
}
