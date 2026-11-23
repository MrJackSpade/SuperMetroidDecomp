namespace Game.Data;

/// <summary>The names of the areas of the world, as rooms, maps and progress give them.</summary>
public static class Areas
{
    /// <summary>Crateria.</summary>
    public const string Crateria = "crateria";

    /// <summary>Brinstar.</summary>
    public const string Brinstar = "brinstar";

    /// <summary>Norfair.</summary>
    public const string Norfair = "norfair";

    /// <summary>The Wrecked Ship.</summary>
    public const string WreckedShip = "wrecked_ship";

    /// <summary>Maridia.</summary>
    public const string Maridia = "maridia";

    /// <summary>Tourian.</summary>
    public const string Tourian = "tourian";

    /// <summary>Ceres Station.</summary>
    public const string Ceres = "ceres";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [Crateria, Brinstar, Norfair, WreckedShip, Maridia, Tourian, Ceres];
}
