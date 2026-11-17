using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The Samus projectile compositions timed projectile records draw, by family.</summary>
public static class ProjectileSpriteDefinitions
{
    /// <summary>
    /// Supported projectile-composition JSON schema revision, four, naming every composition by family and pose (#1762), each
    /// pose for the stage it shows, with a composition the cartridge stores again byte for byte held once (#2295).
    /// </summary>
    public const int Version = 4;
    /// <summary>Installed JSON resource filename for editable projectile sprite compositions, separate from their timing, damage, and collision mechanics.</summary>
    public const string FileName = "projectile-compositions.json";
    /// <summary>Maximum sprite parts allowed in one authored composition, 128, matching the screen's sprite capacity rather than an animation-frame count.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileColumns = 16;
    /// <summary>Sixteen columns and thirty-two rows in the logical OBJ character grid; row times sixteen plus column forms the nine-bit tile index, with large-part neighbors wrapping natively.</summary>
    public const int TileRows = 32;

    /// <summary>The Power beam fired left: one character, which the Wave beam's centre, Ice's star and the Spazer and Plasma startups' first strip also show.</summary>
    private static readonly ProjectileSpriteId PowerLeft = new(ProjectileSpriteGroup.Power, "Left");
    /// <summary>The Power beam fired up left, which Ice's cross also shows.</summary>
    private static readonly ProjectileSpriteId PowerUpLeft = new(ProjectileSpriteGroup.Power, "UpLeft");
    /// <summary>The Power beam fired up, which Ice's narrow star also shows.</summary>
    private static readonly ProjectileSpriteId PowerUp = new(ProjectileSpriteGroup.Power, "Up");
    /// <summary>The Power beam fired up right, which Ice's mirrored cross also shows.</summary>
    private static readonly ProjectileSpriteId PowerUpRight = new(ProjectileSpriteGroup.Power, "UpRight");
    /// <summary>The missile explosion's ball, which the Super Missile explosion opens with too.</summary>
    private static readonly ProjectileSpriteId MissileExplosionBall = new(ProjectileSpriteGroup.MissileExplosion, "Ball");
    /// <summary>The missile explosion's flare, which the Super Missile explosion shows too.</summary>
    private static readonly ProjectileSpriteId MissileExplosionFlare = new(ProjectileSpriteGroup.MissileExplosion, "Flare");

    /// <summary>
    /// Charged Wave's cores and paired lobes, in storage order: each lobe pair's swing from the core (Near, Midway, Far, Peak)
    /// and its alternate drawing. Charged Ice Wave stores all of them again, byte for byte, and charged Ice's cross and star
    /// are its alternate core and core.
    /// </summary>
    private static readonly ProjectileSpriteId[] ChargedWave = Poses(ProjectileSpriteGroup.ChargedWave,
        "Core", "CoreAlternate", "Right.LobesNear", "Right.LobesNearAlternate", "Right.LobesMidway",
        "Right.LobesMidwayAlternate", "Right.LobesFar", "Right.LobesFarAlternate", "Right.LobesPeak",
        "Right.LobesPeakAlternate", "DownRight.LobesNear", "DownRight.LobesNearAlternate", "DownRight.LobesMidway",
        "DownRight.LobesMidwayAlternate", "DownRight.LobesFar", "DownRight.LobesFarAlternate", "DownRight.LobesPeak",
        "DownRight.LobesPeakAlternate", "UpRight.LobesNear", "UpRight.LobesNearAlternate", "UpRight.LobesMidway",
        "UpRight.LobesMidwayAlternate", "UpRight.LobesFar", "UpRight.LobesFarAlternate", "UpRight.LobesPeak",
        "UpRight.LobesPeakAlternate", "Down.LobesNear", "Down.LobesNearAlternate", "Down.LobesMidway",
        "Down.LobesMidwayAlternate", "Down.LobesFar", "Down.LobesFarAlternate", "Down.LobesPeak", "Down.LobesPeakAlternate");
    /// <summary>Charged Wave's core.</summary>
    private static readonly ProjectileSpriteId ChargedWaveCore = ChargedWave[0];
    /// <summary>Charged Wave's alternate core.</summary>
    private static readonly ProjectileSpriteId ChargedWaveCoreAlternate = ChargedWave[1];

