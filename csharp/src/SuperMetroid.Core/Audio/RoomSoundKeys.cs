namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="RoomSound"/> (#2520), written out so renaming a member never changes a name the
/// installation, an installed document or a save spells it by.
/// </summary>
internal static class RoomSoundKeys
{
    /// <summary>Each RoomSound member's installed key.</summary>
    internal static InstalledKeys<RoomSound> Installed { get; } = new(new Dictionary<RoomSound, string>
    {
        [RoomSound.Silence] = "Silence",
        [RoomSound.LowHealthBeep] = "LowHealthBeep",
        [RoomSound.SpeedBooster] = "SpeedBooster",
        [RoomSound.HardLanding] = "HardLanding",
        [RoomSound.Landing] = "Landing",
        [RoomSound.Footsteps] = "Footsteps",
        [RoomSound.DoorOpened] = "DoorOpened",
        [RoomSound.DoorClosed] = "DoorClosed",
        [RoomSound.MissileDoorHit] = "MissileDoorHit",
        [RoomSound.EnemyFrozen] = "EnemyFrozen",
        [RoomSound.Elevator] = "Elevator",
        [RoomSound.StoredShinespark] = "StoredShinespark",
        [RoomSound.IntroTypewriter] = "IntroTypewriter",
        [RoomSound.GateMoving] = "GateMoving",
        [RoomSound.Shinespark] = "Shinespark",
        [RoomSound.ShinesparkEnd] = "ShinesparkEnd",
        [RoomSound.ShinesparkEndShort] = "ShinesparkEndShort",
        [RoomSound.ProjectileImpact] = "ProjectileImpact",
        [RoomSound.GunshipPadOpening] = "GunshipPadOpening",
        [RoomSound.GunshipPadClosing] = "GunshipPadClosing",
        [RoomSound.MotherBrainFootstep] = "MotherBrainFootstep",
        [RoomSound.MotherBrainBlueRings] = "MotherBrainBlueRings",
        [RoomSound.BabyMetroidDeath] = "BabyMetroidDeath",
        [RoomSound.DraygonDyingCry] = "DraygonDyingCry",
        [RoomSound.CrocomireSpit] = "CrocomireSpit",
        [RoomSound.PhantoonFlame] = "PhantoonFlame",
        [RoomSound.KraidEarthquake] = "KraidEarthquake",
        [RoomSound.KraidLint] = "KraidLint",
        [RoomSound.RidleyTailWhip] = "RidleyTailWhip",
        [RoomSound.CrocomireAcidDamage] = "CrocomireAcidDamage",
        [RoomSound.BabyMetroidHighCry] = "BabyMetroidHighCry",
        [RoomSound.CeresBabyMetroidCry] = "CeresBabyMetroidCry",
        [RoomSound.SpeedBoosterOrElevatorStop] = "SpeedBoosterOrElevatorStop",
        [RoomSound.BabyMetroidHitchedCry] = "BabyMetroidHitchedCry",
        [RoomSound.BabyMetroidLowCry] = "BabyMetroidLowCry",
        [RoomSound.PhantoonMaterialise] = "PhantoonMaterialise",
        [RoomSound.PhantoonSuperMissiled] = "PhantoonSuperMissiled",
        [RoomSound.PauseBeep] = "PauseBeep",
        [RoomSound.ResumedSpeedBooster] = "ResumedSpeedBooster",
        [RoomSound.CeresDoorOpening] = "CeresDoorOpening",
        [RoomSound.IncrementalEnergy] = "IncrementalEnergy",
        [RoomSound.MotherBrainGlassShatter] = "MotherBrainGlassShatter",
    });
}
