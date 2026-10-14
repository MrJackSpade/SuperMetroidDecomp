namespace SuperMetroid.Core.Game;

/// <summary>
/// Index into Mother Brain's three-entry phase-two attack dispatcher at
/// <c>$A9:B654</c>. The values are array indices in the cartridge, not host-only states.
/// </summary>
public enum MotherBrainAttackPhase : ushort
{
    /// <summary>Index 0 selects the next head/body attack using Samus movement and sampled RNG, then seeds the 64-update cooldown.</summary>
    ChooseAttack = 0,
    /// <summary>Index 1 decrements the unsigned attack cooldown until zero while the chosen head bytecode runs independently.</summary>
    Cooldown = 1,
    /// <summary>Index 2 resets the attack sub-dispatcher and returns the body to ordinary phase-two thinking.</summary>
    EndAttack = 2,
}
