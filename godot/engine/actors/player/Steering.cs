namespace Engine.Actors;

/// <summary>
/// How the player's own speed across stands to the way they face. The speed is a size; with this, and the way the
/// player faces, it says which way it carries them and whether it is gaining or slowing.
/// </summary>
public enum Steering
{
    /// <summary>Steered: the speed carries the player the way they face and gains up to the top speed of what they are doing.</summary>
    Steered,

    /// <summary>
    /// Unsteered: the speed carries the player the way they face with nothing driving it. On the ground it slows to
    /// rest and is then steered again; in the air it carries on as if steered.
    /// </summary>
    Unsteered,

    /// <summary>
    /// Reversing: the player has turned and still moves the old way, against the way they now face, slowing to rest;
    /// they are then steered again.
    /// </summary>
    Reversing,
}
