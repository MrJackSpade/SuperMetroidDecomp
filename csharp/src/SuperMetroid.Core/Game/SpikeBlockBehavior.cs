namespace SuperMetroid.Core.Game;

/// <summary>The solid spike-block BTS values with a damaging reaction in bank $94.</summary>
public enum SpikeBlockBehavior : byte
{
    /// <summary>Spike-block BTS zero selects the conditional 60-energy reaction at `$94:8E83`.</summary>
    Heavy = 0,

    /// <summary>Spike-block BTS one selects the 16-energy reaction at `$94:8ECF`.</summary>
    Light = 1,

    /// <summary>Spike-block BTS three selects the alternate 16-energy reaction at `$94:8F0A`.</summary>
    AlternateLight = 3,
}
