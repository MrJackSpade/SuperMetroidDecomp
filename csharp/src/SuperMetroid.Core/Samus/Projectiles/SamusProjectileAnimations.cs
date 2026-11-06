using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' programs (#1301). Generated from the verified animation scripts:
/// each eight-byte timed record shows a <see cref="SamusProjectileFrame"/> for its duration, and the lists' gotos and
/// deletes end their programs.
/// </summary>
internal static class SamusProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<SamusProjectileProgram, AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>[]>
    {
        [P.PowerUpFacingRight] =
        [
            Show(P.PowerUpFacingRight, 0, 15),
            GoTo(P.PowerUpFacingRight),
        ],
        [P.PowerUpRight] =
        [
            Show(P.PowerUpRight, 0, 15),
            GoTo(P.PowerUpRight),
        ],
        [P.PowerRight] =
        [
            Show(P.PowerRight, 0, 15),
            GoTo(P.PowerRight),
        ],
        [P.PowerDownRight] =
        [
            Show(P.PowerDownRight, 0, 15),
            GoTo(P.PowerDownRight),
        ],
        [P.PowerDownFacingRight] =
        [
            Show(P.PowerDownFacingRight, 0, 15),
            GoTo(P.PowerDownFacingRight),
        ],
        [P.PowerDownLeft] =
        [
            Show(P.PowerDownLeft, 0, 15),
            GoTo(P.PowerDownLeft),
        ],
        [P.PowerLeft] =
        [
            Show(P.PowerLeft, 0, 15),
            GoTo(P.PowerLeft),
        ],
        [P.PowerUpLeft] =
        [
            Show(P.PowerUpLeft, 0, 15),
            GoTo(P.PowerUpLeft),
        ],
        [P.WaveUpFacingRight] =
        [
            Show(P.WaveUpFacingRight, 0, 4),
            FallInto(P.WaveDownFacingRight),
        ],
        [P.WaveDownFacingRight] =
        [
            Show(P.WaveDownFacingRight, 0, 1),
            Show(P.WaveDownFacingRight, 1, 1),
            Show(P.WaveDownFacingRight, 2, 1),
            Show(P.WaveDownFacingRight, 3, 1),
            Show(P.WaveDownFacingRight, 4, 1),
            Show(P.WaveDownFacingRight, 5, 1),
            Show(P.WaveDownFacingRight, 6, 1),
            Show(P.WaveDownFacingRight, 7, 1),
            Show(P.WaveDownFacingRight, 8, 1),
            Show(P.WaveDownFacingRight, 9, 1),
            Show(P.WaveDownFacingRight, 10, 1),
            Show(P.WaveDownFacingRight, 11, 1),
            Show(P.WaveDownFacingRight, 12, 1),
            Show(P.WaveDownFacingRight, 13, 1),
            Show(P.WaveDownFacingRight, 14, 1),
            Show(P.WaveDownFacingRight, 15, 1),
            GoTo(P.WaveDownFacingRight),
        ],
        [P.WaveUpRight] =
        [
            Show(P.WaveUpRight, 0, 1),
            Show(P.WaveUpRight, 1, 1),
            Show(P.WaveUpRight, 2, 1),
            Show(P.WaveUpRight, 3, 1),
            Show(P.WaveUpRight, 4, 1),
            Show(P.WaveUpRight, 5, 1),
            Show(P.WaveUpRight, 6, 1),
            Show(P.WaveUpRight, 7, 1),
            Show(P.WaveUpRight, 8, 1),
            Show(P.WaveUpRight, 9, 1),
            Show(P.WaveUpRight, 10, 1),
            Show(P.WaveUpRight, 11, 1),
            Show(P.WaveUpRight, 12, 1),
            Show(P.WaveUpRight, 13, 1),
            Show(P.WaveUpRight, 14, 1),
            Show(P.WaveUpRight, 15, 1),
            GoTo(P.WaveUpRight),
        ],
        [P.WaveRight] =
        [
            Show(P.WaveRight, 0, 1),
            Show(P.WaveRight, 1, 1),
            Show(P.WaveRight, 2, 1),
            Show(P.WaveRight, 3, 1),
            Show(P.WaveRight, 4, 1),
            Show(P.WaveRight, 5, 1),
            Show(P.WaveRight, 6, 1),
            Show(P.WaveRight, 7, 1),
            Show(P.WaveRight, 8, 1),
            Show(P.WaveRight, 9, 1),
            Show(P.WaveRight, 10, 1),
            Show(P.WaveRight, 11, 1),
            Show(P.WaveRight, 12, 1),
            Show(P.WaveRight, 13, 1),
            Show(P.WaveRight, 14, 1),
            Show(P.WaveRight, 15, 1),
            GoTo(P.WaveRight),
        ],
        [P.WaveDownRight] =
        [
            Show(P.WaveDownRight, 0, 1),
            Show(P.WaveDownRight, 1, 1),
            Show(P.WaveDownRight, 2, 1),
            Show(P.WaveDownRight, 3, 1),
            Show(P.WaveDownRight, 4, 1),
            Show(P.WaveDownRight, 5, 1),
            Show(P.WaveDownRight, 6, 1),
            Show(P.WaveDownRight, 7, 1),
            Show(P.WaveDownRight, 8, 1),
            Show(P.WaveDownRight, 9, 1),
            Show(P.WaveDownRight, 10, 1),
            Show(P.WaveDownRight, 11, 1),
            Show(P.WaveDownRight, 12, 1),
            Show(P.WaveDownRight, 13, 1),
            Show(P.WaveDownRight, 14, 1),
            Show(P.WaveDownRight, 15, 1),
            GoTo(P.WaveDownRight),
        ],
        [P.IceUpFacingRight] =
        [
            Show(P.IceUpFacingRight, 0, 1),
            Show(P.IceUpFacingRight, 1, 1),
            Show(P.IceUpFacingRight, 2, 1),
            Show(P.IceUpFacingRight, 3, 1),
            GoTo(P.IceUpFacingRight),
        ],
        [P.SpazerUpFacingRight] =
        [
            Show(P.SpazerUpFacingRight, 0, 2),
            Show(P.SpazerUpFacingRight, 1, 2),
            FallInto(P.SpazerUpFacingRightLoop),
        ],
        [P.SpazerUpFacingRightLoop] =
        [
            Show(P.SpazerUpFacingRightLoop, 0, 2),
            GoTo(P.SpazerUpFacingRightLoop),
        ],
        [P.SpazerUpRight] =
        [
            Show(P.SpazerUpRight, 0, 2),
            Show(P.SpazerUpRight, 1, 2),
            FallInto(P.SpazerUpRightLoop),
        ],
        [P.SpazerUpRightLoop] =
        [
            Show(P.SpazerUpRightLoop, 0, 2),
            GoTo(P.SpazerUpRightLoop),
        ],
        [P.SpazerRight] =
        [
            Show(P.SpazerRight, 0, 2),
            Show(P.SpazerRight, 1, 2),
            FallInto(P.SpazerRightLoop),
        ],
        [P.SpazerRightLoop] =
        [
            Show(P.SpazerRightLoop, 0, 2),
            GoTo(P.SpazerRightLoop),
        ],
        [P.SpazerDownRight] =
        [
            Show(P.SpazerDownRight, 0, 2),
            Show(P.SpazerDownRight, 1, 2),
            FallInto(P.SpazerDownRightLoop),
        ],
        [P.SpazerDownRightLoop] =
        [
            Show(P.SpazerDownRightLoop, 0, 2),
            GoTo(P.SpazerDownRightLoop),
        ],
        [P.SpazerDownFacingRight] =
        [
            Show(P.SpazerDownFacingRight, 0, 2),
            Show(P.SpazerDownFacingRight, 1, 2),
            FallInto(P.SpazerDownFacingRightLoop),
        ],
        [P.SpazerDownFacingRightLoop] =
        [
            Show(P.SpazerDownFacingRightLoop, 0, 2),
            GoTo(P.SpazerDownFacingRightLoop),
        ],
        [P.SpazerDownLeft] =
        [
            Show(P.SpazerDownLeft, 0, 2),
            Show(P.SpazerDownLeft, 1, 2),
            FallInto(P.SpazerDownLeftLoop),
        ],
        [P.SpazerDownLeftLoop] =
        [
            Show(P.SpazerDownLeftLoop, 0, 2),
            GoTo(P.SpazerDownLeftLoop),
        ],
        [P.SpazerLeft] =
        [
            Show(P.SpazerLeft, 0, 2),
            Show(P.SpazerLeft, 1, 2),
            FallInto(P.SpazerLeftLoop),
        ],
        [P.SpazerLeftLoop] =
        [
            Show(P.SpazerLeftLoop, 0, 2),
            GoTo(P.SpazerLeftLoop),
        ],
        [P.SpazerUpLeft] =
        [
            Show(P.SpazerUpLeft, 0, 2),
            Show(P.SpazerUpLeft, 1, 2),
            FallInto(P.SpazerUpLeftLoop),
        ],
        [P.SpazerUpLeftLoop] =
        [
            Show(P.SpazerUpLeftLoop, 0, 2),
            GoTo(P.SpazerUpLeftLoop),
        ],
        [P.SpazerWaveUpFacingRight] =
        [
            Show(P.SpazerWaveUpFacingRight, 0, 2),
            Show(P.SpazerWaveUpFacingRight, 1, 2),
            Show(P.SpazerWaveUpFacingRight, 2, 2),
            Show(P.SpazerWaveUpFacingRight, 3, 2),
            Show(P.SpazerWaveUpFacingRight, 4, 2),
            Show(P.SpazerWaveUpFacingRight, 5, 2),
            Show(P.SpazerWaveUpFacingRight, 6, 2),
            Show(P.SpazerWaveUpFacingRight, 7, 2),
            Show(P.SpazerWaveUpFacingRight, 8, 2),
            Show(P.SpazerWaveUpFacingRight, 9, 2),
            GoTo(P.SpazerWaveUpFacingRight),
        ],
        [P.SpazerWaveUpRight] =
        [
            Show(P.SpazerWaveUpRight, 0, 2),
            Show(P.SpazerWaveUpRight, 1, 2),
            Show(P.SpazerWaveUpRight, 2, 2),
            Show(P.SpazerWaveUpRight, 3, 2),
            Show(P.SpazerWaveUpRight, 4, 2),
            Show(P.SpazerWaveUpRight, 5, 2),
            Show(P.SpazerWaveUpRight, 6, 2),
            Show(P.SpazerWaveUpRight, 7, 2),
            Show(P.SpazerWaveUpRight, 8, 2),
            Show(P.SpazerWaveUpRight, 9, 2),
            GoTo(P.SpazerWaveUpRight),
        ],
        [P.SpazerWaveRight] =
        [
            Show(P.SpazerWaveRight, 0, 2),
            Show(P.SpazerWaveRight, 1, 2),
            Show(P.SpazerWaveRight, 2, 2),
            Show(P.SpazerWaveRight, 3, 2),
            Show(P.SpazerWaveRight, 4, 2),
            Show(P.SpazerWaveRight, 5, 2),
            Show(P.SpazerWaveRight, 6, 2),
            Show(P.SpazerWaveRight, 7, 2),
            Show(P.SpazerWaveRight, 8, 2),
            Show(P.SpazerWaveRight, 9, 2),
            GoTo(P.SpazerWaveRight),
        ],
        [P.SpazerWaveDownRight] =
        [
            Show(P.SpazerWaveDownRight, 0, 2),
            Show(P.SpazerWaveDownRight, 1, 2),
            Show(P.SpazerWaveDownRight, 2, 2),
            Show(P.SpazerWaveDownRight, 3, 2),
            Show(P.SpazerWaveDownRight, 4, 2),
            Show(P.SpazerWaveDownRight, 5, 2),
            Show(P.SpazerWaveDownRight, 6, 2),
            Show(P.SpazerWaveDownRight, 7, 2),
            Show(P.SpazerWaveDownRight, 8, 2),
            Show(P.SpazerWaveDownRight, 9, 2),
            GoTo(P.SpazerWaveDownRight),
        ],
        [P.SpazerWaveDownFacingRight] =
        [
            Show(P.SpazerWaveDownFacingRight, 0, 2),
            Show(P.SpazerWaveDownFacingRight, 1, 2),
            Show(P.SpazerWaveDownFacingRight, 2, 2),
            Show(P.SpazerWaveDownFacingRight, 3, 2),
            Show(P.SpazerWaveDownFacingRight, 4, 2),
            Show(P.SpazerWaveDownFacingRight, 5, 2),
            Show(P.SpazerWaveDownFacingRight, 6, 2),
            Show(P.SpazerWaveDownFacingRight, 7, 2),
            Show(P.SpazerWaveDownFacingRight, 8, 2),
            Show(P.SpazerWaveDownFacingRight, 9, 2),
            GoTo(P.SpazerWaveDownFacingRight),
        ],
        [P.SpazerWaveDownLeft] =
        [
            Show(P.SpazerWaveDownLeft, 0, 2),
            Show(P.SpazerWaveDownLeft, 1, 2),
            Show(P.SpazerWaveDownLeft, 2, 2),
            Show(P.SpazerWaveDownLeft, 3, 2),
            Show(P.SpazerWaveDownLeft, 4, 2),
            Show(P.SpazerWaveDownLeft, 5, 2),
            Show(P.SpazerWaveDownLeft, 6, 2),
            Show(P.SpazerWaveDownLeft, 7, 2),
            Show(P.SpazerWaveDownLeft, 8, 2),
            Show(P.SpazerWaveDownLeft, 9, 2),
            GoTo(P.SpazerWaveDownLeft),
        ],
        [P.SpazerWaveLeft] =
        [
            Show(P.SpazerWaveLeft, 0, 2),
            Show(P.SpazerWaveLeft, 1, 2),
            Show(P.SpazerWaveLeft, 2, 2),
            Show(P.SpazerWaveLeft, 3, 2),
            Show(P.SpazerWaveLeft, 4, 2),
            Show(P.SpazerWaveLeft, 5, 2),
            Show(P.SpazerWaveLeft, 6, 2),
            Show(P.SpazerWaveLeft, 7, 2),
            Show(P.SpazerWaveLeft, 8, 2),
            Show(P.SpazerWaveLeft, 9, 2),
            GoTo(P.SpazerWaveLeft),
        ],
        [P.SpazerWaveUpLeft] =
        [
            Show(P.SpazerWaveUpLeft, 0, 2),
            Show(P.SpazerWaveUpLeft, 1, 2),
            Show(P.SpazerWaveUpLeft, 2, 2),
            Show(P.SpazerWaveUpLeft, 3, 2),
            Show(P.SpazerWaveUpLeft, 4, 2),
            Show(P.SpazerWaveUpLeft, 5, 2),
            Show(P.SpazerWaveUpLeft, 6, 2),
            Show(P.SpazerWaveUpLeft, 7, 2),
            Show(P.SpazerWaveUpLeft, 8, 2),
            Show(P.SpazerWaveUpLeft, 9, 2),
            GoTo(P.SpazerWaveUpLeft),
        ],
        [P.PlasmaUpFacingRight] =
        [
            Show(P.PlasmaUpFacingRight, 0, 1),
            FallInto(P.PlasmaUpFacingRightLoop),
        ],
        [P.PlasmaUpFacingRightLoop] =
        [
            Show(P.PlasmaUpFacingRightLoop, 0, 15),
            GoTo(P.PlasmaUpFacingRightLoop),
        ],
        [P.PlasmaUpRight] =
        [
            Show(P.PlasmaUpRight, 0, 1),
            FallInto(P.PlasmaUpRightLoop),
        ],
        [P.PlasmaUpRightLoop] =
        [
            Show(P.PlasmaUpRightLoop, 0, 15),
            GoTo(P.PlasmaUpRightLoop),
        ],
        [P.PlasmaRight] =
        [
            Show(P.PlasmaRight, 0, 1),
            FallInto(P.PlasmaRightLoop),
        ],
        [P.PlasmaRightLoop] =
        [
            Show(P.PlasmaRightLoop, 0, 15),
            GoTo(P.PlasmaRightLoop),
        ],
        [P.PlasmaDownRight] =
        [
            Show(P.PlasmaDownRight, 0, 1),
            FallInto(P.PlasmaDownRightLoop),
        ],
        [P.PlasmaDownRightLoop] =
        [
            Show(P.PlasmaDownRightLoop, 0, 15),
            GoTo(P.PlasmaDownRightLoop),
        ],
        [P.PlasmaIceWaveUpFacingRight] =
        [
            Show(P.PlasmaIceWaveUpFacingRight, 0, 1),
            FallInto(P.PlasmaWaveUpFacingRight),
        ],
        [P.PlasmaWaveUpFacingRight] =
        [
            Show(P.PlasmaWaveUpFacingRight, 0, 2),
            Show(P.PlasmaWaveUpFacingRight, 1, 2),
            Show(P.PlasmaWaveUpFacingRight, 2, 2),
            Show(P.PlasmaWaveUpFacingRight, 3, 2),
            Show(P.PlasmaWaveUpFacingRight, 4, 2),
            Show(P.PlasmaWaveUpFacingRight, 5, 2),
            Show(P.PlasmaWaveUpFacingRight, 6, 2),
            Show(P.PlasmaWaveUpFacingRight, 7, 2),
            GoTo(P.PlasmaWaveUpFacingRight),
        ],
        [P.PlasmaIceWaveUpRight] =
        [
            Show(P.PlasmaIceWaveUpRight, 0, 1),
            FallInto(P.PlasmaWaveUpRight),
        ],
        [P.PlasmaWaveUpRight] =
        [
            Show(P.PlasmaWaveUpRight, 0, 2),
            Show(P.PlasmaWaveUpRight, 1, 2),
            Show(P.PlasmaWaveUpRight, 2, 2),
            Show(P.PlasmaWaveUpRight, 3, 2),
            Show(P.PlasmaWaveUpRight, 4, 2),
            Show(P.PlasmaWaveUpRight, 5, 2),
            Show(P.PlasmaWaveUpRight, 6, 2),
            Show(P.PlasmaWaveUpRight, 7, 2),
            GoTo(P.PlasmaWaveUpRight),
        ],
        [P.PlasmaWaveRight] =
        [
            Show(P.PlasmaWaveRight, 0, 1),
            FallInto(P.PlasmaWaveRightLoop),
        ],
        [P.PlasmaWaveRightLoop] =
        [
            Show(P.PlasmaWaveRightLoop, 0, 2),
            Show(P.PlasmaWaveRightLoop, 1, 2),
            Show(P.PlasmaWaveRightLoop, 2, 2),
            Show(P.PlasmaWaveRightLoop, 3, 2),
            Show(P.PlasmaWaveRightLoop, 4, 2),
            Show(P.PlasmaWaveRightLoop, 5, 2),
            Show(P.PlasmaWaveRightLoop, 6, 2),
            Show(P.PlasmaWaveRightLoop, 7, 2),
            GoTo(P.PlasmaWaveRightLoop),
        ],
        [P.PlasmaIceWaveDownRight] =
        [
            Show(P.PlasmaIceWaveDownRight, 0, 1),
            FallInto(P.PlasmaWaveDownRight),
        ],
        [P.PlasmaWaveDownRight] =
        [
            Show(P.PlasmaWaveDownRight, 0, 2),
            Show(P.PlasmaWaveDownRight, 1, 2),
            Show(P.PlasmaWaveDownRight, 2, 2),
            Show(P.PlasmaWaveDownRight, 3, 2),
            Show(P.PlasmaWaveDownRight, 4, 2),
            Show(P.PlasmaWaveDownRight, 5, 2),
            Show(P.PlasmaWaveDownRight, 6, 2),
            Show(P.PlasmaWaveDownRight, 7, 2),
            GoTo(P.PlasmaWaveDownRight),
        ],
        [P.ChargedPowerUpFacingRight] =
        [
            Show(P.ChargedPowerUpFacingRight, 0, 1),
            Show(P.ChargedPowerUpFacingRight, 1, 1),
            GoTo(P.ChargedPowerUpFacingRight),
        ],
        [P.ChargedPowerUpRight] =
        [
            Show(P.ChargedPowerUpRight, 0, 1),
            Show(P.ChargedPowerUpRight, 1, 1),
            GoTo(P.ChargedPowerUpRight),
        ],
        [P.ChargedPowerRight] =
        [
            Show(P.ChargedPowerRight, 0, 1),
            Show(P.ChargedPowerRight, 1, 1),
            GoTo(P.ChargedPowerRight),
        ],
        [P.ChargedPowerDownRight] =
        [
            Show(P.ChargedPowerDownRight, 0, 1),
            Show(P.ChargedPowerDownRight, 1, 1),
            GoTo(P.ChargedPowerDownRight),
        ],
        [P.ChargedPowerDownFacingRight] =
        [
            Show(P.ChargedPowerDownFacingRight, 0, 1),
            Show(P.ChargedPowerDownFacingRight, 1, 1),
            GoTo(P.ChargedPowerDownFacingRight),
        ],
        [P.ChargedPowerDownLeft] =
        [
            Show(P.ChargedPowerDownLeft, 0, 1),
            Show(P.ChargedPowerDownLeft, 1, 1),
            GoTo(P.ChargedPowerDownLeft),
        ],
        [P.ChargedPowerLeft] =
        [
            Show(P.ChargedPowerLeft, 0, 1),
            Show(P.ChargedPowerLeft, 1, 1),
            GoTo(P.ChargedPowerLeft),
        ],
        [P.ChargedPowerUpLeft] =
        [
            Show(P.ChargedPowerUpLeft, 0, 1),
            Show(P.ChargedPowerUpLeft, 1, 1),
            GoTo(P.ChargedPowerUpLeft),
        ],
        [P.ChargedWaveUpFacingRight] =
        [
            Show(P.ChargedWaveUpFacingRight, 0, 3),
            FallInto(P.ChargedWaveDownFacingRight),
        ],
        [P.ChargedWaveDownFacingRight] =
        [
            Show(P.ChargedWaveDownFacingRight, 0, 1),
            Show(P.ChargedWaveDownFacingRight, 1, 1),
            Show(P.ChargedWaveDownFacingRight, 2, 1),
            Show(P.ChargedWaveDownFacingRight, 3, 1),
            Show(P.ChargedWaveDownFacingRight, 4, 1),
            Show(P.ChargedWaveDownFacingRight, 5, 1),
            Show(P.ChargedWaveDownFacingRight, 6, 1),
            Show(P.ChargedWaveDownFacingRight, 7, 1),
            Show(P.ChargedWaveDownFacingRight, 8, 1),
            Show(P.ChargedWaveDownFacingRight, 9, 1),
            Show(P.ChargedWaveDownFacingRight, 10, 1),
            Show(P.ChargedWaveDownFacingRight, 11, 1),
            Show(P.ChargedWaveDownFacingRight, 12, 1),
            Show(P.ChargedWaveDownFacingRight, 13, 1),
            Show(P.ChargedWaveDownFacingRight, 14, 1),
            Show(P.ChargedWaveDownFacingRight, 15, 1),
            GoTo(P.ChargedWaveDownFacingRight),
        ],
        [P.ChargedWaveUpRight] =
        [
            Show(P.ChargedWaveUpRight, 0, 1),
            Show(P.ChargedWaveUpRight, 1, 1),
            Show(P.ChargedWaveUpRight, 2, 1),
            Show(P.ChargedWaveUpRight, 3, 1),
            Show(P.ChargedWaveUpRight, 4, 1),
            Show(P.ChargedWaveUpRight, 5, 1),
            Show(P.ChargedWaveUpRight, 6, 1),
            Show(P.ChargedWaveUpRight, 7, 1),
            Show(P.ChargedWaveUpRight, 8, 1),
            Show(P.ChargedWaveUpRight, 9, 1),
            Show(P.ChargedWaveUpRight, 10, 1),
            Show(P.ChargedWaveUpRight, 11, 1),
            Show(P.ChargedWaveUpRight, 12, 1),
            Show(P.ChargedWaveUpRight, 13, 1),
            Show(P.ChargedWaveUpRight, 14, 1),
            Show(P.ChargedWaveUpRight, 15, 1),
            GoTo(P.ChargedWaveUpRight),
        ],
        [P.ChargedWaveRight] =
        [
            Show(P.ChargedWaveRight, 0, 1),
            Show(P.ChargedWaveRight, 1, 1),
            Show(P.ChargedWaveRight, 2, 1),
            Show(P.ChargedWaveRight, 3, 1),
            Show(P.ChargedWaveRight, 4, 1),
            Show(P.ChargedWaveRight, 5, 1),
            Show(P.ChargedWaveRight, 6, 1),
            Show(P.ChargedWaveRight, 7, 1),
            Show(P.ChargedWaveRight, 8, 1),
            Show(P.ChargedWaveRight, 9, 1),
            Show(P.ChargedWaveRight, 10, 1),
            Show(P.ChargedWaveRight, 11, 1),
            Show(P.ChargedWaveRight, 12, 1),
            Show(P.ChargedWaveRight, 13, 1),
            Show(P.ChargedWaveRight, 14, 1),
            Show(P.ChargedWaveRight, 15, 1),
            GoTo(P.ChargedWaveRight),
        ],
        [P.ChargedWaveDownRight] =
        [
            Show(P.ChargedWaveDownRight, 0, 1),
            Show(P.ChargedWaveDownRight, 1, 1),
            Show(P.ChargedWaveDownRight, 2, 1),
            Show(P.ChargedWaveDownRight, 3, 1),
            Show(P.ChargedWaveDownRight, 4, 1),
            Show(P.ChargedWaveDownRight, 5, 1),
            Show(P.ChargedWaveDownRight, 6, 1),
            Show(P.ChargedWaveDownRight, 7, 1),
            Show(P.ChargedWaveDownRight, 8, 1),
            Show(P.ChargedWaveDownRight, 9, 1),
            Show(P.ChargedWaveDownRight, 10, 1),
            Show(P.ChargedWaveDownRight, 11, 1),
            Show(P.ChargedWaveDownRight, 12, 1),
            Show(P.ChargedWaveDownRight, 13, 1),
            Show(P.ChargedWaveDownRight, 14, 1),
            Show(P.ChargedWaveDownRight, 15, 1),
            GoTo(P.ChargedWaveDownRight),
        ],
        [P.ChargedIceUpFacingRight] =
        [
            Show(P.ChargedIceUpFacingRight, 0, 1),
            Show(P.ChargedIceUpFacingRight, 1, 1),
            Show(P.ChargedIceUpFacingRight, 2, 1),
            Show(P.ChargedIceUpFacingRight, 3, 1),
            GoTo(P.ChargedIceUpFacingRight),
        ],
        [P.ChargedIceWaveUpFacingRight] =
        [
            Show(P.ChargedIceWaveUpFacingRight, 0, 3),
            FallInto(P.ChargedIceWaveDownFacingRight),
        ],
        [P.ChargedIceWaveDownFacingRight] =
        [
            Show(P.ChargedIceWaveDownFacingRight, 0, 1),
            Show(P.ChargedIceWaveDownFacingRight, 1, 1),
            Show(P.ChargedIceWaveDownFacingRight, 2, 1),
            Show(P.ChargedIceWaveDownFacingRight, 3, 1),
            Show(P.ChargedIceWaveDownFacingRight, 4, 1),
            Show(P.ChargedIceWaveDownFacingRight, 5, 1),
            Show(P.ChargedIceWaveDownFacingRight, 6, 1),
            Show(P.ChargedIceWaveDownFacingRight, 7, 1),
            Show(P.ChargedIceWaveDownFacingRight, 8, 1),
            Show(P.ChargedIceWaveDownFacingRight, 9, 1),
            Show(P.ChargedIceWaveDownFacingRight, 10, 1),
            Show(P.ChargedIceWaveDownFacingRight, 11, 1),
            Show(P.ChargedIceWaveDownFacingRight, 12, 1),
            Show(P.ChargedIceWaveDownFacingRight, 13, 1),
            Show(P.ChargedIceWaveDownFacingRight, 14, 1),
            Show(P.ChargedIceWaveDownFacingRight, 15, 1),
            GoTo(P.ChargedIceWaveDownFacingRight),
        ],
        [P.ChargedIceWaveUpRight] =
        [
            Show(P.ChargedIceWaveUpRight, 0, 1),
            Show(P.ChargedIceWaveUpRight, 1, 1),
            Show(P.ChargedIceWaveUpRight, 2, 1),
            Show(P.ChargedIceWaveUpRight, 3, 1),
            Show(P.ChargedIceWaveUpRight, 4, 1),
            Show(P.ChargedIceWaveUpRight, 5, 1),
            Show(P.ChargedIceWaveUpRight, 6, 1),
            Show(P.ChargedIceWaveUpRight, 7, 1),
            Show(P.ChargedIceWaveUpRight, 8, 1),
            Show(P.ChargedIceWaveUpRight, 9, 1),
            Show(P.ChargedIceWaveUpRight, 10, 1),
            Show(P.ChargedIceWaveUpRight, 11, 1),
            Show(P.ChargedIceWaveUpRight, 12, 1),
            Show(P.ChargedIceWaveUpRight, 13, 1),
            Show(P.ChargedIceWaveUpRight, 14, 1),
            Show(P.ChargedIceWaveUpRight, 15, 1),
            GoTo(P.ChargedIceWaveUpRight),
        ],
        [P.ChargedIceWaveRight] =
        [
            Show(P.ChargedIceWaveRight, 0, 1),
            Show(P.ChargedIceWaveRight, 1, 1),
            Show(P.ChargedIceWaveRight, 2, 1),
            Show(P.ChargedIceWaveRight, 3, 1),
            Show(P.ChargedIceWaveRight, 4, 1),
            Show(P.ChargedIceWaveRight, 5, 1),
            Show(P.ChargedIceWaveRight, 6, 1),
            Show(P.ChargedIceWaveRight, 7, 1),
            Show(P.ChargedIceWaveRight, 8, 1),
            Show(P.ChargedIceWaveRight, 9, 1),
            Show(P.ChargedIceWaveRight, 10, 1),
            Show(P.ChargedIceWaveRight, 11, 1),
            Show(P.ChargedIceWaveRight, 12, 1),
            Show(P.ChargedIceWaveRight, 13, 1),
            Show(P.ChargedIceWaveRight, 14, 1),
            Show(P.ChargedIceWaveRight, 15, 1),
            GoTo(P.ChargedIceWaveRight),
        ],
        [P.ChargedIceWaveDownRight] =
        [
            Show(P.ChargedIceWaveDownRight, 0, 1),
            Show(P.ChargedIceWaveDownRight, 1, 1),
            Show(P.ChargedIceWaveDownRight, 2, 1),
            Show(P.ChargedIceWaveDownRight, 3, 1),
            Show(P.ChargedIceWaveDownRight, 4, 1),
            Show(P.ChargedIceWaveDownRight, 5, 1),
            Show(P.ChargedIceWaveDownRight, 6, 1),
            Show(P.ChargedIceWaveDownRight, 7, 1),
            Show(P.ChargedIceWaveDownRight, 8, 1),
            Show(P.ChargedIceWaveDownRight, 9, 1),
            Show(P.ChargedIceWaveDownRight, 10, 1),
            Show(P.ChargedIceWaveDownRight, 11, 1),
            Show(P.ChargedIceWaveDownRight, 12, 1),
            Show(P.ChargedIceWaveDownRight, 13, 1),
            Show(P.ChargedIceWaveDownRight, 14, 1),
            Show(P.ChargedIceWaveDownRight, 15, 1),
            GoTo(P.ChargedIceWaveDownRight),
        ],
        [P.ChargedSpazerUpFacingRight] =
        [
            Show(P.ChargedSpazerUpFacingRight, 0, 1),
            Show(P.ChargedSpazerUpFacingRight, 1, 1),
            Show(P.ChargedSpazerUpFacingRight, 2, 1),
            Show(P.ChargedSpazerUpFacingRight, 3, 1),
            Show(P.ChargedSpazerUpFacingRight, 4, 1),
            Show(P.ChargedSpazerUpFacingRight, 5, 1),
            Show(P.ChargedSpazerUpFacingRight, 6, 1),
            Show(P.ChargedSpazerUpFacingRight, 7, 1),
            FallInto(P.ChargedSpazerUpFacingRightLoop),
        ],
        [P.ChargedSpazerUpFacingRightLoop] =
        [
            Show(P.ChargedSpazerUpFacingRightLoop, 0, 1),
            Show(P.ChargedSpazerUpFacingRightLoop, 1, 1),
            GoTo(P.ChargedSpazerUpFacingRightLoop),
        ],
        [P.ChargedSpazerUpRight] =
        [
            Show(P.ChargedSpazerUpRight, 0, 1),
            Show(P.ChargedSpazerUpRight, 1, 1),
            Show(P.ChargedSpazerUpRight, 2, 1),
            Show(P.ChargedSpazerUpRight, 3, 1),
            Show(P.ChargedSpazerUpRight, 4, 1),
            Show(P.ChargedSpazerUpRight, 5, 1),
            Show(P.ChargedSpazerUpRight, 6, 1),
            Show(P.ChargedSpazerUpRight, 7, 1),
            FallInto(P.ChargedSpazerUpRightLoop),
        ],
        [P.ChargedSpazerUpRightLoop] =
        [
            Show(P.ChargedSpazerUpRightLoop, 0, 1),
            Show(P.ChargedSpazerUpRightLoop, 1, 1),
            GoTo(P.ChargedSpazerUpRightLoop),
        ],
        [P.ChargedSpazerRight] =
        [
            Show(P.ChargedSpazerRight, 0, 1),
            Show(P.ChargedSpazerRight, 1, 1),
            Show(P.ChargedSpazerRight, 2, 1),
            Show(P.ChargedSpazerRight, 3, 1),
            Show(P.ChargedSpazerRight, 4, 1),
            Show(P.ChargedSpazerRight, 5, 1),
            Show(P.ChargedSpazerRight, 6, 1),
            Show(P.ChargedSpazerRight, 7, 1),
            FallInto(P.ChargedSpazerRightLoop),
        ],
        [P.ChargedSpazerRightLoop] =
        [
            Show(P.ChargedSpazerRightLoop, 0, 1),
            Show(P.ChargedSpazerRightLoop, 1, 1),
            GoTo(P.ChargedSpazerRightLoop),
        ],
        [P.ChargedSpazerDownRight] =
        [
            Show(P.ChargedSpazerDownRight, 0, 1),
            Show(P.ChargedSpazerDownRight, 1, 1),
            Show(P.ChargedSpazerDownRight, 2, 1),
            Show(P.ChargedSpazerDownRight, 3, 1),
            Show(P.ChargedSpazerDownRight, 4, 1),
            Show(P.ChargedSpazerDownRight, 5, 1),
            Show(P.ChargedSpazerDownRight, 6, 1),
            Show(P.ChargedSpazerDownRight, 7, 1),
            FallInto(P.ChargedSpazerDownRightLoop),
        ],
        [P.ChargedSpazerDownRightLoop] =
        [
            Show(P.ChargedSpazerDownRightLoop, 0, 1),
            Show(P.ChargedSpazerDownRightLoop, 1, 1),
            GoTo(P.ChargedSpazerDownRightLoop),
        ],
        [P.ChargedSpazerWaveUpFacingRight] =
        [
            Show(P.ChargedSpazerWaveUpFacingRight, 0, 1),
            Show(P.ChargedSpazerWaveUpFacingRight, 1, 1),
            Show(P.ChargedSpazerWaveUpFacingRight, 2, 1),
            Show(P.ChargedSpazerWaveUpFacingRight, 3, 1),
            FallInto(P.ChargedSpazerWaveUpFacingRightLoop),
        ],
        [P.ChargedSpazerWaveUpFacingRightLoop] =
        [
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveUpFacingRightLoop),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            Show(P.ChargedSpazerWaveUpRight, 0, 1),
            Show(P.ChargedSpazerWaveUpRight, 1, 1),
            Show(P.ChargedSpazerWaveUpRight, 2, 1),
            Show(P.ChargedSpazerWaveUpRight, 3, 1),
            FallInto(P.ChargedSpazerWaveUpRightLoop),
        ],
        [P.ChargedSpazerWaveUpRightLoop] =
        [
            Show(P.ChargedSpazerWaveUpRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveUpRightLoop),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            Show(P.ChargedSpazerWaveRight, 0, 1),
            Show(P.ChargedSpazerWaveRight, 1, 1),
            Show(P.ChargedSpazerWaveRight, 2, 1),
            Show(P.ChargedSpazerWaveRight, 3, 1),
            FallInto(P.ChargedSpazerWaveRightLoop),
        ],
        [P.ChargedSpazerWaveRightLoop] =
        [
            Show(P.ChargedSpazerWaveRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveRightLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveRightLoop),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            Show(P.ChargedSpazerWaveDownRight, 0, 1),
            Show(P.ChargedSpazerWaveDownRight, 1, 1),
            Show(P.ChargedSpazerWaveDownRight, 2, 1),
            Show(P.ChargedSpazerWaveDownRight, 3, 1),
            FallInto(P.ChargedSpazerWaveDownRightLoop),
        ],
        [P.ChargedSpazerWaveDownRightLoop] =
        [
            Show(P.ChargedSpazerWaveDownRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveDownRightLoop),
        ],
        [P.ChargedSpazerWaveDownFacingRight] =
        [
            Show(P.ChargedSpazerWaveDownFacingRight, 0, 1),
            Show(P.ChargedSpazerWaveDownFacingRight, 1, 1),
            Show(P.ChargedSpazerWaveDownFacingRight, 2, 1),
            Show(P.ChargedSpazerWaveDownFacingRight, 3, 1),
            FallInto(P.ChargedSpazerWaveDownFacingRightLoop),
        ],
        [P.ChargedSpazerWaveDownFacingRightLoop] =
        [
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveDownFacingRightLoop),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            Show(P.ChargedSpazerWaveDownLeft, 0, 1),
            Show(P.ChargedSpazerWaveDownLeft, 1, 1),
            Show(P.ChargedSpazerWaveDownLeft, 2, 1),
            Show(P.ChargedSpazerWaveDownLeft, 3, 1),
            FallInto(P.ChargedSpazerWaveDownLeftLoop),
        ],
        [P.ChargedSpazerWaveDownLeftLoop] =
        [
            Show(P.ChargedSpazerWaveDownLeftLoop, 0, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 1, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 2, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 3, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 4, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 5, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 6, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 7, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 8, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 9, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 10, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 11, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 12, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 13, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 14, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 15, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 16, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 17, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 18, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveDownLeftLoop),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            Show(P.ChargedSpazerWaveLeft, 0, 1),
            Show(P.ChargedSpazerWaveLeft, 1, 1),
            Show(P.ChargedSpazerWaveLeft, 2, 1),
            Show(P.ChargedSpazerWaveLeft, 3, 1),
            FallInto(P.ChargedSpazerWaveLeftLoop),
        ],
        [P.ChargedSpazerWaveLeftLoop] =
        [
            Show(P.ChargedSpazerWaveLeftLoop, 0, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 1, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 2, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 3, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 4, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 5, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 6, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 7, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 8, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 9, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 10, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 11, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 12, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 13, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 14, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 15, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 16, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 17, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 18, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveLeftLoop),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            Show(P.ChargedSpazerWaveUpLeft, 0, 1),
            Show(P.ChargedSpazerWaveUpLeft, 1, 1),
            Show(P.ChargedSpazerWaveUpLeft, 2, 1),
            Show(P.ChargedSpazerWaveUpLeft, 3, 1),
            FallInto(P.ChargedSpazerWaveUpLeftLoop),
        ],
        [P.ChargedSpazerWaveUpLeftLoop] =
        [
            Show(P.ChargedSpazerWaveUpLeftLoop, 0, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 1, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 2, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 3, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 4, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 5, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 6, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 7, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 8, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 9, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 10, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 11, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 12, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 13, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 14, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 15, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 16, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 17, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 18, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 19, 1),
            GoTo(P.ChargedSpazerWaveUpLeftLoop),
        ],
        [P.ChargedPlasmaUpFacingRight] =
        [
            Show(P.ChargedPlasmaUpFacingRight, 0, 1),
            Show(P.ChargedPlasmaUpFacingRight, 1, 1),
            Show(P.ChargedPlasmaUpFacingRight, 2, 1),
            Show(P.ChargedPlasmaUpFacingRight, 3, 1),
            Show(P.ChargedPlasmaUpFacingRight, 4, 1),
            Show(P.ChargedPlasmaUpFacingRight, 5, 1),
            FallInto(P.ChargedPlasmaUpFacingRightLoop),
        ],
        [P.ChargedPlasmaUpFacingRightLoop] =
        [
            Show(P.ChargedPlasmaUpFacingRightLoop, 0, 1),
            Show(P.ChargedPlasmaUpFacingRightLoop, 1, 1),
            GoTo(P.ChargedPlasmaUpFacingRightLoop),
        ],
        [P.ChargedPlasmaUpRight] =
        [
            Show(P.ChargedPlasmaUpRight, 0, 1),
            Show(P.ChargedPlasmaUpRight, 1, 1),
            Show(P.ChargedPlasmaUpRight, 2, 1),
            Show(P.ChargedPlasmaUpRight, 3, 1),
            Show(P.ChargedPlasmaUpRight, 4, 1),
            Show(P.ChargedPlasmaUpRight, 5, 1),
            FallInto(P.ChargedPlasmaUpRightLoop),
        ],
        [P.ChargedPlasmaUpRightLoop] =
        [
            Show(P.ChargedPlasmaUpRightLoop, 0, 1),
            Show(P.ChargedPlasmaUpRightLoop, 1, 1),
            GoTo(P.ChargedPlasmaUpRightLoop),
        ],
        [P.ChargedPlasmaRight] =
        [
            Show(P.ChargedPlasmaRight, 0, 1),
            Show(P.ChargedPlasmaRight, 1, 1),
            Show(P.ChargedPlasmaRight, 2, 1),
            Show(P.ChargedPlasmaRight, 3, 1),
            Show(P.ChargedPlasmaRight, 4, 1),
            Show(P.ChargedPlasmaRight, 5, 1),
            FallInto(P.ChargedPlasmaRightLoop),
        ],
        [P.ChargedPlasmaRightLoop] =
        [
            Show(P.ChargedPlasmaRightLoop, 0, 1),
            Show(P.ChargedPlasmaRightLoop, 1, 1),
            GoTo(P.ChargedPlasmaRightLoop),
        ],
        [P.ChargedPlasmaDownRight] =
        [
            Show(P.ChargedPlasmaDownRight, 0, 1),
            Show(P.ChargedPlasmaDownRight, 1, 1),
            Show(P.ChargedPlasmaDownRight, 2, 1),
            Show(P.ChargedPlasmaDownRight, 3, 1),
            Show(P.ChargedPlasmaDownRight, 4, 1),
            Show(P.ChargedPlasmaDownRight, 5, 1),
            FallInto(P.ChargedPlasmaDownRightLoop),
        ],
        [P.ChargedPlasmaDownRightLoop] =
        [
            Show(P.ChargedPlasmaDownRightLoop, 0, 1),
            Show(P.ChargedPlasmaDownRightLoop, 1, 1),
            GoTo(P.ChargedPlasmaDownRightLoop),
        ],
        [P.ChargedPlasmaWaveUpFacingRight] =
        [
            Show(P.ChargedPlasmaWaveUpFacingRight, 0, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 1, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 2, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 3, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 4, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 5, 1),
            FallInto(P.ChargedPlasmaWaveUpFacingRightLoop),
        ],
        [P.ChargedPlasmaWaveUpFacingRightLoop] =
        [
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 15, 1),
            GoTo(P.ChargedPlasmaWaveUpFacingRightLoop),
        ],
        [P.ChargedPlasmaWaveUpRight] =
        [
            Show(P.ChargedPlasmaWaveUpRight, 0, 1),
            Show(P.ChargedPlasmaWaveUpRight, 1, 1),
            Show(P.ChargedPlasmaWaveUpRight, 2, 1),
            Show(P.ChargedPlasmaWaveUpRight, 3, 1),
            Show(P.ChargedPlasmaWaveUpRight, 4, 1),
            Show(P.ChargedPlasmaWaveUpRight, 5, 1),
            FallInto(P.ChargedPlasmaWaveUpRightLoop),
        ],
        [P.ChargedPlasmaWaveUpRightLoop] =
        [
            Show(P.ChargedPlasmaWaveUpRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 15, 1),
            GoTo(P.ChargedPlasmaWaveUpRightLoop),
        ],
        [P.ChargedPlasmaWaveRight] =
        [
            Show(P.ChargedPlasmaWaveRight, 0, 1),
            Show(P.ChargedPlasmaWaveRight, 1, 1),
            Show(P.ChargedPlasmaWaveRight, 2, 1),
            Show(P.ChargedPlasmaWaveRight, 3, 1),
            Show(P.ChargedPlasmaWaveRight, 4, 1),
            Show(P.ChargedPlasmaWaveRight, 5, 1),
            FallInto(P.ChargedPlasmaWaveRightLoop),
        ],
        [P.ChargedPlasmaWaveRightLoop] =
        [
            Show(P.ChargedPlasmaWaveRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 15, 1),
            GoTo(P.ChargedPlasmaWaveRightLoop),
        ],
        [P.ChargedPlasmaWaveDownRight] =
        [
            Show(P.ChargedPlasmaWaveDownRight, 0, 1),
            Show(P.ChargedPlasmaWaveDownRight, 1, 1),
            Show(P.ChargedPlasmaWaveDownRight, 2, 1),
            Show(P.ChargedPlasmaWaveDownRight, 3, 1),
            Show(P.ChargedPlasmaWaveDownRight, 4, 1),
            Show(P.ChargedPlasmaWaveDownRight, 5, 1),
            FallInto(P.ChargedPlasmaWaveDownRightLoop),
        ],
        [P.ChargedPlasmaWaveDownRightLoop] =
        [
            Show(P.ChargedPlasmaWaveDownRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 15, 1),
            GoTo(P.ChargedPlasmaWaveDownRightLoop),
        ],
        [P.MissileUpFacingRight] =
        [
            Show(P.MissileUpFacingRight, 0, 15),
            GoTo(P.MissileUpFacingRight),
        ],
        [P.MissileUpRight] =
        [
            Show(P.MissileUpRight, 0, 15),
            GoTo(P.MissileUpRight),
        ],
        [P.MissileRight] =
        [
            Show(P.MissileRight, 0, 15),
            GoTo(P.MissileRight),
        ],
        [P.MissileDownRight] =
        [
            Show(P.MissileDownRight, 0, 15),
            GoTo(P.MissileDownRight),
        ],
        [P.MissileDownFacingRight] =
        [
            Show(P.MissileDownFacingRight, 0, 15),
            GoTo(P.MissileDownFacingRight),
        ],
        [P.MissileDownLeft] =
        [
            Show(P.MissileDownLeft, 0, 15),
            GoTo(P.MissileDownLeft),
        ],
        [P.MissileLeft] =
        [
            Show(P.MissileLeft, 0, 15),
            GoTo(P.MissileLeft),
        ],
        [P.MissileUpLeft] =
        [
            Show(P.MissileUpLeft, 0, 15),
            GoTo(P.MissileUpLeft),
        ],
        [P.SuperMissileUpFacingRight] =
        [
            Show(P.SuperMissileUpFacingRight, 0, 15),
            GoTo(P.SuperMissileUpFacingRight),
        ],
        [P.SuperMissileUpRight] =
        [
            Show(P.SuperMissileUpRight, 0, 15),
            GoTo(P.SuperMissileUpRight),
        ],
        [P.SuperMissileRight] =
        [
            Show(P.SuperMissileRight, 0, 15),
            GoTo(P.SuperMissileRight),
        ],
        [P.SuperMissileDownRight] =
        [
            Show(P.SuperMissileDownRight, 0, 15),
            GoTo(P.SuperMissileDownRight),
        ],
        [P.SuperMissileDownFacingRight] =
        [
            Show(P.SuperMissileDownFacingRight, 0, 15),
            GoTo(P.SuperMissileDownFacingRight),
        ],
        [P.SuperMissileDownLeft] =
        [
            Show(P.SuperMissileDownLeft, 0, 15),
            GoTo(P.SuperMissileDownLeft),
        ],
        [P.SuperMissileLeft] =
        [
            Show(P.SuperMissileLeft, 0, 15),
            GoTo(P.SuperMissileLeft),
        ],
        [P.SuperMissileUpLeft] =
        [
            Show(P.SuperMissileUpLeft, 0, 15),
            GoTo(P.SuperMissileUpLeft),
        ],
        [P.SuperMissileLink] =
        [
            Show(P.SuperMissileLink, 0, 15),
            GoTo(P.SuperMissileLink),
        ],
        [P.PowerBomb] =
        [
            Show(P.PowerBomb, 0, 5),
            Show(P.PowerBomb, 1, 5),
            Show(P.PowerBomb, 2, 5),
            GoTo(P.PowerBomb),
        ],
        [P.FastPowerBomb] =
        [
            Show(P.FastPowerBomb, 0, 1),
            Show(P.FastPowerBomb, 1, 1),
            Show(P.FastPowerBomb, 2, 1),
            GoTo(P.FastPowerBomb),
        ],
        [P.Bomb] =
        [
            Show(P.Bomb, 0, 5),
            Show(P.Bomb, 1, 5),
            Show(P.Bomb, 2, 5),
            Show(P.Bomb, 3, 5),
            GoTo(P.Bomb),
        ],
        [P.FastBomb] =
        [
            Show(P.FastBomb, 0, 1),
            Show(P.FastBomb, 1, 1),
            Show(P.FastBomb, 2, 1),
            Show(P.FastBomb, 3, 1),
            GoTo(P.FastBomb),
        ],
        [P.BeamExplosion] =
        [
            Show(P.BeamExplosion, 0, 3),
            Show(P.BeamExplosion, 1, 3),
            Show(P.BeamExplosion, 2, 3),
            Show(P.BeamExplosion, 3, 3),
            Show(P.BeamExplosion, 4, 3),
            Show(P.BeamExplosion, 5, 3),
            Stop,
        ],
        [P.MissileExplosion] =
        [
            Show(P.MissileExplosion, 0, 3),
            Show(P.MissileExplosion, 1, 3),
            Show(P.MissileExplosion, 2, 3),
            Show(P.MissileExplosion, 3, 3),
            Show(P.MissileExplosion, 4, 3),
            Show(P.MissileExplosion, 5, 3),
            Stop,
        ],
        [P.BombExplosion] =
        [
            Show(P.BombExplosion, 0, 2),
            Show(P.BombExplosion, 1, 2),
            Show(P.BombExplosion, 2, 2),
            Show(P.BombExplosion, 3, 2),
            Show(P.BombExplosion, 4, 2),
            Stop,
        ],
        [P.PlasmaSBA] =
        [
            Show(P.PlasmaSBA, 0, 2),
            Show(P.PlasmaSBA, 1, 2),
            Show(P.PlasmaSBA, 2, 2),
            Show(P.PlasmaSBA, 3, 2),
            Show(P.PlasmaSBA, 4, 2),
            GoTo(P.PlasmaSBA),
        ],
        [P.SuperMissileExplosion] =
        [
            Show(P.SuperMissileExplosion, 0, 5),
            Show(P.SuperMissileExplosion, 1, 5),
            Show(P.SuperMissileExplosion, 2, 5),
            Show(P.SuperMissileExplosion, 3, 5),
            Show(P.SuperMissileExplosion, 4, 5),
            Show(P.SuperMissileExplosion, 5, 5),
            Stop,
        ],
        [P.ShinesparkEchoUpFacingRight] =
        [
            Show(P.ShinesparkEchoUpFacingRight, 0, 2),
            Show(P.ShinesparkEchoUpFacingRight, 1, 2),
            Show(P.ShinesparkEchoUpFacingRight, 2, 2),
            Show(P.ShinesparkEchoUpFacingRight, 3, 2),
            GoTo(P.ShinesparkEchoUpFacingRight),
        ],
        [P.SpazerSBATrailUpFacingRight] =
        [
            Show(P.SpazerSBATrailUpFacingRight, 0, 2),
            Show(P.SpazerSBATrailUpFacingRight, 1, 2),
            FallInto(P.SpazerSBATrailUpFacingRightLoop),
        ],
        [P.SpazerSBATrailUpFacingRightLoop] =
        [
            Show(P.SpazerSBATrailUpFacingRightLoop, 0, 2),
            GoTo(P.SpazerSBATrailUpFacingRightLoop),
        ],
        [P.WaveSBA] =
        [
            Show(P.WaveSBA, 0, 8),
            Show(P.WaveSBA, 1, 8),
            GoTo(P.WaveSBA),
        ],
    });

    /// <summary>Shows <paramref name="program"/>'s timed record <paramref name="show"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> Show(SamusProjectileProgram program, int show, int duration) => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.Show(new(program, show), duration);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> GoTo(SamusProjectileProgram program) => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, the list authored next.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> FallInto(SamusProjectileProgram program) => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallInto(program);

    /// <summary>Deletes the projectile.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> Stop => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.Stop;
}
