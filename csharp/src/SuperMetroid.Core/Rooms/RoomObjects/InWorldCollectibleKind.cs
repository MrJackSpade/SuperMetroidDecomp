namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Twenty-one entries shared by all three retail permanent-item tables.</summary>
public enum InWorldCollectibleKind
{
    /// <summary>Item-table entry zero: adds 100 maximum health and refills health to the new maximum.</summary>
    EnergyTank,
    /// <summary>Item-table entry one: adds five missiles to both capacity and current ammunition.</summary>
    MissileTank,
    /// <summary>Item-table entry two: adds five Super Missiles to both capacity and current ammunition.</summary>
    SuperMissileTank,
    /// <summary>Item-table entry three: adds five Power Bombs to both capacity and current ammunition.</summary>
    PowerBombTank,
    /// <summary>Item-table entry four: collects and equips the Bombs equipment flag.</summary>
    Bombs,
    /// <summary>Item-table entry five: collects and equips the Charge beam flag.</summary>
    ChargeBeam,
    /// <summary>Item-table entry six: collects and equips the Ice beam flag.</summary>
    IceBeam,
    /// <summary>Item-table entry seven: collects and equips the Hi-Jump Boots equipment flag.</summary>
    HiJumpBoots,
    /// <summary>Item-table entry eight: collects and equips Speed Booster; its Chozo presentation also resumes lava motion after the pickup message.</summary>
    SpeedBooster,
    /// <summary>Item-table entry nine: collects and equips the Wave beam flag.</summary>
    WaveBeam,
    /// <summary>Item-table entry ten: collects and equips Spazer while unequipping Plasma.</summary>
    SpazerBeam,
    /// <summary>Item-table entry eleven: collects and equips the Spring Ball equipment flag.</summary>
    SpringBall,
    /// <summary>Item-table entry twelve: collects and equips Varia Suit and clears the projectile flare counter before its message.</summary>
    VariaSuit,
    /// <summary>Item-table entry thirteen: collects and equips Gravity Suit and clears the projectile flare counter before its message.</summary>
    GravitySuit,
    /// <summary>Item-table entry fourteen: collects and equips the X-Ray Scope equipment flag.</summary>
    XrayScope,
    /// <summary>Item-table entry fifteen: collects and equips Plasma while unequipping Spazer.</summary>
    PlasmaBeam,
    /// <summary>Item-table entry sixteen: collects and equips the Grapple Beam equipment flag.</summary>
    GrappleBeam,
    /// <summary>Item-table entry seventeen: collects and equips the Space Jump equipment flag.</summary>
    SpaceJump,
    /// <summary>Item-table entry eighteen: collects and equips the Screw Attack equipment flag.</summary>
    ScrewAttack,
    /// <summary>Item-table entry nineteen: collects and equips the Morph Ball equipment flag.</summary>
    MorphBall,
    /// <summary>Item-table entry twenty: adds 100 reserve-energy capacity and selects automatic reserve mode if no mode was set.</summary>
    ReserveTank,
}
