namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The Samus projectile compositions native timed projectile records draw, by family.</summary>
public static class ProjectileSpriteDefinitions
{
    /// <summary>Supported projectile-composition JSON schema revision, two, naming every composition by family and index.</summary>
    public const int Version = 2;
    /// <summary>Installed JSON resource filename for editable projectile OAM compositions, separate from their timing, damage, and collision mechanics.</summary>
    public const string FileName = "projectile-compositions.json";
    /// <summary>Maximum OAM parts allowed in one authored composition, 128, matching the hardware OBJ entry capacity rather than an animation-frame count.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileColumns = 16;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileRows = 32;

    /// <summary>How many compositions each projectile family holds, in the cartridge's storage order.</summary>
    private static readonly ProjectileSpriteGroupSize[] Groups =
    [
        new(ProjectileSpriteGroup.Nothing, 1),
        new(ProjectileSpriteGroup.Power, 8),
        new(ProjectileSpriteGroup.Plasma, 4),
        new(ProjectileSpriteGroup.MissileExplosion, 6),
        new(ProjectileSpriteGroup.BombExplosion, 5),
        new(ProjectileSpriteGroup.SuperMissileExplosion, 6),
        new(ProjectileSpriteGroup.PowerBomb, 3),
        new(ProjectileSpriteGroup.BeamExplosion, 6),
        new(ProjectileSpriteGroup.Bomb, 4),
        new(ProjectileSpriteGroup.Missile, 8),
        new(ProjectileSpriteGroup.SuperMissile, 8),
        new(ProjectileSpriteGroup.Wave, 33),
        new(ProjectileSpriteGroup.ChargedWave, 34),
        new(ProjectileSpriteGroup.ChargedIceWave, 34),
        new(ProjectileSpriteGroup.PlasmaWave, 60),
        new(ProjectileSpriteGroup.Spazer, 47),
        new(ProjectileSpriteGroup.ChargedSpazer, 72),
        new(ProjectileSpriteGroup.ChargedPower, 16),
        new(ProjectileSpriteGroup.ChargedIce, 4),
        new(ProjectileSpriteGroup.Ice, 4),
        new(ProjectileSpriteGroup.SpazerStartup, 22),
        new(ProjectileSpriteGroup.PlasmaStartup, 32),
    ];

    /// <summary>The 417 compositions native timed projectile records draw, in the cartridge's storage order, including the zero-part Nothing composition.</summary>
    public static IReadOnlyList<ProjectileSpriteId> All { get; } = [.. Groups.SelectMany(group => group.Members)];
}
