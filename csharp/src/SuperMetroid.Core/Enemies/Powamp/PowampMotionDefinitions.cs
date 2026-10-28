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

    /// <summary>
    /// $86:D21A/D22A, PowampSpike_VelocityTable_X/Y: each compass direction's signed 8.8 acceleration, ±$20 on each
    /// axis the direction leans along. The Y component is the X component rotated two compass steps.
    /// </summary>
    internal static readonly EightEightVector<RoomPixels>[] SpikeAccelerations =
        [new(0, -32), new(32, -32), new(32, 0), new(32, 32), new(0, 32), new(-32, 32), new(-32, 0), new(-32, -32)];
}
