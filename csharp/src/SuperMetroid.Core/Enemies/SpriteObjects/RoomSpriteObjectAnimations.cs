using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Enemies.SpriteObjects.RoomSpriteObjectFrame;
using P = SuperMetroid.Core.Enemies.SpriteObjects.RoomSpriteObjectSegment;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// Every room sprite-object animation the named object kinds reach, as a typed animation: each timed beat shows a
/// <see cref="RoomSpriteObjectFrame"/>, a hold keeps the last frame showing, deletion stops the animation, and a jump
/// continues at another animation.
/// </summary>
internal static class RoomSpriteObjectAnimations
{
    /// <summary>Every room sprite-object animation.</summary>
    internal static AnimationSegments<RoomSpriteObjectSegment, RoomSpriteObjectFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RoomSpriteObjectSegment, AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>[]>
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
        ],
        [P.SmallExplosion] =
        [
            Show(F.SmallExplosion0, 4),
            Show(F.SmallExplosion1, 6),
            Show(F.SmallExplosion2, 5),
            Show(F.SmallExplosion3, 5),
            Show(F.SmallExplosion4, 5),
            Show(F.SmallExplosion5, 6),
        ],
        [P.DudShot] =
        [
            Show(F.DudShot0, 4),
            Show(F.DudShot1, 4),
            Show(F.DudShot2, 4),
            Show(F.DudShot3, 4),
            Show(F.DudShot4, 4),
            Show(F.DudShot5, 4),
        ],
        [P.SmallDudShot] =
        [
            Show(F.PirateLandingDustCloud0, 5),
            Show(F.SmallDustCloud0, 5),
            Show(F.SmallDustCloud1, 5),
            Show(F.SmallDustCloud2, 5),
            Show(F.SmallDustCloud3, 5),
        ],
        [P.SpacePirateLandingDustCloud] =
        [
            Show(F.PirateLandingDustCloud0, 3),
            Show(F.PirateLandingDustCloud1, 3),
            Show(F.PirateLandingDustCloud2, 3),
            Show(F.PirateLandingDustCloud3, 3),
        ],
        [P.Smoke] =
        [
            Show(F.Smoke0, 8),
            Show(F.Smoke1, 8),
            Show(F.Smoke2, 8),
            Show(F.Smoke3, 8),
        ],
        [P.BigExplosion] =
        [
            Show(F.BigExplosion0, 5),
            Show(F.BigExplosion1, 5),
            Show(F.BigExplosion2, 5),
            Show(F.BigExplosion3, 5),
            Show(F.BigExplosion4, 5),
            Show(F.BigExplosion5, 5),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(F.BigDustCloud0, 2),
            Show(F.BigDustCloud1, 2),
            Show(F.BigDustCloud2, 2),
            Show(F.BigDustCloud3, 2),
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
        ],
        [P.PuromiSplash] =
        [
            Show(F.PuromiSplash0, 2),
            Show(F.PuromiSplash0, 2),
            Show(F.PuromiSplash1, 2),
            Show(F.PuromiSplash2, 2),
            Show(F.PuromiSplash4, 2),
        ],
        [P.FallingSparkTrail] =
        [
            Show(F.FallingSparkTrail0, 3),
            Show(F.FallingSparkTrail1, 3),
            Show(F.FallingSparkTrail2, 3),
            Show(F.FallingSparkTrail3, 3),
        ],
        [P.MetroidElectricity] =
        [
            Show(F.MetroidElectricity0, 2),
            Show(F.MetroidElectricity1, 3),
            Show(F.MetroidElectricity2, 2),
            Show(F.MetroidElectricity0, 1),
            Show(F.MetroidElectricity2, 3),
            Show(F.Nothing, 4),
            Show(F.MetroidElectricity3, 4),
            Show(F.MetroidElectricity4, 5),
            Show(F.MetroidElectricity5, 2),
            Show(F.MetroidElectricity4, 1),
            Show(F.Nothing, 4),
            Show(F.MetroidElectricity12, 2),
            Show(F.MetroidElectricity13, 3),
            Show(F.MetroidElectricity14, 4),
            Show(F.Nothing, 7),
            Show(F.MetroidElectricity9, 3),
            Show(F.MetroidElectricity10, 2),
            Show(F.MetroidElectricity11, 1),
            Show(F.MetroidElectricity10, 2),
            Show(F.MetroidElectricity11, 3),
            Show(F.Nothing, 4),
            Show(F.MetroidElectricity15, 4),
            Show(F.MetroidElectricity16, 2),
            Show(F.MetroidElectricity17, 2),
            Show(F.MetroidElectricity15, 3),
            Show(F.Nothing, 1),
            Show(F.MetroidElectricity18, 4),
            Show(F.MetroidElectricity19, 3),
            Show(F.MetroidElectricity20, 3),
            Show(F.MetroidElectricity19, 2),
            Show(F.Nothing, 7),
        ],
        [P.MetroidElectricityLoop] =
        [
            Show(F.MetroidElectricity6, 2),
            Show(F.MetroidElectricity7, 3),
            Show(F.MetroidElectricity8, 4),
            Show(F.MetroidElectricity6, 2),
            Show(F.MetroidElectricity8, 1),
            Show(F.Nothing, 3),
            Show(F.MetroidElectricity21, 2),
            Show(F.MetroidElectricity22, 3),
            Show(F.MetroidElectricity23, 4),
            Show(F.MetroidElectricity21, 1),
            Show(F.Nothing, 3),
            Show(F.MetroidElectricity6, 3),
            Show(F.MetroidElectricity8, 4),
            Show(F.Nothing, 32),
            Show(F.MetroidElectricity0, 2),
            Show(F.MetroidElectricity1, 1),
            Show(F.MetroidElectricity2, 2),
            Show(F.MetroidElectricity0, 1),
            Show(F.Nothing, 4),
            Show(F.MetroidElectricity15, 3),
            Show(F.MetroidElectricity16, 4),
            Show(F.MetroidElectricity17, 5),
            Show(F.Nothing, 4),
            Show(F.MetroidElectricity9, 2),
            Show(F.MetroidElectricity10, 1),
            Show(F.MetroidElectricity11, 3),
            Show(F.MetroidElectricity9, 1),
            Show(F.Nothing, 6),
            Show(F.MetroidElectricity3, 3),
            Show(F.MetroidElectricity4, 4),
            Show(F.MetroidElectricity5, 2),
        ],
        [P.MetroidShell] =
        [
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell1, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
        ],
        [P.MetroidShellLoop] =
        [
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell0, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
            Show(F.MetroidShell2, 1),
            Show(F.Nothing, 1),
        ],
        [P.YappingMawBaseFacingDown] =
        [
            Show(F.YappingMawBaseFacingDown, 1),
        ],
        [P.YappingMawBaseFacingUp] =
        [
            Show(F.YappingMawBaseFacingUp, 1),
        ],
        [P.EvirFacingLeft] =
        [
            Show(F.EvirFacingLeft0, 10),
            Show(F.EvirFacingLeft1, 10),
            Show(F.EvirFacingLeft2, 10),
            Show(F.EvirFacingLeft3, 10),
        ],
        [P.EvirFacingRight] =
        [
            Show(F.EvirFacingRight0, 10),
            Show(F.EvirFacingRight1, 10),
            Show(F.EvirFacingRight2, 10),
            Show(F.EvirFacingRight3, 10),
        ],
        [P.EnemyHit] =
        [
            Show(F.EnemyHit0, 2),
            Show(F.EnemyHit1, 2),
            Show(F.EnemyHit2, 2),
            Show(F.EnemyHit3, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RoomSpriteObjectSegment, Animation<RoomSpriteObjectSegment>> StartingAt { get; } = new Dictionary<RoomSpriteObjectSegment, Animation<RoomSpriteObjectSegment>>
    {
        [P.DraygonFoamingAtTheMouth] = new([new(P.DraygonFoamingAtTheMouth, 0, 8, 1)], AnimationEnd.Stops),
        [P.SmallExplosion] = new([new(P.SmallExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.DudShot] = new([new(P.DudShot, 0, 6, 1)], AnimationEnd.Stops),
        [P.SmallDudShot] = new([new(P.SmallDudShot, 0, 5, 1)], AnimationEnd.Stops),
        [P.SpacePirateLandingDustCloud] = new([new(P.SpacePirateLandingDustCloud, 0, 4, 1)], AnimationEnd.Stops),
        [P.Smoke] = new([new(P.Smoke, 0, 4, 1)], AnimationEnd.Stops),
        [P.BigExplosion] = new([new(P.BigExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.ShortBigDustCloud] = new([new(P.ShortBigDustCloud, 0, 4, 1)], AnimationEnd.Stops),
        [P.BigDustCloud] = new([new(P.BigDustCloud, 0, 8, 1)], AnimationEnd.Stops),
        [P.ShortDraygonBreathBubbles] = new([new(P.ShortDraygonBreathBubbles, 0, 9, 1)], AnimationEnd.Stops),
        [P.PuromiBody] = new([new(P.PuromiBody, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PuromiRightExplosion] = new([new(P.PuromiRightExplosion, 0, 7, 1)], AnimationEnd.Stops),
        [P.PuromiLeftExplosion] = new([new(P.PuromiLeftExplosion, 0, 7, 1)], AnimationEnd.Stops),
        [P.PuromiSplash] = new([new(P.PuromiSplash, 0, 5, 1)], AnimationEnd.Stops),
        [P.FallingSparkTrail] = new([new(P.FallingSparkTrail, 0, 4, 1)], AnimationEnd.Stops),
        [P.MetroidElectricity] = new([new(P.MetroidElectricity, 0, 31, 1), new(P.MetroidElectricityLoop, 0, 31, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetroidElectricityLoop] = new([new(P.MetroidElectricityLoop, 0, 31, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidShell] = new([new(P.MetroidShell, 0, 32, 1), new(P.MetroidShellLoop, 0, 30, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetroidShellLoop] = new([new(P.MetroidShellLoop, 0, 30, 1)], AnimationEnd.LoopsFrom(0)),
        [P.YappingMawBaseFacingDown] = new([new(P.YappingMawBaseFacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.YappingMawBaseFacingUp] = new([new(P.YappingMawBaseFacingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.EvirFacingLeft] = new([new(P.EvirFacingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EvirFacingRight] = new([new(P.EvirFacingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EnemyHit] = new([new(P.EnemyHit, 0, 4, 1)], AnimationEnd.Stops),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RoomSpriteObjectSegment> StartingWith(RoomSpriteObjectSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue> Show(RoomSpriteObjectFrame frame, int duration) => new AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>.Frame(frame, duration);
}