    /// <summary>The Spazer beam's up-right seed, which charged Spazer's up-right startup grows into.</summary>
    private static readonly ProjectileSpriteId SpazerUpRightSeed = new(ProjectileSpriteGroup.Spazer, "UpRight.Seed");
    /// <summary>The Spazer beam's down-right seed, which charged Spazer's down-right startup grows into.</summary>
    private static readonly ProjectileSpriteId SpazerDownRightSeed = new(ProjectileSpriteGroup.Spazer, "DownRight.Seed");
    /// <summary>The Spazer beam's up-left seed, which charged Spazer's up-left startup grows into.</summary>
    private static readonly ProjectileSpriteId SpazerUpLeftSeed = new(ProjectileSpriteGroup.Spazer, "UpLeft.Seed");
    /// <summary>The Spazer beam's downward seed, which charged Spazer's downward startup grows into.</summary>
    private static readonly ProjectileSpriteId SpazerDownSeed = new(ProjectileSpriteGroup.Spazer, "Down.Seed");
    /// <summary>The Spazer beam's leftward lanes splitting, which its rightward lanes show too.</summary>
    private static readonly ProjectileSpriteId SpazerLeftSplitting = new(ProjectileSpriteGroup.Spazer, "Left.Splitting");
    /// <summary>The Spazer beam's upward seed, which charged Spazer's upward startup grows into.</summary>
    private static readonly ProjectileSpriteId SpazerUpSeed = new(ProjectileSpriteGroup.Spazer, "Up.Seed");
    /// <summary>Charged Spazer's leftward lanes splitting, which its rightward lanes show too.</summary>
    private static readonly ProjectileSpriteId ChargedSpazerLeftSplitting = new(ProjectileSpriteGroup.ChargedSpazer, "Left.Splitting");
    /// <summary>Charged Power's shot fired up left, which it shows in every direction but left.</summary>
    private static readonly ProjectileSpriteId ChargedPowerUpLeft = new(ProjectileSpriteGroup.ChargedPower, "UpLeft");
    /// <summary>Charged Power's alternate drawing fired left, which it shows in every direction.</summary>
    private static readonly ProjectileSpriteId ChargedPowerLeftAlternate = new(ProjectileSpriteGroup.ChargedPower, "LeftAlternate");
    /// <summary>Charged Spazer's upward startup strip, which charged Plasma's upward core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupUpShort = new(ProjectileSpriteGroup.SpazerStartup, "Up.Short");
    /// <summary>Charged Spazer's horizontal alternate startup strip, which charged Plasma's rightward alternate core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupHorizontalAlternateShort = new(ProjectileSpriteGroup.SpazerStartup, "HorizontalAlternate.Short");
    /// <summary>Charged Spazer's down-right alternate startup strip, which charged Plasma's down-right alternate core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupDownRightAlternateShort = new(ProjectileSpriteGroup.SpazerStartup, "DownRightAlternate.Short");
    /// <summary>Charged Spazer's vertical alternate startup strip, which charged Plasma's upward alternate core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupVerticalAlternateShort = new(ProjectileSpriteGroup.SpazerStartup, "VerticalAlternate.Short");
    /// <summary>Charged Spazer's up-right alternate startup strip, which charged Plasma's up-right alternate core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupUpRightAlternateShort = new(ProjectileSpriteGroup.SpazerStartup, "UpRightAlternate.Short");

