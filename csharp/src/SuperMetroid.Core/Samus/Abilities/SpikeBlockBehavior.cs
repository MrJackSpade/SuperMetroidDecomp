namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The solid spikes with a damaging reaction (<see cref="SpikeHazardDefinitions.DamagingSpikeBlocks"/>).</summary>
public enum SpikeBlockBehavior
{
    /// <summary>The conditional 60-energy reaction.</summary>
    Heavy,

    /// <summary>The 16-energy reaction, which the destroyed turret's spike shares; its different treatment under the grapple swing is the swing's own.</summary>
    Light,
}
