namespace SuperMetroid.Core.Game;

/// <summary>Periodic eye-particle definition shared by the two bank-$A5 eye handlers.</summary>
public static class DraygonEyeEffects
{
    /// <summary>$A5:C48D/$C513 test the eye frame counter's low seven bits before spawning.</summary>
    public const ushort CadenceMask = 0x7f;
    /// <summary>$A5:C48D/$C513 pass variant $18 to the shared $86:E509 dust initializer.</summary>
    public const ushort DustVariant = 0x18;
    /// <summary>Both native eye handlers offset the particle 24 pixels toward the facing side.</summary>
    public const int HorizontalOffset = 24;
    /// <summary>Both native eye handlers place the particle 32 pixels above the eye record.</summary>
    public const int VerticalOffset = -32;
}
