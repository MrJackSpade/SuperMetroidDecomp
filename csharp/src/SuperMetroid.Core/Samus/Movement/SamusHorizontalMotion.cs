using System.Collections.Frozen;
using static SuperMetroid.Core.Samus.Movement.SamusHorizontalMotionDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The horizontal speed record each medium and movement type selects.</summary>
internal static class SamusHorizontalMotion
{
    /// <summary>Each movement type's position in <see cref="RecordOrder"/>; the records no movement type selects have none.</summary>
    private static readonly FrozenDictionary<SamusMovementType, int> RecordPosition =
        RecordOrder.SelectMany((movement, position) => movement is { } selected
                ? [KeyValuePair.Create(selected, position)]
                : Enumerable.Empty<KeyValuePair<SamusMovementType, int>>())
            .ToFrozenDictionary();

    /// <summary>
    /// The row the horizontal speed-table selection picks for a movement type in a medium's table: the
    /// table's first row plus the movement type's record position, which for the air table's held and special
    /// movement lands in the water table.
    /// </summary>
    internal static int Row(SamusLiquidMedium medium, SamusMovementType movement) =>
        FirstRow(medium) + RecordPosition[movement];

    /// <summary>The row of <paramref name="medium"/>'s table's first record among the three adjacent tables.</summary>
    internal static int FirstRow(SamusLiquidMedium medium) => medium switch
    {
        SamusLiquidMedium.Air => 0,
        SamusLiquidMedium.Water => AirRecordCount,
        SamusLiquidMedium.LavaOrAcid => AirRecordCount + LiquidRecordCount,
        _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
    };

    /// <summary>
    /// Resolves exact records in the three adjacent speed tables by row. Table selection adds the
    /// movement type to the table's first row, so air movement types 26/27 deliberately read water rows
    /// 0/1. Rows past the authored records, and the two records no movement type selects, are not mechanics
    /// definitions.
    /// </summary>
    internal static bool TryResolveRow(int row, out SpeedTableEntry entry)
    {
        if ((uint)row >= RowCount)
        {
            entry = default;
            return false;
        }
        bool air = row < AirRecordCount;
        bool water = false;
        SignedSixteenSixteen deceleration;
        if (air)
        {
            deceleration = AirDeceleration;
        }
        else
        {
            row -= AirRecordCount;
            water = row < LiquidRecordCount;
            if (!water) row -= LiquidRecordCount;
            deceleration = water ? WaterDeceleration : LavaDeceleration;
        }
        if (RecordOrder[row] is not { } movement)
        {
            entry = default;
            return false;
        }
        SignedSixteenSixteen maximum = movement switch
        {
            SamusMovementType.Running => air || water ? RunningMaximum : LavaRunningMaximum,
            SamusMovementType.NormalJumping or SamusMovementType.PostureTransition or
                SamusMovementType.SpringBallInAir or SamusMovementType.Grappling => JumpingMaximum,
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping => SpinJumpingMaximum,
            SamusMovementType.MorphBallGround or SamusMovementType.SpringBallGround => air ? AirMorphBallGroundMaximum : LiquidMorphBallGroundMaximum,
            SamusMovementType.Falling or SamusMovementType.GlitchBall => FallingMaximum,
            SamusMovementType.MorphBallFalling or SamusMovementType.SpringBallFalling =>
                air ? FallingMaximum : water ? WaterBallFallingMaximum : LavaBallFallingMaximum,
            SamusMovementType.GlitchBallKnockback or SamusMovementType.CompactAirborne => GlitchBallMaximum,
            SamusMovementType.Knockback or SamusMovementType.DraygonHeld or SamusMovementType.Special => KnockbackMaximum,
            SamusMovementType.Moonwalking => MoonwalkingMaximum,
            SamusMovementType.DamageBoost => air ? KnockbackMaximum : MoonwalkingMaximum,
            SamusMovementType.Standing or SamusMovementType.Crouching or SamusMovementType.TurningOnGround or SamusMovementType.RanIntoWall or
                SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling => SignedSixteenSixteen.Zero,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {movement}."),
        };
        // All rows use the same authored acceleration except these named movement cases.
        SignedSixteenSixteen acceleration = movement switch
        {
            SamusMovementType.Running => air ? AirRunningAcceleration : LiquidGroundAcceleration,
            SamusMovementType.GlitchBall or SamusMovementType.GlitchBallKnockback => GlitchBallAcceleration,
            SamusMovementType.Knockback => KnockbackAcceleration,
            SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling when !air => LiquidGroundAcceleration,
            SamusMovementType.Standing or SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
                SamusMovementType.MorphBallGround or SamusMovementType.Crouching or SamusMovementType.Falling or
                SamusMovementType.MorphBallFalling or SamusMovementType.CompactAirborne or SamusMovementType.TurningOnGround or
                SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling or SamusMovementType.WallJumping or
                SamusMovementType.RanIntoWall or SamusMovementType.Grappling or
                SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
                SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special => DefaultAcceleration,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {movement}."),
        };
        entry = new(acceleration, maximum, deceleration);
        return true;
    }

    /// <summary>The authored standalone record <paramref name="record"/> names.</summary>
    internal static SpeedTableEntry Standalone(SamusStandaloneSpeedRecord record) => record switch
    {
        SamusStandaloneSpeedRecord.DiagonalBombJump => DiagonalBombJump,
        SamusStandaloneSpeedRecord.GrappleReleaseAir or SamusStandaloneSpeedRecord.GrappleReleaseWater or
            SamusStandaloneSpeedRecord.GrappleReleaseLavaAcid => GrappleRelease,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusStandaloneSpeedRecord)} {record}."),
    };
}
