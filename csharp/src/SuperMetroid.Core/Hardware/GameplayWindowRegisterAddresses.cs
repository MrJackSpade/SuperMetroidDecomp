namespace SuperMetroid.Core.Hardware;

/// <summary>WRAM shadow bytes consumed by bank-$80's NmiUpdateIoRegisters.</summary>
public static class GameplayWindowRegisterAddresses
{
    /// <summary>$7E:0060, reg_W12SEL; BG1/BG2 window selection.</summary>
    public const ushort Window12Selection = 0x60;
    /// <summary>$7E:0061, reg_W34SEL; BG3/BG4 window selection.</summary>
    public const ushort Window34Selection = 0x61;
    /// <summary>$7E:0062, reg_WOBJSEL; OBJ/color window selection.</summary>
    public const ushort ObjectColorSelection = 0x62;
    /// <summary>$7E:0063, reg_WH0; first window left edge.</summary>
    public const ushort FirstLeft = 0x63;
    /// <summary>$7E:0064, reg_WH1; first window right edge.</summary>
    public const ushort FirstRight = 0x64;
    /// <summary>$7E:0065, reg_WH2; second window left edge.</summary>
    public const ushort SecondLeft = 0x65;
    /// <summary>$7E:0066, reg_WH3; second window right edge.</summary>
    public const ushort SecondRight = 0x66;
    /// <summary>$7E:0067, reg_WBGLOG; BG window Boolean operations.</summary>
    public const ushort BackgroundLogic = 0x67;
    /// <summary>$7E:0068, reg_WOBJLOG; OBJ/color window Boolean operations.</summary>
    public const ushort ObjectColorLogic = 0x68;
    /// <summary>$7E:0069, reg_TM; cached main-screen designation.</summary>
    public const ushort MainScreen = 0x69;
    /// <summary>$7E:006A, gameplay_TM; NMI's copy used by the gameplay IRQ.</summary>
    public const ushort GameplayMainScreen = 0x6A;
    /// <summary>$7E:006B, reg_TS; cached subscreen designation.</summary>
    public const ushort Subscreen = 0x6B;
    /// <summary>$7E:006C, reg_TMW; main-screen window admission.</summary>
    public const ushort MainScreenWindow = 0x6C;
    /// <summary>$7E:006D, reg_TSW; subscreen window admission.</summary>
    public const ushort SubscreenWindow = 0x6D;
}
