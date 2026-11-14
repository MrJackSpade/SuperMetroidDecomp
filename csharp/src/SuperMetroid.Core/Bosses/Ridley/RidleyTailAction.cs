namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What Ridley's tail is doing, as his fight steers it.</summary>
public enum RidleyTailAction
{
    /// <summary>Nothing steers the tail: it keeps its segments where they are.</summary>
    None,
    /// <summary>The tail curls and whips freely behind Ridley in flight.</summary>
    Neutral,
    /// <summary>The tail swings down for the pogo, then points down.</summary>
    PogoSetup,
    /// <summary>The tail points straight down until its segments settle, then starts the pogo.</summary>
    PointDown,
    /// <summary>The tail bounces Ridley off the floor and whips at Samus.</summary>
    Pogo,
    /// <summary>The tail readies a downward stab at Samus as Ridley descends.</summary>
    StabSetup,
    /// <summary>The tail stabs straight down, slower than the setup's swing.</summary>
    Stab,
}
