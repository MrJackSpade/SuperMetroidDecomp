namespace Game.Data;

/// <summary>The names of this game's kinds of damage, beside the engine's own two. A hit carries a set of them: an ice-wave beam shot is beam, ice and wave.</summary>
public static class HitKinds
{
    /// <summary>Any shot of the beam.</summary>
    public const string Beam = "beam";

    /// <summary>A charged shot.</summary>
    public const string Charged = "charged";

    /// <summary>The ice beam.</summary>
    public const string Ice = "ice";

    /// <summary>The wave beam.</summary>
    public const string Wave = "wave";

    /// <summary>The spazer.</summary>
    public const string Spazer = "spazer";

    /// <summary>The plasma beam.</summary>
    public const string Plasma = "plasma";

    /// <summary>A missile.</summary>
    public const string Missile = "missile";

    /// <summary>A super missile.</summary>
    public const string SuperMissile = "super_missile";

    /// <summary>A bomb.</summary>
    public const string Bomb = "bomb";

    /// <summary>A power bomb.</summary>
    public const string PowerBomb = "power_bomb";

    /// <summary>Samus's body during a screw attack.</summary>
    public const string ScrewAttack = "screw_attack";

    /// <summary>Samus's body at full speed-booster run.</summary>
    public const string SpeedBoost = "speed_boost";

    /// <summary>Samus's body during a shinespark.</summary>
    public const string Shinespark = "shinespark";

    /// <summary>Samus's body spin jumping with a charged beam.</summary>
    public const string ChargedSpin = "charged_spin";

    /// <summary>The grapple beam.</summary>
    public const string Grapple = "grapple";

    /// <summary>Being seen by the X-ray scope; it reveals and never damages.</summary>
    public const string Xray = "xray";

    /// <summary>A digging machine's saw, which cuts what only it can cut.</summary>
    public const string Saw = "saw";

    /// <summary>The kinds a blow of Samus's own body carries, whichever of her powers it is.</summary>
    public static IReadOnlyList<Engine.Shared.HitKind> OfHerBody { get; } = [new(ScrewAttack), new(SpeedBoost), new(Shinespark), new(ChargedSpin)];

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [Beam, Charged, Ice, Wave, Spazer, Plasma, Missile, SuperMissile, Bomb, PowerBomb, ScrewAttack, SpeedBoost, Shinespark, ChargedSpin, Grapple, Xray, Saw];
}
