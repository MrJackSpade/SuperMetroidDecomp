namespace SuperMetroid.Core.Samus.Death;

/// <summary>Game-state phases that own Samus after fatal damage has started the death sequence.</summary>
public enum SamusDeathSequencePhase
{
    /// <summary>Normal gameplay owns Samus; no death sequence is active.</summary>
    Inactive,
    /// <summary>The first death game state advances the fatal pose for sixteen updates before flashing.</summary>
    PreFlashing,
    /// <summary>The flashing game state alternates death shades before the suit explodes.</summary>
    Flashing,
    /// <summary>The suit-explosion game state advances explosion frames, shades, and room whiteout.</summary>
    SuitExplosion,
    /// <summary>The terminal white frame is installed and the outer room fade may take ownership.</summary>
    Complete,
}
