namespace SuperMetroid.Core.Game;

/// <summary>The exclusive authored color phases of ordinary Dachora.</summary>
public enum DachoraPalettePhase
{
    Default,
    Speed,
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
    /// <summary>Four native speed-image pointers at $A7:F787.</summary>
    public const int SpeedPointerTable = 0xa7f787;
    /// <summary>Four native shine-image pointers at $A7:F92D.</summary>
    public const int ShinePointerTable = 0xa7f92d;
    public const int ColorsPerFrame = 16;
    public const int AnimatedFrameCount = 4;
    public const int FrameByteCount = ColorsPerFrame * sizeof(ushort);

    /// <summary>Returns the authored color source for a cartridge-selected phase/frame.</summary>
    public static int Source(DachoraPalettePhase phase, int frame) => phase switch
    {
        DachoraPalettePhase.Default when frame == 0 => DefaultSource,
        DachoraPalettePhase.Speed when (uint)frame < AnimatedFrameCount =>
            SpeedSource + frame * FrameByteCount,
        DachoraPalettePhase.Shine when (uint)frame < AnimatedFrameCount =>
            ShineSource + frame * FrameByteCount,
        _ => throw new ArgumentOutOfRangeException(nameof(frame),
            $"Dachora phase {phase} has no frame {frame}."),
    };
}
