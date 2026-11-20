namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The solid spikes with a damaging reaction (<see cref="SpikeHazardDefinitions.DamagingSpikeBlocks"/>).</summary>
public enum SpikeBlockBehavior
{
    /// <summary>The conditional heavy reaction (<see cref="SpikeHazardDefinitions.HeavySpikeDamage"/>).</summary>
    Heavy,

    /// <summary>The light reaction (<see cref="SpikeHazardDefinitions.LightSpikeDamage"/>), which the destroyed turret's spike shares; its different treatment under the grapple swing is the swing's own.</summary>
    Light,
}
