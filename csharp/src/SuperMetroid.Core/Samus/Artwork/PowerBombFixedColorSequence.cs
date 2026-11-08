namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Two mutually exclusive fixed-color sequences.</summary>
public enum PowerBombFixedColorSequence
{
    /// <summary>The pre-explosion colours: sixteen fixed-color colours selected by the pre-explosion radius through its white and yellow phases.</summary>
    PreExplosion,
    /// <summary>The explosion colours: thirty-two radius-selected fixed-color colours shared by Power Bomb, Crystal Flash, and the Ceres station explosion.</summary>
    Explosion,
}
