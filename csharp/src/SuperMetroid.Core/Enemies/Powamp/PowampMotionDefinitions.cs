namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp balloon motion and clockwise spike acceleration.</summary>
public static class PowampMotionDefinitions
{
    /// <summary>$A8:C1A1, PowampWiggleTable: twelve phases of the signed three-pixel triangle wave.</summary>
    public const int WigglePhaseCount = 12;

    /// <summary>$A8:C1A1, PowampWiggleTable: the triangle wave's offset at each phase, centered at phases zero and six.</summary>
    internal static readonly short[] WiggleOffsets = [0, 1, 2, 3, 2, 1, 0, -1, -2, -3, -2, -1];

    /// <summary>$A8:C277, HandlePowampBalloonYOffset rising offsets: four pixels higher per inflation pose.</summary>
    internal static readonly short[] RisingBalloonOffsets = [-12, -16, -20];

    /// <summary>$A8:C27D, HandlePowampBalloonYOffset sinking offsets: four pixels lower per inflation pose.</summary>
    internal static readonly short[] SinkingBalloonOffsets = [-20, -16, -12];

    /// <summary>$86:D21A/D22A, PowampSpike_VelocityTable_X/Y: eight clockwise compass directions beginning up.</summary>
    public const int SpikeDirectionCount = 8;

    /// <summary>$86:D21A, PowampSpike_VelocityTable_X: signed 8.8 acceleration, zero vertically and ±$20 laterally.</summary>
    internal static readonly short[] SpikeXAccelerations = [0, 32, 32, 32, 0, -32, -32, -32];

    /// <summary>$86:D22A, PowampSpike_VelocityTable_Y: the X acceleration rotated two compass steps.</summary>
    internal static readonly short[] SpikeYAccelerations = [-32, -32, 0, 32, 32, 32, 0, -32];
}
