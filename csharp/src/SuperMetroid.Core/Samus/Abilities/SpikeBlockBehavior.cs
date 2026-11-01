namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The solid spike-block BTS values with a damaging reaction.</summary>
public enum SpikeBlockBehavior
{
    /// <summary>Spike-block BTS zero selects the conditional 60-energy reaction.</summary>
    Heavy = 0,

    /// <summary>Spike-block BTS one selects the 16-energy reaction.</summary>
    Light = 1,

    /// <summary>Spike-block BTS three selects the alternate 16-energy reaction.</summary>
    AlternateLight = 3,
}
