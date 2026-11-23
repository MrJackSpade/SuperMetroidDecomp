namespace Game.Data;

/// <summary>The names of the equipment Samus can own and switch on, as the inventory, data files and saves write them.</summary>
public static class Equipment
{
    /// <summary>The Varia suit.</summary>
    public const string VariaSuit = "varia_suit";

    /// <summary>The Gravity suit.</summary>
    public const string GravitySuit = "gravity_suit";

    /// <summary>The morph ball.</summary>
    public const string MorphBall = "morph_ball";

    /// <summary>Bombs.</summary>
    public const string Bombs = "bombs";

    /// <summary>The spring ball.</summary>
    public const string SpringBall = "spring_ball";

    /// <summary>The screw attack.</summary>
    public const string ScrewAttack = "screw_attack";

    /// <summary>The hi-jump boots.</summary>
    public const string HiJumpBoots = "hi_jump_boots";

    /// <summary>The space jump.</summary>
    public const string SpaceJump = "space_jump";

    /// <summary>The speed booster.</summary>
    public const string SpeedBooster = "speed_booster";

    /// <summary>The grapple beam.</summary>
    public const string GrappleBeam = "grapple_beam";

    /// <summary>The X-ray scope.</summary>
    public const string XrayScope = "xray_scope";

    /// <summary>The charge beam.</summary>
    public const string ChargeBeam = "charge_beam";

    /// <summary>The ice beam.</summary>
    public const string IceBeam = "ice_beam";

    /// <summary>The wave beam.</summary>
    public const string WaveBeam = "wave_beam";

    /// <summary>The spazer.</summary>
    public const string Spazer = "spazer";

    /// <summary>The plasma beam.</summary>
    public const string PlasmaBeam = "plasma_beam";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [VariaSuit, GravitySuit, MorphBall, Bombs, SpringBall, ScrewAttack, HiJumpBoots, SpaceJump, SpeedBooster, GrappleBeam, XrayScope, ChargeBeam, IceBeam, WaveBeam, Spazer, PlasmaBeam];
}
