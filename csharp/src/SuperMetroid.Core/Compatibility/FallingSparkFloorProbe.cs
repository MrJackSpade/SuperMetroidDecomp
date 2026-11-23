namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a falling spark's floor probe creeps it down before it falls (#2064). In each update a spark falls, it first
/// looks for the floor by moving down through the room's blocks, and that probe moves it by its whole fall speed taken
/// as so many 256ths of a pixel: nothing at rest, a 256th of a pixel at one pixel an update, up to four 256ths at its
/// fastest. The probe's move stays, and the spark then falls by its own speed as well, so it creeps slightly faster than
/// its speed says; a floor the probe meets is the spark's landing. The creep is gameplay-observable: without it a spark
/// lands on a later update, or at another height, than the game's.
/// </summary>
/// <remarks>
/// Consulted once in each enemy-projectile update of a falling spark whose whole fall speed is zero or more, before
/// gravity speeds it up and before it falls by its own speed; the spark moves through the enemy projectiles' block
/// collision by the creep, and a floor met there starts its floor impact. Evidence: the spark's falling routine hands the
/// whole-pixel word of its fall speed to the enemy projectiles' vertical block-collision mover, which reads its argument
/// as a speed in 256ths of a pixel, and the spark's own sixteen-bit fall is added afterwards.
/// </remarks>
public sealed partial class FallingSparkFloorProbe
{
    /// <summary>The creep for each whole pixel of fall speed: a 256th of a pixel.</summary>
    private readonly SignedEightEight _creepPerWholePixel = SignedEightEight.FinestStep;

    /// <summary>
    /// The distance the probe creeps a spark falling at <paramref name="fallSpeed"/>, as a speed of one update: a 256th of
    /// a pixel for each whole pixel of the speed.
    /// </summary>
    /// <param name="fallSpeed">The spark's fall speed, whose whole pixels are zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The spark is rising: its whole fall speed is below zero.</exception>
    public SignedEightEight CreepFor(SignedSixteenSixteen fallSpeed) =>
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            Ensure.AtLeastZero(fallSpeed.WholePixels) * _creepPerWholePixel.InTwoHundredFiftySixthsOfPixel);
}
