namespace SuperMetroid.Core.Input;

/// <summary>
/// The demos' recordings (#1300, #1600), one per recorded input a demo or its owner starts: the title's demonstrations play
/// one each in the player's place, and the intro plays its own.
/// </summary>
public enum DemoInputRecordingId
{
    /// <summary>Samus swings across Maridia's Mt. Everest on the Grapple Beam, in the completion set.</summary>
    AttractAdvancedGrappleBeam,
    /// <summary>Samus crosses the Green Hill Zone, Brinstar's long diagonal room.</summary>
    AttractBrinstarDiagonalRoom,
    /// <summary>Samus, nearly out of energy at the Landing Site, refills it with a Crystal Flash, in the completion set.</summary>
    AttractCrystalFlash,
    /// <summary>Samus drops into Brinstar's Dachora room.</summary>
    AttractDachora,
    /// <summary>
    /// Samus's diagonal shinespark across the Landing Site, until the shinespark is released into
    /// <see cref="AttractDiagonalShinesparkContinuation"/>.
    /// </summary>
    AttractDiagonalShinespark,
    /// <summary>The rest of the diagonal shinespark's demonstration, once its release lets the shinespark continue.</summary>
    AttractDiagonalShinesparkContinuation,
    /// <summary>Samus opens the eye door on the way to Kraid's room.</summary>
    AttractEyeDoor,
    /// <summary>Samus crosses Green Brinstar's Fireflea room.</summary>
    AttractFirefleaRoom,
    /// <summary>Samus shinesparks across the Landing Site toward the Gauntlet's entrance, in the completion set.</summary>
    AttractGauntletEntrance,
    /// <summary>Samus crosses Norfair's Spiky Acid Snakes Tunnel on the Grapple Beam.</summary>
    AttractGrappleBeam,
    /// <summary>Samus fires the Ice Beam in the room below Maridia's Spazer.</summary>
    AttractIceBeam,
    /// <summary>Samus climbs the Landing Site by an infinite bomb jump, in the completion set.</summary>
    AttractInfiniteBombJump,
    /// <summary>Samus fights Kraid.</summary>
    AttractKraid,
    /// <summary>Samus sets out from her ship at the Landing Site, the first demonstration after power-on.</summary>
    AttractLandingSite,
    /// <summary>Samus crosses Norfair's Lava Dive toward the entrance to Lower Norfair.</summary>
    AttractLowerNorfairEntrance,
    /// <summary>Samus opens a missile door in Brinstar's Construction Zone.</summary>
    AttractMissileDoor,
    /// <summary>Samus runs the Wrecked Ship's Basement, the hall before Phantoon.</summary>
    AttractPrePhantoonHall,
    /// <summary>Samus fights through the Kihunters' hall before Spore Spawn.</summary>
    AttractPreSporeSpawnHall,
    /// <summary>Samus spin-jumps through Brinstar's Big Pink with a charged beam, a pseudo Screw Attack.</summary>
    AttractPseudoScrewAttack,
    /// <summary>Samus climbs Red Brinstar's Caterpillar room toward its elevator.</summary>
    AttractRedBrinstarElevator,
    /// <summary>Samus Screw Attacks along Crateria's Flyway.</summary>
    AttractScrewAttack,
    /// <summary>Samus fires a special beam attack in Green Brinstar's main shaft, in the completion set.</summary>
    AttractSpecialBeamAttack,
    /// <summary>Samus runs Norfair's Frog Speedway with the Speed Booster.</summary>
    AttractSpeedBooster,
    /// <summary>Samus enters the statue room at Tourian's entrance, Kraid already defeated.</summary>
    AttractTourianEntrance,
    /// <summary>The input that ends the intro's discovery of the Baby, once Samus has looked at it.</summary>
    IntroBabyDiscoveryEnd,
    /// <summary>The intro's discovery of the Baby: the input that runs Samus left toward it.</summary>
    IntroBabyDiscoveryRunningLeft,
    /// <summary>The intro's discovery of the Baby: the input that stops Samus and has her look.</summary>
    IntroBabyDiscoveryStopAndLook,
    /// <summary>The input Samus fires with in the intro's flashback of her fight with Mother Brain.</summary>
    IntroMotherBrainFlashback,
}
