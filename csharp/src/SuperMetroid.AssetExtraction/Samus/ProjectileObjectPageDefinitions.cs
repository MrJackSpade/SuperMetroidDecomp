namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where the beam and trail characters sit on the OBJ page their uploads place them in.</summary>
internal static class ProjectileObjectPageDefinitions
{
    /// <summary>OBJ character $30: the first of the eight beam characters `$90:AC8D` copies.</summary>
    public const int BeamFirstCharacter = 48;

    /// <summary>OBJ character $38: the first of the eight ice/wave trail characters.</summary>
    public const int IceWaveTrailFirstCharacter = 56;

    /// <summary>OBJ character $48: the first of the four missile trail characters.</summary>
    public const int MissileTrailFirstCharacter = 72;

    /// <summary>OBJ character $E0: the first of the intro's decompressed cinematic characters, over its intro sheet's tail.</summary>
    public const int IntroCinematicObjectsFirstCharacter = 224;
}
