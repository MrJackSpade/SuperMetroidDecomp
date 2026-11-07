using SuperMetroid.Core.Samus.Artwork;
using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' timed records (#1301, #1517), by the program that shows them, in the order it shows them: each
/// record's stock spritemap, its hitbox radii and the animation frame trails read. A record is named by its program and
/// its place there (<see cref="SamusProjectileFrame"/>), the identity installed frame bindings key on, so a program
/// added or removed leaves every other record's identity alone.
/// </summary>
internal static class SamusProjectileRecordDefinitions
{
    /// <summary>Every program's timed records, in the order it shows them.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileProgram, SamusProjectileRecord[]> ByProgram =
        new Dictionary<SamusProjectileProgram, SamusProjectileRecord[]>
    {
        [P.PowerUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Power, 2), new(4, 4), 0),
        ],
        [P.PowerUpRight] =
        [
            new(new(ProjectileSpriteGroup.Power, 3), new(8, 4), 0),
        ],
        [P.PowerRight] =
        [
            new(new(ProjectileSpriteGroup.Power, 4), new(8, 4), 0),
        ],
        [P.PowerDownRight] =
        [
            new(new(ProjectileSpriteGroup.Power, 5), new(8, 4), 0),
        ],
        [P.PowerDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Power, 6), new(4, 4), 0),
        ],
        [P.PowerDownLeft] =
        [
            new(new(ProjectileSpriteGroup.Power, 7), new(8, 4), 0),
        ],
        [P.PowerLeft] =
        [
            new(new(ProjectileSpriteGroup.Power, 0), new(8, 4), 0),
        ],
        [P.PowerUpLeft] =
        [
            new(new(ProjectileSpriteGroup.Power, 1), new(8, 4), 0),
        ],
        [P.WaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Nothing, 0), new(12, 4), 0),
        ],
        [P.WaveDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Wave, 0), new(12, 4), 0),
            new(new(ProjectileSpriteGroup.Wave, 17), new(12, 4), 1),
            new(new(ProjectileSpriteGroup.Wave, 18), new(12, 4), 2),
            new(new(ProjectileSpriteGroup.Wave, 19), new(16, 4), 3),
            new(new(ProjectileSpriteGroup.Wave, 20), new(20, 4), 4),
            new(new(ProjectileSpriteGroup.Wave, 19), new(16, 4), 5),
            new(new(ProjectileSpriteGroup.Wave, 18), new(12, 4), 6),
            new(new(ProjectileSpriteGroup.Wave, 17), new(12, 4), 7),
            new(new(ProjectileSpriteGroup.Wave, 0), new(12, 4), 8),
            new(new(ProjectileSpriteGroup.Wave, 21), new(12, 4), 9),
            new(new(ProjectileSpriteGroup.Wave, 22), new(12, 4), 10),
            new(new(ProjectileSpriteGroup.Wave, 23), new(16, 4), 11),
            new(new(ProjectileSpriteGroup.Wave, 24), new(20, 4), 12),
            new(new(ProjectileSpriteGroup.Wave, 23), new(16, 4), 13),
            new(new(ProjectileSpriteGroup.Wave, 22), new(12, 4), 14),
            new(new(ProjectileSpriteGroup.Wave, 21), new(12, 4), 15),
        ],
        [P.WaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.Wave, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Wave, 25), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.Wave, 26), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.Wave, 27), new(10, 10), 3),
            new(new(ProjectileSpriteGroup.Wave, 28), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.Wave, 27), new(10, 10), 5),
            new(new(ProjectileSpriteGroup.Wave, 26), new(8, 8), 6),
            new(new(ProjectileSpriteGroup.Wave, 25), new(6, 6), 7),
            new(new(ProjectileSpriteGroup.Wave, 0), new(4, 4), 8),
            new(new(ProjectileSpriteGroup.Wave, 29), new(6, 6), 9),
            new(new(ProjectileSpriteGroup.Wave, 30), new(8, 8), 10),
            new(new(ProjectileSpriteGroup.Wave, 31), new(10, 10), 11),
            new(new(ProjectileSpriteGroup.Wave, 32), new(12, 12), 12),
            new(new(ProjectileSpriteGroup.Wave, 31), new(10, 10), 13),
            new(new(ProjectileSpriteGroup.Wave, 30), new(8, 8), 14),
            new(new(ProjectileSpriteGroup.Wave, 29), new(8, 8), 15),
        ],
        [P.WaveRight] =
        [
            new(new(ProjectileSpriteGroup.Wave, 0), new(4, 12), 0),
            new(new(ProjectileSpriteGroup.Wave, 1), new(4, 12), 1),
            new(new(ProjectileSpriteGroup.Wave, 2), new(4, 12), 2),
            new(new(ProjectileSpriteGroup.Wave, 3), new(4, 16), 3),
            new(new(ProjectileSpriteGroup.Wave, 4), new(4, 20), 4),
            new(new(ProjectileSpriteGroup.Wave, 3), new(4, 16), 5),
            new(new(ProjectileSpriteGroup.Wave, 2), new(4, 12), 6),
            new(new(ProjectileSpriteGroup.Wave, 1), new(4, 12), 7),
            new(new(ProjectileSpriteGroup.Wave, 0), new(4, 12), 8),
            new(new(ProjectileSpriteGroup.Wave, 5), new(4, 12), 9),
            new(new(ProjectileSpriteGroup.Wave, 6), new(4, 12), 10),
            new(new(ProjectileSpriteGroup.Wave, 7), new(4, 16), 11),
            new(new(ProjectileSpriteGroup.Wave, 8), new(4, 20), 12),
            new(new(ProjectileSpriteGroup.Wave, 7), new(4, 16), 13),
            new(new(ProjectileSpriteGroup.Wave, 6), new(4, 12), 14),
            new(new(ProjectileSpriteGroup.Wave, 5), new(4, 12), 15),
        ],
        [P.WaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.Wave, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Wave, 9), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.Wave, 10), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.Wave, 11), new(10, 10), 3),
            new(new(ProjectileSpriteGroup.Wave, 12), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.Wave, 11), new(10, 10), 5),
            new(new(ProjectileSpriteGroup.Wave, 10), new(8, 8), 6),
            new(new(ProjectileSpriteGroup.Wave, 9), new(6, 6), 7),
            new(new(ProjectileSpriteGroup.Wave, 0), new(4, 4), 8),
            new(new(ProjectileSpriteGroup.Wave, 13), new(6, 6), 9),
            new(new(ProjectileSpriteGroup.Wave, 14), new(8, 8), 10),
            new(new(ProjectileSpriteGroup.Wave, 15), new(10, 10), 11),
            new(new(ProjectileSpriteGroup.Wave, 16), new(12, 12), 12),
            new(new(ProjectileSpriteGroup.Wave, 15), new(10, 10), 13),
            new(new(ProjectileSpriteGroup.Wave, 14), new(8, 8), 14),
            new(new(ProjectileSpriteGroup.Wave, 13), new(8, 8), 15),
        ],
        [P.IceUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Ice, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Ice, 1), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.Ice, 2), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.Ice, 3), new(8, 8), 3),
        ],
        [P.SpazerUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 35), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 37), new(12, 8), 1),
        ],
        [P.SpazerUpFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 40), new(20, 8), 2),
        ],
        [P.SpazerUpRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 1), new(12, 12), 1),
        ],
        [P.SpazerUpRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 4), new(16, 16), 2),
        ],
        [P.SpazerRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 41), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.Spazer, 43), new(8, 12), 1),
        ],
        [P.SpazerRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 46), new(8, 20), 2),
        ],
        [P.SpazerDownRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 6), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 7), new(12, 12), 1),
        ],
        [P.SpazerDownRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 10), new(16, 16), 2),
        ],
        [P.SpazerDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 23), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 25), new(12, 8), 1),
        ],
        [P.SpazerDownFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 28), new(20, 8), 2),
        ],
        [P.SpazerDownLeft] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 12), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 13), new(12, 12), 1),
        ],
        [P.SpazerDownLeftLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 16), new(16, 16), 2),
        ],
        [P.SpazerLeft] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 29), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.Spazer, 31), new(8, 12), 1),
        ],
        [P.SpazerLeftLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 34), new(8, 20), 2),
        ],
        [P.SpazerUpLeft] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 17), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 18), new(12, 12), 1),
        ],
        [P.SpazerUpLeftLoop] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 21), new(16, 16), 2),
        ],
        [P.SpazerWaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 35), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 36), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.Spazer, 37), new(12, 8), 2),
            new(new(ProjectileSpriteGroup.Spazer, 38), new(17, 8), 3),
            new(new(ProjectileSpriteGroup.Spazer, 39), new(19, 8), 4),
            new(new(ProjectileSpriteGroup.Spazer, 40), new(20, 8), 5),
            new(new(ProjectileSpriteGroup.Spazer, 39), new(19, 8), 6),
            new(new(ProjectileSpriteGroup.Spazer, 38), new(17, 8), 7),
            new(new(ProjectileSpriteGroup.Spazer, 37), new(12, 8), 8),
            new(new(ProjectileSpriteGroup.Spazer, 36), new(12, 8), 9),
        ],
        [P.SpazerWaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 5), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.Spazer, 1), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.Spazer, 2), new(16, 16), 3),
            new(new(ProjectileSpriteGroup.Spazer, 3), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.Spazer, 4), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.Spazer, 3), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.Spazer, 2), new(16, 16), 7),
            new(new(ProjectileSpriteGroup.Spazer, 1), new(12, 12), 8),
            new(new(ProjectileSpriteGroup.Spazer, 5), new(8, 8), 9),
        ],
        [P.SpazerWaveRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 41), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.Spazer, 42), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.Spazer, 43), new(8, 12), 2),
            new(new(ProjectileSpriteGroup.Spazer, 44), new(8, 17), 3),
            new(new(ProjectileSpriteGroup.Spazer, 45), new(8, 19), 4),
            new(new(ProjectileSpriteGroup.Spazer, 46), new(8, 20), 5),
            new(new(ProjectileSpriteGroup.Spazer, 45), new(8, 19), 6),
            new(new(ProjectileSpriteGroup.Spazer, 44), new(8, 17), 7),
            new(new(ProjectileSpriteGroup.Spazer, 43), new(8, 12), 8),
            new(new(ProjectileSpriteGroup.Spazer, 42), new(8, 12), 9),
        ],
        [P.SpazerWaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 6), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 11), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.Spazer, 7), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.Spazer, 8), new(16, 16), 3),
            new(new(ProjectileSpriteGroup.Spazer, 9), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.Spazer, 10), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.Spazer, 9), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.Spazer, 8), new(16, 16), 7),
            new(new(ProjectileSpriteGroup.Spazer, 7), new(12, 12), 8),
            new(new(ProjectileSpriteGroup.Spazer, 11), new(8, 8), 9),
        ],
        [P.SpazerWaveDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 23), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 24), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.Spazer, 25), new(12, 8), 2),
            new(new(ProjectileSpriteGroup.Spazer, 26), new(17, 8), 3),
            new(new(ProjectileSpriteGroup.Spazer, 27), new(19, 8), 4),
            new(new(ProjectileSpriteGroup.Spazer, 28), new(20, 8), 5),
            new(new(ProjectileSpriteGroup.Spazer, 27), new(19, 8), 6),
            new(new(ProjectileSpriteGroup.Spazer, 26), new(17, 8), 7),
            new(new(ProjectileSpriteGroup.Spazer, 25), new(12, 8), 8),
            new(new(ProjectileSpriteGroup.Spazer, 24), new(12, 8), 9),
        ],
        [P.SpazerWaveDownLeft] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 12), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 13), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.Spazer, 14), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.Spazer, 15), new(16, 16), 3),
            new(new(ProjectileSpriteGroup.Spazer, 16), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.Spazer, 16), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.Spazer, 16), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.Spazer, 15), new(16, 16), 7),
            new(new(ProjectileSpriteGroup.Spazer, 14), new(12, 12), 8),
            new(new(ProjectileSpriteGroup.Spazer, 13), new(8, 8), 9),
        ],
        [P.SpazerWaveLeft] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 29), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.Spazer, 30), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.Spazer, 31), new(8, 12), 2),
            new(new(ProjectileSpriteGroup.Spazer, 32), new(8, 17), 3),
            new(new(ProjectileSpriteGroup.Spazer, 33), new(8, 19), 4),
            new(new(ProjectileSpriteGroup.Spazer, 34), new(8, 20), 5),
            new(new(ProjectileSpriteGroup.Spazer, 33), new(8, 19), 6),
            new(new(ProjectileSpriteGroup.Spazer, 32), new(8, 17), 7),
            new(new(ProjectileSpriteGroup.Spazer, 31), new(8, 12), 8),
            new(new(ProjectileSpriteGroup.Spazer, 30), new(8, 12), 9),
        ],
        [P.SpazerWaveUpLeft] =
        [
            new(new(ProjectileSpriteGroup.Spazer, 17), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.Spazer, 22), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.Spazer, 18), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.Spazer, 19), new(16, 16), 3),
            new(new(ProjectileSpriteGroup.Spazer, 20), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.Spazer, 21), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.Spazer, 20), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.Spazer, 19), new(16, 16), 7),
            new(new(ProjectileSpriteGroup.Spazer, 18), new(12, 12), 8),
            new(new(ProjectileSpriteGroup.Spazer, 22), new(8, 8), 9),
        ],
        [P.PlasmaUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 4), new(8, 16), 0),
        ],
        [P.PlasmaUpFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Plasma, 2), new(8, 16), 1),
        ],
        [P.PlasmaUpRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 12), new(8, 8), 0),
        ],
        [P.PlasmaUpRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Plasma, 3), new(8, 8), 1),
        ],
        [P.PlasmaRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 0), new(8, 8), 0),
        ],
        [P.PlasmaRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Plasma, 0), new(16, 8), 1),
        ],
        [P.PlasmaDownRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 8), new(8, 8), 0),
        ],
        [P.PlasmaDownRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Plasma, 1), new(8, 8), 1),
        ],
        [P.PlasmaIceWaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 4), new(12, 16), 0),
        ],
        [P.PlasmaWaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 20), new(12, 16), 1),
            new(new(ProjectileSpriteGroup.PlasmaWave, 21), new(12, 16), 2),
            new(new(ProjectileSpriteGroup.PlasmaWave, 22), new(17, 16), 3),
            new(new(ProjectileSpriteGroup.PlasmaWave, 23), new(19, 16), 4),
            new(new(ProjectileSpriteGroup.PlasmaWave, 24), new(20, 16), 5),
            new(new(ProjectileSpriteGroup.PlasmaWave, 23), new(19, 16), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 22), new(17, 16), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 21), new(12, 16), 8),
        ],
        [P.PlasmaIceWaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 12), new(8, 8), 0),
        ],
        [P.PlasmaWaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 50), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaWave, 51), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.PlasmaWave, 52), new(16, 16), 3),
            new(new(ProjectileSpriteGroup.PlasmaWave, 53), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.PlasmaWave, 54), new(20, 20), 5),
            new(new(ProjectileSpriteGroup.PlasmaWave, 53), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 52), new(16, 16), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 51), new(12, 12), 8),
        ],
        [P.PlasmaWaveRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 0), new(8, 12), 0),
        ],
        [P.PlasmaWaveRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 0), new(16, 12), 1),
            new(new(ProjectileSpriteGroup.PlasmaWave, 1), new(16, 12), 2),
            new(new(ProjectileSpriteGroup.PlasmaWave, 2), new(16, 17), 3),
            new(new(ProjectileSpriteGroup.PlasmaWave, 3), new(16, 19), 4),
            new(new(ProjectileSpriteGroup.PlasmaWave, 4), new(16, 20), 5),
            new(new(ProjectileSpriteGroup.PlasmaWave, 3), new(16, 19), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 2), new(16, 17), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 1), new(16, 12), 8),
        ],
        [P.PlasmaIceWaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 8), new(8, 8), 0),
        ],
        [P.PlasmaWaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 10), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaWave, 11), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.PlasmaWave, 12), new(16, 16), 3),
            new(new(ProjectileSpriteGroup.PlasmaWave, 13), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.PlasmaWave, 14), new(20, 20), 5),
            new(new(ProjectileSpriteGroup.PlasmaWave, 13), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 12), new(16, 16), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 11), new(12, 12), 8),
        ],
        [P.ChargedPowerUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 2), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 10), new(8, 8), 1),
        ],
        [P.ChargedPowerUpRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 3), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 11), new(8, 8), 1),
        ],
        [P.ChargedPowerRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 4), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 12), new(8, 8), 1),
        ],
        [P.ChargedPowerDownRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 5), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 13), new(8, 8), 1),
        ],
        [P.ChargedPowerDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 6), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 14), new(8, 8), 1),
        ],
        [P.ChargedPowerDownLeft] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 7), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 15), new(8, 8), 1),
        ],
        [P.ChargedPowerLeft] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 8), new(8, 8), 1),
        ],
        [P.ChargedPowerUpLeft] =
        [
            new(new(ProjectileSpriteGroup.ChargedPower, 1), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedPower, 9), new(8, 8), 1),
        ],
        [P.ChargedWaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Nothing, 0), new(12, 8), 0),
        ],
        [P.ChargedWaveDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedWave, 0), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedWave, 1), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.ChargedWave, 26), new(16, 8), 2),
            new(new(ProjectileSpriteGroup.ChargedWave, 27), new(16, 8), 3),
            new(new(ProjectileSpriteGroup.ChargedWave, 28), new(20, 8), 4),
            new(new(ProjectileSpriteGroup.ChargedWave, 29), new(20, 8), 5),
            new(new(ProjectileSpriteGroup.ChargedWave, 30), new(23, 8), 6),
            new(new(ProjectileSpriteGroup.ChargedWave, 31), new(23, 8), 7),
            new(new(ProjectileSpriteGroup.ChargedWave, 32), new(24, 8), 8),
            new(new(ProjectileSpriteGroup.ChargedWave, 33), new(24, 8), 9),
            new(new(ProjectileSpriteGroup.ChargedWave, 30), new(23, 8), 10),
            new(new(ProjectileSpriteGroup.ChargedWave, 31), new(23, 8), 11),
            new(new(ProjectileSpriteGroup.ChargedWave, 28), new(20, 8), 12),
            new(new(ProjectileSpriteGroup.ChargedWave, 29), new(20, 8), 13),
            new(new(ProjectileSpriteGroup.ChargedWave, 27), new(16, 8), 14),
            new(new(ProjectileSpriteGroup.ChargedWave, 26), new(16, 8), 15),
        ],
        [P.ChargedWaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedWave, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedWave, 1), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.ChargedWave, 18), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.ChargedWave, 19), new(12, 12), 3),
            new(new(ProjectileSpriteGroup.ChargedWave, 20), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.ChargedWave, 21), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.ChargedWave, 22), new(17, 17), 6),
            new(new(ProjectileSpriteGroup.ChargedWave, 23), new(17, 17), 7),
            new(new(ProjectileSpriteGroup.ChargedWave, 24), new(18, 18), 8),
            new(new(ProjectileSpriteGroup.ChargedWave, 25), new(18, 18), 9),
            new(new(ProjectileSpriteGroup.ChargedWave, 22), new(17, 17), 10),
            new(new(ProjectileSpriteGroup.ChargedWave, 23), new(17, 17), 11),
            new(new(ProjectileSpriteGroup.ChargedWave, 20), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedWave, 21), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedWave, 18), new(12, 12), 14),
            new(new(ProjectileSpriteGroup.ChargedWave, 19), new(12, 12), 15),
        ],
        [P.ChargedWaveRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedWave, 0), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.ChargedWave, 1), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.ChargedWave, 2), new(8, 16), 2),
            new(new(ProjectileSpriteGroup.ChargedWave, 3), new(8, 16), 3),
            new(new(ProjectileSpriteGroup.ChargedWave, 4), new(8, 20), 4),
            new(new(ProjectileSpriteGroup.ChargedWave, 5), new(8, 20), 5),
            new(new(ProjectileSpriteGroup.ChargedWave, 6), new(8, 23), 6),
            new(new(ProjectileSpriteGroup.ChargedWave, 7), new(8, 23), 7),
            new(new(ProjectileSpriteGroup.ChargedWave, 8), new(8, 24), 8),
            new(new(ProjectileSpriteGroup.ChargedWave, 9), new(8, 24), 9),
            new(new(ProjectileSpriteGroup.ChargedWave, 6), new(8, 23), 10),
            new(new(ProjectileSpriteGroup.ChargedWave, 7), new(8, 23), 11),
            new(new(ProjectileSpriteGroup.ChargedWave, 4), new(8, 20), 12),
            new(new(ProjectileSpriteGroup.ChargedWave, 5), new(8, 20), 13),
            new(new(ProjectileSpriteGroup.ChargedWave, 3), new(8, 16), 14),
            new(new(ProjectileSpriteGroup.ChargedWave, 2), new(8, 16), 15),
        ],
        [P.ChargedWaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedWave, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedWave, 1), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.ChargedWave, 10), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.ChargedWave, 11), new(12, 12), 3),
            new(new(ProjectileSpriteGroup.ChargedWave, 12), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.ChargedWave, 13), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.ChargedWave, 14), new(17, 17), 6),
            new(new(ProjectileSpriteGroup.ChargedWave, 15), new(17, 17), 7),
            new(new(ProjectileSpriteGroup.ChargedWave, 16), new(18, 18), 8),
            new(new(ProjectileSpriteGroup.ChargedWave, 17), new(18, 18), 9),
            new(new(ProjectileSpriteGroup.ChargedWave, 14), new(17, 17), 10),
            new(new(ProjectileSpriteGroup.ChargedWave, 15), new(17, 17), 11),
            new(new(ProjectileSpriteGroup.ChargedWave, 12), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedWave, 13), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedWave, 10), new(12, 12), 14),
            new(new(ProjectileSpriteGroup.ChargedWave, 11), new(12, 12), 15),
        ],
        [P.ChargedIceUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedIce, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedIce, 1), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.ChargedIce, 2), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.ChargedIce, 3), new(8, 8), 3),
        ],
        [P.ChargedIceWaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Nothing, 0), new(12, 8), 0),
        ],
        [P.ChargedIceWaveDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedIceWave, 0), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 1), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 26), new(16, 8), 2),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 27), new(16, 8), 3),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 28), new(20, 8), 4),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 29), new(20, 8), 5),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 30), new(23, 8), 6),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 31), new(23, 8), 7),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 32), new(24, 8), 8),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 33), new(24, 8), 9),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 30), new(23, 8), 10),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 31), new(23, 8), 11),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 28), new(20, 8), 12),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 29), new(20, 8), 13),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 27), new(16, 8), 14),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 26), new(16, 8), 15),
        ],
        [P.ChargedIceWaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedIceWave, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 1), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 18), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 19), new(12, 12), 3),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 20), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 21), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 22), new(17, 17), 6),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 23), new(17, 17), 7),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 24), new(18, 18), 8),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 25), new(18, 18), 9),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 22), new(17, 17), 10),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 23), new(17, 17), 11),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 20), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 21), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 18), new(12, 12), 14),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 19), new(12, 12), 15),
        ],
        [P.ChargedIceWaveRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedIceWave, 0), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 1), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 2), new(8, 16), 2),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 3), new(8, 16), 3),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 4), new(8, 20), 4),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 5), new(8, 20), 5),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 6), new(8, 23), 6),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 7), new(8, 23), 7),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 8), new(8, 24), 8),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 9), new(8, 24), 9),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 6), new(8, 23), 10),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 7), new(8, 23), 11),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 4), new(8, 20), 12),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 5), new(8, 20), 13),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 3), new(8, 16), 14),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 2), new(8, 16), 15),
        ],
        [P.ChargedIceWaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.ChargedIceWave, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 1), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 10), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 11), new(12, 12), 3),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 12), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 13), new(16, 16), 5),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 14), new(17, 17), 6),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 15), new(17, 17), 7),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 16), new(18, 18), 8),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 17), new(18, 18), 9),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 14), new(17, 17), 10),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 15), new(17, 17), 11),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 12), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 13), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 10), new(12, 12), 14),
            new(new(ProjectileSpriteGroup.ChargedIceWave, 11), new(12, 12), 15),
        ],
        [P.ChargedSpazerUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 4), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 18), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 5), new(12, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 19), new(12, 8), 3),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 60), new(12, 16), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 6), new(12, 16), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 62), new(12, 16), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 8), new(12, 16), 7),
        ],
        [P.ChargedSpazerUpFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 65), new(20, 16), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 11), new(20, 16), 9),
        ],
        [P.ChargedSpazerUpRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 6), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 20), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 7), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 21), new(8, 8), 3),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 24), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 18), new(12, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 25), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 20), new(16, 16), 7),
        ],
        [P.ChargedSpazerUpRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 28), new(20, 20), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 23), new(20, 20), 9),
        ],
        [P.ChargedSpazerRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 8), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 14), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 9), new(8, 12), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 15), new(8, 12), 3),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 54), new(16, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 0), new(16, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 56), new(16, 12), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 2), new(16, 12), 7),
        ],
        [P.ChargedSpazerRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 59), new(16, 20), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 5), new(16, 20), 9),
        ],
        [P.ChargedSpazerDownRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 10), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 16), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 11), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 17), new(8, 8), 3),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 30), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 12), new(12, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 31), new(16, 16), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 14), new(16, 16), 7),
        ],
        [P.ChargedSpazerDownRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 34), new(20, 20), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 17), new(20, 20), 9),
        ],
        [P.ChargedSpazerWaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 4), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 18), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 5), new(12, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 19), new(12, 8), 3),
        ],
        [P.ChargedSpazerWaveUpFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 60), new(12, 16), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 6), new(12, 16), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 61), new(12, 16), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 7), new(12, 16), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 62), new(12, 16), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 8), new(12, 16), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 63), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 9), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 64), new(18, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 10), new(18, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 65), new(20, 16), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 11), new(20, 16), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 64), new(18, 16), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 10), new(18, 16), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 63), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 9), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 62), new(12, 16), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 8), new(12, 16), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 61), new(8, 16), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 7), new(8, 16), 23),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 6), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 20), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 7), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 21), new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveUpRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 24), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 18), new(12, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 29), new(12, 12), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 19), new(12, 12), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 25), new(16, 16), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 20), new(16, 16), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 26), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 21), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 27), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 22), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 28), new(20, 20), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 23), new(20, 20), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 27), new(16, 16), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 22), new(16, 16), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 26), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 21), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 25), new(16, 16), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 20), new(16, 16), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 29), new(12, 12), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 19), new(12, 12), 23),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 8), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 14), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 9), new(8, 12), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 15), new(8, 12), 3),
        ],
        [P.ChargedSpazerWaveRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 66), new(16, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 0), new(16, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 67), new(16, 12), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 1), new(16, 12), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 68), new(16, 12), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 2), new(16, 12), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 69), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 3), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 70), new(16, 18), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 4), new(16, 18), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 71), new(16, 20), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 5), new(16, 20), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 70), new(16, 18), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 4), new(16, 18), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 69), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 3), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 68), new(16, 12), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 2), new(16, 12), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 67), new(16, 8), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 1), new(16, 8), 23),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 10), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 16), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 11), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 17), new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveDownRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 30), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 12), new(12, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 35), new(12, 12), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 13), new(12, 12), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 31), new(16, 16), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 14), new(16, 16), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 32), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 15), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 33), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 16), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 34), new(20, 20), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 17), new(20, 20), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 33), new(16, 16), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 16), new(16, 16), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 32), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 15), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 31), new(16, 16), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 14), new(16, 16), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 35), new(12, 12), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 13), new(12, 12), 23),
        ],
        [P.ChargedSpazerWaveDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 12), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 18), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 13), new(12, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 19), new(12, 8), 3),
        ],
        [P.ChargedSpazerWaveDownFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 48), new(12, 16), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 6), new(12, 16), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 49), new(12, 16), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 7), new(12, 16), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 50), new(12, 16), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 8), new(12, 16), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 51), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 9), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 52), new(18, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 10), new(18, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 53), new(20, 16), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 11), new(20, 16), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 52), new(18, 16), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 10), new(18, 16), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 51), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 9), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 50), new(12, 16), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 8), new(12, 16), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 49), new(8, 16), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 7), new(8, 16), 23),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 6), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 20), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 7), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 21), new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveDownLeftLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 36), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 18), new(12, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 41), new(12, 12), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 19), new(12, 12), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 37), new(16, 16), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 20), new(16, 16), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 38), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 21), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 39), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 22), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 40), new(20, 20), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 23), new(20, 20), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 39), new(16, 16), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 22), new(16, 16), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 38), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 21), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 37), new(16, 16), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 20), new(16, 16), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 41), new(12, 12), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 19), new(12, 12), 23),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 0), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 14), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 1), new(8, 12), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 15), new(8, 12), 3),
        ],
        [P.ChargedSpazerWaveLeftLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 54), new(16, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 0), new(16, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 55), new(16, 12), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 1), new(16, 12), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 56), new(16, 12), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 2), new(16, 12), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 57), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 3), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 58), new(16, 18), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 4), new(16, 18), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 59), new(16, 20), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 5), new(16, 20), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 58), new(16, 18), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 4), new(16, 18), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 57), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 3), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 56), new(16, 12), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 2), new(16, 12), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 55), new(16, 8), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 1), new(16, 8), 23),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            new(new(ProjectileSpriteGroup.SpazerStartup, 2), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.SpazerStartup, 16), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.SpazerStartup, 3), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.SpazerStartup, 17), new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveUpLeftLoop] =
        [
            new(new(ProjectileSpriteGroup.ChargedSpazer, 42), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 12), new(12, 12), 5),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 47), new(12, 12), 6),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 13), new(12, 12), 7),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 43), new(16, 16), 8),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 14), new(16, 16), 9),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 44), new(16, 16), 10),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 15), new(16, 16), 11),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 45), new(16, 16), 12),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 16), new(16, 16), 13),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 46), new(20, 20), 14),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 17), new(20, 20), 15),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 45), new(16, 16), 16),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 16), new(16, 16), 17),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 44), new(16, 16), 18),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 15), new(16, 16), 19),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 43), new(16, 16), 20),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 14), new(16, 16), 21),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 47), new(12, 12), 22),
            new(new(ProjectileSpriteGroup.ChargedSpazer, 13), new(12, 12), 23),
        ],
        [P.ChargedPlasmaUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 4), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 20), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 5), new(8, 16), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 21), new(8, 16), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 6), new(8, 24), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 22), new(8, 24), 5),
        ],
        [P.ChargedPlasmaUpFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 7), new(8, 28), 6),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 23), new(8, 28), 7),
        ],
        [P.ChargedPlasmaUpRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 12), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 28), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 13), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 29), new(12, 12), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 14), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 30), new(16, 16), 5),
        ],
        [P.ChargedPlasmaUpRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 15), new(20, 20), 6),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 31), new(20, 20), 7),
        ],
        [P.ChargedPlasmaRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 16), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 1), new(16, 8), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 17), new(16, 8), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 2), new(24, 8), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 18), new(24, 8), 5),
        ],
        [P.ChargedPlasmaRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 3), new(28, 8), 6),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 19), new(28, 8), 7),
        ],
        [P.ChargedPlasmaDownRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 8), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 24), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 9), new(12, 12), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 25), new(12, 12), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 10), new(16, 16), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 26), new(16, 16), 5),
        ],
        [P.ChargedPlasmaDownRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 11), new(20, 20), 6),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 27), new(20, 20), 7),
        ],
        [P.ChargedPlasmaWaveUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 4), new(12, 8), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 20), new(12, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 5), new(12, 8), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 21), new(12, 8), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 6), new(12, 24), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 22), new(12, 24), 5),
        ],
        [P.ChargedPlasmaWaveUpFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 25), new(12, 30), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 40), new(12, 30), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 26), new(12, 30), 8),
            new(new(ProjectileSpriteGroup.PlasmaWave, 41), new(12, 30), 9),
            new(new(ProjectileSpriteGroup.PlasmaWave, 27), new(17, 30), 10),
            new(new(ProjectileSpriteGroup.PlasmaWave, 42), new(17, 30), 11),
            new(new(ProjectileSpriteGroup.PlasmaWave, 28), new(19, 30), 12),
            new(new(ProjectileSpriteGroup.PlasmaWave, 43), new(19, 30), 13),
            new(new(ProjectileSpriteGroup.PlasmaWave, 29), new(20, 30), 14),
            new(new(ProjectileSpriteGroup.PlasmaWave, 44), new(20, 30), 15),
            new(new(ProjectileSpriteGroup.PlasmaWave, 28), new(19, 30), 16),
            new(new(ProjectileSpriteGroup.PlasmaWave, 43), new(19, 30), 17),
            new(new(ProjectileSpriteGroup.PlasmaWave, 27), new(17, 30), 18),
            new(new(ProjectileSpriteGroup.PlasmaWave, 42), new(17, 30), 19),
            new(new(ProjectileSpriteGroup.PlasmaWave, 26), new(12, 30), 20),
            new(new(ProjectileSpriteGroup.PlasmaWave, 41), new(12, 30), 21),
        ],
        [P.ChargedPlasmaWaveUpRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 12), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 28), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 13), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 29), new(8, 8), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 14), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 30), new(12, 12), 5),
        ],
        [P.ChargedPlasmaWaveUpRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 55), new(12, 12), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 45), new(12, 12), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 56), new(16, 16), 8),
            new(new(ProjectileSpriteGroup.PlasmaWave, 46), new(16, 16), 9),
            new(new(ProjectileSpriteGroup.PlasmaWave, 57), new(17, 17), 10),
            new(new(ProjectileSpriteGroup.PlasmaWave, 47), new(17, 17), 11),
            new(new(ProjectileSpriteGroup.PlasmaWave, 58), new(20, 20), 12),
            new(new(ProjectileSpriteGroup.PlasmaWave, 48), new(20, 20), 13),
            new(new(ProjectileSpriteGroup.PlasmaWave, 59), new(24, 24), 14),
            new(new(ProjectileSpriteGroup.PlasmaWave, 49), new(24, 24), 15),
            new(new(ProjectileSpriteGroup.PlasmaWave, 58), new(20, 20), 16),
            new(new(ProjectileSpriteGroup.PlasmaWave, 48), new(20, 20), 17),
            new(new(ProjectileSpriteGroup.PlasmaWave, 57), new(17, 17), 18),
            new(new(ProjectileSpriteGroup.PlasmaWave, 47), new(17, 17), 19),
            new(new(ProjectileSpriteGroup.PlasmaWave, 56), new(16, 16), 20),
            new(new(ProjectileSpriteGroup.PlasmaWave, 46), new(16, 16), 21),
        ],
        [P.ChargedPlasmaWaveRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 0), new(8, 12), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 16), new(8, 12), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 1), new(8, 12), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 17), new(8, 12), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 2), new(24, 12), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 18), new(24, 12), 5),
        ],
        [P.ChargedPlasmaWaveRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 5), new(28, 12), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 30), new(28, 12), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 6), new(28, 12), 8),
            new(new(ProjectileSpriteGroup.PlasmaWave, 31), new(28, 12), 9),
            new(new(ProjectileSpriteGroup.PlasmaWave, 7), new(28, 17), 10),
            new(new(ProjectileSpriteGroup.PlasmaWave, 32), new(28, 17), 11),
            new(new(ProjectileSpriteGroup.PlasmaWave, 8), new(28, 19), 12),
            new(new(ProjectileSpriteGroup.PlasmaWave, 33), new(28, 19), 13),
            new(new(ProjectileSpriteGroup.PlasmaWave, 9), new(28, 20), 14),
            new(new(ProjectileSpriteGroup.PlasmaWave, 34), new(28, 20), 15),
            new(new(ProjectileSpriteGroup.PlasmaWave, 8), new(28, 19), 16),
            new(new(ProjectileSpriteGroup.PlasmaWave, 33), new(28, 19), 17),
            new(new(ProjectileSpriteGroup.PlasmaWave, 7), new(28, 17), 18),
            new(new(ProjectileSpriteGroup.PlasmaWave, 32), new(28, 17), 19),
            new(new(ProjectileSpriteGroup.PlasmaWave, 6), new(28, 12), 20),
            new(new(ProjectileSpriteGroup.PlasmaWave, 31), new(28, 12), 21),
        ],
        [P.ChargedPlasmaWaveDownRight] =
        [
            new(new(ProjectileSpriteGroup.PlasmaStartup, 8), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 24), new(8, 8), 1),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 9), new(8, 8), 2),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 25), new(8, 8), 3),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 10), new(12, 12), 4),
            new(new(ProjectileSpriteGroup.PlasmaStartup, 26), new(12, 12), 5),
        ],
        [P.ChargedPlasmaWaveDownRightLoop] =
        [
            new(new(ProjectileSpriteGroup.PlasmaWave, 15), new(12, 12), 6),
            new(new(ProjectileSpriteGroup.PlasmaWave, 35), new(12, 12), 7),
            new(new(ProjectileSpriteGroup.PlasmaWave, 16), new(16, 16), 8),
            new(new(ProjectileSpriteGroup.PlasmaWave, 36), new(16, 16), 9),
            new(new(ProjectileSpriteGroup.PlasmaWave, 17), new(17, 17), 10),
            new(new(ProjectileSpriteGroup.PlasmaWave, 37), new(17, 17), 11),
            new(new(ProjectileSpriteGroup.PlasmaWave, 18), new(20, 20), 12),
            new(new(ProjectileSpriteGroup.PlasmaWave, 38), new(20, 20), 13),
            new(new(ProjectileSpriteGroup.PlasmaWave, 19), new(24, 24), 14),
            new(new(ProjectileSpriteGroup.PlasmaWave, 39), new(24, 24), 15),
            new(new(ProjectileSpriteGroup.PlasmaWave, 18), new(20, 20), 16),
            new(new(ProjectileSpriteGroup.PlasmaWave, 38), new(20, 20), 17),
            new(new(ProjectileSpriteGroup.PlasmaWave, 17), new(17, 17), 18),
            new(new(ProjectileSpriteGroup.PlasmaWave, 37), new(17, 17), 19),
            new(new(ProjectileSpriteGroup.PlasmaWave, 16), new(16, 16), 20),
            new(new(ProjectileSpriteGroup.PlasmaWave, 36), new(16, 16), 21),
        ],
        [P.MissileUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Missile, 2), new(4, 4), 0),
        ],
        [P.MissileUpRight] =
        [
            new(new(ProjectileSpriteGroup.Missile, 3), new(4, 4), 0),
        ],
        [P.MissileRight] =
        [
            new(new(ProjectileSpriteGroup.Missile, 4), new(4, 4), 0),
        ],
        [P.MissileDownRight] =
        [
            new(new(ProjectileSpriteGroup.Missile, 5), new(4, 4), 0),
        ],
        [P.MissileDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Missile, 6), new(4, 4), 0),
        ],
        [P.MissileDownLeft] =
        [
            new(new(ProjectileSpriteGroup.Missile, 7), new(4, 4), 0),
        ],
        [P.MissileLeft] =
        [
            new(new(ProjectileSpriteGroup.Missile, 0), new(4, 4), 0),
        ],
        [P.MissileUpLeft] =
        [
            new(new(ProjectileSpriteGroup.Missile, 1), new(4, 4), 0),
        ],
        [P.SuperMissileUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 2), new(8, 8), 0),
        ],
        [P.SuperMissileUpRight] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 3), new(8, 8), 0),
        ],
        [P.SuperMissileRight] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 4), new(8, 8), 0),
        ],
        [P.SuperMissileDownRight] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 5), new(8, 8), 0),
        ],
        [P.SuperMissileDownFacingRight] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 6), new(8, 8), 0),
        ],
        [P.SuperMissileDownLeft] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 7), new(8, 8), 0),
        ],
        [P.SuperMissileLeft] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 0), new(8, 8), 0),
        ],
        [P.SuperMissileUpLeft] =
        [
            new(new(ProjectileSpriteGroup.SuperMissile, 1), new(8, 8), 0),
        ],
        [P.SuperMissileLink] =
        [
            new(new(ProjectileSpriteGroup.Nothing, 0), new(8, 8), 0),
        ],
        [P.PowerBomb] =
        [
            new(new(ProjectileSpriteGroup.PowerBomb, 0), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.PowerBomb, 1), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.PowerBomb, 2), new(4, 4), 0),
        ],
        [P.FastPowerBomb] =
        [
            new(new(ProjectileSpriteGroup.PowerBomb, 0), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.PowerBomb, 1), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.PowerBomb, 2), new(4, 4), 0),
        ],
        [P.Bomb] =
        [
            new(new(ProjectileSpriteGroup.Bomb, 0), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.Bomb, 1), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.Bomb, 2), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.Bomb, 3), new(4, 4), 0),
        ],
        [P.FastBomb] =
        [
            new(new(ProjectileSpriteGroup.Bomb, 0), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.Bomb, 1), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.Bomb, 2), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.Bomb, 3), new(4, 4), 0),
        ],
        [P.BeamExplosion] =
        [
            new(new(ProjectileSpriteGroup.BeamExplosion, 0), new(0, 0), 0),
            new(new(ProjectileSpriteGroup.BeamExplosion, 1), new(0, 0), 0),
            new(new(ProjectileSpriteGroup.BeamExplosion, 2), new(0, 0), 0),
            new(new(ProjectileSpriteGroup.BeamExplosion, 3), new(0, 0), 0),
            new(new(ProjectileSpriteGroup.BeamExplosion, 4), new(0, 0), 0),
            new(new(ProjectileSpriteGroup.BeamExplosion, 5), new(0, 0), 0),
        ],
        [P.MissileExplosion] =
        [
            new(new(ProjectileSpriteGroup.MissileExplosion, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.MissileExplosion, 1), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.MissileExplosion, 2), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.MissileExplosion, 3), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.MissileExplosion, 4), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.MissileExplosion, 5), new(8, 8), 0),
        ],
        [P.BombExplosion] =
        [
            new(new(ProjectileSpriteGroup.BombExplosion, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 1), new(12, 12), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 2), new(16, 16), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 3), new(16, 16), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 4), new(16, 16), 0),
        ],
        [P.PlasmaSBA] =
        [
            new(new(ProjectileSpriteGroup.BombExplosion, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 1), new(12, 12), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 2), new(16, 16), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 3), new(16, 16), 0),
            new(new(ProjectileSpriteGroup.BombExplosion, 4), new(16, 16), 0),
        ],
        [P.SuperMissileExplosion] =
        [
            new(new(ProjectileSpriteGroup.SuperMissileExplosion, 0), new(8, 8), 0),
            new(new(ProjectileSpriteGroup.SuperMissileExplosion, 1), new(12, 12), 0),
            new(new(ProjectileSpriteGroup.SuperMissileExplosion, 2), new(16, 16), 0),
            new(new(ProjectileSpriteGroup.SuperMissileExplosion, 3), new(16, 16), 0),
            new(new(ProjectileSpriteGroup.SuperMissileExplosion, 4), new(16, 16), 0),
            new(new(ProjectileSpriteGroup.SuperMissileExplosion, 5), new(16, 16), 0),
        ],
        [P.ShinesparkEchoUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Nothing, 0), new(32, 32), 0),
            new(new(ProjectileSpriteGroup.Nothing, 0), new(32, 32), 1),
            new(new(ProjectileSpriteGroup.Nothing, 0), new(32, 32), 2),
            new(new(ProjectileSpriteGroup.Nothing, 0), new(32, 32), 3),
        ],
        [P.SpazerSBATrailUpFacingRight] =
        [
            new(new(ProjectileSpriteGroup.Nothing, 0), new(4, 8), 0),
            new(new(ProjectileSpriteGroup.Nothing, 0), new(12, 8), 1),
        ],
        [P.SpazerSBATrailUpFacingRightLoop] =
        [
            new(new(ProjectileSpriteGroup.Nothing, 0), new(20, 8), 2),
        ],
        [P.WaveSBA] =
        [
            new(new(ProjectileSpriteGroup.ChargedWave, 0), new(4, 4), 0),
            new(new(ProjectileSpriteGroup.ChargedWave, 1), new(4, 4), 1),
        ],
    };

    /// <summary>The record <paramref name="frame"/> shows.</summary>
    internal static SamusProjectileRecord Get(SamusProjectileFrame frame) => ByProgram[frame.Program][frame.Show];
}
