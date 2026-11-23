namespace Game.Data;

/// <summary>The names of Samus's weapons, as her weapon data files name them.</summary>
public static class Arsenal
{
    /// <summary>The beam, with whatever beam equipment is switched on.</summary>
    public const string Beam = "beam";

    /// <summary>A missile.</summary>
    public const string Missile = "missile";

    /// <summary>A super missile.</summary>
    public const string SuperMissile = "super_missile";

    /// <summary>A bomb, laid in the morph ball.</summary>
    public const string Bomb = "bomb";

    /// <summary>A power bomb, laid in the morph ball.</summary>
    public const string PowerBomb = "power_bomb";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [Beam, Missile, SuperMissile, Bomb, PowerBomb];
}