    /// <summary>
    /// The composition each of the cartridge's 417 projectile composition records holds, in its storage order (#1762, #2295):
    /// each named by its family and pose, the pose naming the direction it points or travels and the stage of its animation
    /// it shows. A wave's poses name the side it swings to and how far (Near, Midway, Far, Peak), a spread its lanes'
    /// widening from its seed (Splitting, Parting, Spreading, NearlySpread, Spread), and an alternate the drawing the beam
    /// shows on alternate updates. A record that stores, byte for byte, a composition stored earlier holds that composition:
    /// it is one picture, named once, for the family that stores it first, and the import maps both records to it.
    /// </summary>
    internal static IReadOnlyList<ProjectileSpriteId> Stored { get; } =
    [
        new(ProjectileSpriteGroup.Nothing, "Empty"),
        PowerLeft, PowerUpLeft, PowerUp, PowerUpRight,
        .. Poses(ProjectileSpriteGroup.Power, "Right", "DownRight", "Down", "DownLeft"),
        .. Poses(ProjectileSpriteGroup.Plasma, "Right", "DownRight", "Up", "UpRight"),
        new(ProjectileSpriteGroup.MissileExplosion, "Spark"), MissileExplosionBall,
        new(ProjectileSpriteGroup.MissileExplosion, "Blast"), MissileExplosionFlare,
        .. Poses(ProjectileSpriteGroup.MissileExplosion, "Burst", "Embers"),
        .. Poses(ProjectileSpriteGroup.BombExplosion, "Orb", "SparklingOrb", "Burst", "WideBurst", "Ring"),
        MissileExplosionBall, new(ProjectileSpriteGroup.SuperMissileExplosion, "Blast"), MissileExplosionFlare,
        .. Poses(ProjectileSpriteGroup.SuperMissileExplosion, "RingedBall", "Ring", "FadingRing"),
        .. Poses(ProjectileSpriteGroup.PowerBomb, "Bright", "Dimming", "Dark"),
        .. Poses(ProjectileSpriteGroup.BeamExplosion, "Speck", "Glint", "Flash", "RingedFlash", "Ring", "FadingRing"),
        .. Poses(ProjectileSpriteGroup.Bomb, "Dark", "Smouldering", "Glowing", "Flashing"),
        .. Poses(ProjectileSpriteGroup.Missile, "Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft"),
        .. Poses(ProjectileSpriteGroup.SuperMissile, "Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft"),
        PowerLeft,
        .. Poses(ProjectileSpriteGroup.Wave, "Right.UpNear", "Right.UpMidway", "Right.UpFar", "Right.UpPeak", "Right.DownNear",
            "Right.DownMidway", "Right.DownFar", "Right.DownPeak", "DownRight.UpRightNear", "DownRight.UpRightMidway",
            "DownRight.UpRightFar", "DownRight.UpRightPeak", "DownRight.DownLeftNear", "DownRight.DownLeftMidway",
            "DownRight.DownLeftFar", "DownRight.DownLeftPeak", "Down.RightNear", "Down.RightMidway", "Down.RightFar",
            "Down.RightPeak", "Down.LeftNear", "Down.LeftMidway", "Down.LeftFar", "Down.LeftPeak", "UpRight.UpLeftNear",
            "UpRight.UpLeftMidway", "UpRight.UpLeftFar", "UpRight.UpLeftPeak", "UpRight.DownRightNear",
            "UpRight.DownRightMidway", "UpRight.DownRightFar", "UpRight.DownRightPeak"),
        .. ChargedWave,
        .. ChargedWave,
        .. Poses(ProjectileSpriteGroup.PlasmaWave, "RightShort.Core", "RightShort.LobesNear", "RightShort.LobesMidway",
            "RightShort.LobesFar", "RightShort.LobesPeak", "RightLong.Core", "RightLong.LobesNear", "RightLong.LobesMidway",
            "RightLong.LobesFar", "RightLong.LobesPeak", "DownRightShort.Core", "DownRightShort.LobesNear",
            "DownRightShort.LobesMidway", "DownRightShort.LobesFar", "DownRightShort.LobesPeak", "DownRightLong.Core",
            "DownRightLong.LobesNear", "DownRightLong.LobesMidway", "DownRightLong.LobesFar", "DownRightLong.LobesPeak",
            "UpShort.Core", "UpShort.LobesNear", "UpShort.LobesMidway", "UpShort.LobesFar", "UpShort.LobesPeak",
            "UpLong.Core", "UpLong.LobesNear", "UpLong.LobesMidway", "UpLong.LobesFar", "UpLong.LobesPeak",
            "RightAlternate.Core", "RightAlternate.LobesNear", "RightAlternate.LobesMidway", "RightAlternate.LobesFar",
            "RightAlternate.LobesPeak", "DownRightAlternate.Core", "DownRightAlternate.LobesNear",
            "DownRightAlternate.LobesMidway", "DownRightAlternate.LobesFar", "DownRightAlternate.LobesPeak",
            "UpAlternate.Core", "UpAlternate.LobesNear", "UpAlternate.LobesMidway", "UpAlternate.LobesFar",
            "UpAlternate.LobesPeak", "UpRightAlternate.Core", "UpRightAlternate.LobesNear", "UpRightAlternate.LobesMidway",
            "UpRightAlternate.LobesFar", "UpRightAlternate.LobesPeak", "UpRightShort.Core", "UpRightShort.LobesNear",
            "UpRightShort.LobesMidway", "UpRightShort.LobesFar", "UpRightShort.LobesPeak", "UpRightLong.Core",
            "UpRightLong.LobesNear", "UpRightLong.LobesMidway", "UpRightLong.LobesFar", "UpRightLong.LobesPeak"),
        SpazerUpRightSeed,
        .. Poses(ProjectileSpriteGroup.Spazer, "UpRight.Parting", "UpRight.Spreading", "UpRight.NearlySpread",
            "UpRight.Spread", "UpRight.Splitting"),
        SpazerDownRightSeed,
        .. Poses(ProjectileSpriteGroup.Spazer, "DownRight.Parting", "DownRight.Spreading", "DownRight.NearlySpread",
            "DownRight.Spread", "DownRight.Splitting", "DownLeft.Seed", "DownLeft.Parting", "DownLeft.Spreading",
            "DownLeft.NearlySpread", "DownLeft.Spread"),
        SpazerUpLeftSeed,
        .. Poses(ProjectileSpriteGroup.Spazer, "UpLeft.Parting", "UpLeft.Spreading", "UpLeft.NearlySpread", "UpLeft.Spread",
            "UpLeft.Splitting"),
        SpazerDownSeed,
        .. Poses(ProjectileSpriteGroup.Spazer, "Down.Splitting", "Down.Parting", "Down.Spreading", "Down.NearlySpread",
            "Down.Spread", "Left.Seed"),
        SpazerLeftSplitting,
        .. Poses(ProjectileSpriteGroup.Spazer, "Left.Parting", "Left.Spreading", "Left.NearlySpread", "Left.Spread"),
        SpazerUpSeed,
        .. Poses(ProjectileSpriteGroup.Spazer, "Up.Splitting", "Up.Parting", "Up.Spreading", "Up.NearlySpread", "Up.Spread",
            "Right.Seed"),
        SpazerLeftSplitting,
        .. Poses(ProjectileSpriteGroup.Spazer, "Right.Parting", "Right.Spreading", "Right.NearlySpread", "Right.Spread"),
        .. Poses(ProjectileSpriteGroup.ChargedSpazer, "HorizontalAlternate.Seed", "HorizontalAlternate.Splitting",
            "HorizontalAlternate.Parting", "HorizontalAlternate.Spreading", "HorizontalAlternate.NearlySpread",
            "HorizontalAlternate.Spread", "VerticalAlternate.Seed", "VerticalAlternate.Splitting", "VerticalAlternate.Parting",
            "VerticalAlternate.Spreading", "VerticalAlternate.NearlySpread", "VerticalAlternate.Spread",
            "DownRightAlternate.Seed", "DownRightAlternate.Splitting", "DownRightAlternate.Parting",
            "DownRightAlternate.Spreading", "DownRightAlternate.NearlySpread", "DownRightAlternate.Spread",
            "UpRightAlternate.Seed", "UpRightAlternate.Splitting", "UpRightAlternate.Parting", "UpRightAlternate.Spreading",
            "UpRightAlternate.NearlySpread", "UpRightAlternate.Spread", "UpRight.Seed", "UpRight.Parting",
            "UpRight.Spreading", "UpRight.NearlySpread", "UpRight.Spread", "UpRight.Splitting", "DownRight.Seed",
            "DownRight.Parting", "DownRight.Spreading", "DownRight.NearlySpread", "DownRight.Spread", "DownRight.Splitting",
            "DownLeft.Seed", "DownLeft.Parting", "DownLeft.Spreading", "DownLeft.NearlySpread", "DownLeft.Spread",
            "DownLeft.Splitting", "UpLeft.Seed", "UpLeft.Parting", "UpLeft.Spreading", "UpLeft.NearlySpread", "UpLeft.Spread",
            "UpLeft.Splitting", "Down.Seed", "Down.Splitting", "Down.Parting", "Down.Spreading", "Down.NearlySpread",
            "Down.Spread", "Left.Seed"),
        ChargedSpazerLeftSplitting,
        .. Poses(ProjectileSpriteGroup.ChargedSpazer, "Left.Parting", "Left.Spreading", "Left.NearlySpread", "Left.Spread",
            "Up.Seed", "Up.Splitting", "Up.Parting", "Up.Spreading", "Up.NearlySpread", "Up.Spread", "Right.Seed"),
        ChargedSpazerLeftSplitting,
        .. Poses(ProjectileSpriteGroup.ChargedSpazer, "Right.Parting", "Right.Spreading", "Right.NearlySpread", "Right.Spread"),
        new(ProjectileSpriteGroup.ChargedPower, "Left"),
        ChargedPowerUpLeft, ChargedPowerUpLeft, ChargedPowerUpLeft, ChargedPowerUpLeft, ChargedPowerUpLeft, ChargedPowerUpLeft,
        ChargedPowerUpLeft,
        ChargedPowerLeftAlternate, ChargedPowerLeftAlternate, ChargedPowerLeftAlternate, ChargedPowerLeftAlternate,
        ChargedPowerLeftAlternate, ChargedPowerLeftAlternate, ChargedPowerLeftAlternate, ChargedPowerLeftAlternate,
        ChargedWaveCoreAlternate, ChargedWaveCore, ChargedWaveCoreAlternate, ChargedWaveCore,
        PowerLeft, PowerUpLeft, PowerUp, PowerUpRight,
        PowerLeft, new(ProjectileSpriteGroup.SpazerStartup, "Left.Long"),
        new(ProjectileSpriteGroup.SpazerStartup, "UpLeft.Short"), SpazerUpLeftSeed,
        SpazerStartupUpShort, SpazerUpSeed,
        new(ProjectileSpriteGroup.SpazerStartup, "UpRight.Short"), SpazerUpRightSeed,
        .. Poses(ProjectileSpriteGroup.SpazerStartup, "Right.Short", "Right.Long", "DownRight.Short"), SpazerDownRightSeed,
        new(ProjectileSpriteGroup.SpazerStartup, "Down.Short"), SpazerDownSeed,
        SpazerStartupHorizontalAlternateShort, new(ProjectileSpriteGroup.SpazerStartup, "HorizontalAlternate.Long"),
        SpazerStartupDownRightAlternateShort, new(ProjectileSpriteGroup.SpazerStartup, "DownRightAlternate.Long"),
        SpazerStartupVerticalAlternateShort, new(ProjectileSpriteGroup.SpazerStartup, "VerticalAlternate.Long"),
        SpazerStartupUpRightAlternateShort, new(ProjectileSpriteGroup.SpazerStartup, "UpRightAlternate.Long"),
        PowerLeft, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "Right.Short", "Right.Long", "Right.Full"),
        SpazerStartupUpShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "Up.Short", "Up.Long", "Up.Full",
            "DownRight.Core", "DownRight.Short", "DownRight.Long", "DownRight.Full", "UpRight.Core", "UpRight.Short",
            "UpRight.Long", "UpRight.Full"),
        SpazerStartupHorizontalAlternateShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "RightAlternate.Short",
            "RightAlternate.Long", "RightAlternate.Full"),
        SpazerStartupVerticalAlternateShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "UpAlternate.Short",
            "UpAlternate.Long", "UpAlternate.Full"),
        SpazerStartupDownRightAlternateShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "DownRightAlternate.Short",
            "DownRightAlternate.Long", "DownRightAlternate.Full"),
        SpazerStartupUpRightAlternateShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "UpRightAlternate.Short",
            "UpRightAlternate.Long", "UpRightAlternate.Full"),
    ];

    /// <summary>
    /// The distinct compositions timed projectile records draw, in the order the cartridge first stores them, including the
    /// zero-part Nothing composition: one per picture, a composition stored again byte for byte held once (#1635, #2295).
    /// </summary>
    public static IReadOnlyList<ProjectileSpriteId> All { get; } = [.. Stored.Distinct()];

    /// <summary>The compositions of <paramref name="group"/> showing <paramref name="poses"/>, in that order.</summary>
    private static ProjectileSpriteId[] Poses(ProjectileSpriteGroup group, params string[] poses) =>
        [.. poses.Select(pose => new ProjectileSpriteId(group, pose))];
}
