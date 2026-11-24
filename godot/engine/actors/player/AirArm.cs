namespace Engine.Actors;

/// <summary>How a player aiming level in the air holds their weapon arm.</summary>
public enum AirArm
{
    /// <summary>At rest: neither of the others.</summary>
    AtRest,

    /// <summary>Steering the way they face, where that makes a stance of its own.</summary>
    Forward,

    /// <summary>Held out, as the attack is held.</summary>
    Out,
}
