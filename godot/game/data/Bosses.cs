namespace Game.Data;

/// <summary>The names of the bosses and mini-bosses. Each one's defeat is a fact in progress.</summary>
public static class Bosses
{
    /// <summary>The Bomb Torizo, in Crateria.</summary>
    public const string BombTorizo = "bomb_torizo";

    /// <summary>Spore Spawn, in Brinstar.</summary>
    public const string SporeSpawn = "spore_spawn";

    /// <summary>Kraid, in Brinstar.</summary>
    public const string Kraid = "kraid";

    /// <summary>Crocomire, in Norfair.</summary>
    public const string Crocomire = "crocomire";

    /// <summary>Phantoon, in the Wrecked Ship.</summary>
    public const string Phantoon = "phantoon";

    /// <summary>Botwoon, in Maridia.</summary>
    public const string Botwoon = "botwoon";

    /// <summary>Draygon, in Maridia.</summary>
    public const string Draygon = "draygon";

    /// <summary>The Golden Torizo, in Norfair.</summary>
    public const string GoldenTorizo = "golden_torizo";

    /// <summary>Ridley, in Norfair.</summary>
    public const string Ridley = "ridley";

    /// <summary>Ridley, on Ceres Station.</summary>
    public const string CeresRidley = "ceres_ridley";

    /// <summary>Mother Brain, in Tourian.</summary>
    public const string MotherBrain = "mother_brain";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [BombTorizo, SporeSpawn, Kraid, Crocomire, Phantoon, Botwoon, Draygon, GoldenTorizo, Ridley, CeresRidley, MotherBrain];

    /// <summary>The fact progress records when <paramref name="boss"/> is defeated.</summary>
    public static string Defeated(string boss) => $"{boss}_defeated";
}
