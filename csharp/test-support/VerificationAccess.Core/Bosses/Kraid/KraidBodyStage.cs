namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The stage Kraid's body is in, each running phases of its own type (#2208): his rise through the floor
/// (<see cref="KraidRisePhase"/>), his fight (<see cref="KraidFightPhase"/>), his growth into the second phase
/// (<see cref="KraidGrowthPhase"/>) and his death (<see cref="KraidDeathPhase"/>).
/// </summary>
public enum KraidBodyStage
{
    /// <summary>He rises through the floor at the start of the encounter, outside the fight.</summary>
    Rising,
    /// <summary>He fights: thinking, attacking with his mouth open and reacting to hits.</summary>
    Fighting,
    /// <summary>He grows into his second phase, which is part of the fight.</summary>
    Growing,
    /// <summary>He dies: his fade, his sinking and the room's repair.</summary>
    Dying,
}
