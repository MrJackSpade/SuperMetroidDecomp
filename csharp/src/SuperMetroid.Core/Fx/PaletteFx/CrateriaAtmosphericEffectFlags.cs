namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Composable room-policy bits used by the duplicated Crateria footstep and landing tables.
/// </summary>
[Flags]
internal enum CrateriaAtmosphericEffectFlags
{
    /// <summary>No special landing, entrance, or wet-footstep behavior.</summary>
    None = 0,
    /// <summary>Landing Site atmospheric effect policy.</summary>
    LandingSite = 1,
    /// <summary>Wrecked Ship entrance atmospheric effect policy.</summary>
    WreckedShipEntrance = 2,
    /// <summary>Wet-floor footstep policy for selected Crateria rooms.</summary>
    WetFootsteps = 4,
}
