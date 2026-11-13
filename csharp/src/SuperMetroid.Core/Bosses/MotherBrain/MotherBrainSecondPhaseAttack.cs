namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The attack Mother Brain's second-phase attack roll chooses for grounded Samus when the roll does not press her attack
/// (#1619): <see cref="MotherBrainRandomDefinitions.NearAttack"/> and <see cref="MotherBrainRandomDefinitions.FarAttack"/>.
/// </summary>
internal enum MotherBrainSecondPhaseAttack
{
    /// <summary>No attack: her brain returns to its phase-two neutral pose for the cooldown.</summary>
    Neutral,

    /// <summary>Her brain fires four onion rings.</summary>
    FourOnionRings,

    /// <summary>Her brain lines up quickly and fires the laser.</summary>
    Laser,

    /// <summary>She walks back and drops a bomb, unless one of her bombs is already live.</summary>
    Bomb,
}
