namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The twenty-one permanent items, each shown as a plain item, a Chozo orb or a shot-revealed item. Its values are arbitrary.</summary>
public enum InWorldCollectibleKind
{
    /// <summary>An Energy Tank: adds a tank's maximum health (<see cref="Progression.SamusEnergyDefinitions.EnergyPerTank"/>) and refills health to the new maximum.</summary>
    EnergyTank,
    /// <summary>A Missile tank: adds its missiles to both capacity and current ammunition.</summary>
    MissileTank,
    /// <summary>A Super Missile tank: adds its Super Missiles to both capacity and current ammunition.</summary>
    SuperMissileTank,
    /// <summary>A Power Bomb tank: adds its Power Bombs to both capacity and current ammunition.</summary>
    PowerBombTank,
    /// <summary>The Bombs: collects and equips the Bombs equipment flag.</summary>
    Bombs,
    /// <summary>The Charge Beam: collects and equips the Charge beam flag.</summary>
    ChargeBeam,
    /// <summary>The Ice Beam: collects and equips the Ice beam flag.</summary>
    IceBeam,
    /// <summary>The Hi-Jump Boots: collects and equips the Hi-Jump Boots equipment flag.</summary>
    HiJumpBoots,
    /// <summary>The Speed Booster: collects and equips Speed Booster; its Chozo presentation also resumes lava motion after the pickup message.</summary>
    SpeedBooster,
    /// <summary>The Wave Beam: collects and equips the Wave beam flag.</summary>
    WaveBeam,
    /// <summary>The Spazer: collects and equips Spazer while unequipping Plasma.</summary>
    SpazerBeam,
    /// <summary>The Spring Ball: collects and equips the Spring Ball equipment flag.</summary>
    SpringBall,
    /// <summary>The Varia Suit: collects and equips Varia Suit and clears the beam's charge before its message.</summary>
    VariaSuit,
    /// <summary>The Gravity Suit: collects and equips Gravity Suit and clears the beam's charge before its message.</summary>
    GravitySuit,
    /// <summary>The X-Ray Scope: collects and equips the X-Ray Scope equipment flag.</summary>
    XrayScope,
    /// <summary>The Plasma Beam: collects and equips Plasma while unequipping Spazer.</summary>
    PlasmaBeam,
    /// <summary>The Grapple Beam: collects and equips the Grapple Beam equipment flag.</summary>
    GrappleBeam,
    /// <summary>The Space Jump: collects and equips the Space Jump equipment flag.</summary>
    SpaceJump,
    /// <summary>The Screw Attack: collects and equips the Screw Attack equipment flag.</summary>
    ScrewAttack,
    /// <summary>The Morph Ball: collects and equips the Morph Ball equipment flag.</summary>
    MorphBall,
    /// <summary>A Reserve Tank: adds a tank's reserve-energy capacity and selects automatic reserve mode if no mode was set.</summary>
    ReserveTank,
}
