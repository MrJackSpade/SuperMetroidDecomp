namespace SuperMetroid.Core.Samus.Death;

/// <summary>Game-state phases that own Samus after fatal damage has started the death sequence.</summary>
public abstract partial record SamusDeathSequencePhase
{

    /// <summary>Normal gameplay owns Samus; no death sequence is active.</summary>
    public sealed partial record Inactive : SamusDeathSequencePhase;

    /// <summary>The phases of a death sequence that has begun, its completion included.</summary>
    public abstract partial record Begun : SamusDeathSequencePhase
    {

        /// <summary>The first death game state advances the fatal pose for <see cref="Abilities.SamusDeathSequenceDefinitions.PreFlashFrameCount"/> before flashing.</summary>
        public sealed partial record PreFlashing : Begun;

        /// <summary>The flashing game state alternates death shades before the suit explodes.</summary>
        public sealed partial record Flashing : Begun;

        /// <summary>The suit-explosion game state advances explosion frames, shades, and room whiteout.</summary>
        public sealed partial record SuitExplosion : Begun;

        /// <summary>The terminal white frame is installed and the outer room fade may take ownership.</summary>
        public sealed partial record Complete : Begun;
    }
}
