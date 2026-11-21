using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms that the aims folded into one angle convention (#2555) read the game's sine exactly as before. The cartridge's
/// aim calculation gives a word counted clockwise from up, which its consumers took as a sine-table angle unconverted: its
/// sine across and its sine a quarter turn back down. The game now takes the aim as the angle anticlockwise from right and
/// reads its cosine across and its negative sine down. For every one of the 256 aim words this check compares every
/// read each consumer makes (the signed and fine sines, the one-pixel and volley velocities, the eye-door shot's
/// acceleration, the fly's dive, Rio's hover, the thrown Super Missile folded either way and the short circle's pixels at
/// every radius) with the read of the unconverted word, and the compass directions each aim picks with the word's
/// octant arithmetic.
/// </summary>
internal static class AimAngleConventionVerification
{
    /// <summary>The largest radius the short circle reads compared: every radius the Yapping Maw's target product takes.</summary>
    private const int LargestRadius = 255;

    /// <summary>The eight compass directions in clockwise order from up, as the cartridge's octants count them.</summary>
    /// <summary>The straight directions clockwise from up, one a quarter of the turn.</summary>
    private static readonly StraightDirection[] QuarterDirections = [StraightDirection.Up, StraightDirection.Right, StraightDirection.Down, StraightDirection.Left];

    private static readonly CompassDirection[] OctantDirections =
    [
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft,
    ];

    /// <summary>Compares every aim word's unconverted reads with the reads of its angle in the game's one convention.</summary>
    internal static void VerifyAimAngleConvention()
    {
        TrigonometryAngle quarter = TrigonometryAngle.QuarterTurn, half = TrigonometryAngle.HalfTurn;
        int compared = 0;
        for (int word = 0; word < 256; word++)
        {
            // The word read unconverted, and the same aim in the one convention.
            var unconverted = new TrigonometryAngle(word);
            TrigonometryAngle aim = TrigonometryAngle.Wrapped(quarter.RawValue - word);
            string context = $"aim word {word}";

            // The sine across is the aim's cosine, the sine a quarter back down its negative sine, and the reverse.
            foreach ((TrigonometryAngle before, TrigonometryAngle after, string read) in new[]
                {
                    (unconverted, aim + quarter, "across"), (unconverted - quarter, aim + half, "down"),
                    (unconverted + quarter, aim, "across turned on"), (unconverted + half, aim - quarter, "down turned on"),
                })
            {
                AssertEqual(Trigonometry.SignedSine(before), Trigonometry.SignedSine(after), $"{context} signed sine {read}");
                AssertEqual(Trigonometry.FineSignedSine(before), Trigonometry.FineSignedSine(after), $"{context} fine sine {read}");
                foreach (int peak in new[] { 1, 2, CrocomireProjectileDefinitions.VolleySpeed })
                    AssertEqual(EnemyMath.SineVelocity(before, peak), EnemyMath.SineVelocity(after, peak), $"{context} {peak}-pixel velocity {read}");
                AssertEqual(EyeDoorBehavior.ReadEyeDoorAcceleration(before), EyeDoorBehavior.ReadEyeDoorAcceleration(after), $"{context} eye-door acceleration {read}");
                for (int radius = 0; radius <= LargestRadius; radius++)
                    AssertEqual(EnemyMath.CircleSinePixels(before, radius), EnemyMath.CircleSinePixels(after, radius), $"{context} circle pixels at radius {radius} {read}");
                compared++;
            }

            // Each consumer's whole velocity, against the unconverted word's sine across and sine a quarter back down.
            AssertEqual(new PixelVelocity<RoomPixels>(
                    EnemyMath.SineVelocity(unconverted, CrocomireProjectileDefinitions.VolleySpeed),
                    EnemyMath.SineVelocity(unconverted - quarter, CrocomireProjectileDefinitions.VolleySpeed)),
                CrocomireCombat.CalculateCrocomireProjectileVelocity(aim), $"{context} Crocomire volley");
            AssertEqual(new PixelVelocity<RoomPixels>(
                    EnemyMath.SineVelocity(unconverted, FlyMotionDefinitions.DiveAcrossSpeed),
                    EnemyMath.SineVelocity(unconverted - quarter, FlyMotionDefinitions.DiveDownSpeed)),
                FliesBehavior.DiveVelocity(aim), $"{context} fly dive");
            AssertEqual(RioBehavior.HoverVelocity(unconverted + quarter), RioBehavior.HoverVelocity(aim), $"{context} Rio hover across");
            AssertEqual(RioBehavior.HoverVelocity(unconverted), RioBehavior.HoverVelocity(aim + quarter), $"{context} Rio hover down");
            foreach ((TrigonometryAngle folded, TrigonometryAngle foldedAim, string way) in new[]
                {
                    (unconverted.InFirstHalfTurn(), aim.InHalfTurnToward(HorizontalDirection.Right), "toward"),
                    (unconverted.InSecondHalfTurn(), aim.InHalfTurnToward(HorizontalDirection.Left), "away"),
                })
            {
                SignedEightEight speed = TorizoMotionDefinitions.ThrownSuperMissileSpeed;
                AssertEqual(TorizoAttacks.VelocityAlong(speed, folded), new PixelVelocity<RoomPixels>(
                        SpeedAlongAngle.Along(speed, foldedAim + quarter), SpeedAlongAngle.Along(speed, foldedAim + half)),
                    $"{context} thrown Super Missile {way}");
            }

            // The compass directions the aim picks are the word's octant, nearest octant and nearest quarter.
            AssertEqual(OctantDirections[word / 32], aim.DirectionOfEighth(), $"{context} octant");
            AssertEqual(OctantDirections[(word + 16) % 256 / 32], aim.NearestDirection(), $"{context} nearest octant");
            AssertEqual(QuarterDirections[(word + 32) % 256 / 64], aim.NearestStraightDirection(), $"{context} nearest quarter");
        }
        Console.WriteLine($"Aim angle convention: {compared} sine, velocity, acceleration and circle reads over every radius of all 256 aim words, " +
            "and every consumer's velocity and compass pick, equal the unconverted word's.");
    }

    /// <summary>Runs the aim angle convention check.</summary>
    [VerificationCommand("--aim-angle-convention")]
    internal static int AimAngleConventionCommand()
    {
        SuiteTimingVerification.Suite(nameof(VerifyAimAngleConvention), VerifyAimAngleConvention);
        return 0;
    }
}
