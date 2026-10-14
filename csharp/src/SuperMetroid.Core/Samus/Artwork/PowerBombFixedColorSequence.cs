namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Two mutually exclusive cartridge fixed-color sequences.</summary>
public enum PowerBombFixedColorSequence
{
    /// <summary>$88:9079, PowerBomb_PreExplosion_Colors: sixteen fixed-color RGB5 triplets selected by the pre-explosion radius through its white and yellow phases.</summary>
    PreExplosion,
    /// <summary>$88:8D85, PowerBombExplosion_Colors: thirty-two radius-selected fixed-color RGB5 triplets shared by Power Bomb, Crystal Flash, and the Ceres station explosion.</summary>
    Explosion,
}
