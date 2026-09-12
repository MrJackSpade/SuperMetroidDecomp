namespace SuperMetroid.Core.Game;

/// <summary>Native definition tables for Crocomire's mouth volley.</summary>
public static class CrocomireProjectileRomData
{
    /// <summary>$86:9059, CrocomiresProjectile_Gradients, indexed by the body's volley counter.</summary>
    public const int Gradients = 0x869059;
    /// <summary>$86:909B/$909C and $90A7/$90A8 shift each signed component left twice.</summary>
    public const int VelocityMultiplier = 4;

    /// <summary>$86:906B-$906C: $B620, the first two setup-instruction bytes read as the ninth shot's gradient.</summary>
    public const short FinalShotOverreadGradient = unchecked((short)0xb620);

    /// <summary>Reads the nine authored gradients plus the known final-shot overread; preserves ignored low selector bit.</summary>
    public static short GradientForSpawnParameter(ushort parameter) => (parameter >> 1) switch
    {
        0 or 3 or 6 => -16,
        1 or 4 or 7 => 0,
        2 or 5 or 8 => 32,
        9 => FinalShotOverreadGradient,
        _ => throw new InvalidDataException($"Crocomire gradient selector ${parameter:X4} is outside the authored volley and its known final overread."),
    };
}
