namespace SuperMetroid.Core.Game;

/// <summary>Authored bank-$A9 RGB5 images used by the live Shitroid encounter.</summary>
public static class ShitroidColorRomData
{
    /// <summary>$A9:F6D1, eight four-color frames for the actor's normal palette cycle.</summary>
    public const int NormalCycle = 0xa9f6d1;

    /// <summary>$A9:F8C6, sixteen-color target image for the sidehopper victim.</summary>
    public const int SidehopperTarget = 0xa9f8c6;

    /// <summary>$A9:F8E6, sixteen-color target image for Shitroid.</summary>
    public const int ShitroidTarget = 0xa9f8e6;

    /// <summary>$A9:F8A6, sixteen-color target image for the dead sidehopper.</summary>
    public const int DeadSidehopperTarget = 0xa9f8a6;

    /// <summary>$A9:F6D1-F6D7 repeats the four innard colors at $F8F0-F8F6; first index in the fifteen-color initial image.</summary>
    public const int NormalFirstInitialColor = 4;

    /// <summary>$A9:F6D1-F710 subtracts five RGB5 units at each of four dimming levels before retracing. Reviewed organ-only symmetric painted light performance; independent of phase timing and cry scheduling.</summary>
    public const int NormalOrganDimmingStep = 5;

    /// <summary>$A9:F6D1/F6D3 red highlights clip to31 in the first two levels, requiring five units above their target red before clipping. Reviewed clipped organ-highlight policy; its magnitude is distinct from the dimming step.</summary>
    public const int NormalOrganRedHeadroom = 5;

    /// <summary>$A9:F6E7/F6EF darkest innard red stops at5 while green/blue reach zero. Reviewed residual organ-red paint intensity; this is not a gameplay threshold.</summary>
    public const int NormalOrganRedFloor = 5;
    /// <summary>Eight normal innard-pulse images at $A9:F6D1-$F710, with symmetric dimming/retracing rows; this image count does not encode the actor's palette-update cadence or cry timing.</summary>
    public const int NormalFrameCount = 8;
    /// <summary>Four packed RGB5 words per normal pulse row, an eight-byte source stride, written to CGRAM slots 165..168 rather than replacing the complete OBJ palette.</summary>
    public const int NormalColorsPerFrame = 4;
    /// <summary>Sixteen packed RGB5 words in each $A9:F8A6/$F8C6/$F8E6 target image, including its transparent-slot payload: a complete 32-byte OBJ palette row for initialization or target-color transitions.</summary>
    public const int TargetColorCount = 16;
}
