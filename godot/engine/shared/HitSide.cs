namespace Engine.Shared;

/// <summary>Whose hit it is, which decides whom it can hurt.</summary>
public enum HitSide
{
    /// <summary>The player's: it hurts enemies, barriers and blocks.</summary>
    Player,

    /// <summary>The enemies': it hurts the player.</summary>
    Enemies,

    /// <summary>Nobody's, such as a hazard of the room: it hurts whoever meets it.</summary>
    Neutral,
}
