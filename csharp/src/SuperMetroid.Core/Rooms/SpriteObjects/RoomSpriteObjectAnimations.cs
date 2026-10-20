using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Rooms.SpriteObjects.RoomSpriteObjectFrame;
using P = SuperMetroid.Core.Rooms.SpriteObjects.RoomSpriteObjectProgram;

namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>
/// Every bank-$B4 room sprite-object list the named object kinds reach, as a typed program generated from the native
/// lists: each timed record shows a <see cref="RoomSpriteObjectFrame"/>, the repeat-last instruction holds the last frame,
/// delete stops the program, and a goto continues at another program.
/// </summary>
internal static class RoomSpriteObjectAnimations
{
    /// <summary>Every room sprite-object program.</summary>
    internal static AnimationProgramSet<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<RoomSpriteObjectProgram, AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>[]>
    {
        [P.DraygonFoamingAtTheMouth] =
        [
            Show(F.DraygonFoamingAtTheMouth0, 6),
            Show(F.DraygonFoamingAtTheMouth1, 6),
            Show(F.DraygonFoamingAtTheMouth2, 7),
            Show(F.DraygonFoamingAtTheMouth3, 7),
            Show(F.DraygonFoamingAtTheMouth4, 8),
            Show(F.DraygonFoamingAtTheMouth5, 8),
            Show(F.DraygonFoamingAtTheMouth6, 9),
            Show(F.DraygonFoamingAtTheMouth7, 9),
            Stop,
        ],
        [P.SmallExplosion] =
        [
            Show(F.SmallExplosion0, 4),
            Show(F.SmallExplosion1, 6),
            Show(F.SmallExplosion2, 5),
            Show(F.SmallExplosion3, 5),
            Show(F.SmallExplosion4, 5),
            Show(F.SmallExplosion5, 6),
            Stop,
        ],
        [P.DudShot] =
        [
            Show(F.DudShot0, 4),
            Show(F.DudShot1, 4),
            Show(F.DudShot2, 4),
            Show(F.DudShot3, 4),
            Show(F.DudShot4, 4),
            Show(F.DudShot5, 4),
            Stop,
        ],
        [P.SmallDudShot] =
        [
            Show(F.PirateLandingDustCloud0, 5),
            Show(F.SmallDustCloud0, 5),
            Show(F.SmallDustCloud1, 5),
            Show(F.SmallDustCloud2, 5),
            Show(F.SmallDustCloud3, 5),
            Stop,
        ],
        [P.SpacePirateLandingDustCloud] =
        [
            Show(F.PirateLandingDustCloud0, 3),
            Show(F.PirateLandingDustCloud1, 3),
            Show(F.PirateLandingDustCloud2, 3),
            Show(F.PirateLandingDustCloud3, 3),
            Stop,
        ],
        [P.Smoke] =
        [
            Show(F.Smoke0, 8),
            Show(F.Smoke1, 8),
            Show(F.Smoke2, 8),
            Show(F.Smoke3, 8),
            Stop,
        ],
        [P.BigExplosion] =
        [
            Show(F.BigExplosion0, 5),
            Show(F.BigExplosion1, 5),
            Show(F.BigExplosion2, 5),
            Show(F.BigExplosion3, 5),
            Show(F.BigExplosion4, 5),
            Show(F.BigExplosion5, 5),
            Stop,
        ],
        [P.ShortBigDustCloud] =
        [
            Show(F.BigDustCloud0, 2),
            Show(F.BigDustCloud1, 2),
            Show(F.BigDustCloud2, 2),
            Show(F.BigDustCloud3, 2),
            Stop,
        ],
        [P.BigDustCloud] =
        [
            Show(F.BigDustCloud0, 5),
            Show(F.BigDustCloud1, 5),
            Show(F.BigDustCloud2, 5),
            Show(F.BigDustCloud3, 5),
            Show(F.BigDustCloud4, 5),
            Show(F.BigDustCloud5, 5),
            Show(F.BigDustCloud6, 5),
            Show(F.BigDustCloud7, 5),
            Stop,
        ],
        [P.ShortDraygonBreathBubbles] =
        [
            Show(F.ShortDraygonBreathBubbles0, 3),
            Show(F.ShortDraygonBreathBubbles1, 3),
            Show(F.ShortDraygonBreathBubbles2, 3),
            Show(F.ShortDraygonBreathBubbles3, 3),
            Show(F.ShortDraygonBreathBubbles4, 3),
            Show(F.ShortDraygonBreathBubbles5, 3),
            Show(F.ShortDraygonBreathBubbles6, 3),
            Show(F.ShortDraygonBreathBubbles7, 3),
            Show(F.ShortDraygonBreathBubbles8, 3),
            Stop,
        ],
        [P.PuromiBody] =
        [
            Show(F.Puromi0, 3),
            Show(F.Puromi1, 3),
            Show(F.Puromi4, 3),
            Show(F.Puromi2, 3),
            Show(F.Puromi3, 3),
            Show(F.Puromi5, 3),
            Show(F.Puromi0, 3),
            Show(F.Puromi1, 3),
            Show(F.Puromi6, 3),
            Show(F.Puromi2, 3),
            Show(F.Puromi3, 3),
            Show(F.Puromi7, 3),
            GoTo(P.PuromiBody),
        ],
        [P.PuromiRightExplosion] =
        [
            Show(F.PuromiRightExplosion0, 2),
            Show(F.PuromiRightExplosion1, 2),
            Show(F.PuromiRightExplosion2, 2),
            Show(F.PuromiRightExplosion3, 2),
            Show(F.PuromiRightExplosion4, 2),
            Show(F.PuromiRightExplosion5, 2),
            Show(F.PuromiRightExplosion6, 2),
            Stop,
        ],
        [P.PuromiLeftExplosion] =
        [
            Show(F.PuromiLeftExplosion0, 2),
            Show(F.PuromiLeftExplosion1, 2),
            Show(F.PuromiLeftExplosion2, 2),
            Show(F.PuromiLeftExplosion3, 2),
            Show(F.PuromiLeftExplosion4, 2),
            Show(F.PuromiLeftExplosion5, 2),
            Show(F.PuromiLeftExplosion6, 2),
            Stop,
        ],
        [P.PuromiSplash] =
        [
            Show(F.PuromiSplash0, 2),
            Show(F.PuromiSplash0, 2),
            Show(F.PuromiSplash1, 2),
            Show(F.PuromiSplash2, 2),
            Show(F.PuromiSplash4, 2),
            Stop,
        ],
        [P.FallingSparkTrail] =
        [
            Show(F.FallingSparkTrail0, 3),
            Show(F.FallingSparkTrail1, 3),
            Show(F.FallingSparkTrail2, 3),
            Show(F.FallingSparkTrail3, 3),
            Stop,
        ],
        [P.MetroidElectricity] =
        [
            Show(F.MetroidElectricity0, 2),
            Show(F.MetroidElectricity1, 3),
            Show(F.MetroidElectricity2, 2),
            Show(F.MetroidElectricity0, 1),
            Show(F.MetroidElectricity2, 3),
            Show(F.SpriteObjectDrawInstEmpty, 4),
            Show(F.MetroidElectricity3, 4),
            Show(F.MetroidElectricity4, 5),
            Show(F.MetroidElectricity5, 2),
            Show(F.MetroidElectricity4, 1),
            Show(F.SpriteObjectDrawInstEmpty, 4),
            Show(F.MetroidElectricityC, 2),
            Show(F.MetroidElectricityD, 3),
            Show(F.MetroidElectricityE, 4),
            Show(F.SpriteObjectDrawInstEmpty, 7),
            Show(F.MetroidElectricity9, 3),
            Show(F.MetroidElectricityA, 2),
            Show(F.MetroidElectricityB, 1),
            Show(F.MetroidElectricityA, 2),
            Show(F.MetroidElectricityB, 3),
            Show(F.SpriteObjectDrawInstEmpty, 4),
            Show(F.MetroidElectricityF, 4),
            Show(F.MetroidElectricity10, 2),
            Show(F.MetroidElectricity11, 2),
            Show(F.MetroidElectricityF, 3),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidElectricity12, 4),
            Show(F.MetroidElectricity13, 3),
            Show(F.MetroidElectricity14, 3),
            Show(F.MetroidElectricity13, 2),
            Show(F.SpriteObjectDrawInstEmpty, 7),
            FallInto(P.Unused33),
        ],
        [P.Unused33] =
        [
            Show(F.MetroidElectricity6, 2),
            Show(F.MetroidElectricity7, 3),
            Show(F.MetroidElectricity8, 4),
            Show(F.MetroidElectricity6, 2),
            Show(F.MetroidElectricity8, 1),
            Show(F.SpriteObjectDrawInstEmpty, 3),
            Show(F.MetroidElectricity15, 2),
            Show(F.MetroidElectricity16, 3),
            Show(F.MetroidElectricity17, 4),
            Show(F.MetroidElectricity15, 1),
            Show(F.SpriteObjectDrawInstEmpty, 3),
            Show(F.MetroidElectricity6, 3),
            Show(F.MetroidElectricity8, 4),
            Show(F.SpriteObjectDrawInstEmpty, 32),
            Show(F.MetroidElectricity0, 2),
            Show(F.MetroidElectricity1, 1),
            Show(F.MetroidElectricity2, 2),
            Show(F.MetroidElectricity0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 4),
            Show(F.MetroidElectricityF, 3),
            Show(F.MetroidElectricity10, 4),
            Show(F.MetroidElectricity11, 5),
            Show(F.SpriteObjectDrawInstEmpty, 4),
            Show(F.MetroidElectricity9, 2),
            Show(F.MetroidElectricityA, 1),
            Show(F.MetroidElectricityB, 3),
            Show(F.MetroidElectricity9, 1),
            Show(F.SpriteObjectDrawInstEmpty, 6),
            Show(F.MetroidElectricity3, 3),
            Show(F.MetroidElectricity4, 4),
            Show(F.MetroidElectricity5, 2),
            GoTo(P.Unused33),
        ],
        [P.MetroidShell] =
        [
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell1, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            FallInto(P.Unused35),
        ],
        [P.Unused35] =
        [
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell0, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            Show(F.MetroidShell2, 1),
            Show(F.SpriteObjectDrawInstEmpty, 1),
            GoTo(P.Unused35),
        ],
        [P.YappingMawBaseFacingDown] =
        [
            Show(F.YappingMawBaseFacingDown, 1),
            Hold,
        ],
        [P.YappingMawBaseFacingUp] =
        [
            Show(F.YappingMawBaseFacingUp, 1),
            Hold,
        ],
        [P.EvirFacingLeft] =
        [
            Show(F.EvirFacingLeft0, 10),
            Show(F.EvirFacingLeft1, 10),
            Show(F.EvirFacingLeft2, 10),
            Show(F.EvirFacingLeft3, 10),
            GoTo(P.EvirFacingLeft),
        ],
        [P.EvirFacingRight] =
        [
            Show(F.EvirFacingRight0, 10),
            Show(F.EvirFacingRight1, 10),
            Show(F.EvirFacingRight2, 10),
            Show(F.EvirFacingRight3, 10),
            GoTo(P.EvirFacingRight),
        ],
    });

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> Show(RoomSpriteObjectFrame frame, int duration) => AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.Show(frame, duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> GoTo(RoomSpriteObjectProgram program) => AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> FallInto(RoomSpriteObjectProgram program) => AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.FallInto(program);

    /// <summary>Keeps the last frame.</summary>
    private static AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> Hold => AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.Hold;

    /// <summary>Deletes the object.</summary>
    private static AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> Stop => AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.Stop;
}
