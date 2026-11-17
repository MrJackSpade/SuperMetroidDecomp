namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The situations Norfair Ridley chooses his attack in, one at a time.</summary>
public enum RidleyAttackSituation
{
    /// <summary>
    /// Half his choices start the pogo attack and half the swoop (<see cref="RidleyRandomDefinitions.OrdinaryAttackPogos"/>):
    /// below <see cref="NorfairRidleyMotionDefinitions.WoundedAttackHealth"/>, and above it when Samus is neither low in the
    /// arena nor grabbable.
    /// </summary>
    Ordinary,
    /// <summary>
    /// Grabbable Samus above the pogo zone, with Ridley at least <see cref="NorfairRidleyMotionDefinitions.WoundedAttackHealth"/>:
    /// three in four of his choices lunge at her and the rest start the pogo attack
    /// (<see cref="RidleyRandomDefinitions.DamageBoostingAttackLunges"/>).
    /// </summary>
    DamageBoosting,
    /// <summary>
    /// Samus in the pogo zone, at or below <see cref="NorfairRidleyArenaDefinitions.PogoZoneTopY"/>, with Ridley at least
    /// <see cref="NorfairRidleyMotionDefinitions.WoundedAttackHealth"/>, always starts the pogo attack.
    /// </summary>
    PogoZone,
    /// <summary>Samus's spin-jump movement takes priority and always starts the wall sweep.</summary>
    SpinJumping,
    /// <summary>Zero health with Samus not spin jumping always lunges at her; Ridley counts these death lunges.</summary>
    ZeroHealth,
}
