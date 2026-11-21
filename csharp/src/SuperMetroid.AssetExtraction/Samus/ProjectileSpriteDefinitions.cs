using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.DefinitionRun;

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

    /// <summary>Charged Wave's core.</summary>
    private static readonly ProjectileSpriteId ChargedWaveCore = new(ProjectileSpriteGroup.ChargedWave, "Core");

    /// <summary>Charged Wave's core as a starburst, which its flicker trades with the orb.</summary>
    private static readonly ProjectileSpriteId ChargedWaveCoreStarburst = new(ProjectileSpriteGroup.ChargedWave, "CoreStarburst");

    /// <summary>
    /// Charged Wave's cores and paired lobes, in storage order: its core as an orb and as a starburst, and each lobe pair's
    /// swing from the core (Near, Midway, Far, Peak) with the orb on its upper (or left) lobe and then on its lower (or right)
    /// one, the starburst on the other: the two pictures its flicker trades. Charged Ice Wave stores all of them again, byte
    /// for byte, and charged Ice's cross and star are its starburst core and core.
    /// </summary>
    private static readonly ProjectileSpriteId[] ChargedWave = [ChargedWaveCore, ChargedWaveCoreStarburst, .. Poses(ProjectileSpriteGroup.ChargedWave,
        "Right.LobesNear", "Right.LobesNearOrbBelow", "Right.LobesMidway",
        "Right.LobesMidwayOrbBelow", "Right.LobesFar", "Right.LobesFarOrbBelow", "Right.LobesPeak",
        "Right.LobesPeakOrbBelow", "DownRight.LobesNear", "DownRight.LobesNearOrbBelow", "DownRight.LobesMidway",
        "DownRight.LobesMidwayOrbBelow", "DownRight.LobesFar", "DownRight.LobesFarOrbBelow", "DownRight.LobesPeak",
        "DownRight.LobesPeakOrbBelow", "UpRight.LobesNear", "UpRight.LobesNearOrbBelow", "UpRight.LobesMidway",
        "UpRight.LobesMidwayOrbBelow", "UpRight.LobesFar", "UpRight.LobesFarOrbBelow", "UpRight.LobesPeak",
        "UpRight.LobesPeakOrbBelow", "Down.LobesNear", "Down.LobesNearOrbRight", "Down.LobesMidway",
        "Down.LobesMidwayOrbRight", "Down.LobesFar", "Down.LobesFarOrbRight", "Down.LobesPeak", "Down.LobesPeakOrbRight")];

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
    /// <summary>Charged Power's orb, which its flicker trades with the starburst in every direction.</summary>
    private static readonly ProjectileSpriteId ChargedPowerOrb = new(ProjectileSpriteGroup.ChargedPower, "Orb");
    /// <summary>Charged Spazer's upward startup strip, which charged Plasma's upward core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupUpShort = new(ProjectileSpriteGroup.SpazerStartup, "Up.Short");
    /// <summary>Charged Spazer's thick horizontal startup strip, which charged Plasma's thick rightward core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupHorizontalThickShort = new(ProjectileSpriteGroup.SpazerStartup, "HorizontalThick.Short");
    /// <summary>Charged Spazer's thick down-right startup strip, which charged Plasma's thick down-right core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupDownRightThickShort = new(ProjectileSpriteGroup.SpazerStartup, "DownRightThick.Short");
    /// <summary>Charged Spazer's thick vertical startup strip, which charged Plasma's thick upward core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupVerticalThickShort = new(ProjectileSpriteGroup.SpazerStartup, "VerticalThick.Short");
    /// <summary>Charged Spazer's thick up-right startup strip, which charged Plasma's thick up-right core also shows.</summary>
    private static readonly ProjectileSpriteId SpazerStartupUpRightThickShort = new(ProjectileSpriteGroup.SpazerStartup, "UpRightThick.Short");

    /// <summary>The Power beam's shots, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> PowerShots { get; } =
    [
        PowerLeft, PowerUpLeft, PowerUp, PowerUpRight,
        .. Poses(ProjectileSpriteGroup.Power, "Right", "DownRight", "Down", "DownLeft"),
    ];

    /// <summary>The Plasma beam's shots, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> PlasmaShots { get; } =
    [
        .. Poses(ProjectileSpriteGroup.Plasma, "Right", "DownRight", "Up", "UpRight"),
    ];

    /// <summary>The missile explosion's stages, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> MissileExplosions { get; } =
    [
        new(ProjectileSpriteGroup.MissileExplosion, "Spark"), MissileExplosionBall,
        new(ProjectileSpriteGroup.MissileExplosion, "Blast"), MissileExplosionFlare,
        .. Poses(ProjectileSpriteGroup.MissileExplosion, "Burst", "Embers"),
    ];

    /// <summary>The bomb explosion's stages, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> BombExplosions { get; } =
    [
        .. Poses(ProjectileSpriteGroup.BombExplosion, "Orb", "SparklingOrb", "Burst", "WideBurst", "Ring"),
    ];

    /// <summary>The Super Missile explosion's stages, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> SuperMissileExplosions { get; } =
    [
        MissileExplosionBall, new(ProjectileSpriteGroup.SuperMissileExplosion, "Blast"), MissileExplosionFlare,
        .. Poses(ProjectileSpriteGroup.SuperMissileExplosion, "RingedBall", "Ring", "FadingRing"),
    ];

    /// <summary>The Power Bomb's stages, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> PowerBombs { get; } =
    [
        .. Poses(ProjectileSpriteGroup.PowerBomb, "Bright", "Dimming", "Dark"),
    ];

    /// <summary>The beam explosion's stages, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> BeamExplosions { get; } =
    [
        .. Poses(ProjectileSpriteGroup.BeamExplosion, "Speck", "Glint", "Flash", "RingedFlash", "Ring", "FadingRing"),
    ];

    /// <summary>The bomb's stages, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> Bombs { get; } =
    [
        .. Poses(ProjectileSpriteGroup.Bomb, "Dark", "Smouldering", "Glowing", "Flashing"),
    ];

    /// <summary>The missile's directions, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> Missiles { get; } =
    [
        .. Poses(ProjectileSpriteGroup.Missile, "Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft"),
    ];

    /// <summary>The Super Missile's directions, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> SuperMissiles { get; } =
    [
        .. Poses(ProjectileSpriteGroup.SuperMissile, "Left", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft"),
    ];

    /// <summary>The Wave beam's centre and swings, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> WaveShots { get; } =
    [
        PowerLeft,
        .. Poses(ProjectileSpriteGroup.Wave, "Right.UpNear", "Right.UpMidway", "Right.UpFar", "Right.UpPeak", "Right.DownNear",
            "Right.DownMidway", "Right.DownFar", "Right.DownPeak", "DownRight.UpRightNear", "DownRight.UpRightMidway",
            "DownRight.UpRightFar", "DownRight.UpRightPeak", "DownRight.DownLeftNear", "DownRight.DownLeftMidway",
            "DownRight.DownLeftFar", "DownRight.DownLeftPeak", "Down.RightNear", "Down.RightMidway", "Down.RightFar",
            "Down.RightPeak", "Down.LeftNear", "Down.LeftMidway", "Down.LeftFar", "Down.LeftPeak", "UpRight.UpLeftNear",
            "UpRight.UpLeftMidway", "UpRight.UpLeftFar", "UpRight.UpLeftPeak", "UpRight.DownRightNear",
            "UpRight.DownRightMidway", "UpRight.DownRightFar", "UpRight.DownRightPeak"),
    ];

    /// <summary>Charged Wave's cores and lobes, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> ChargedWaveShots { get; } =
    [
        .. ChargedWave,
    ];

    /// <summary>Charged Ice Wave's cores and lobes, Charged Wave's again, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> ChargedIceWaveShots { get; } =
    [
        .. ChargedWave,
    ];

    /// <summary>The Plasma Wave beam's cores and lobes, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> PlasmaWaveShots { get; } =
    [
        .. Poses(ProjectileSpriteGroup.PlasmaWave, "RightShort.Core", "RightShort.LobesNear", "RightShort.LobesMidway",
            "RightShort.LobesFar", "RightShort.LobesPeak", "RightLong.Core", "RightLong.LobesNear", "RightLong.LobesMidway",
            "RightLong.LobesFar", "RightLong.LobesPeak", "DownRightShort.Core", "DownRightShort.LobesNear",
            "DownRightShort.LobesMidway", "DownRightShort.LobesFar", "DownRightShort.LobesPeak", "DownRightLong.Core",
            "DownRightLong.LobesNear", "DownRightLong.LobesMidway", "DownRightLong.LobesFar", "DownRightLong.LobesPeak",
            "UpShort.Core", "UpShort.LobesNear", "UpShort.LobesMidway", "UpShort.LobesFar", "UpShort.LobesPeak",
            "UpLong.Core", "UpLong.LobesNear", "UpLong.LobesMidway", "UpLong.LobesFar", "UpLong.LobesPeak",
            "RightThick.Core", "RightThick.LobesNear", "RightThick.LobesMidway", "RightThick.LobesFar",
            "RightThick.LobesPeak", "DownRightThick.Core", "DownRightThick.LobesNear",
            "DownRightThick.LobesMidway", "DownRightThick.LobesFar", "DownRightThick.LobesPeak",
            "UpThick.Core", "UpThick.LobesNear", "UpThick.LobesMidway", "UpThick.LobesFar",
            "UpThick.LobesPeak", "UpRightThick.Core", "UpRightThick.LobesNear", "UpRightThick.LobesMidway",
            "UpRightThick.LobesFar", "UpRightThick.LobesPeak", "UpRightShort.Core", "UpRightShort.LobesNear",
            "UpRightShort.LobesMidway", "UpRightShort.LobesFar", "UpRightShort.LobesPeak", "UpRightLong.Core",
            "UpRightLong.LobesNear", "UpRightLong.LobesMidway", "UpRightLong.LobesFar", "UpRightLong.LobesPeak"),
    ];

    /// <summary>The Spazer beam's seeds and spreads, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> SpazerShots { get; } =
    [
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
    ];

    /// <summary>Charged Spazer's seeds and spreads, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> ChargedSpazerShots { get; } =
    [
        .. Poses(ProjectileSpriteGroup.ChargedSpazer, "HorizontalThick.Seed", "HorizontalThick.Splitting",
            "HorizontalThick.Parting", "HorizontalThick.Spreading", "HorizontalThick.NearlySpread",
            "HorizontalThick.Spread", "VerticalThick.Seed", "VerticalThick.Splitting", "VerticalThick.Parting",
            "VerticalThick.Spreading", "VerticalThick.NearlySpread", "VerticalThick.Spread",
            "DownRightThick.Seed", "DownRightThick.Splitting", "DownRightThick.Parting",
            "DownRightThick.Spreading", "DownRightThick.NearlySpread", "DownRightThick.Spread",
            "UpRightThick.Seed", "UpRightThick.Splitting", "UpRightThick.Parting", "UpRightThick.Spreading",
            "UpRightThick.NearlySpread", "UpRightThick.Spread", "UpRight.Seed", "UpRight.Parting",
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
    ];

    /// <summary>Charged Power's shots and orb, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> ChargedPowerShots { get; } =
    [
        new(ProjectileSpriteGroup.ChargedPower, "Left"),
        .. Repeated(ChargedPowerUpLeft, 7),
        .. Repeated(ChargedPowerOrb, 8),
    ];

    /// <summary>Charged Ice's cross and star, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> ChargedIceShots { get; } =
    [
        ChargedWaveCoreStarburst, ChargedWaveCore, ChargedWaveCoreStarburst, ChargedWaveCore,
    ];

    /// <summary>The Ice beam's shots, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> IceShots { get; } =
    [
        PowerLeft, PowerUpLeft, PowerUp, PowerUpRight,
    ];

    /// <summary>Charged Spazer's startup strips, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> SpazerStartups { get; } =
    [
        PowerLeft, new(ProjectileSpriteGroup.SpazerStartup, "Left.Long"),
        new(ProjectileSpriteGroup.SpazerStartup, "UpLeft.Short"), SpazerUpLeftSeed,
        SpazerStartupUpShort, SpazerUpSeed,
        new(ProjectileSpriteGroup.SpazerStartup, "UpRight.Short"), SpazerUpRightSeed,
        .. Poses(ProjectileSpriteGroup.SpazerStartup, "Right.Short", "Right.Long", "DownRight.Short"), SpazerDownRightSeed,
        new(ProjectileSpriteGroup.SpazerStartup, "Down.Short"), SpazerDownSeed,
        SpazerStartupHorizontalThickShort, new(ProjectileSpriteGroup.SpazerStartup, "HorizontalThick.Long"),
        SpazerStartupDownRightThickShort, new(ProjectileSpriteGroup.SpazerStartup, "DownRightThick.Long"),
        SpazerStartupVerticalThickShort, new(ProjectileSpriteGroup.SpazerStartup, "VerticalThick.Long"),
        SpazerStartupUpRightThickShort, new(ProjectileSpriteGroup.SpazerStartup, "UpRightThick.Long"),
    ];

    /// <summary>Charged Plasma's startup strips, in their records' storage order (#2565).</summary>
    internal static IReadOnlyList<ProjectileSpriteId> PlasmaStartups { get; } =
    [
        PowerLeft, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "Right.Short", "Right.Long", "Right.Full"),
        SpazerStartupUpShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "Up.Short", "Up.Long", "Up.Full",
            "DownRight.Core", "DownRight.Short", "DownRight.Long", "DownRight.Full", "UpRight.Core", "UpRight.Short",
            "UpRight.Long", "UpRight.Full"),
        SpazerStartupHorizontalThickShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "RightThick.Short",
            "RightThick.Long", "RightThick.Full"),
        SpazerStartupVerticalThickShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "UpThick.Short",
            "UpThick.Long", "UpThick.Full"),
        SpazerStartupDownRightThickShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "DownRightThick.Short",
            "DownRightThick.Long", "DownRightThick.Full"),
        SpazerStartupUpRightThickShort, .. Poses(ProjectileSpriteGroup.PlasmaStartup, "UpRightThick.Short",
            "UpRightThick.Long", "UpRightThick.Full"),
    ];

    /// <summary>
    /// The composition each of the cartridge's 416 pictured projectile composition records holds, in its storage order after
    /// the empty record no picture is composed from, which a frame showing nothing selects (#1762, #2295, #2558):
    /// each named by its family and pose, the pose naming the direction it points or travels and the stage of its animation
    /// it shows. A wave's poses name the side it swings to and how far (Near, Midway, Far, Peak), a spread its lanes'
    /// widening from its seed (Splitting, Parting, Spreading, NearlySpread, Spread), and a charged shot's flicker the
    /// picture it trades on alternate updates: Charged Power's orb, Charged Wave's starburst core and lobes with the orb
    /// moved (OrbBelow, OrbRight), and the Spazer and Plasma beams' thick beam. A record that stores, byte for byte, a composition stored earlier holds that composition:
    /// it is one picture, named once, for the family that stores it first, and the import maps both records to it. The
    /// groups are listed in their storage order; where each group's records lie is the import's layout of it
    /// (<see cref="ProjectileSpriteLocations"/>).
    /// </summary>
    internal static IReadOnlyList<ProjectileSpriteId> Stored { get; } =
    [
        .. PowerShots,
        .. PlasmaShots,
        .. MissileExplosions,
        .. BombExplosions,
        .. SuperMissileExplosions,
        .. PowerBombs,
        .. BeamExplosions,
        .. Bombs,
        .. Missiles,
        .. SuperMissiles,
        .. WaveShots,
        .. ChargedWaveShots,
        .. ChargedIceWaveShots,
        .. PlasmaWaveShots,
        .. SpazerShots,
        .. ChargedSpazerShots,
        .. ChargedPowerShots,
        .. ChargedIceShots,
        .. IceShots,
        .. SpazerStartups,
        .. PlasmaStartups,
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
