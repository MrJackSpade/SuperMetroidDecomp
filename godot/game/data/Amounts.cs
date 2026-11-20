namespace Game.Data;

/// <summary>The names of the amounts Samus's inventory counts, each with a capacity. Energy is the engine's own.</summary>
public static class Amounts
{
    /// <summary>Energy.</summary>
    public const string Energy = "energy";

    /// <summary>Energy held in reserve tanks.</summary>
    public const string ReserveEnergy = "reserve_energy";

    /// <summary>Missiles.</summary>
    public const string Missiles = "missiles";

    /// <summary>Super missiles.</summary>
    public const string SuperMissiles = "super_missiles";

    /// <summary>Power bombs.</summary>
    public const string PowerBombs = "power_bombs";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [Energy, ReserveEnergy, Missiles, SuperMissiles, PowerBombs];
}
