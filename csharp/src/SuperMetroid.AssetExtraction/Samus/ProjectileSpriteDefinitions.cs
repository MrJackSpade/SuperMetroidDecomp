using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The Samus projectile compositions timed projectile records draw, by family.</summary>
public static class ProjectileSpriteDefinitions
{
    /// <summary>Supported projectile-composition JSON schema revision, three, naming every composition by family and pose (#1762).</summary>
    public const int Version = 3;
    /// <summary>Installed JSON resource filename for editable projectile sprite compositions, separate from their timing, damage, and collision mechanics.</summary>
    public const string FileName = "projectile-compositions.json";
    /// <summary>Maximum sprite parts allowed in one authored composition, 128, matching the screen's sprite capacity rather than an animation-frame count.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileColumns = 16;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileRows = 32;

    /// <summary>
    /// Each projectile family's poses, in the game's storage order (#1762): each pose names the direction its composition
    /// points or travels and the stage of its animation it shows. A wave's poses name the side it swings to, a spread its
    /// widening from its seed, and an alternate the drawing the beam shows on alternate updates.
    /// </summary>
    private static readonly ProjectileSpriteFamily[] Families =
    [
        new(ProjectileSpriteGroup.Nothing, ["Empty"]),
        new(ProjectileSpriteGroup.Power, ["Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft"]),
        new(ProjectileSpriteGroup.Plasma, ["Right", "DownRight", "Up", "UpRight"]),
        new(ProjectileSpriteGroup.MissileExplosion, ["Stage1", "Stage2", "Stage3", "Stage4", "Stage5", "Stage6"]),
        new(ProjectileSpriteGroup.BombExplosion, ["Stage1", "Stage2", "Stage3", "Stage4", "Stage5"]),
        new(ProjectileSpriteGroup.SuperMissileExplosion, ["Stage1", "Stage2", "Stage3", "Stage4", "Stage5", "Stage6"]),
        new(ProjectileSpriteGroup.PowerBomb, ["Phase1", "Phase2", "Phase3"]),
        new(ProjectileSpriteGroup.BeamExplosion, ["Stage1", "Stage2", "Stage3", "Stage4", "Stage5", "Stage6"]),
        new(ProjectileSpriteGroup.Bomb, ["Phase1", "Phase2", "Phase3", "Phase4"]),
        new(ProjectileSpriteGroup.Missile, ["Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft"]),
        new(ProjectileSpriteGroup.SuperMissile, ["Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft"]),
        new(ProjectileSpriteGroup.Wave, ["Centre", "Right.Up1", "Right.Up2", "Right.Up3", "Right.Up4", "Right.Down1",
            "Right.Down2", "Right.Down3", "Right.Down4", "DownRight.UpRight1", "DownRight.UpRight2", "DownRight.UpRight3",
            "DownRight.UpRight4", "DownRight.DownLeft1", "DownRight.DownLeft2", "DownRight.DownLeft3",
            "DownRight.DownLeft4", "Down.Right1", "Down.Right2", "Down.Right3", "Down.Right4", "Down.Left1", "Down.Left2",
            "Down.Left3", "Down.Left4", "UpRight.UpLeft1", "UpRight.UpLeft2", "UpRight.UpLeft3", "UpRight.UpLeft4",
            "UpRight.DownRight1", "UpRight.DownRight2", "UpRight.DownRight3", "UpRight.DownRight4"]),
        new(ProjectileSpriteGroup.ChargedWave, ["Core", "CoreAlternate", "Right.Spread1", "Right.Spread1Alternate",
            "Right.Spread2", "Right.Spread2Alternate", "Right.Spread3", "Right.Spread3Alternate", "Right.Spread4",
            "Right.Spread4Alternate", "DownRight.Spread1", "DownRight.Spread1Alternate", "DownRight.Spread2",
            "DownRight.Spread2Alternate", "DownRight.Spread3", "DownRight.Spread3Alternate", "DownRight.Spread4",
            "DownRight.Spread4Alternate", "UpRight.Spread1", "UpRight.Spread1Alternate", "UpRight.Spread2",
            "UpRight.Spread2Alternate", "UpRight.Spread3", "UpRight.Spread3Alternate", "UpRight.Spread4",
            "UpRight.Spread4Alternate", "Down.Spread1", "Down.Spread1Alternate", "Down.Spread2", "Down.Spread2Alternate",
            "Down.Spread3", "Down.Spread3Alternate", "Down.Spread4", "Down.Spread4Alternate"]),
        new(ProjectileSpriteGroup.ChargedIceWave, ["Core", "CoreAlternate", "Right.Spread1", "Right.Spread1Alternate",
            "Right.Spread2", "Right.Spread2Alternate", "Right.Spread3", "Right.Spread3Alternate", "Right.Spread4",
            "Right.Spread4Alternate", "DownRight.Spread1", "DownRight.Spread1Alternate", "DownRight.Spread2",
            "DownRight.Spread2Alternate", "DownRight.Spread3", "DownRight.Spread3Alternate", "DownRight.Spread4",
            "DownRight.Spread4Alternate", "UpRight.Spread1", "UpRight.Spread1Alternate", "UpRight.Spread2",
            "UpRight.Spread2Alternate", "UpRight.Spread3", "UpRight.Spread3Alternate", "UpRight.Spread4",
            "UpRight.Spread4Alternate", "Down.Spread1", "Down.Spread1Alternate", "Down.Spread2", "Down.Spread2Alternate",
            "Down.Spread3", "Down.Spread3Alternate", "Down.Spread4", "Down.Spread4Alternate"]),
        new(ProjectileSpriteGroup.PlasmaWave, ["RightShort.Core", "RightShort.Spread1", "RightShort.Spread2",
            "RightShort.Spread3", "RightShort.Spread4", "RightLong.Core", "RightLong.Spread1", "RightLong.Spread2",
            "RightLong.Spread3", "RightLong.Spread4", "DownRightShort.Core", "DownRightShort.Spread1",
            "DownRightShort.Spread2", "DownRightShort.Spread3", "DownRightShort.Spread4", "DownRightLong.Core",
            "DownRightLong.Spread1", "DownRightLong.Spread2", "DownRightLong.Spread3", "DownRightLong.Spread4",
            "UpShort.Core", "UpShort.Spread1", "UpShort.Spread2", "UpShort.Spread3", "UpShort.Spread4", "UpLong.Core",
            "UpLong.Spread1", "UpLong.Spread2", "UpLong.Spread3", "UpLong.Spread4", "RightAlternate.Core",
            "RightAlternate.Spread1", "RightAlternate.Spread2", "RightAlternate.Spread3", "RightAlternate.Spread4",
            "DownRightAlternate.Core", "DownRightAlternate.Spread1", "DownRightAlternate.Spread2",
            "DownRightAlternate.Spread3", "DownRightAlternate.Spread4", "UpAlternate.Core", "UpAlternate.Spread1",
            "UpAlternate.Spread2", "UpAlternate.Spread3", "UpAlternate.Spread4", "UpRightAlternate.Core",
            "UpRightAlternate.Spread1", "UpRightAlternate.Spread2", "UpRightAlternate.Spread3", "UpRightAlternate.Spread4",
            "UpRightShort.Core", "UpRightShort.Spread1", "UpRightShort.Spread2", "UpRightShort.Spread3",
            "UpRightShort.Spread4", "UpRightLong.Core", "UpRightLong.Spread1", "UpRightLong.Spread2", "UpRightLong.Spread3",
            "UpRightLong.Spread4"]),
        new(ProjectileSpriteGroup.Spazer, ["UpRight.Seed", "UpRight.Spread2", "UpRight.Spread3", "UpRight.Spread4",
            "UpRight.Spread5", "UpRight.Spread1", "DownRight.Seed", "DownRight.Spread2", "DownRight.Spread3",
            "DownRight.Spread4", "DownRight.Spread5", "DownRight.Spread1", "DownLeft.Seed", "DownLeft.Spread1",
            "DownLeft.Spread2", "DownLeft.Spread3", "DownLeft.Spread4", "UpLeft.Seed", "UpLeft.Spread2", "UpLeft.Spread3",
            "UpLeft.Spread4", "UpLeft.Spread5", "UpLeft.Spread1", "Down.Seed", "Down.Spread1", "Down.Spread2",
            "Down.Spread3", "Down.Spread4", "Down.Spread5", "Left.Seed", "Left.Spread1", "Left.Spread2", "Left.Spread3",
            "Left.Spread4", "Left.Spread5", "Up.Seed", "Up.Spread1", "Up.Spread2", "Up.Spread3", "Up.Spread4", "Up.Spread5",
            "Right.Seed", "Right.Spread1", "Right.Spread2", "Right.Spread3", "Right.Spread4", "Right.Spread5"]),
        new(ProjectileSpriteGroup.ChargedSpazer, ["HorizontalAlternate.Seed", "HorizontalAlternate.Spread1",
            "HorizontalAlternate.Spread2", "HorizontalAlternate.Spread3", "HorizontalAlternate.Spread4",
            "HorizontalAlternate.Spread5", "VerticalAlternate.Seed", "VerticalAlternate.Spread1",
            "VerticalAlternate.Spread2", "VerticalAlternate.Spread3", "VerticalAlternate.Spread4",
            "VerticalAlternate.Spread5", "DownRightAlternate.Seed", "DownRightAlternate.Spread1",
            "DownRightAlternate.Spread2", "DownRightAlternate.Spread3", "DownRightAlternate.Spread4",
            "DownRightAlternate.Spread5", "UpRightAlternate.Seed", "UpRightAlternate.Spread1", "UpRightAlternate.Spread2",
            "UpRightAlternate.Spread3", "UpRightAlternate.Spread4", "UpRightAlternate.Spread5", "UpRight.Seed",
            "UpRight.Spread2", "UpRight.Spread3", "UpRight.Spread4", "UpRight.Spread5", "UpRight.Spread1", "DownRight.Seed",
            "DownRight.Spread2", "DownRight.Spread3", "DownRight.Spread4", "DownRight.Spread5", "DownRight.Spread1",
            "DownLeft.Seed", "DownLeft.Spread2", "DownLeft.Spread3", "DownLeft.Spread4", "DownLeft.Spread5",
            "DownLeft.Spread1", "UpLeft.Seed", "UpLeft.Spread2", "UpLeft.Spread3", "UpLeft.Spread4", "UpLeft.Spread5",
            "UpLeft.Spread1", "Down.Seed", "Down.Spread1", "Down.Spread2", "Down.Spread3", "Down.Spread4", "Down.Spread5",
            "Left.Seed", "Left.Spread1", "Left.Spread2", "Left.Spread3", "Left.Spread4", "Left.Spread5", "Up.Seed",
            "Up.Spread1", "Up.Spread2", "Up.Spread3", "Up.Spread4", "Up.Spread5", "Right.Seed", "Right.Spread1",
            "Right.Spread2", "Right.Spread3", "Right.Spread4", "Right.Spread5"]),
        new(ProjectileSpriteGroup.ChargedPower, ["Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down",
            "DownLeft", "LeftAlternate", "UpLeftAlternate", "UpAlternate", "UpRightAlternate", "RightAlternate",
            "DownRightAlternate", "DownAlternate", "DownLeftAlternate"]),
        new(ProjectileSpriteGroup.ChargedIce, ["Phase1", "Phase2", "Phase3", "Phase4"]),
        new(ProjectileSpriteGroup.Ice, ["Phase1", "Phase2", "Phase3", "Phase4"]),
        new(ProjectileSpriteGroup.SpazerStartup, ["Left.Stage1", "Left.Stage2", "UpLeft.Stage1", "UpLeft.Stage2",
            "Up.Stage1", "Up.Stage2", "UpRight.Stage1", "UpRight.Stage2", "Right.Stage1", "Right.Stage2",
            "DownRight.Stage1", "DownRight.Stage2", "Down.Stage1", "Down.Stage2", "HorizontalAlternate.Stage1",
            "HorizontalAlternate.Stage2", "DownRightAlternate.Stage1", "DownRightAlternate.Stage2",
            "VerticalAlternate.Stage1", "VerticalAlternate.Stage2", "UpRightAlternate.Stage1", "UpRightAlternate.Stage2"]),
        new(ProjectileSpriteGroup.PlasmaStartup, ["Right.Core", "Right.Short", "Right.Long", "Right.Full", "Up.Core",
            "Up.Short", "Up.Long", "Up.Full", "DownRight.Core", "DownRight.Short", "DownRight.Long", "DownRight.Full",
            "UpRight.Core", "UpRight.Short", "UpRight.Long", "UpRight.Full", "RightAlternate.Core", "RightAlternate.Short",
            "RightAlternate.Long", "RightAlternate.Full", "UpAlternate.Core", "UpAlternate.Short", "UpAlternate.Long",
            "UpAlternate.Full", "DownRightAlternate.Core", "DownRightAlternate.Short", "DownRightAlternate.Long",
            "DownRightAlternate.Full", "UpRightAlternate.Core", "UpRightAlternate.Short", "UpRightAlternate.Long",
            "UpRightAlternate.Full"]),
    ];

    /// <summary>The 417 compositions timed projectile records draw, in the game's storage order, including the zero-part Nothing composition.</summary>
    public static IReadOnlyList<ProjectileSpriteId> All { get; } =
        [.. Families.SelectMany(family => family.Poses.Select(pose => new ProjectileSpriteId(family.Group, pose)))];
}
