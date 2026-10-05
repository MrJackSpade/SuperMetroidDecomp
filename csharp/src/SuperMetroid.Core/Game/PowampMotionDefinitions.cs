namespace SuperMetroid.Core.Game;

/// <summary>Calculated Powamp balloon motion and clockwise spike acceleration.</summary>
public static class PowampMotionDefinitions
{
    /// <summary>$A8:C1A1, PowampWiggleTable: twelve phases of the signed three-pixel triangle wave.</summary>
    public const int WigglePhaseCount = 12;

    /// <summary>$A8:C1A1, PowampWiggleTable: centered at phases zero and six.</summary>
    public static short WiggleOffset(int phase)
    {
        if ((uint)phase >= WigglePhaseCount)
            throw new InvalidDataException($"Invalid Powamp wiggle phase {phase}.");
        int halfPhase = phase % 6;
        int magnitude = Math.Min(halfPhase, 6 - halfPhase);
        return (short)(phase < 6 ? magnitude : -magnitude);
    }

    /// <summary>$A8:C277/C27D, HandlePowampBalloonYOffset rising/sinking offsets: four pixels per inflation pose.</summary>
    public static short BalloonOffset(int pose, bool sinking)
    {
        if ((uint)pose >= 3)
            throw new IndexOutOfRangeException();
        return (short)(sinking ? -20 + 4 * pose : -12 - 4 * pose);
    }

    /// <summary>$86:D21A/D22A, PowampSpike_VelocityTable_X/Y: eight clockwise compass directions beginning up.</summary>
    public const int SpikeDirectionCount = 8;

    /// <summary>$86:D21A, PowampSpike_VelocityTable_X: signed 8.8 acceleration, zero vertically and ±$20 laterally.</summary>
    public static short SpikeXAcceleration(int direction)
    {
        if ((uint)direction >= SpikeDirectionCount)
            throw new InvalidDataException($"Invalid Powamp spike direction {direction}.");
        return (short)((direction & 3) == 0 ? 0 : direction < 4 ? 32 : -32);
    }

    /// <summary>$86:D22A, PowampSpike_VelocityTable_Y: the X acceleration rotated two compass steps.</summary>
    public static short SpikeYAcceleration(int direction)
    {
        if ((uint)direction >= SpikeDirectionCount)
            throw new InvalidDataException($"Invalid Powamp spike direction {direction}.");
        return SpikeXAcceleration((direction + 6) & 7);
    }
}
