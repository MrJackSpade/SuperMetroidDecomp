namespace SuperMetroid.Core.Assets;

/// <summary>Visual flare placement resource contract, including native low-nibble overread selections.</summary>
public static class ChargeFlarePlacementDefinitions
{
    public const string FileName = "charge-flare-placement.json";
    public const int Version = 1;
    /// <summary>The renderer retains all four direction bits, including values beyond named directions.</summary>
    public const int DirectionCount = 16;
    public static string Key(bool running, int direction) => $"{(running ? "running" : "standing")}-{direction:D2}";
}
