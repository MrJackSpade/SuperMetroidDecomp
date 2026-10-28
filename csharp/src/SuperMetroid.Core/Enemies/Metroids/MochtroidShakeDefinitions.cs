namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Cardinal displacement in shipped shake dispatch $A3:A88F, selected by timer bits1..2.</summary>
internal static class MochtroidShakeDefinitions
{
    /// <summary>$A3:A76D/A775, MocktroidShakeVelocityTable X/Y: the two-pixel shake amplitude.
    /// Axis and sign derive from the timer; the amplitude is an authored scalar (see residualScalarInputsReview).</summary>
    internal const int Amplitude = 2;

    /// <summary>The shake displacement timer bits 1..2 select: right, up, left or down in that order.</summary>
    internal static readonly Offset16<RoomPixels>[] Offsets =
        [new(Amplitude, 0), new(0, -Amplitude), new(-Amplitude, 0), new(0, Amplitude)];
}
