namespace SuperMetroid.Core.Game;

/// <summary>Cardinal displacement in shipped shake dispatch $A3:A88F, selected by timer bits1..2.</summary>
internal static class MochtroidShakeDefinitions
{
    /// <summary>$A3:A76D/A775, MocktroidShakeVelocityTable X/Y: the two-pixel shake amplitude.
    /// Axis and sign derive from the timer; the amplitude is an authored scalar (see residualScalarInputsReview).</summary>
    private const int Amplitude = 2;

    internal static (int X, int Y) Offset(ushort timer)
    {
        int direction = (timer & 6) >> 1;
        int signedAmplitude = (direction & 2) == 0 ? Amplitude : -Amplitude;
        return (direction & 1) == 0 ? (signedAmplitude, 0) : (0, -signedAmplitude);
    }
}
