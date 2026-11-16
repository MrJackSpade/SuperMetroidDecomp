namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Mutually exclusive attack distributions selected by the Ridley AI.</summary>
public enum RidleyAttackSituation
{
    /// <summary>
    /// Choices 0..3 start the pogo attack and 4..7 the swoop: below <see cref="NorfairRidleyMotionDefinitions.WoundedAttackHealth"/>,
    /// and above it when Samus is neither low in the arena nor grabbable.
    /// </summary>
    Ordinary,
    /// <summary>
    /// Grabbable Samus above the pogo zone, with Ridley at least <see cref="NorfairRidleyMotionDefinitions.WoundedAttackHealth"/>:
    /// choices 0..5 lunge at her and 6..7 start the pogo attack.
    /// </summary>
    DamageBoosting,
    /// <summary>
    /// Samus in the pogo zone, at or below <see cref="NorfairRidleyArenaDefinitions.PogoZoneTopY"/>, with Ridley at least
    /// <see cref="NorfairRidleyMotionDefinitions.WoundedAttackHealth"/>, starts the pogo attack for every choice.
    /// </summary>
    PogoZone,
    /// <summary>Samus's spin-jump movement takes priority and starts the wall sweep for every choice.</summary>
    SpinJumping,
    /// <summary>Zero health with Samus not spin jumping lunges at her for every choice; Ridley counts these death lunges.</summary>
    ZeroHealth,
}
