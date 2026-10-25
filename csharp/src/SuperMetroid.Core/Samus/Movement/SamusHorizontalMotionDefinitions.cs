using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Pinned bank-$90 horizontal mechanics, separate from animation and palette definitions.</summary>
internal static class SamusHorizontalMotionDefinitions
{
    /// <summary>$90:9F55..A08C normal-air speed records end after damage boost.</summary>
    private const int AirRecordCount = 26;
    /// <summary>$90:A08D and A1DD water/lava records additionally include held and special movement.</summary>
    private const int LiquidRecordCount = 28;
    /// <summary>Records across the three adjacent tables: air, then water, then lava/acid.</summary>
    internal const int RowCount = AirRecordCount + 2 * LiquidRecordCount;

    /// <summary>
    /// The movement type each table's records describe, in record order. The air table stops after damage boost, so
    /// selecting the air table's held or special record reads the water table's first records.
    /// </summary>
    private static readonly SamusMovementType[] RecordOrder =
    [
        SamusMovementType.Standing, SamusMovementType.Running, SamusMovementType.NormalJumping,
        SamusMovementType.SpinJumping, SamusMovementType.MorphBallGround, SamusMovementType.Crouching,
        SamusMovementType.Falling, SamusMovementType.UnusedGlitchBall, SamusMovementType.MorphBallFalling,
        SamusMovementType.UnusedGlitchBallAlternate, SamusMovementType.Knockback, SamusMovementType.Unused0B,
        SamusMovementType.Unused0C, SamusMovementType.Unused0D, SamusMovementType.TurningOnGround,
        SamusMovementType.PostureTransition, SamusMovementType.Moonwalking, SamusMovementType.SpringBallGround,
        SamusMovementType.SpringBallInAir, SamusMovementType.SpringBallFalling, SamusMovementType.WallJumping,
        SamusMovementType.RanIntoWall, SamusMovementType.Grappling, SamusMovementType.TurningWhileJumping,
        SamusMovementType.TurningWhileFalling, SamusMovementType.DamageBoost, SamusMovementType.DraygonHeld,
        SamusMovementType.Special,
    ];

    /// <summary>Each movement type's position in <see cref="RecordOrder"/>.</summary>
    private static readonly FrozenDictionary<SamusMovementType, int> RecordPosition =
        Enumerable.Range(0, RecordOrder.Length).ToFrozenDictionary(position => RecordOrder[position]);

    /// <summary>
    /// The row <c>Samus_DetermineSpeedTableEntryPtr_X</c> selects for a movement type in a medium's table: the
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
    /// Resolves exact records in the three adjacent native tables by row. Table selection adds the
    /// movement type to the table's first row, so air movement types 26/27 deliberately read water rows
    /// 0/1. Rows past the authored records are not treated as additional mechanics definitions.
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
        int decelerationFraction;
        if (air)
        {
            decelerationFraction = 0x8000;
        }
        else
        {
            row -= AirRecordCount;
            water = row < LiquidRecordCount;
            if (!water) row -= LiquidRecordCount;
            decelerationFraction = water ? 0x0800 : 0x4000;
        }
        SamusMovementType movement = RecordOrder[row];
        uint maximum = movement switch
        {
            SamusMovementType.Running => air || water ? 0x2c000u : 0x1c000u,
            SamusMovementType.NormalJumping or SamusMovementType.PostureTransition or
                SamusMovementType.SpringBallInAir or SamusMovementType.Grappling => 0x14000u,
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping => 0x16000u,
            SamusMovementType.MorphBallGround or SamusMovementType.SpringBallGround => air ? 0x34000u : 0x2c000u,
            SamusMovementType.Falling or SamusMovementType.UnusedGlitchBall => 0x10000u,
            SamusMovementType.MorphBallFalling or SamusMovementType.SpringBallFalling =>
                air ? 0x10000u : water ? 0x18000u : 0x16000u,
            SamusMovementType.UnusedGlitchBallAlternate or SamusMovementType.Unused0D => 0x20000u,
            SamusMovementType.Knockback or SamusMovementType.DraygonHeld or SamusMovementType.Special => 0x50000u,
            SamusMovementType.Moonwalking => 0x08000u,
            SamusMovementType.DamageBoost => air ? 0x50000u : 0x08000u,
            SamusMovementType.Standing or SamusMovementType.Crouching or SamusMovementType.Unused0B or
                SamusMovementType.Unused0C or SamusMovementType.TurningOnGround or SamusMovementType.RanIntoWall or
                SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling => 0,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {movement}."),
        };
        // All rows use the same authored acceleration except these named movement cases.
        uint acceleration = movement switch
        {
            SamusMovementType.Running => air ? 0x3000u : 0x0400u,
            SamusMovementType.UnusedGlitchBall or SamusMovementType.UnusedGlitchBallAlternate => 0x20000u,
            SamusMovementType.Knockback => 0x18000u,
            SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling when !air => 0x0400u,
            SamusMovementType.Standing or SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
                SamusMovementType.MorphBallGround or SamusMovementType.Crouching or SamusMovementType.Falling or
                SamusMovementType.MorphBallFalling or SamusMovementType.Unused0B or SamusMovementType.Unused0C or
                SamusMovementType.Unused0D or SamusMovementType.TurningOnGround or
                SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling or SamusMovementType.WallJumping or
                SamusMovementType.RanIntoWall or SamusMovementType.Grappling or
                SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
                SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special => 0xc000u,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {movement}."),
        };
        entry = new(
            SignedSixteenSixteen.FromRaw(unchecked((int)acceleration)),
            SignedSixteenSixteen.FromRaw(unchecked((int)maximum)),
            SignedSixteenSixteen.FromParts(0, decelerationFraction));
        return true;
    }

    /// <summary>$90:9F25 XAccelSpeeds_DiagonalBombJump: acceleration, maximum and deceleration whole/fraction pairs.</summary>
    private static readonly SpeedTableEntry DiagonalBombJump = new(
        Acceleration: SignedSixteenSixteen.FromParts(0, 0x3000),
        MaximumSpeed: SignedSixteenSixteen.FromParts(3, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 0x0800));

    /// <summary>$90:9F31/$9F3D/$9F49 XAccelSpeeds_DisconnectGrappleInAir/InWater/InLavaAcid: three identical authored records.</summary>
    private static readonly SpeedTableEntry GrappleRelease = new(
        Acceleration: SignedSixteenSixteen.FromParts(0, 0x3000),
        MaximumSpeed: SignedSixteenSixteen.FromParts(15, 0),
        Deceleration: SignedSixteenSixteen.FromParts(0, 0x1000));

    /// <summary>The authored standalone record <paramref name="record"/> names.</summary>
    internal static SpeedTableEntry Standalone(SamusStandaloneSpeedRecord record) => record switch
    {
        SamusStandaloneSpeedRecord.DiagonalBombJump => DiagonalBombJump,
        SamusStandaloneSpeedRecord.GrappleReleaseAir or SamusStandaloneSpeedRecord.GrappleReleaseWater or
            SamusStandaloneSpeedRecord.GrappleReleaseLavaAcid => GrappleRelease,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusStandaloneSpeedRecord)} {record}."),
    };
}
