namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How long a Dachora's warning and charge last, and how its speed-boost echoes trail it (#2279).</summary>
internal static class DachoraTimingDefinitions
{
    /// <summary>How long a woken Dachora blinks before its first run, in updates.</summary>
    internal const int BlinkDuration = 120;

    /// <summary>How long a Dachora stores its shine before it launches, in updates.</summary>
    internal const int ChargeDuration = 60;

    /// <summary>How many speed-boost echoes trail a Dachora.</summary>
    internal const int EchoCount = 4;

    /// <summary>The updates a shinesparking Dachora waits between placing echoes on its trail.</summary>
    internal const int EchoPlacementInterval = 1;

    /// <summary>The updates each placed echo stays visible.</summary>
    internal const int EchoLifetime = 8;
}
