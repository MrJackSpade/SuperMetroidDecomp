using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Projectiles.MiscDustProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The thirty misc-dust explosion, smoke, dust, beam and drop programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MiscDustProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class MiscDustProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<MiscDustProjectileProgram, AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:E0EE
        [P.BeamCharge] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E100
        [P.ElbowChargeParticle] =
        [
            Show(4, 5),
            Show(5, 4),
            Show(6, 3),
            Show(7, 3),
            Show(8, 3),
            Show(9, 3),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E11A
        [P.ElbowChargeEnergy] =
        [
            Show(10, 4),
            Show(11, 3),
            Show(12, 2),
            Show(13, 2),
            Show(14, 2),
            Show(15, 2),
            Show(16, 12),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E138
        [P.SmallExplosion] =
        [
            Show(17, 4),
            Show(18, 6),
            Show(19, 5),
            Show(20, 5),
            Show(21, 5),
            Show(22, 6),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E152
        [P.RainbowBeamExplosion] =
        [
            Show(23, 3),
            Show(24, 3),
            Show(25, 4),
            Show(26, 4),
            Show(27, 4),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E168
        [P.BeamTrail] =
        [
            Show(28, 8),
            Show(29, 8),
            Show(30, 8),
            Show(31, 8),
            Show(32, 24),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E17E
        [P.DudShot] =
        [
            Show(33, 4),
            Show(34, 4),
            Show(35, 4),
            Show(36, 4),
            Show(37, 4),
            Show(38, 4),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E198
        [P.PowerBomb] =
        [
            Show(39, 5),
            Show(40, 5),
            Show(41, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E1A6
        [P.ElevatorPad] =
        [
            Show(42, 1),
            Show(43, 1),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E1B0
        [P.SmallDustCloud] =
        [
            Show(44, 5),
            Show(45, 5),
            Show(46, 5),
            Show(47, 5),
            Show(48, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E1C6
        [P.CorpseDustCloud] =
        [
            Show(49, 3),
            Show(50, 3),
            Show(51, 3),
            Show(52, 3),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E1D8
        [P.EyeDoorSweatDrop] =
        [
            Show(53, 5),
            Show(54, 5),
            Show(55, 5),
            Show(56, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E1EA
        [P.Smoke] =
        [
            Show(57, 8),
            Show(58, 8),
            Show(59, 8),
            Show(60, 8),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E1FC
        [P.LoopingElevatorPad] =
        [
            Show(61, 1),
            Show(62, 1),
            GoTo(P.LoopingElevatorPad),
        ],
        // $86:E208
        [P.BigExplosion] =
        [
            Show(63, 5),
            Show(64, 5),
            Show(65, 5),
            Show(66, 5),
            Show(67, 5),
            Show(68, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E222
        [P.SmallEnergyDrop] =
        [
            Show(69, 8),
            Show(70, 8),
            Show(71, 8),
            Show(72, 8),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E234
        [P.BigEnergyDrop] =
        [
            Show(73, 8),
            Show(74, 8),
            Show(75, 8),
            Show(76, 8),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E246
        [P.Bomb] =
        [
            Show(77, 5),
            Show(78, 5),
            Show(79, 5),
            Show(80, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E258
        [P.WeirdEnergyDrop] =
        [
            Show(81, 16),
            Show(82, 16),
            Show(83, 16),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E266
        [P.RockParticles] =
        [
            Show(84, 2),
            Show(85, 2),
            Show(86, 2),
            Show(87, 2),
            Show(88, 2),
            Show(89, 2),
            Show(90, 2),
            Show(91, 2),
            Show(92, 2),
            Show(93, 2),
            Show(94, 2),
            Show(95, 2),
            Show(96, 2),
            Show(97, 2),
            Show(98, 2),
            Show(99, 2),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E2A8
        [P.ShortBigDustCloud] =
        [
            Show(100, 2),
            Show(101, 2),
            Show(102, 2),
            Show(103, 2),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E2BA
        [P.ShortBeamCloud] =
        [
            Show(104, 3),
            Show(105, 3),
            Show(106, 3),
            Show(107, 3),
            Show(108, 3),
            Show(109, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E2D4
        [P.MediumBeamCloud] =
        [
            Show(110, 3),
            Show(111, 3),
            Show(112, 3),
            Show(113, 3),
            Show(114, 3),
            Show(115, 3),
            Show(116, 3),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E2F2
        [P.BigDustCloud] =
        [
            Show(117, 5),
            Show(118, 5),
            Show(119, 5),
            Show(120, 5),
            Show(121, 5),
            Show(122, 5),
            Show(123, 5),
            Show(124, 5),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E314
        [P.LongBeam] =
        [
            Show(125, 1),
            Show(126, 1),
            Show(127, 1),
            Show(128, 1),
            Show(129, 1),
            Show(130, 1),
            Show(131, 1),
            Show(132, 1),
            Show(133, 1),
            Show(134, 1),
            Show(135, 1),
            Show(136, 1),
            Show(137, 1),
            Show(138, 1),
            Show(139, 1),
            Show(140, 1),
            Show(141, 1),
            Show(142, 1),
            Show(143, 1),
            Show(144, 1),
            Show(145, 1),
            Show(146, 1),
            Show(147, 1),
            Show(148, 1),
            Show(149, 1),
            Show(150, 1),
            Show(151, 1),
            Show(152, 1),
            Show(153, 1),
            Show(154, 1),
            Show(155, 1),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E392
        [P.FlickeringLongBeam] =
        [
            Show(156, 1),
            Show(157, 1),
            Show(158, 1),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E3A0
        [P.DraygonBreathBubbles] =
        [
            Show(159, 8),
            Show(160, 8),
            Show(161, 8),
            Show(162, 8),
            Show(163, 8),
            Show(164, 8),
            Show(165, 8),
            Show(166, 8),
            Show(167, 8),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E3C6
        [P.SaveStationElectricity] =
        [
            Show(168, 1),
            Show(169, 1),
            Show(170, 1),
            Show(171, 1),
            Show(172, 1),
            Show(173, 1),
            Show(174, 1),
            Show(175, 1),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E3E8
        [P.ExpandingGate] =
        [
            Show(176, 16),
            Show(177, 16),
            Show(178, 16),
            Show(179, 16),
            Show(180, 16),
            Show(181, 16),
            Show(182, 16),
            Show(183, 16),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:E40A
        [P.ContractingGate] =
        [
            Show(184, 4),
            Show(185, 4),
            Show(186, 4),
            Show(187, 4),
            Show(188, 4),
            Show(189, 4),
            Show(190, 4),
            Show(191, 4),
            Cue(new(CK.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue> GoTo(MiscDustProjectileProgram program) => AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue>.Hold;
}
