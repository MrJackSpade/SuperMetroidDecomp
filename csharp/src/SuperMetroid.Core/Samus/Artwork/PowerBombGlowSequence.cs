namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The two glow sequences a Power Bomb shows over the scene, each selected by its own radius.</summary>
public enum PowerBombGlowSequence
{
    /// <summary>The pre-explosion colours: glow colours selected by the pre-explosion radius through its white and yellow phases.</summary>
    PreExplosion,
    /// <summary>The explosion colours: radius-selected glow colours shared by Power Bomb, Crystal Flash, and the Ceres station explosion.</summary>
    Explosion,
}
