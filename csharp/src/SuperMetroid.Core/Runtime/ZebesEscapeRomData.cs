namespace SuperMetroid.Core.Runtime;

/// <summary>Native room-main escape effect definitions in bank $8F.</summary>
public static class ZebesEscapeRomData
{
    /// <summary>$8F:C1D6 explosion sprite-object IDs, eight byte entries.</summary>
    public const int SpriteTable = 0x8fc1d6;
    /// <summary>$8F:C1DE explosion library-two sound IDs, eight byte entries.</summary>
    public const int SoundTable = 0x8fc1de;
    /// <summary>$8F:E594/E5BC duration of the temporary diagonal quake.</summary>
    public const ushort DiagonalFrames = 42;
    /// <summary>$8F:C933 light horizontal shaking, including enemies.</summary>
    public const ushort LightHorizontal = 18;
    /// <summary>$8F:C946 medium horizontal shaking, including enemies.</summary>
    public const ushort MediumHorizontal = 21;
    /// <summary>$8F:E59A temporary medium diagonal shake.</summary>
    public const ushort MediumDiagonal = 23;
    /// <summary>$8F:E5C2 temporary strong diagonal shake.</summary>
    public const ushort StrongDiagonal = 26;
    /// <summary>$8F:E58F threshold for the light-to-diagonal transition.</summary>
    public const ushort LightChance = 512;
    /// <summary>$8F:E5B7 threshold for the medium-to-diagonal transition.</summary>
    public const ushort MediumChance = 384;
    /// <summary>$8F:C17D blank foreground tile excluded from outdoor explosions.</summary>
    public const ushort BlankTile = 255;
    /// <summary>$8F:C12C keeps the earthquake active by setting its high bit.</summary>
    public const ushort ContinuousTimerBit = 0x8000;
    /// <summary>$8F:919C mainstreet strong horizontal quake including enemies.</summary>
    public const ushort MainstreetQuake = 24;
    /// <summary>$8F:91BD landing-site strong horizontal background quake.</summary>
    public const ushort LandingQuake = 6;
}
