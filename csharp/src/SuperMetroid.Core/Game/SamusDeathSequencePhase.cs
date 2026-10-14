namespace SuperMetroid.Core.Game;

/// <summary>Game-state phases that own Samus after fatal damage has reached bank `$9B`.</summary>
public enum SamusDeathSequencePhase
{
    /// <summary>Normal gameplay owns Samus; no bank-$9B death sequence is active.</summary>
    Inactive,
    /// <summary>Game state $16 advances the fatal pose for sixteen calls before flashing.</summary>
    PreFlashing,
    /// <summary>Game state $17 alternates death palettes and queues the five graphics segments.</summary>
    Flashing,
    /// <summary>Game state $18 advances explosion spritemaps, palettes, and room whiteout.</summary>
    SuitExplosion,
    /// <summary>The terminal white frame is installed and the outer room fade may take ownership.</summary>
    Complete,
}
