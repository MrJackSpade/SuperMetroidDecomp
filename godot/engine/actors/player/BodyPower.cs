using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// A power that, while active, makes the player's own body deliver a hit to whatever it touches: enemies, barriers
/// and blocks alike. The engine knows that it is active and what hit it gives; the game says when it is active.
/// </summary>
public sealed class BodyPower
{
    /// <summary>A power of the given name, damage and kinds, not yet active.</summary>
    public BodyPower(string name, int damage, params HitKind[] kinds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Hit = new Hit(damage, HitSide.Player, kinds);
    }

    /// <summary>The power's name.</summary>
    public string Name { get; }

    /// <summary>The hit the player's body delivers while the power is active.</summary>
    public Hit Hit { get; }

    /// <summary>Whether the power is active.</summary>
    public bool IsActive { get; set; }
}
