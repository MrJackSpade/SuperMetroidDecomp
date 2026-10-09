namespace SuperMetroid.Core.Game;

/// <summary>The exclusive authored color phases of ordinary Dachora.</summary>
public enum DachoraPalettePhase
{
    /// <summary>The ordinary complete OBJ palette restored outside speed and shine animation.</summary>
    Default,

    /// <summary>The four-frame acceleration palette cycle used at maximum running speed.</summary>
    Speed,

    /// <summary>The four-frame stored-shine palette cycle used while charging and launching.</summary>
    Shine,
}

/// <summary>Bank-$A7 palette images and selector tables for Dachora enemy $E5FF.</summary>
public static class DachoraColorRomData
{
    /// <summary>Default OBJ palette at $A7:F225.</summary>
    public const int DefaultSource = 0xa7f225;
    /// <summary>Four consecutive speed images at $A7:F245..F2C4.</summary>
    public const int SpeedSource = 0xa7f245;
    /// <summary>Four consecutive stored-shine images at $A7:F2C5..F344.</summary>
    public const int ShineSource = 0xa7f2c5;
    /// <summary>Sixteen RGB5 words in each complete Dachora OBJ palette image.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Four ordered images in each speed or stored-shine palette phase.</summary>
    public const int AnimatedFrameCount = 4;

    /// <summary>Native byte length of one complete sixteen-color palette image.</summary>
    public const int FrameByteCount = ColorsPerFrame * sizeof(ushort);
}
