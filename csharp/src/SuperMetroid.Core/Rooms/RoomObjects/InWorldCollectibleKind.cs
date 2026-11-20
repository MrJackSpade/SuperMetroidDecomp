namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The twenty-one permanent items, each shown as a plain item, a Chozo orb or a shot-revealed item. Its values are arbitrary.</summary>
public enum InWorldCollectibleKind
{
    /// <summary>An Energy Tank: adds 100 maximum health and refills health to the new maximum.</summary>
    EnergyTank,
    /// <summary>A Missile tank: adds five missiles to both capacity and current ammunition.</summary>
    MissileTank,
    /// <summary>A Super Missile tank: adds five Super Missiles to both capacity and current ammunition.</summary>
    SuperMissileTank,
    /// <summary>A Power Bomb tank: adds five Power Bombs to both capacity and current ammunition.</summary>
    PowerBombTank,
    /// <summary>The Bombs: collected and equipped.</summary>
    Bombs,
    /// <summary>The Charge Beam: collected and equipped.</summary>
    ChargeBeam,
    /// <summary>The Ice Beam: collected and equipped.</summary>
    IceBeam,
    /// <summary>The Hi-Jump Boots: collected and equipped.</summary>
    HiJumpBoots,
    /// <summary>The Speed Booster: collects and equips Speed Booster; its Chozo presentation also resumes lava motion after the pickup message.</summary>
    SpeedBooster,
    /// <summary>The Wave Beam: collected and equipped.</summary>
    WaveBeam,
    /// <summary>The Spazer: collects and equips Spazer while unequipping Plasma.</summary>
    SpazerBeam,
    /// <summary>The Spring Ball: collected and equipped.</summary>
    SpringBall,
    /// <summary>The Varia Suit: collects and equips Varia Suit and clears the beam's charge before its message.</summary>
    VariaSuit,
    /// <summary>The Gravity Suit: collects and equips Gravity Suit and clears the beam's charge before its message.</summary>
    GravitySuit,
    /// <summary>The X-Ray Scope: collected and equipped.</summary>
    XrayScope,
    /// <summary>The Plasma Beam: collects and equips Plasma while unequipping Spazer.</summary>
    PlasmaBeam,
    /// <summary>The Grapple Beam: collected and equipped.</summary>
    GrappleBeam,
    /// <summary>The Space Jump: collected and equipped.</summary>
    SpaceJump,
    /// <summary>The Screw Attack: collected and equipped.</summary>
    ScrewAttack,
    /// <summary>The Morph Ball: collected and equipped.</summary>
    MorphBall,
    /// <summary>A Reserve Tank: adds 100 reserve-energy capacity and selects automatic reserve mode if no mode was set.</summary>
    ReserveTank,
}
