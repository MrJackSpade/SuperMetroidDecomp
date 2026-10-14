namespace SuperMetroid.Core.Audio;

/// <summary>Named, proven sound sequences in cartridge SFX library three.</summary>
public static class SoundEffectLibrary3Sounds
{
    /// <summary>
    /// Library-three <c>$16</c>, which the shared Mother Brain body footstep effect (for example
    /// <c>$A9:9622</c>) queues only while the encounter is in form three.
    /// </summary>
    public static readonly SoundEffectId MotherBrainFormThreeFootstep = new(SoundEffectLibrary.Library3, 0x16); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$86:AB4B and $86:B3EF queue library-three $13 (Max6) when a Torizo Chozo orb or eye beam strikes the floor.</summary>
    public static readonly SoundEffectId TorizoProjectileFloorImpact = new(SoundEffectLibrary.Library3, 0x13); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A9:CBFF-CC02 queues library-three $19 with Max6 when the Baby Metroid takes its fatal blow.</summary>
    public static readonly SoundEffectId BabyMetroidDeathCry = new(SoundEffectLibrary.Library3, 0x19); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$91:F076 queues library-three $04 for a landing with whole Y speed at least five.</summary>
    public static readonly SoundEffectId HardLanding = new(SoundEffectLibrary.Library3, 0x04); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$91:F08A queues library-three $05 for a nonzero landing below whole Y speed five.</summary>
    public static readonly SoundEffectId SoftLanding = new(SoundEffectLibrary.Library3, 0x05); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>
    /// <c>$A3:A97C</c>, <c>$A3:ED91</c>, and <c>$A8:BE8D</c> queue library-three
    /// <c>$2D</c> once per attached Mochtroid, Metroid, or Beetom on the shared
    /// enemy-clock phase.
    /// </summary>
    public static readonly SoundEffectId AttachedEnemyDrain =
        new(SoundEffectLibrary.Library3, 0x2d); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$82:DC3E/$AF7A, library-three $2D during automatic/manual reserve refill.</summary>
    public static readonly SoundEffectId ReserveRefill = new(SoundEffectLibrary.Library3, 0x2d); // magic-number-audit: allow(AudioId) - named cartridge reserve-refill SFX identity
    /// <summary>$A0:A8AE, normal enemy shot: library-three $0A when an enemy becomes frozen.</summary>
    public static readonly SoundEffectId EnemyFreeze = new(SoundEffectLibrary.Library3, 0x0a); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$90:85A6 queues library-three $03 when Speed Booster reaches stage four.</summary>
    public static readonly SoundEffectId SpeedBoosterEcho = new(SoundEffectLibrary.Library3, 0x03); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$90:F5A1-F5A4 queues library-three $25 to end an armed Speed Booster echo after boosting stops.</summary>
    public static readonly SoundEffectId StopSpeedBoosterEcho = new(SoundEffectLibrary.Library3, 0x25); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$A2:AA47/AB58 queue library-three $14 as the ship's pad opens for entry/exit.</summary>
    public static readonly SoundEffectId GunshipEntrancePad = new(SoundEffectLibrary.Library3, 0x14); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$A2:AB9D queues library-three $15 as the ship's entrance pad closes after exit.</summary>
    public static readonly SoundEffectId GunshipEntrancePadClosing = new(SoundEffectLibrary.Library3, 0x15); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>$82:A92B queues library-three $2A when the pause/map palette animation loops.</summary>
    public static readonly SoundEffectId MapPaletteLoop = new(SoundEffectLibrary.Library3, 0x2a); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
    /// <summary>Stops/cancels every currently active library-three sound.</summary>
    public static readonly SoundEffectId CancelAll = new(SoundEffectLibrary.Library3, 0x01); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Samus taking periodic damage from room heat, lava, or acid.</summary>
    public static readonly SoundEffectId EnvironmentalDamage = new(SoundEffectLibrary.Library3, 0x2d); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A6:BFC9 queues library-three $24 (Max6) when the Ceres Baby Metroid cries.</summary>
    public static readonly SoundEffectId CeresBabyCry = new(SoundEffectLibrary.Library3, 0x24); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>SolidVerticalCollision_WallJumpTriggered queues library-three $05 (Max6) as the wall jump starts.</summary>
    public static readonly SoundEffectId WallJump = new(SoundEffectLibrary.Library3, 0x05); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
}
