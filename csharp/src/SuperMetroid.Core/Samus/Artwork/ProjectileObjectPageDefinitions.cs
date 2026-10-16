namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where the beam and trail characters sit on the OBJ page their uploads place them in.</summary>
internal static class ProjectileObjectPageDefinitions
{
    /// <summary>VMADD $6000: first word of the OBJ character table gameplay and the intro select (OBSEL $03).</summary>
    public const int ObjectCharacterBaseWord = 0x6000;

    /// <summary>VRAM words in one 4-bpp character.</summary>
    public const int WordsPerCharacter = 16;

    /// <summary>OBJ character $30: the eight beam characters `$90:AC8D` uploads to VMADD $6300.</summary>
    public const int BeamFirstCharacter = (BeamTileAtlasDefinitions.DestinationWord - ObjectCharacterBaseWord) / WordsPerCharacter;

    /// <summary>OBJ character $38: the eight ice/wave trail characters at VMADD $6380.</summary>
    public const int IceWaveTrailFirstCharacter = (ProjectileTrailAtlasDefinitions.IceWaveDestinationWord - ObjectCharacterBaseWord) / WordsPerCharacter;

    /// <summary>OBJ character $48: the four missile trail characters at VMADD $6480.</summary>
    public const int MissileTrailFirstCharacter = (ProjectileTrailAtlasDefinitions.MissileDestinationWord - ObjectCharacterBaseWord) / WordsPerCharacter;

    /// <summary>OBJ character $E0: the intro's decompressed cinematic characters at VMADD $6E00, over its intro sheet's tail.</summary>
    public const int IntroCinematicObjectsFirstCharacter = (0x6e00 - ObjectCharacterBaseWord) / WordsPerCharacter;
}
