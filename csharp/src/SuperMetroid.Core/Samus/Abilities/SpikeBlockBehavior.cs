namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The solid spikes with a damaging reaction (<see cref="SpikeHazardDefinitions.DamagingSpikeBlocks"/>).</summary>
public enum SpikeBlockBehavior
{
    /// <summary>The conditional 60-energy reaction.</summary>
    Heavy,

    /// <summary>The 16-energy reaction.</summary>
    Light,

    /// <summary>The alternate 16-energy reaction.</summary>
    AlternateLight,
}
