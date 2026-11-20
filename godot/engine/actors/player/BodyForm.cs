namespace Engine.Actors;

/// <summary>The shape the player's body is in, which decides how tall it is.</summary>
public enum BodyForm
{
    /// <summary>Upright.</summary>
    Standing,

    /// <summary>Crouched: shorter, and not moving.</summary>
    Crouching,

    /// <summary>The low form: short enough for low gaps, and able to move.</summary>
    Crawling,
}
