namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The atmospheric effect a Crateria room gives Samus's landings and running footsteps (#2056): each room the policy
/// names has exactly one; every other Crateria room has none.
/// </summary>
internal enum CrateriaAtmosphericEffect
{
    /// <summary>The Landing Site: her landings and footsteps splash while it rains.</summary>
    LandingSite,
    /// <summary>The Wrecked Ship entrance: her landings and footsteps splash at and below its waterline.</summary>
    WreckedShipEntrance,
    /// <summary>A wet floor: her landings and footsteps always splash.</summary>
    WetFootsteps,
}
