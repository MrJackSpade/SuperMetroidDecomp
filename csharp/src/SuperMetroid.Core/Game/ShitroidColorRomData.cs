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

    public const int NormalFrameCount = 8;
    public const int NormalColorsPerFrame = 4;
    public const int TargetColorCount = 16;
}
