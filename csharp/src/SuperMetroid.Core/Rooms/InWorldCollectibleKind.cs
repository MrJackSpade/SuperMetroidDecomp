using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Twenty-one entries shared by all three retail permanent-item header tables.</summary>
public enum InWorldCollectibleKind : byte
{
    /// <summary>Header-table entry zero: adds 100 maximum health and refills health to the new maximum.</summary>
    EnergyTank,
    /// <summary>Header-table entry one: adds five missiles to both capacity and current ammunition.</summary>
    MissileTank,
    /// <summary>Header-table entry two: adds five Super Missiles to both capacity and current ammunition.</summary>
    SuperMissileTank,
    /// <summary>Header-table entry three: adds five Power Bombs to both capacity and current ammunition.</summary>
    PowerBombTank,
    /// <summary>Header-table entry four: collects and equips the Bombs equipment bit.</summary>
    Bombs,
    /// <summary>Header-table entry five: collects and equips the Charge beam bit.</summary>
    ChargeBeam,
    /// <summary>Header-table entry six: collects and equips the Ice beam bit.</summary>
    IceBeam,
    /// <summary>Header-table entry seven: collects and equips the Hi-Jump Boots equipment bit.</summary>
    HiJumpBoots,
    /// <summary>Header-table entry eight: collects and equips Speed Booster; its Chozo presentation also resumes lava motion after the pickup message.</summary>
    SpeedBooster,
    /// <summary>Header-table entry nine: collects and equips the Wave beam bit.</summary>
    WaveBeam,
    /// <summary>Header-table entry ten: collects and equips Spazer while unequipping Plasma.</summary>
    SpazerBeam,
    /// <summary>Header-table entry eleven: collects and equips the Spring Ball equipment bit.</summary>
    SpringBall,
    /// <summary>Header-table entry twelve: collects and equips Varia Suit and clears the projectile flare counter before its message.</summary>
    VariaSuit,
    /// <summary>Header-table entry thirteen: collects and equips Gravity Suit and clears the projectile flare counter before its message.</summary>
    GravitySuit,
    /// <summary>Header-table entry fourteen: collects and equips the X-Ray Scope equipment bit.</summary>
    XrayScope,
    /// <summary>Header-table entry fifteen: collects and equips Plasma while unequipping Spazer.</summary>
    PlasmaBeam,
    /// <summary>Header-table entry sixteen: collects and equips the Grapple Beam equipment bit.</summary>
    GrappleBeam,
    /// <summary>Header-table entry seventeen: collects and equips the Space Jump equipment bit.</summary>
    SpaceJump,
    /// <summary>Header-table entry eighteen: collects and equips the Screw Attack equipment bit.</summary>
    ScrewAttack,
    /// <summary>Header-table entry nineteen: collects and equips the Morph Ball equipment bit.</summary>
    MorphBall,
    /// <summary>Header-table entry twenty: adds 100 reserve-energy capacity and selects automatic reserve mode if no mode was set.</summary>
    ReserveTank,
}
