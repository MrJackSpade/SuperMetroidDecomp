namespace SuperMetroid.Core.Audio;

/// <summary>Named, proven sound sequences in cartridge SFX library two.</summary>
public static class SoundEffectLibrary2Sounds
{
    /// <summary>Library-two $25, queued (Max6) with each explosion by the Ceres escape door's destruction routine at $A6:F7DC.</summary>
    public static readonly SoundEffectId CeresDoorExplosion = new(SoundEffectLibrary.Library2, 0x25); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A6:B74F queues library-two $76 when Ridley's tail hits terrain.</summary>
    public static SoundEffectId RidleyTailTerrainImpact => SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x76); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A8:F6DC, shared KiHunter acid-spit instruction, queues library-two $4C with Max6 once per spit.</summary>
    public static readonly SoundEffectId KiHunterAcidSpit = new(SoundEffectLibrary.Library2, 0x4c); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $24, the small explosion queued (Max6) at $86:B44F by the old Tourian escape shaft fake wall.</summary>
    public static readonly SoundEffectId SmallExplosion = new(SoundEffectLibrary.Library2, 0x24); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$86:B7DB queues library-two $19 (Max6) as the Tourian statue eye releases its particles.</summary>
    public static readonly SoundEffectId TourianStatueRelease = new(SoundEffectLibrary.Library2, 0x19); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A9:B52B queues library-two $6E (Max6) when a missile or Super Missile hits Mother Brain's glass.</summary>
    public static readonly SoundEffectId MotherBrainGlassHit = new(SoundEffectLibrary.Library2, 0x6e); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $01: a collected small energy drop.</summary>
    public static readonly SoundEffectId SmallEnergyPickup = new(SoundEffectLibrary.Library2, 0x01); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $02: a collected big energy drop.</summary>
    public static readonly SoundEffectId BigEnergyPickup = new(SoundEffectLibrary.Library2, 0x02); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $03: a collected missile drop.</summary>
    public static readonly SoundEffectId MissilePickup = new(SoundEffectLibrary.Library2, 0x03); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $04: a collected Super Missile drop.</summary>
    public static readonly SoundEffectId SuperMissilePickup = new(SoundEffectLibrary.Library2, 0x04); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $05: a collected Power Bomb drop.</summary>
    public static readonly SoundEffectId PowerBombPickup = new(SoundEffectLibrary.Library2, 0x05); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$93:80EF KillProjectileInner: beam impact explosion, library two $0C, Max6.</summary>
    public static readonly SoundEffectId BeamImpact = new(SoundEffectLibrary.Library2, 0x0c); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$93:80CF KillProjectileInner: shared regular/Super Missile impact, library two $07, Max6.</summary>
    public static readonly SoundEffectId MissileImpact = new(SoundEffectLibrary.Library2, 0x07); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Library-two $56: Varia/Gravity transformation, queued with Max6 at $91:D593/$D66B by VariaSuitPickup/GravitySuitPickup.</summary>
    public static readonly SoundEffectId SuitTransformation = new(SoundEffectLibrary.Library2, 0x56); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Stops/cancels every currently active library-two sound.</summary>
    public static readonly SoundEffectId CancelAll = new(SoundEffectLibrary.Library2, 0x71); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A0:A55A, NormalEnemyTouchAI: a speed-boosting, sparking or screw-attacking Samus damages the enemy she touches.</summary>
    public static readonly SoundEffectId EnemyContactDamaged = new(SoundEffectLibrary.Library2, 0x0b); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Opens the standard horizontal or vertical room door actor.</summary>
    public static readonly SoundEffectId DoorOpening = new(SoundEffectLibrary.Library2, 0x57); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Plays when Samus acquires a permanent equipment item.</summary>
    public static readonly SoundEffectId PermanentItemAcquisition = new(SoundEffectLibrary.Library2, 0x0a); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>
    /// Normal Morph Ball bomb explosion queued with Max6 by <c>Bomb_Func2</c> at
    /// <c>$90:C128</c> when the bomb fuse reaches zero.
    /// </summary>
    public static readonly SoundEffectId BombExplosion = new(SoundEffectLibrary.Library2, 0x08); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>
    /// Repeating lavaquake/Tourian-reveal rumble queued by
    /// <c>Handle_Earthquake_SoundEffect</c> at <c>$88:B21D</c>.
    /// </summary>
    public static readonly SoundEffectId Earthquake = new(SoundEffectLibrary.Library2, 0x46); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Torizo instruction PlayShotTorizoSFX queues library-two $27 (Max6).</summary>
    public static readonly SoundEffectId ShotTorizo = new(SoundEffectLibrary.Library2, 0x27); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Torizo instruction PlayTorizoFootstepsSFX queues library-two $4B (Max6).</summary>
    public static readonly SoundEffectId TorizoFootsteps = new(SoundEffectLibrary.Library2, 0x4b); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Golden Torizo instruction QueueEggReleasedSFX queues library-two $34 (Max6).</summary>
    public static readonly SoundEffectId GoldenTorizoEggReleased = new(SoundEffectLibrary.Library2, 0x34); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Golden Torizo instruction QueueLaserSFX queues library-two $67 (Max6).</summary>
    public static readonly SoundEffectId GoldenTorizoLaser = new(SoundEffectLibrary.Library2, 0x67); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>Torizo instruction QueueSonicBoomSFX queues library-two $48 (Max6).</summary>
    public static readonly SoundEffectId TorizoSonicBoom = new(SoundEffectLibrary.Library2, 0x48); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$86:B14B and $86:B166 queue library-two $22 (Max6) as a Golden Torizo egg hatches.</summary>
    public static readonly SoundEffectId GoldenTorizoEggHatch = new(SoundEffectLibrary.Library2, 0x22); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$86:A4AA queues library-two $26 (Max1) as Bomb Torizo's explosive swipe appears.</summary>
    public static readonly SoundEffectId BombTorizoExplosiveSwipe = new(SoundEffectLibrary.Library2, 0x26); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A6:AABD queues library-two $4E (Max6) on the first entry of Ceres Ridley's getaway table.</summary>
    public static readonly SoundEffectId CeresRidleyGetaway = new(SoundEffectLibrary.Library2, 0x4e); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>The Ceres falling tile queues library-two $6D (Max6) when it lands and bursts into dust.</summary>
    public static readonly SoundEffectId CeresFallingTileImpact = new(SoundEffectLibrary.Library2, 0x6d); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A2:A75C queues library-two $4D (Max6) each time the idling gunship's engine timer reloads.</summary>
    public static readonly SoundEffectId GunshipEngine = new(SoundEffectLibrary.Library2, 0x4d); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A3:C70F queues library-two $5B (Max6) on the frame Skree releases its dive.</summary>
    public static readonly SoundEffectId SkreeDive = new(SoundEffectLibrary.Library2, 0x5b); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>$A3:C771 queues library-two $5C (Max6) when a diving Skree hits the floor and burrows.</summary>
    public static readonly SoundEffectId SkreeBurrow = new(SoundEffectLibrary.Library2, 0x5c); // magic-number-audit: allow(AudioId) - named cartridge SFX identity

    /// <summary>A dead Sidehopper or Tourian monster queues library-two $10 (Max3) every eighth dust burst.</summary>
    public static readonly SoundEffectId DeadMonsterDust = new(SoundEffectLibrary.Library2, 0x10); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
}
