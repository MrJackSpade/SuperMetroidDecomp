using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' programs (#1301), in place of their bank-$93 lists. Generated from the verified native lists:
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
            Show(0, 15),
            GoTo(P.PowerUpFacingRight),
        ],
        [P.PowerUpRight] =
        [
            Show(1, 15),
            GoTo(P.PowerUpRight),
        ],
        [P.PowerRight] =
        [
            Show(2, 15),
            GoTo(P.PowerRight),
        ],
        [P.PowerDownRight] =
        [
            Show(3, 15),
            GoTo(P.PowerDownRight),
        ],
        [P.PowerDownFacingRight] =
        [
            Show(4, 15),
            GoTo(P.PowerDownFacingRight),
        ],
        [P.PowerDownLeft] =
        [
            Show(5, 15),
            GoTo(P.PowerDownLeft),
        ],
        [P.PowerLeft] =
        [
            Show(6, 15),
            GoTo(P.PowerLeft),
        ],
        [P.PowerUpLeft] =
        [
            Show(7, 15),
            GoTo(P.PowerUpLeft),
        ],
        [P.WaveUpFacingRight] =
        [
            Show(8, 4),
            FallInto(P.WaveDownFacingRight),
        ],
        [P.WaveDownFacingRight] =
        [
            Show(9, 1),
            Show(10, 1),
            Show(11, 1),
            Show(12, 1),
            Show(13, 1),
            Show(14, 1),
            Show(15, 1),
            Show(16, 1),
            Show(17, 1),
            Show(18, 1),
            Show(19, 1),
            Show(20, 1),
            Show(21, 1),
            Show(22, 1),
            Show(23, 1),
            Show(24, 1),
            GoTo(P.WaveDownFacingRight),
        ],
        [P.WaveUpRight] =
        [
            Show(25, 1),
            Show(26, 1),
            Show(27, 1),
            Show(28, 1),
            Show(29, 1),
            Show(30, 1),
            Show(31, 1),
            Show(32, 1),
            Show(33, 1),
            Show(34, 1),
            Show(35, 1),
            Show(36, 1),
            Show(37, 1),
            Show(38, 1),
            Show(39, 1),
            Show(40, 1),
            GoTo(P.WaveUpRight),
        ],
        [P.WaveRight] =
        [
            Show(41, 1),
            Show(42, 1),
            Show(43, 1),
            Show(44, 1),
            Show(45, 1),
            Show(46, 1),
            Show(47, 1),
            Show(48, 1),
            Show(49, 1),
            Show(50, 1),
            Show(51, 1),
            Show(52, 1),
            Show(53, 1),
            Show(54, 1),
            Show(55, 1),
            Show(56, 1),
            GoTo(P.WaveRight),
        ],
        [P.WaveDownRight] =
        [
            Show(57, 1),
            Show(58, 1),
            Show(59, 1),
            Show(60, 1),
            Show(61, 1),
            Show(62, 1),
            Show(63, 1),
            Show(64, 1),
            Show(65, 1),
            Show(66, 1),
            Show(67, 1),
            Show(68, 1),
            Show(69, 1),
            Show(70, 1),
            Show(71, 1),
            Show(72, 1),
            GoTo(P.WaveDownRight),
        ],
        [P.IceUpFacingRight] =
        [
            Show(73, 1),
            Show(74, 1),
            Show(75, 1),
            Show(76, 1),
            GoTo(P.IceUpFacingRight),
        ],
        [P.SpazerUpFacingRight] =
        [
            Show(77, 2),
            Show(78, 2),
            FallInto(P.SpazerUpFacingRightLoop),
        ],
        [P.SpazerUpFacingRightLoop] =
        [
            Show(79, 2),
            GoTo(P.SpazerUpFacingRightLoop),
        ],
        [P.SpazerUpRight] =
        [
            Show(80, 2),
            Show(81, 2),
            FallInto(P.SpazerUpRightLoop),
        ],
        [P.SpazerUpRightLoop] =
        [
            Show(82, 2),
            GoTo(P.SpazerUpRightLoop),
        ],
        [P.SpazerRight] =
        [
            Show(83, 2),
            Show(84, 2),
            FallInto(P.SpazerRightLoop),
        ],
        [P.SpazerRightLoop] =
        [
            Show(85, 2),
            GoTo(P.SpazerRightLoop),
        ],
        [P.SpazerDownRight] =
        [
            Show(86, 2),
            Show(87, 2),
            FallInto(P.SpazerDownRightLoop),
        ],
        [P.SpazerDownRightLoop] =
        [
            Show(88, 2),
            GoTo(P.SpazerDownRightLoop),
        ],
        [P.SpazerDownFacingRight] =
        [
            Show(89, 2),
            Show(90, 2),
            FallInto(P.SpazerDownFacingRightLoop),
        ],
        [P.SpazerDownFacingRightLoop] =
        [
            Show(91, 2),
            GoTo(P.SpazerDownFacingRightLoop),
        ],
        [P.SpazerDownLeft] =
        [
            Show(92, 2),
            Show(93, 2),
            FallInto(P.SpazerDownLeftLoop),
        ],
        [P.SpazerDownLeftLoop] =
        [
            Show(94, 2),
            GoTo(P.SpazerDownLeftLoop),
        ],
        [P.SpazerLeft] =
        [
            Show(95, 2),
            Show(96, 2),
            FallInto(P.SpazerLeftLoop),
        ],
        [P.SpazerLeftLoop] =
        [
            Show(97, 2),
            GoTo(P.SpazerLeftLoop),
        ],
        [P.SpazerUpLeft] =
        [
            Show(98, 2),
            Show(99, 2),
            FallInto(P.SpazerUpLeftLoop),
        ],
        [P.SpazerUpLeftLoop] =
        [
            Show(100, 2),
            GoTo(P.SpazerUpLeftLoop),
        ],
        [P.SpazerWaveUpFacingRight] =
        [
            Show(101, 2),
            Show(102, 2),
            Show(103, 2),
            Show(104, 2),
            Show(105, 2),
            Show(106, 2),
            Show(107, 2),
            Show(108, 2),
            Show(109, 2),
            Show(110, 2),
            GoTo(P.SpazerWaveUpFacingRight),
        ],
        [P.SpazerWaveUpRight] =
        [
            Show(111, 2),
            Show(112, 2),
            Show(113, 2),
            Show(114, 2),
            Show(115, 2),
            Show(116, 2),
            Show(117, 2),
            Show(118, 2),
            Show(119, 2),
            Show(120, 2),
            GoTo(P.SpazerWaveUpRight),
        ],
        [P.SpazerWaveRight] =
        [
            Show(121, 2),
            Show(122, 2),
            Show(123, 2),
            Show(124, 2),
            Show(125, 2),
            Show(126, 2),
            Show(127, 2),
            Show(128, 2),
            Show(129, 2),
            Show(130, 2),
            GoTo(P.SpazerWaveRight),
        ],
        [P.SpazerWaveDownRight] =
        [
            Show(131, 2),
            Show(132, 2),
            Show(133, 2),
            Show(134, 2),
            Show(135, 2),
            Show(136, 2),
            Show(137, 2),
            Show(138, 2),
            Show(139, 2),
            Show(140, 2),
            GoTo(P.SpazerWaveDownRight),
        ],
        [P.SpazerWaveDownFacingRight] =
        [
            Show(141, 2),
            Show(142, 2),
            Show(143, 2),
            Show(144, 2),
            Show(145, 2),
            Show(146, 2),
            Show(147, 2),
            Show(148, 2),
            Show(149, 2),
            Show(150, 2),
            GoTo(P.SpazerWaveDownFacingRight),
        ],
        [P.SpazerWaveDownLeft] =
        [
            Show(151, 2),
            Show(152, 2),
            Show(153, 2),
            Show(154, 2),
            Show(155, 2),
            Show(156, 2),
            Show(157, 2),
            Show(158, 2),
            Show(159, 2),
            Show(160, 2),
            GoTo(P.SpazerWaveDownLeft),
        ],
        [P.SpazerWaveLeft] =
        [
            Show(161, 2),
            Show(162, 2),
            Show(163, 2),
            Show(164, 2),
            Show(165, 2),
            Show(166, 2),
            Show(167, 2),
            Show(168, 2),
            Show(169, 2),
            Show(170, 2),
            GoTo(P.SpazerWaveLeft),
        ],
        [P.SpazerWaveUpLeft] =
        [
            Show(171, 2),
            Show(172, 2),
            Show(173, 2),
            Show(174, 2),
            Show(175, 2),
            Show(176, 2),
            Show(177, 2),
            Show(178, 2),
            Show(179, 2),
            Show(180, 2),
            GoTo(P.SpazerWaveUpLeft),
        ],
        [P.PlasmaUpFacingRight] =
        [
            Show(181, 1),
            FallInto(P.PlasmaUpFacingRightLoop),
        ],
        [P.PlasmaUpFacingRightLoop] =
        [
            Show(182, 15),
            GoTo(P.PlasmaUpFacingRightLoop),
        ],
        [P.PlasmaUpRight] =
        [
            Show(183, 1),
            FallInto(P.PlasmaUpRightLoop),
        ],
        [P.PlasmaUpRightLoop] =
        [
            Show(184, 15),
            GoTo(P.PlasmaUpRightLoop),
        ],
        [P.PlasmaRight] =
        [
            Show(185, 1),
            FallInto(P.PlasmaRightLoop),
        ],
        [P.PlasmaRightLoop] =
        [
            Show(186, 15),
            GoTo(P.PlasmaRightLoop),
        ],
        [P.PlasmaDownRight] =
        [
            Show(187, 1),
            FallInto(P.PlasmaDownRightLoop),
        ],
        [P.PlasmaDownRightLoop] =
        [
            Show(188, 15),
            GoTo(P.PlasmaDownRightLoop),
        ],
        [P.PlasmaIceWaveUpFacingRight] =
        [
            Show(189, 1),
            FallInto(P.PlasmaWaveUpFacingRight),
        ],
        [P.PlasmaWaveUpFacingRight] =
        [
            Show(190, 2),
            Show(191, 2),
            Show(192, 2),
            Show(193, 2),
            Show(194, 2),
            Show(195, 2),
            Show(196, 2),
            Show(197, 2),
            GoTo(P.PlasmaWaveUpFacingRight),
        ],
        [P.PlasmaIceWaveUpRight] =
        [
            Show(198, 1),
            FallInto(P.PlasmaWaveUpRight),
        ],
        [P.PlasmaWaveUpRight] =
        [
            Show(199, 2),
            Show(200, 2),
            Show(201, 2),
            Show(202, 2),
            Show(203, 2),
            Show(204, 2),
            Show(205, 2),
            Show(206, 2),
            GoTo(P.PlasmaWaveUpRight),
        ],
        [P.PlasmaWaveRight] =
        [
            Show(207, 1),
            FallInto(P.PlasmaWaveRightLoop),
        ],
        [P.PlasmaWaveRightLoop] =
        [
            Show(208, 2),
            Show(209, 2),
            Show(210, 2),
            Show(211, 2),
            Show(212, 2),
            Show(213, 2),
            Show(214, 2),
            Show(215, 2),
            GoTo(P.PlasmaWaveRightLoop),
        ],
        [P.PlasmaIceWaveDownRight] =
        [
            Show(216, 1),
            FallInto(P.PlasmaWaveDownRight),
        ],
        [P.PlasmaWaveDownRight] =
        [
            Show(217, 2),
            Show(218, 2),
            Show(219, 2),
            Show(220, 2),
            Show(221, 2),
            Show(222, 2),
            Show(223, 2),
            Show(224, 2),
            GoTo(P.PlasmaWaveDownRight),
        ],
        [P.ChargedPowerUpFacingRight] =
        [
            Show(225, 1),
            Show(226, 1),
            GoTo(P.ChargedPowerUpFacingRight),
        ],
        [P.ChargedPowerUpRight] =
        [
            Show(227, 1),
            Show(228, 1),
            GoTo(P.ChargedPowerUpRight),
        ],
        [P.ChargedPowerRight] =
        [
            Show(229, 1),
            Show(230, 1),
            GoTo(P.ChargedPowerRight),
        ],
        [P.ChargedPowerDownRight] =
        [
            Show(231, 1),
            Show(232, 1),
            GoTo(P.ChargedPowerDownRight),
        ],
        [P.ChargedPowerDownFacingRight] =
        [
            Show(233, 1),
            Show(234, 1),
            GoTo(P.ChargedPowerDownFacingRight),
        ],
        [P.ChargedPowerDownLeft] =
        [
            Show(235, 1),
            Show(236, 1),
            GoTo(P.ChargedPowerDownLeft),
        ],
        [P.ChargedPowerLeft] =
        [
            Show(237, 1),
            Show(238, 1),
            GoTo(P.ChargedPowerLeft),
        ],
        [P.ChargedPowerUpLeft] =
        [
            Show(239, 1),
            Show(240, 1),
            GoTo(P.ChargedPowerUpLeft),
        ],
        [P.ChargedWaveUpFacingRight] =
        [
            Show(241, 3),
            FallInto(P.ChargedWaveDownFacingRight),
        ],
        [P.ChargedWaveDownFacingRight] =
        [
            Show(242, 1),
            Show(243, 1),
            Show(244, 1),
            Show(245, 1),
            Show(246, 1),
            Show(247, 1),
            Show(248, 1),
            Show(249, 1),
            Show(250, 1),
            Show(251, 1),
            Show(252, 1),
            Show(253, 1),
            Show(254, 1),
            Show(255, 1),
            Show(256, 1),
            Show(257, 1),
            GoTo(P.ChargedWaveDownFacingRight),
        ],
        [P.ChargedWaveUpRight] =
        [
            Show(258, 1),
            Show(259, 1),
            Show(260, 1),
            Show(261, 1),
            Show(262, 1),
            Show(263, 1),
            Show(264, 1),
            Show(265, 1),
            Show(266, 1),
            Show(267, 1),
            Show(268, 1),
            Show(269, 1),
            Show(270, 1),
            Show(271, 1),
            Show(272, 1),
            Show(273, 1),
            GoTo(P.ChargedWaveUpRight),
        ],
        [P.ChargedWaveRight] =
        [
            Show(274, 1),
            Show(275, 1),
            Show(276, 1),
            Show(277, 1),
            Show(278, 1),
            Show(279, 1),
            Show(280, 1),
            Show(281, 1),
            Show(282, 1),
            Show(283, 1),
            Show(284, 1),
            Show(285, 1),
            Show(286, 1),
            Show(287, 1),
            Show(288, 1),
            Show(289, 1),
            GoTo(P.ChargedWaveRight),
        ],
        [P.ChargedWaveDownRight] =
        [
            Show(290, 1),
            Show(291, 1),
            Show(292, 1),
            Show(293, 1),
            Show(294, 1),
            Show(295, 1),
            Show(296, 1),
            Show(297, 1),
            Show(298, 1),
            Show(299, 1),
            Show(300, 1),
            Show(301, 1),
            Show(302, 1),
            Show(303, 1),
            Show(304, 1),
            Show(305, 1),
            GoTo(P.ChargedWaveDownRight),
        ],
        [P.ChargedIceUpFacingRight] =
        [
            Show(306, 1),
            Show(307, 1),
            Show(308, 1),
            Show(309, 1),
            GoTo(P.ChargedIceUpFacingRight),
        ],
        [P.ChargedIceWaveUpFacingRight] =
        [
            Show(310, 3),
            FallInto(P.ChargedIceWaveDownFacingRight),
        ],
        [P.ChargedIceWaveDownFacingRight] =
        [
            Show(311, 1),
            Show(312, 1),
            Show(313, 1),
            Show(314, 1),
            Show(315, 1),
            Show(316, 1),
            Show(317, 1),
            Show(318, 1),
            Show(319, 1),
            Show(320, 1),
            Show(321, 1),
            Show(322, 1),
            Show(323, 1),
            Show(324, 1),
            Show(325, 1),
            Show(326, 1),
            GoTo(P.ChargedIceWaveDownFacingRight),
        ],
        [P.ChargedIceWaveUpRight] =
        [
            Show(327, 1),
            Show(328, 1),
            Show(329, 1),
            Show(330, 1),
            Show(331, 1),
            Show(332, 1),
            Show(333, 1),
            Show(334, 1),
            Show(335, 1),
            Show(336, 1),
            Show(337, 1),
            Show(338, 1),
            Show(339, 1),
            Show(340, 1),
            Show(341, 1),
            Show(342, 1),
            GoTo(P.ChargedIceWaveUpRight),
        ],
        [P.ChargedIceWaveRight] =
        [
            Show(343, 1),
            Show(344, 1),
            Show(345, 1),
            Show(346, 1),
            Show(347, 1),
            Show(348, 1),
            Show(349, 1),
            Show(350, 1),
            Show(351, 1),
            Show(352, 1),
            Show(353, 1),
            Show(354, 1),
            Show(355, 1),
            Show(356, 1),
            Show(357, 1),
            Show(358, 1),
            GoTo(P.ChargedIceWaveRight),
        ],
        [P.ChargedIceWaveDownRight] =
        [
            Show(359, 1),
            Show(360, 1),
            Show(361, 1),
            Show(362, 1),
            Show(363, 1),
            Show(364, 1),
            Show(365, 1),
            Show(366, 1),
            Show(367, 1),
            Show(368, 1),
            Show(369, 1),
            Show(370, 1),
            Show(371, 1),
            Show(372, 1),
            Show(373, 1),
            Show(374, 1),
            GoTo(P.ChargedIceWaveDownRight),
        ],
        [P.ChargedSpazerUpFacingRight] =
        [
            Show(375, 1),
            Show(376, 1),
            Show(377, 1),
            Show(378, 1),
            Show(379, 1),
            Show(380, 1),
            Show(381, 1),
            Show(382, 1),
            FallInto(P.ChargedSpazerUpFacingRightLoop),
        ],
        [P.ChargedSpazerUpFacingRightLoop] =
        [
            Show(383, 1),
            Show(384, 1),
            GoTo(P.ChargedSpazerUpFacingRightLoop),
        ],
        [P.ChargedSpazerUpRight] =
        [
            Show(385, 1),
            Show(386, 1),
            Show(387, 1),
            Show(388, 1),
            Show(389, 1),
            Show(390, 1),
            Show(391, 1),
            Show(392, 1),
            FallInto(P.ChargedSpazerUpRightLoop),
        ],
        [P.ChargedSpazerUpRightLoop] =
        [
            Show(393, 1),
            Show(394, 1),
            GoTo(P.ChargedSpazerUpRightLoop),
        ],
        [P.ChargedSpazerRight] =
        [
            Show(395, 1),
            Show(396, 1),
            Show(397, 1),
            Show(398, 1),
            Show(399, 1),
            Show(400, 1),
            Show(401, 1),
            Show(402, 1),
            FallInto(P.ChargedSpazerRightLoop),
        ],
        [P.ChargedSpazerRightLoop] =
        [
            Show(403, 1),
            Show(404, 1),
            GoTo(P.ChargedSpazerRightLoop),
        ],
        [P.ChargedSpazerDownRight] =
        [
            Show(405, 1),
            Show(406, 1),
            Show(407, 1),
            Show(408, 1),
            Show(409, 1),
            Show(410, 1),
            Show(411, 1),
            Show(412, 1),
            FallInto(P.ChargedSpazerDownRightLoop),
        ],
        [P.ChargedSpazerDownRightLoop] =
        [
            Show(413, 1),
            Show(414, 1),
            GoTo(P.ChargedSpazerDownRightLoop),
        ],
        [P.ChargedSpazerWaveUpFacingRight] =
        [
            Show(415, 1),
            Show(416, 1),
            Show(417, 1),
            Show(418, 1),
            FallInto(P.ChargedSpazerWaveUpFacingRightLoop),
        ],
        [P.ChargedSpazerWaveUpFacingRightLoop] =
        [
            Show(419, 1),
            Show(420, 1),
            Show(421, 1),
            Show(422, 1),
            Show(423, 1),
            Show(424, 1),
            Show(425, 1),
            Show(426, 1),
            Show(427, 1),
            Show(428, 1),
            Show(429, 1),
            Show(430, 1),
            Show(431, 1),
            Show(432, 1),
            Show(433, 1),
            Show(434, 1),
            Show(435, 1),
            Show(436, 1),
            Show(437, 1),
            Show(438, 1),
            GoTo(P.ChargedSpazerWaveUpFacingRightLoop),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            Show(439, 1),
            Show(440, 1),
            Show(441, 1),
            Show(442, 1),
            FallInto(P.ChargedSpazerWaveUpRightLoop),
        ],
        [P.ChargedSpazerWaveUpRightLoop] =
        [
            Show(443, 1),
            Show(444, 1),
            Show(445, 1),
            Show(446, 1),
            Show(447, 1),
            Show(448, 1),
            Show(449, 1),
            Show(450, 1),
            Show(451, 1),
            Show(452, 1),
            Show(453, 1),
            Show(454, 1),
            Show(455, 1),
            Show(456, 1),
            Show(457, 1),
            Show(458, 1),
            Show(459, 1),
            Show(460, 1),
            Show(461, 1),
            Show(462, 1),
            GoTo(P.ChargedSpazerWaveUpRightLoop),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            Show(463, 1),
            Show(464, 1),
            Show(465, 1),
            Show(466, 1),
            FallInto(P.ChargedSpazerWaveRightLoop),
        ],
        [P.ChargedSpazerWaveRightLoop] =
        [
            Show(467, 1),
            Show(468, 1),
            Show(469, 1),
            Show(470, 1),
            Show(471, 1),
            Show(472, 1),
            Show(473, 1),
            Show(474, 1),
            Show(475, 1),
            Show(476, 1),
            Show(477, 1),
            Show(478, 1),
            Show(479, 1),
            Show(480, 1),
            Show(481, 1),
            Show(482, 1),
            Show(483, 1),
            Show(484, 1),
            Show(485, 1),
            Show(486, 1),
            GoTo(P.ChargedSpazerWaveRightLoop),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            Show(487, 1),
            Show(488, 1),
            Show(489, 1),
            Show(490, 1),
            FallInto(P.ChargedSpazerWaveDownRightLoop),
        ],
        [P.ChargedSpazerWaveDownRightLoop] =
        [
            Show(491, 1),
            Show(492, 1),
            Show(493, 1),
            Show(494, 1),
            Show(495, 1),
            Show(496, 1),
            Show(497, 1),
            Show(498, 1),
            Show(499, 1),
            Show(500, 1),
            Show(501, 1),
            Show(502, 1),
            Show(503, 1),
            Show(504, 1),
            Show(505, 1),
            Show(506, 1),
            Show(507, 1),
            Show(508, 1),
            Show(509, 1),
            Show(510, 1),
            GoTo(P.ChargedSpazerWaveDownRightLoop),
        ],
        [P.ChargedSpazerWaveDownFacingRight] =
        [
            Show(511, 1),
            Show(512, 1),
            Show(513, 1),
            Show(514, 1),
            FallInto(P.ChargedSpazerWaveDownFacingRightLoop),
        ],
        [P.ChargedSpazerWaveDownFacingRightLoop] =
        [
            Show(515, 1),
            Show(516, 1),
            Show(517, 1),
            Show(518, 1),
            Show(519, 1),
            Show(520, 1),
            Show(521, 1),
            Show(522, 1),
            Show(523, 1),
            Show(524, 1),
            Show(525, 1),
            Show(526, 1),
            Show(527, 1),
            Show(528, 1),
            Show(529, 1),
            Show(530, 1),
            Show(531, 1),
            Show(532, 1),
            Show(533, 1),
            Show(534, 1),
            GoTo(P.ChargedSpazerWaveDownFacingRightLoop),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            Show(535, 1),
            Show(536, 1),
            Show(537, 1),
            Show(538, 1),
            FallInto(P.ChargedSpazerWaveDownLeftLoop),
        ],
        [P.ChargedSpazerWaveDownLeftLoop] =
        [
            Show(539, 1),
            Show(540, 1),
            Show(541, 1),
            Show(542, 1),
            Show(543, 1),
            Show(544, 1),
            Show(545, 1),
            Show(546, 1),
            Show(547, 1),
            Show(548, 1),
            Show(549, 1),
            Show(550, 1),
            Show(551, 1),
            Show(552, 1),
            Show(553, 1),
            Show(554, 1),
            Show(555, 1),
            Show(556, 1),
            Show(557, 1),
            Show(558, 1),
            GoTo(P.ChargedSpazerWaveDownLeftLoop),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            Show(559, 1),
            Show(560, 1),
            Show(561, 1),
            Show(562, 1),
            FallInto(P.ChargedSpazerWaveLeftLoop),
        ],
        [P.ChargedSpazerWaveLeftLoop] =
        [
            Show(563, 1),
            Show(564, 1),
            Show(565, 1),
            Show(566, 1),
            Show(567, 1),
            Show(568, 1),
            Show(569, 1),
            Show(570, 1),
            Show(571, 1),
            Show(572, 1),
            Show(573, 1),
            Show(574, 1),
            Show(575, 1),
            Show(576, 1),
            Show(577, 1),
            Show(578, 1),
            Show(579, 1),
            Show(580, 1),
            Show(581, 1),
            Show(582, 1),
            GoTo(P.ChargedSpazerWaveLeftLoop),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            Show(583, 1),
            Show(584, 1),
            Show(585, 1),
            Show(586, 1),
            FallInto(P.ChargedSpazerWaveUpLeftLoop),
        ],
        [P.ChargedSpazerWaveUpLeftLoop] =
        [
            Show(587, 1),
            Show(588, 1),
            Show(589, 1),
            Show(590, 1),
            Show(591, 1),
            Show(592, 1),
            Show(593, 1),
            Show(594, 1),
            Show(595, 1),
            Show(596, 1),
            Show(597, 1),
            Show(598, 1),
            Show(599, 1),
            Show(600, 1),
            Show(601, 1),
            Show(602, 1),
            Show(603, 1),
            Show(604, 1),
            Show(605, 1),
            Show(606, 1),
            GoTo(P.ChargedSpazerWaveUpLeftLoop),
        ],
        [P.ChargedPlasmaUpFacingRight] =
        [
            Show(607, 1),
            Show(608, 1),
            Show(609, 1),
            Show(610, 1),
            Show(611, 1),
            Show(612, 1),
            FallInto(P.ChargedPlasmaUpFacingRightLoop),
        ],
        [P.ChargedPlasmaUpFacingRightLoop] =
        [
            Show(613, 1),
            Show(614, 1),
            GoTo(P.ChargedPlasmaUpFacingRightLoop),
        ],
        [P.ChargedPlasmaUpRight] =
        [
            Show(615, 1),
            Show(616, 1),
            Show(617, 1),
            Show(618, 1),
            Show(619, 1),
            Show(620, 1),
            FallInto(P.ChargedPlasmaUpRightLoop),
        ],
        [P.ChargedPlasmaUpRightLoop] =
        [
            Show(621, 1),
            Show(622, 1),
            GoTo(P.ChargedPlasmaUpRightLoop),
        ],
        [P.ChargedPlasmaRight] =
        [
            Show(623, 1),
            Show(624, 1),
            Show(625, 1),
            Show(626, 1),
            Show(627, 1),
            Show(628, 1),
            FallInto(P.ChargedPlasmaRightLoop),
        ],
        [P.ChargedPlasmaRightLoop] =
        [
            Show(629, 1),
            Show(630, 1),
            GoTo(P.ChargedPlasmaRightLoop),
        ],
        [P.ChargedPlasmaDownRight] =
        [
            Show(631, 1),
            Show(632, 1),
            Show(633, 1),
            Show(634, 1),
            Show(635, 1),
            Show(636, 1),
            FallInto(P.ChargedPlasmaDownRightLoop),
        ],
        [P.ChargedPlasmaDownRightLoop] =
        [
            Show(637, 1),
            Show(638, 1),
            GoTo(P.ChargedPlasmaDownRightLoop),
        ],
        [P.ChargedPlasmaWaveUpFacingRight] =
        [
            Show(639, 1),
            Show(640, 1),
            Show(641, 1),
            Show(642, 1),
            Show(643, 1),
            Show(644, 1),
            FallInto(P.ChargedPlasmaWaveUpFacingRightLoop),
        ],
        [P.ChargedPlasmaWaveUpFacingRightLoop] =
        [
            Show(645, 1),
            Show(646, 1),
            Show(647, 1),
            Show(648, 1),
            Show(649, 1),
            Show(650, 1),
            Show(651, 1),
            Show(652, 1),
            Show(653, 1),
            Show(654, 1),
            Show(655, 1),
            Show(656, 1),
            Show(657, 1),
            Show(658, 1),
            Show(659, 1),
            Show(660, 1),
            GoTo(P.ChargedPlasmaWaveUpFacingRightLoop),
        ],
        [P.ChargedPlasmaWaveUpRight] =
        [
            Show(661, 1),
            Show(662, 1),
            Show(663, 1),
            Show(664, 1),
            Show(665, 1),
            Show(666, 1),
            FallInto(P.ChargedPlasmaWaveUpRightLoop),
        ],
        [P.ChargedPlasmaWaveUpRightLoop] =
        [
            Show(667, 1),
            Show(668, 1),
            Show(669, 1),
            Show(670, 1),
            Show(671, 1),
            Show(672, 1),
            Show(673, 1),
            Show(674, 1),
            Show(675, 1),
            Show(676, 1),
            Show(677, 1),
            Show(678, 1),
            Show(679, 1),
            Show(680, 1),
            Show(681, 1),
            Show(682, 1),
            GoTo(P.ChargedPlasmaWaveUpRightLoop),
        ],
        [P.ChargedPlasmaWaveRight] =
        [
            Show(683, 1),
            Show(684, 1),
            Show(685, 1),
            Show(686, 1),
            Show(687, 1),
            Show(688, 1),
            FallInto(P.ChargedPlasmaWaveRightLoop),
        ],
        [P.ChargedPlasmaWaveRightLoop] =
        [
            Show(689, 1),
            Show(690, 1),
            Show(691, 1),
            Show(692, 1),
            Show(693, 1),
            Show(694, 1),
            Show(695, 1),
            Show(696, 1),
            Show(697, 1),
            Show(698, 1),
            Show(699, 1),
            Show(700, 1),
            Show(701, 1),
            Show(702, 1),
            Show(703, 1),
            Show(704, 1),
            GoTo(P.ChargedPlasmaWaveRightLoop),
        ],
        [P.ChargedPlasmaWaveDownRight] =
        [
            Show(705, 1),
            Show(706, 1),
            Show(707, 1),
            Show(708, 1),
            Show(709, 1),
            Show(710, 1),
            FallInto(P.ChargedPlasmaWaveDownRightLoop),
        ],
        [P.ChargedPlasmaWaveDownRightLoop] =
        [
            Show(711, 1),
            Show(712, 1),
            Show(713, 1),
            Show(714, 1),
            Show(715, 1),
            Show(716, 1),
            Show(717, 1),
            Show(718, 1),
            Show(719, 1),
            Show(720, 1),
            Show(721, 1),
            Show(722, 1),
            Show(723, 1),
            Show(724, 1),
            Show(725, 1),
            Show(726, 1),
            GoTo(P.ChargedPlasmaWaveDownRightLoop),
        ],
        [P.MissileUpFacingRight] =
        [
            Show(727, 15),
            GoTo(P.MissileUpFacingRight),
        ],
        [P.MissileUpRight] =
        [
            Show(728, 15),
            GoTo(P.MissileUpRight),
        ],
        [P.MissileRight] =
        [
            Show(729, 15),
            GoTo(P.MissileRight),
        ],
        [P.MissileDownRight] =
        [
            Show(730, 15),
            GoTo(P.MissileDownRight),
        ],
        [P.MissileDownFacingRight] =
        [
            Show(731, 15),
            GoTo(P.MissileDownFacingRight),
        ],
        [P.MissileDownLeft] =
        [
            Show(732, 15),
            GoTo(P.MissileDownLeft),
        ],
        [P.MissileLeft] =
        [
            Show(733, 15),
            GoTo(P.MissileLeft),
        ],
        [P.MissileUpLeft] =
        [
            Show(734, 15),
            GoTo(P.MissileUpLeft),
        ],
        [P.SuperMissileUpFacingRight] =
        [
            Show(735, 15),
            GoTo(P.SuperMissileUpFacingRight),
        ],
        [P.SuperMissileUpRight] =
        [
            Show(736, 15),
            GoTo(P.SuperMissileUpRight),
        ],
        [P.SuperMissileRight] =
        [
            Show(737, 15),
            GoTo(P.SuperMissileRight),
        ],
        [P.SuperMissileDownRight] =
        [
            Show(738, 15),
            GoTo(P.SuperMissileDownRight),
        ],
        [P.SuperMissileDownFacingRight] =
        [
            Show(739, 15),
            GoTo(P.SuperMissileDownFacingRight),
        ],
        [P.SuperMissileDownLeft] =
        [
            Show(740, 15),
            GoTo(P.SuperMissileDownLeft),
        ],
        [P.SuperMissileLeft] =
        [
            Show(741, 15),
            GoTo(P.SuperMissileLeft),
        ],
        [P.SuperMissileUpLeft] =
        [
            Show(742, 15),
            GoTo(P.SuperMissileUpLeft),
        ],
        [P.SuperMissileLink] =
        [
            Show(743, 15),
            GoTo(P.SuperMissileLink),
        ],
        [P.PowerBomb] =
        [
            Show(744, 5),
            Show(745, 5),
            Show(746, 5),
            GoTo(P.PowerBomb),
        ],
        [P.FastPowerBomb] =
        [
            Show(747, 1),
            Show(748, 1),
            Show(749, 1),
            GoTo(P.FastPowerBomb),
        ],
        [P.Bomb] =
        [
            Show(750, 5),
            Show(751, 5),
            Show(752, 5),
            Show(753, 5),
            GoTo(P.Bomb),
        ],
        [P.FastBomb] =
        [
            Show(754, 1),
            Show(755, 1),
            Show(756, 1),
            Show(757, 1),
            GoTo(P.FastBomb),
        ],
        [P.BeamExplosion] =
        [
            Show(758, 3),
            Show(759, 3),
            Show(760, 3),
            Show(761, 3),
            Show(762, 3),
            Show(763, 3),
            Stop,
        ],
        [P.MissileExplosion] =
        [
            Show(764, 3),
            Show(765, 3),
            Show(766, 3),
            Show(767, 3),
            Show(768, 3),
            Show(769, 3),
            Stop,
        ],
        [P.BombExplosion] =
        [
            Show(770, 2),
            Show(771, 2),
            Show(772, 2),
            Show(773, 2),
            Show(774, 2),
            Stop,
        ],
        [P.PlasmaSBA] =
        [
            Show(775, 2),
            Show(776, 2),
            Show(777, 2),
            Show(778, 2),
            Show(779, 2),
            GoTo(P.PlasmaSBA),
        ],
        [P.SuperMissileExplosion] =
        [
            Show(780, 5),
            Show(781, 5),
            Show(782, 5),
            Show(783, 5),
            Show(784, 5),
            Show(785, 5),
            Stop,
        ],
        [P.Projectile25UpFacingRight] =
        [
            Show(786, 2),
            Show(787, 2),
            Show(788, 2),
            Show(789, 2),
            GoTo(P.Projectile25UpFacingRight),
        ],
        [P.ShinesparkEchoUpFacingRight] =
        [
            Show(790, 2),
            Show(791, 2),
            Show(792, 2),
            Show(793, 2),
            GoTo(P.ShinesparkEchoUpFacingRight),
        ],
        [P.SpazerSBATrailUpFacingRight] =
        [
            Show(794, 2),
            Show(795, 2),
            FallInto(P.SpazerSBATrailUpFacingRightLoop),
        ],
        [P.SpazerSBATrailUpFacingRightLoop] =
        [
            Show(796, 2),
            GoTo(P.SpazerSBATrailUpFacingRightLoop),
        ],
        [P.WaveSBA] =
        [
            Show(797, 8),
            Show(798, 8),
            GoTo(P.WaveSBA),
        ],
        [P.Projectile27] =
        [
            Show(799, 3),
            Show(800, 3),
            Show(801, 3),
            Show(802, 3),
            Show(803, 3),
            Show(804, 3),
            GoTo(P.Projectile27),
        ],
    });

    /// <summary>Shows <paramref name="record"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> Show(int record, int duration) => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.Show(new(record), duration);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> GoTo(SamusProjectileProgram program) => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, the list authored next.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> FallInto(SamusProjectileProgram program) => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallInto(program);

    /// <summary>Deletes the projectile.</summary>
    private static AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> Stop => AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.Stop;
}
