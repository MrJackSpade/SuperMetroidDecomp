using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Diagnostics.Configuration;

/// <summary>The loadout and encounter reset the tester's options grant (<see cref="SuperMetroidGameOptions"/>).</summary>
public static class TesterAssistDefinitions
{
    /// <summary>
    /// The full inventory <see cref="SuperMetroidGameOptions.GrantAllEquipment"/> grants: both suits, the Morph Ball and its
    /// upgrades, the boots, Screw Attack, Speed Booster, Grapple Beam and X-Ray Scope, every beam (equipped without Spazer,
    /// which Plasma excludes), 1,499 energy, 400 reserve energy on automatic refill, 230 missiles, 50 super missiles and
    /// 50 power bombs, with every item location collected.
    /// </summary>
    public static SamusLoadout Loadout { get; } = new()
    {
        Equipment = SamusEquipmentFlags.VariaSuit |
            SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack |
            SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump |
            SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam |
            SamusEquipmentFlags.XrayScope,
        CollectedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Ice | SamusBeamFlags.Wave | SamusBeamFlags.Spazer |
            SamusBeamFlags.Plasma,
        EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Ice | SamusBeamFlags.Wave | SamusBeamFlags.Plasma,
        Energy = 1499,
        ReserveEnergy = 400,
        ReserveTankMode = ReserveTankMode.Auto,
        Missiles = 230,
        SuperMissiles = 50,
        PowerBombs = 50,
        CollectsEveryItemLocation = true,
    };

    /// <summary>
    /// The encounters <see cref="SuperMetroidGameOptions.ResetBossesOnLoad"/> returns when a regular save is loaded: the
    /// bosses, minibosses and Torizos of every area but Tourian, the Shaktool path, and the five eye doors.
    /// </summary>
    public static SavedEncounterReset EncounterReset { get; } = new()
    {
        BossAreas = new[] { AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Ceres },
        ClearedEvents = new[] { StoryEvent.ShaktoolClearedPath },
        ClearedDoors = new[]
        {
            LockedDoorDefinitions.KraidEyeDoorEyeDoorFacingLeft, LockedDoorDefinitions.LNFarmingEyeDoorFacingRight,
            LockedDoorDefinitions.BasementDefaultEyeDoorFacingLeft, LockedDoorDefinitions.ThePreciousEyeDoorFacingRight,
            LockedDoorDefinitions.TourianEyeDoorEyeDoorFacingLeft,
        },
    };
}
