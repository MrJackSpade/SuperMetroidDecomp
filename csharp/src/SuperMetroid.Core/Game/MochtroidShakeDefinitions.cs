namespace SuperMetroid.Core.Game;

/// <summary>Cardinal displacement in shipped shake dispatch $A3:A88F, selected by timer bits1..2.</summary>
internal static class MochtroidShakeDefinitions
{
    /// <summary>$A3:A76D/A775, MocktroidShakeVelocityTable X/Y: the two-pixel shake amplitude.
    /// Axis and sign derive from the timer; the amplitude is an authored scalar (see residualScalarInputsReview).</summary>
    private const int Amplitude = 2;

    /// <summary>Timer bits 1..2 select right, up, left or down in that order.</summary>
    internal static (int X, int Y) Offset(ushort timer) => ((timer & 6) >> 1) switch
    {
        0 => (Amplitude, 0),
        1 => (0, -Amplitude),
        2 => (-Amplitude, 0),
        3 => (0, Amplitude),
        _ => throw new InvalidOperationException($"Mochtroid shake timer ${timer:X4} selected no direction."),
    };
}
