using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.MorphBallEye;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #1684 sixteen-way picks made by arc of the turn against the sixteenth arithmetic they replace: the arc
/// division itself, the Morph Ball eye's aim frames and Ridley's drawn tail tip, each for every angle it can be given.
/// The breakup tail tip and the glass shards are checked against the cartridge by their own suites.
/// </summary>
internal static class TurnArcPicksVerification
{
    /// <summary>Runs every sixteen-way pick check.</summary>
    internal static void VerifyTurnArcPicks()
    {
        VerifyTurnArcs();
        VerifyMorphBallEyeAimFrames();
        VerifyRidleyTailTipFrames();
        Console.WriteLine("  Turn-arc picks: the arc division, the eye's 65536 aims and Ridley's 256 tail-tip frames equal the sixteenth arithmetic.");
    }

    /// <summary>An arc division picks by the angle's place from its start, names each arc's start, and rejects uneven arcs.</summary>
    private static void VerifyTurnArcs()
    {
        var arcs = new TurnArcs<int>(TrigonometryAngle.Wrapped(-8), 0, 1, 2, 3);
        for (int units = 0; units < 256; units++)
            AssertEqual(((units + 8) & 255) >> 6, arcs.At(new TrigonometryAngle(units)), $"four arcs from -8, angle {units}");
        AssertEqual(new TrigonometryAngle(56), arcs.StartOf(1), "second arc opens a quarter turn on from the first");
        AssertThrows<ArgumentException>(() => new TurnArcs<int>(TrigonometryAngle.Zero, 0, 1, 2), "three arcs do not divide the turn");
        AssertThrows<ArgumentException>(() => new TurnArcs<int>(TrigonometryAngle.Zero, 0, 0).StartOf(0), "a value held by two arcs has no single start");
    }

    /// <summary>
    /// The eye's aim frame for all 65536 angles against the frame its sixteenth numbers in the aim list it replaced (#2070):
    /// the list's sixteen frames, the first and last each shown twice.
    /// </summary>
    private static void VerifyMorphBallEyeAimFrames()
    {
        EnemyFrameId[] byPose =
        [
            EnemyFrameId.MorphBallEyeLookingUpUpRight, EnemyFrameId.MorphBallEyeLookingUpUpRight, EnemyFrameId.MorphBallEyeLookingUpRight,
            EnemyFrameId.MorphBallEyeLookingRightUpRight, EnemyFrameId.MorphBallEyeLookingRight, EnemyFrameId.MorphBallEyeLookingRightDownRight,
            EnemyFrameId.MorphBallEyeLookingDownRight, EnemyFrameId.MorphBallEyeLookingDownDownRight, EnemyFrameId.MorphBallEyeLookingDownDownLeft,
            EnemyFrameId.MorphBallEyeLookingDownLeft, EnemyFrameId.MorphBallEyeLookingLeftDownLeft, EnemyFrameId.MorphBallEyeLookingLeft,
            EnemyFrameId.MorphBallEyeLookingLeftUpLeft, EnemyFrameId.MorphBallEyeLookingUpLeft, EnemyFrameId.MorphBallEyeLookingUpUpLeft,
            EnemyFrameId.MorphBallEyeLookingUpUpLeft,
        ];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            RotationAngle angle = RotationAngleConstants.FromRaw(raw);
            AssertEqual(byPose[angle.Sector(16)], MorphBallEyeAnimations.AimFrames.At(angle), $"Morph Ball eye angle {raw} aim");
        }
    }

    /// <summary>Ridley's drawn tail-tip frame for all 256 directions against the replaced sixteenth-numbered table.</summary>
    private static void VerifyRidleyTailTipFrames()
    {
        EnemyFrameId[] byOrientation =
        [
            EnemyFrameId.RidleyTailTipPointingToward180, EnemyFrameId.RidleyTailTipPointingToward157, EnemyFrameId.RidleyTailTipPointingToward135,
            EnemyFrameId.RidleyTailTipPointingToward112, EnemyFrameId.RidleyTailTipPointingToward90, EnemyFrameId.RidleyTailTipPointingToward67,
            EnemyFrameId.RidleyTailTipPointingToward45, EnemyFrameId.RidleyTailTipPointingToward22, EnemyFrameId.RidleyTailTipPointingToward0,
            EnemyFrameId.RidleyTailTipPointingToward337, EnemyFrameId.RidleyTailTipPointingToward315, EnemyFrameId.RidleyTailTipPointingToward292,
            EnemyFrameId.RidleyTailTipPointingToward270, EnemyFrameId.RidleyTailTipPointingToward247, EnemyFrameId.RidleyTailTipPointingToward225,
            EnemyFrameId.RidleyTailTipPointingToward202,
        ];
        // The replaced pick: the direction turned half a pose on, in sixteenths of a turn.
        for (int direction = 0; direction < 256; direction++)
            AssertEqual(byOrientation[((direction + 8) & 255) >> 4], RidleySupplementalVisualDefinitions.TailTipFrames.At(new TrigonometryAngle(direction)),
                $"Ridley tail-tip direction {direction} frame");
    }
}
