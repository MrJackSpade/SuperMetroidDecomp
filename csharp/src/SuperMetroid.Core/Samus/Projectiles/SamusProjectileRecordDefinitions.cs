using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' timed records (#1301, #1517), by the program that shows them, in the order it shows them: each
/// record's hitbox radii and the animation frame trails read. What a record pictures is installed only (#1596). A record
/// is named by its program and its place there (<see cref="SamusProjectileFrame"/>), the identity installed frame
/// bindings key on, so a program added or removed leaves every other record's identity alone.
/// </summary>
internal static class SamusProjectileRecordDefinitions
{
    /// <summary>Every program's timed records, in the order it shows them.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileSegment, SamusProjectileRecord[]> ByAnimation =
        new Dictionary<SamusProjectileSegment, SamusProjectileRecord[]>
    {
        [P.PowerUpFacingRight] =
        [
            new(new(4, 4), 0),
        ],
        [P.PowerUpRight] =
        [
            new(new(8, 4), 0),
        ],
        [P.PowerRight] =
        [
            new(new(8, 4), 0),
        ],
        [P.PowerDownRight] =
        [
            new(new(8, 4), 0),
        ],
        [P.PowerDownFacingRight] =
        [
            new(new(4, 4), 0),
        ],
        [P.PowerDownLeft] =
        [
            new(new(8, 4), 0),
        ],
        [P.PowerLeft] =
        [
            new(new(8, 4), 0),
        ],
        [P.PowerUpLeft] =
        [
            new(new(8, 4), 0),
        ],
        [P.WaveUpFacingRight] =
        [
            new(new(12, 4), 0),
        ],
        [P.WaveDownFacingRight] =
        [
            new(new(12, 4), 0),
            new(new(12, 4), 1),
            new(new(12, 4), 2),
            new(new(16, 4), 3),
            new(new(20, 4), 4),
            new(new(16, 4), 5),
            new(new(12, 4), 6),
            new(new(12, 4), 7),
            new(new(12, 4), 8),
            new(new(12, 4), 9),
            new(new(12, 4), 10),
            new(new(16, 4), 11),
            new(new(20, 4), 12),
            new(new(16, 4), 13),
            new(new(12, 4), 14),
            new(new(12, 4), 15),
        ],
        [P.WaveUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(10, 10), 3),
            new(new(12, 12), 4),
            new(new(10, 10), 5),
            new(new(8, 8), 6),
            new(new(6, 6), 7),
            new(new(4, 4), 8),
            new(new(6, 6), 9),
            new(new(8, 8), 10),
            new(new(10, 10), 11),
            new(new(12, 12), 12),
            new(new(10, 10), 13),
            new(new(8, 8), 14),
            new(new(8, 8), 15),
        ],
        [P.WaveRight] =
        [
            new(new(4, 12), 0),
            new(new(4, 12), 1),
            new(new(4, 12), 2),
            new(new(4, 16), 3),
            new(new(4, 20), 4),
            new(new(4, 16), 5),
            new(new(4, 12), 6),
            new(new(4, 12), 7),
            new(new(4, 12), 8),
            new(new(4, 12), 9),
            new(new(4, 12), 10),
            new(new(4, 16), 11),
            new(new(4, 20), 12),
            new(new(4, 16), 13),
            new(new(4, 12), 14),
            new(new(4, 12), 15),
        ],
        [P.WaveDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(10, 10), 3),
            new(new(12, 12), 4),
            new(new(10, 10), 5),
            new(new(8, 8), 6),
            new(new(6, 6), 7),
            new(new(4, 4), 8),
            new(new(6, 6), 9),
            new(new(8, 8), 10),
            new(new(10, 10), 11),
            new(new(12, 12), 12),
            new(new(10, 10), 13),
            new(new(8, 8), 14),
            new(new(8, 8), 15),
        ],
        [P.IceUpFacingRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
        ],
        [P.SpazerUpFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
        ],
        [P.SpazerUpFacingRightLoop] =
        [
            new(new(20, 8), 2),
        ],
        [P.SpazerUpRight] =
        [
            new(new(8, 8), 0),
            new(new(12, 12), 1),
        ],
        [P.SpazerUpRightLoop] =
        [
            new(new(16, 16), 2),
        ],
        [P.SpazerRight] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
        ],
        [P.SpazerRightLoop] =
        [
            new(new(8, 20), 2),
        ],
        [P.SpazerDownRight] =
        [
            new(new(8, 8), 0),
            new(new(12, 12), 1),
        ],
        [P.SpazerDownRightLoop] =
        [
            new(new(16, 16), 2),
        ],
        [P.SpazerDownFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
        ],
        [P.SpazerDownFacingRightLoop] =
        [
            new(new(20, 8), 2),
        ],
        [P.SpazerDownLeft] =
        [
            new(new(8, 8), 0),
            new(new(12, 12), 1),
        ],
        [P.SpazerDownLeftLoop] =
        [
            new(new(16, 16), 2),
        ],
        [P.SpazerLeft] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
        ],
        [P.SpazerLeftLoop] =
        [
            new(new(8, 20), 2),
        ],
        [P.SpazerUpLeft] =
        [
            new(new(8, 8), 0),
            new(new(12, 12), 1),
        ],
        [P.SpazerUpLeftLoop] =
        [
            new(new(16, 16), 2),
        ],
        [P.SpazerWaveUpFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(12, 8), 2),
            new(new(17, 8), 3),
            new(new(19, 8), 4),
            new(new(20, 8), 5),
            new(new(19, 8), 6),
            new(new(17, 8), 7),
            new(new(12, 8), 8),
            new(new(12, 8), 9),
        ],
        [P.SpazerWaveUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(16, 16), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
            new(new(12, 12), 8),
            new(new(8, 8), 9),
        ],
        [P.SpazerWaveRight] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 12), 2),
            new(new(8, 17), 3),
            new(new(8, 19), 4),
            new(new(8, 20), 5),
            new(new(8, 19), 6),
            new(new(8, 17), 7),
            new(new(8, 12), 8),
            new(new(8, 12), 9),
        ],
        [P.SpazerWaveDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(16, 16), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
            new(new(12, 12), 8),
            new(new(8, 8), 9),
        ],
        [P.SpazerWaveDownFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(12, 8), 2),
            new(new(17, 8), 3),
            new(new(19, 8), 4),
            new(new(20, 8), 5),
            new(new(19, 8), 6),
            new(new(17, 8), 7),
            new(new(12, 8), 8),
            new(new(12, 8), 9),
        ],
        [P.SpazerWaveDownLeft] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(16, 16), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
            new(new(12, 12), 8),
            new(new(8, 8), 9),
        ],
        [P.SpazerWaveLeft] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 12), 2),
            new(new(8, 17), 3),
            new(new(8, 19), 4),
            new(new(8, 20), 5),
            new(new(8, 19), 6),
            new(new(8, 17), 7),
            new(new(8, 12), 8),
            new(new(8, 12), 9),
        ],
        [P.SpazerWaveUpLeft] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(16, 16), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
            new(new(12, 12), 8),
            new(new(8, 8), 9),
        ],
        [P.PlasmaUpFacingRight] =
        [
            new(new(8, 16), 0),
        ],
        [P.PlasmaUpFacingRightLoop] =
        [
            new(new(8, 16), 1),
        ],
        [P.PlasmaUpRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.PlasmaUpRightLoop] =
        [
            new(new(8, 8), 1),
        ],
        [P.PlasmaRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.PlasmaRightLoop] =
        [
            new(new(16, 8), 1),
        ],
        [P.PlasmaDownRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.PlasmaDownRightLoop] =
        [
            new(new(8, 8), 1),
        ],
        [P.PlasmaIceWaveUpFacingRight] =
        [
            new(new(12, 16), 0),
        ],
        [P.PlasmaWaveUpFacingRight] =
        [
            new(new(12, 16), 1),
            new(new(12, 16), 2),
            new(new(17, 16), 3),
            new(new(19, 16), 4),
            new(new(20, 16), 5),
            new(new(19, 16), 6),
            new(new(17, 16), 7),
            new(new(12, 16), 8),
        ],
        [P.PlasmaIceWaveUpRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.PlasmaWaveUpRight] =
        [
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(16, 16), 3),
            new(new(16, 16), 4),
            new(new(20, 20), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
            new(new(12, 12), 8),
        ],
        [P.PlasmaWaveRight] =
        [
            new(new(8, 12), 0),
        ],
        [P.PlasmaWaveRightLoop] =
        [
            new(new(16, 12), 1),
            new(new(16, 12), 2),
            new(new(16, 17), 3),
            new(new(16, 19), 4),
            new(new(16, 20), 5),
            new(new(16, 19), 6),
            new(new(16, 17), 7),
            new(new(16, 12), 8),
        ],
        [P.PlasmaIceWaveDownRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.PlasmaWaveDownRight] =
        [
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(16, 16), 3),
            new(new(16, 16), 4),
            new(new(20, 20), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
            new(new(12, 12), 8),
        ],
        [P.ChargedPowerUpFacingRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedPowerUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedPowerRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedPowerDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedPowerDownFacingRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedPowerDownLeft] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedPowerLeft] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedPowerUpLeft] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
        ],
        [P.ChargedWaveUpFacingRight] =
        [
            new(new(12, 8), 0),
        ],
        [P.ChargedWaveDownFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(16, 8), 2),
            new(new(16, 8), 3),
            new(new(20, 8), 4),
            new(new(20, 8), 5),
            new(new(23, 8), 6),
            new(new(23, 8), 7),
            new(new(24, 8), 8),
            new(new(24, 8), 9),
            new(new(23, 8), 10),
            new(new(23, 8), 11),
            new(new(20, 8), 12),
            new(new(20, 8), 13),
            new(new(16, 8), 14),
            new(new(16, 8), 15),
        ],
        [P.ChargedWaveUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(12, 12), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(17, 17), 6),
            new(new(17, 17), 7),
            new(new(18, 18), 8),
            new(new(18, 18), 9),
            new(new(17, 17), 10),
            new(new(17, 17), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(12, 12), 14),
            new(new(12, 12), 15),
        ],
        [P.ChargedWaveRight] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 16), 2),
            new(new(8, 16), 3),
            new(new(8, 20), 4),
            new(new(8, 20), 5),
            new(new(8, 23), 6),
            new(new(8, 23), 7),
            new(new(8, 24), 8),
            new(new(8, 24), 9),
            new(new(8, 23), 10),
            new(new(8, 23), 11),
            new(new(8, 20), 12),
            new(new(8, 20), 13),
            new(new(8, 16), 14),
            new(new(8, 16), 15),
        ],
        [P.ChargedWaveDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(12, 12), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(17, 17), 6),
            new(new(17, 17), 7),
            new(new(18, 18), 8),
            new(new(18, 18), 9),
            new(new(17, 17), 10),
            new(new(17, 17), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(12, 12), 14),
            new(new(12, 12), 15),
        ],
        [P.ChargedIceUpFacingRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
        ],
        [P.ChargedIceWaveUpFacingRight] =
        [
            new(new(12, 8), 0),
        ],
        [P.ChargedIceWaveDownFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(16, 8), 2),
            new(new(16, 8), 3),
            new(new(20, 8), 4),
            new(new(20, 8), 5),
            new(new(23, 8), 6),
            new(new(23, 8), 7),
            new(new(24, 8), 8),
            new(new(24, 8), 9),
            new(new(23, 8), 10),
            new(new(23, 8), 11),
            new(new(20, 8), 12),
            new(new(20, 8), 13),
            new(new(16, 8), 14),
            new(new(16, 8), 15),
        ],
        [P.ChargedIceWaveUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(12, 12), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(17, 17), 6),
            new(new(17, 17), 7),
            new(new(18, 18), 8),
            new(new(18, 18), 9),
            new(new(17, 17), 10),
            new(new(17, 17), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(12, 12), 14),
            new(new(12, 12), 15),
        ],
        [P.ChargedIceWaveRight] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 16), 2),
            new(new(8, 16), 3),
            new(new(8, 20), 4),
            new(new(8, 20), 5),
            new(new(8, 23), 6),
            new(new(8, 23), 7),
            new(new(8, 24), 8),
            new(new(8, 24), 9),
            new(new(8, 23), 10),
            new(new(8, 23), 11),
            new(new(8, 20), 12),
            new(new(8, 20), 13),
            new(new(8, 16), 14),
            new(new(8, 16), 15),
        ],
        [P.ChargedIceWaveDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(12, 12), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
            new(new(17, 17), 6),
            new(new(17, 17), 7),
            new(new(18, 18), 8),
            new(new(18, 18), 9),
            new(new(17, 17), 10),
            new(new(17, 17), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(12, 12), 14),
            new(new(12, 12), 15),
        ],
        [P.ChargedSpazerUpFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(12, 8), 2),
            new(new(12, 8), 3),
            new(new(12, 16), 4),
            new(new(12, 16), 5),
            new(new(12, 16), 6),
            new(new(12, 16), 7),
        ],
        [P.ChargedSpazerUpFacingRightLoop] =
        [
            new(new(20, 16), 8),
            new(new(20, 16), 9),
        ],
        [P.ChargedSpazerUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
            new(new(12, 12), 4),
            new(new(12, 12), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
        ],
        [P.ChargedSpazerUpRightLoop] =
        [
            new(new(20, 20), 8),
            new(new(20, 20), 9),
        ],
        [P.ChargedSpazerRight] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 12), 2),
            new(new(8, 12), 3),
            new(new(16, 12), 4),
            new(new(16, 12), 5),
            new(new(16, 12), 6),
            new(new(16, 12), 7),
        ],
        [P.ChargedSpazerRightLoop] =
        [
            new(new(16, 20), 8),
            new(new(16, 20), 9),
        ],
        [P.ChargedSpazerDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
            new(new(12, 12), 4),
            new(new(12, 12), 5),
            new(new(16, 16), 6),
            new(new(16, 16), 7),
        ],
        [P.ChargedSpazerDownRightLoop] =
        [
            new(new(20, 20), 8),
            new(new(20, 20), 9),
        ],
        [P.ChargedSpazerWaveUpFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(12, 8), 2),
            new(new(12, 8), 3),
        ],
        [P.ChargedSpazerWaveUpFacingRightLoop] =
        [
            new(new(12, 16), 4),
            new(new(12, 16), 5),
            new(new(12, 16), 6),
            new(new(12, 16), 7),
            new(new(12, 16), 8),
            new(new(12, 16), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(18, 16), 12),
            new(new(18, 16), 13),
            new(new(20, 16), 14),
            new(new(20, 16), 15),
            new(new(18, 16), 16),
            new(new(18, 16), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(12, 16), 20),
            new(new(12, 16), 21),
            new(new(8, 16), 22),
            new(new(8, 16), 23),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveUpRightLoop] =
        [
            new(new(12, 12), 4),
            new(new(12, 12), 5),
            new(new(12, 12), 6),
            new(new(12, 12), 7),
            new(new(16, 16), 8),
            new(new(16, 16), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(20, 20), 14),
            new(new(20, 20), 15),
            new(new(16, 16), 16),
            new(new(16, 16), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(16, 16), 20),
            new(new(16, 16), 21),
            new(new(12, 12), 22),
            new(new(12, 12), 23),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 12), 2),
            new(new(8, 12), 3),
        ],
        [P.ChargedSpazerWaveRightLoop] =
        [
            new(new(16, 12), 4),
            new(new(16, 12), 5),
            new(new(16, 12), 6),
            new(new(16, 12), 7),
            new(new(16, 12), 8),
            new(new(16, 12), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(16, 18), 12),
            new(new(16, 18), 13),
            new(new(16, 20), 14),
            new(new(16, 20), 15),
            new(new(16, 18), 16),
            new(new(16, 18), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(16, 12), 20),
            new(new(16, 12), 21),
            new(new(16, 8), 22),
            new(new(16, 8), 23),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveDownRightLoop] =
        [
            new(new(12, 12), 4),
            new(new(12, 12), 5),
            new(new(12, 12), 6),
            new(new(12, 12), 7),
            new(new(16, 16), 8),
            new(new(16, 16), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(20, 20), 14),
            new(new(20, 20), 15),
            new(new(16, 16), 16),
            new(new(16, 16), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(16, 16), 20),
            new(new(16, 16), 21),
            new(new(12, 12), 22),
            new(new(12, 12), 23),
        ],
        [P.ChargedSpazerWaveDownFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(12, 8), 2),
            new(new(12, 8), 3),
        ],
        [P.ChargedSpazerWaveDownFacingRightLoop] =
        [
            new(new(12, 16), 4),
            new(new(12, 16), 5),
            new(new(12, 16), 6),
            new(new(12, 16), 7),
            new(new(12, 16), 8),
            new(new(12, 16), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(18, 16), 12),
            new(new(18, 16), 13),
            new(new(20, 16), 14),
            new(new(20, 16), 15),
            new(new(18, 16), 16),
            new(new(18, 16), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(12, 16), 20),
            new(new(12, 16), 21),
            new(new(8, 16), 22),
            new(new(8, 16), 23),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveDownLeftLoop] =
        [
            new(new(12, 12), 4),
            new(new(12, 12), 5),
            new(new(12, 12), 6),
            new(new(12, 12), 7),
            new(new(16, 16), 8),
            new(new(16, 16), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(20, 20), 14),
            new(new(20, 20), 15),
            new(new(16, 16), 16),
            new(new(16, 16), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(16, 16), 20),
            new(new(16, 16), 21),
            new(new(12, 12), 22),
            new(new(12, 12), 23),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 12), 2),
            new(new(8, 12), 3),
        ],
        [P.ChargedSpazerWaveLeftLoop] =
        [
            new(new(16, 12), 4),
            new(new(16, 12), 5),
            new(new(16, 12), 6),
            new(new(16, 12), 7),
            new(new(16, 12), 8),
            new(new(16, 12), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(16, 18), 12),
            new(new(16, 18), 13),
            new(new(16, 20), 14),
            new(new(16, 20), 15),
            new(new(16, 18), 16),
            new(new(16, 18), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(16, 12), 20),
            new(new(16, 12), 21),
            new(new(16, 8), 22),
            new(new(16, 8), 23),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
        ],
        [P.ChargedSpazerWaveUpLeftLoop] =
        [
            new(new(12, 12), 4),
            new(new(12, 12), 5),
            new(new(12, 12), 6),
            new(new(12, 12), 7),
            new(new(16, 16), 8),
            new(new(16, 16), 9),
            new(new(16, 16), 10),
            new(new(16, 16), 11),
            new(new(16, 16), 12),
            new(new(16, 16), 13),
            new(new(20, 20), 14),
            new(new(20, 20), 15),
            new(new(16, 16), 16),
            new(new(16, 16), 17),
            new(new(16, 16), 18),
            new(new(16, 16), 19),
            new(new(16, 16), 20),
            new(new(16, 16), 21),
            new(new(12, 12), 22),
            new(new(12, 12), 23),
        ],
        [P.ChargedPlasmaUpFacingRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 16), 2),
            new(new(8, 16), 3),
            new(new(8, 24), 4),
            new(new(8, 24), 5),
        ],
        [P.ChargedPlasmaUpFacingRightLoop] =
        [
            new(new(8, 28), 6),
            new(new(8, 28), 7),
        ],
        [P.ChargedPlasmaUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(12, 12), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
        ],
        [P.ChargedPlasmaUpRightLoop] =
        [
            new(new(20, 20), 6),
            new(new(20, 20), 7),
        ],
        [P.ChargedPlasmaRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(16, 8), 2),
            new(new(16, 8), 3),
            new(new(24, 8), 4),
            new(new(24, 8), 5),
        ],
        [P.ChargedPlasmaRightLoop] =
        [
            new(new(28, 8), 6),
            new(new(28, 8), 7),
        ],
        [P.ChargedPlasmaDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(12, 12), 2),
            new(new(12, 12), 3),
            new(new(16, 16), 4),
            new(new(16, 16), 5),
        ],
        [P.ChargedPlasmaDownRightLoop] =
        [
            new(new(20, 20), 6),
            new(new(20, 20), 7),
        ],
        [P.ChargedPlasmaWaveUpFacingRight] =
        [
            new(new(12, 8), 0),
            new(new(12, 8), 1),
            new(new(12, 8), 2),
            new(new(12, 8), 3),
            new(new(12, 24), 4),
            new(new(12, 24), 5),
        ],
        [P.ChargedPlasmaWaveUpFacingRightLoop] =
        [
            new(new(12, 30), 6),
            new(new(12, 30), 7),
            new(new(12, 30), 8),
            new(new(12, 30), 9),
            new(new(17, 30), 10),
            new(new(17, 30), 11),
            new(new(19, 30), 12),
            new(new(19, 30), 13),
            new(new(20, 30), 14),
            new(new(20, 30), 15),
            new(new(19, 30), 16),
            new(new(19, 30), 17),
            new(new(17, 30), 18),
            new(new(17, 30), 19),
            new(new(12, 30), 20),
            new(new(12, 30), 21),
        ],
        [P.ChargedPlasmaWaveUpRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
            new(new(12, 12), 4),
            new(new(12, 12), 5),
        ],
        [P.ChargedPlasmaWaveUpRightLoop] =
        [
            new(new(12, 12), 6),
            new(new(12, 12), 7),
            new(new(16, 16), 8),
            new(new(16, 16), 9),
            new(new(17, 17), 10),
            new(new(17, 17), 11),
            new(new(20, 20), 12),
            new(new(20, 20), 13),
            new(new(24, 24), 14),
            new(new(24, 24), 15),
            new(new(20, 20), 16),
            new(new(20, 20), 17),
            new(new(17, 17), 18),
            new(new(17, 17), 19),
            new(new(16, 16), 20),
            new(new(16, 16), 21),
        ],
        [P.ChargedPlasmaWaveRight] =
        [
            new(new(8, 12), 0),
            new(new(8, 12), 1),
            new(new(8, 12), 2),
            new(new(8, 12), 3),
            new(new(24, 12), 4),
            new(new(24, 12), 5),
        ],
        [P.ChargedPlasmaWaveRightLoop] =
        [
            new(new(28, 12), 6),
            new(new(28, 12), 7),
            new(new(28, 12), 8),
            new(new(28, 12), 9),
            new(new(28, 17), 10),
            new(new(28, 17), 11),
            new(new(28, 19), 12),
            new(new(28, 19), 13),
            new(new(28, 20), 14),
            new(new(28, 20), 15),
            new(new(28, 19), 16),
            new(new(28, 19), 17),
            new(new(28, 17), 18),
            new(new(28, 17), 19),
            new(new(28, 12), 20),
            new(new(28, 12), 21),
        ],
        [P.ChargedPlasmaWaveDownRight] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 1),
            new(new(8, 8), 2),
            new(new(8, 8), 3),
            new(new(12, 12), 4),
            new(new(12, 12), 5),
        ],
        [P.ChargedPlasmaWaveDownRightLoop] =
        [
            new(new(12, 12), 6),
            new(new(12, 12), 7),
            new(new(16, 16), 8),
            new(new(16, 16), 9),
            new(new(17, 17), 10),
            new(new(17, 17), 11),
            new(new(20, 20), 12),
            new(new(20, 20), 13),
            new(new(24, 24), 14),
            new(new(24, 24), 15),
            new(new(20, 20), 16),
            new(new(20, 20), 17),
            new(new(17, 17), 18),
            new(new(17, 17), 19),
            new(new(16, 16), 20),
            new(new(16, 16), 21),
        ],
        [P.MissileUpFacingRight] =
        [
            new(new(4, 4), 0),
        ],
        [P.MissileUpRight] =
        [
            new(new(4, 4), 0),
        ],
        [P.MissileRight] =
        [
            new(new(4, 4), 0),
        ],
        [P.MissileDownRight] =
        [
            new(new(4, 4), 0),
        ],
        [P.MissileDownFacingRight] =
        [
            new(new(4, 4), 0),
        ],
        [P.MissileDownLeft] =
        [
            new(new(4, 4), 0),
        ],
        [P.MissileLeft] =
        [
            new(new(4, 4), 0),
        ],
        [P.MissileUpLeft] =
        [
            new(new(4, 4), 0),
        ],
        [P.SuperMissileUpFacingRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileUpRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileDownRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileDownFacingRight] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileDownLeft] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileLeft] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileUpLeft] =
        [
            new(new(8, 8), 0),
        ],
        [P.SuperMissileLink] =
        [
            new(new(8, 8), 0),
        ],
        [P.PowerBomb] =
        [
            new(new(4, 4), 0),
            new(new(4, 4), 0),
            new(new(4, 4), 0),
        ],
        [P.FastPowerBomb] =
        [
            new(new(4, 4), 0),
            new(new(4, 4), 0),
            new(new(4, 4), 0),
        ],
        [P.Bomb] =
        [
            new(new(4, 4), 0),
            new(new(4, 4), 0),
            new(new(4, 4), 0),
            new(new(4, 4), 0),
        ],
        [P.FastBomb] =
        [
            new(new(4, 4), 0),
            new(new(4, 4), 0),
            new(new(4, 4), 0),
            new(new(4, 4), 0),
        ],
        [P.BeamExplosion] =
        [
            new(new(0, 0), 0),
            new(new(0, 0), 0),
            new(new(0, 0), 0),
            new(new(0, 0), 0),
            new(new(0, 0), 0),
            new(new(0, 0), 0),
        ],
        [P.MissileExplosion] =
        [
            new(new(8, 8), 0),
            new(new(8, 8), 0),
            new(new(8, 8), 0),
            new(new(8, 8), 0),
            new(new(8, 8), 0),
            new(new(8, 8), 0),
        ],
        [P.BombExplosion] =
        [
            new(new(8, 8), 0),
            new(new(12, 12), 0),
            new(new(16, 16), 0),
            new(new(16, 16), 0),
            new(new(16, 16), 0),
        ],
        [P.PlasmaSBA] =
        [
            new(new(8, 8), 0),
            new(new(12, 12), 0),
            new(new(16, 16), 0),
            new(new(16, 16), 0),
            new(new(16, 16), 0),
        ],
        [P.SuperMissileExplosion] =
        [
            new(new(8, 8), 0),
            new(new(12, 12), 0),
            new(new(16, 16), 0),
            new(new(16, 16), 0),
            new(new(16, 16), 0),
            new(new(16, 16), 0),
        ],
        [P.ShinesparkEchoUpFacingRight] =
        [
            new(new(32, 32), 0),
            new(new(32, 32), 1),
            new(new(32, 32), 2),
            new(new(32, 32), 3),
        ],
        [P.SpazerSBATrailUpFacingRight] =
        [
            new(new(4, 8), 0),
            new(new(12, 8), 1),
        ],
        [P.SpazerSBATrailUpFacingRightLoop] =
        [
            new(new(20, 8), 2),
        ],
        [P.WaveSBA] =
        [
            new(new(4, 4), 0),
            new(new(4, 4), 1),
        ],
    };

    /// <summary>The record <paramref name="frame"/> shows.</summary>
    internal static SamusProjectileRecord Get(SamusProjectileFrame frame) => ByAnimation[frame.Animation][frame.Show];
}
