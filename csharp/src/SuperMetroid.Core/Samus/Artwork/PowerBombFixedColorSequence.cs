namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Two mutually exclusive fixed-color sequences.</summary>
public enum PowerBombFixedColorSequence
{
    /// <summary>PowerBomb_PreExplosion_Colors: sixteen fixed-color RGB5 triplets selected by the pre-explosion radius through its white and yellow phases.</summary>
    PreExplosion,
    /// <summary>PowerBombExplosion_Colors: thirty-two radius-selected fixed-color RGB5 triplets shared by Power Bomb, Crystal Flash, and the Ceres station explosion.</summary>
    Explosion,
}
