namespace SuperMetroid.Core.Assets;

/// <summary>Trail tile ownership within $82:8318's standard sprite transfer from Tiles_Standard_Sprite_0.</summary>
public static class ProjectileTrailAtlasDefinitions
{
    public const string FileName = "projectile-trails.png";
    public const int Width = 96;
    public const int Height = 8;
    /// <summary>$9A:D900 contains OBJ tiles $38..$3F: ice and wave trail frames.</summary>
    public const int IceWaveSource = 0x9ad900;
    /// <summary>$9A:DB00 contains OBJ tiles $48..$4B: missile and Super Missile trail frames.</summary>
    public const int MissileSource = 0x9adb00;
    /// <summary>Eight 4-bpp tiles, excluding the unrelated $40..$47 tile gap.</summary>
    public const int IceWaveByteCount = 256;
    /// <summary>Four 4-bpp missile trail tiles.</summary>
    public const int MissileByteCount = 128;
    /// <summary>VRAM word $6380: standard OBJ base $6000 plus tile $38's word offset.</summary>
    public const ushort IceWaveDestinationWord = 0x6380;
    /// <summary>VRAM word $6480: standard OBJ base $6000 plus tile $48's word offset.</summary>
    public const ushort MissileDestinationWord = 0x6480;
}
