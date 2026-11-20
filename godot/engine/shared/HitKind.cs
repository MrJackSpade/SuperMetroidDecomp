namespace Engine.Shared;

/// <summary>
/// A named kind of damage. The engine names the two it deals itself; a game names the rest. What can be hit lists
/// its reactions by kind, and barriers and blocks list the kinds they accept.
/// </summary>
/// <param name="Name">The kind's name, as data files write it.</param>
public readonly record struct HitKind(string Name)
{
    /// <summary>Touching something that hurts to touch.</summary>
    public static HitKind Contact { get; } = new("contact");

    /// <summary>A hazard of the room itself.</summary>
    public static HitKind Hazard { get; } = new("hazard");

    /// <summary>The kind's name.</summary>
    public override string ToString() => Name;
}
