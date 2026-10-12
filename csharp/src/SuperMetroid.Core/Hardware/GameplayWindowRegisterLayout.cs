namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The window, screen-designation and window-admission registers that NMI uploads from the
/// gameplay WRAM cache <c>$7E:0060-$7E:006D</c> (#630). Each cell is the B-bus register;
/// <see cref="ShadowAddress"/> names the WRAM byte the game keeps it in.
/// </summary>
[PpuLayoutBoundary("catalog: gameplay window and screen-designation registers")]
public static class GameplayWindowRegisterLayout
{
    /// <summary>$2123 W12SEL, shadowed at <c>$7E:0060</c> reg_W12SEL: BG1/BG2 window selection.</summary>
    public static readonly PpuLayoutCell Window12Selection = Register(nameof(Window12Selection), 0x23, "SnesWindowSelection pair (u8)");
    /// <summary>$2124 W34SEL, shadowed at <c>$7E:0061</c> reg_W34SEL: BG3/BG4 window selection.</summary>
    public static readonly PpuLayoutCell Window34Selection = Register(nameof(Window34Selection), 0x24, "SnesWindowSelection pair (u8)");
    /// <summary>$2125 WOBJSEL, shadowed at <c>$7E:0062</c> reg_WOBJSEL: OBJ/colour window selection.</summary>
    public static readonly PpuLayoutCell ObjectColorSelection = Register(nameof(ObjectColorSelection), 0x25, "SnesWindowSelection pair (u8)");
    /// <summary>$2126 WH0, shadowed at <c>$7E:0063</c> reg_WH0: first window left edge.</summary>
    public static readonly PpuLayoutCell FirstLeft = Register(nameof(FirstLeft), 0x26, "screen X (u8)");
    /// <summary>$2127 WH1, shadowed at <c>$7E:0064</c> reg_WH1: first window right edge.</summary>
    public static readonly PpuLayoutCell FirstRight = Register(nameof(FirstRight), 0x27, "screen X (u8)");
    /// <summary>$2128 WH2, shadowed at <c>$7E:0065</c> reg_WH2: second window left edge.</summary>
    public static readonly PpuLayoutCell SecondLeft = Register(nameof(SecondLeft), 0x28, "screen X (u8)");
    /// <summary>$2129 WH3, shadowed at <c>$7E:0066</c> reg_WH3: second window right edge.</summary>
    public static readonly PpuLayoutCell SecondRight = Register(nameof(SecondRight), 0x29, "screen X (u8)");
    /// <summary>$212A WBGLOG, shadowed at <c>$7E:0067</c> reg_WBGLOG: BG window Boolean operations.</summary>
    public static readonly PpuLayoutCell BackgroundLogic = Register(nameof(BackgroundLogic), 0x2a, "SnesWindowLogic x4 (u8)");
    /// <summary>$212B WOBJLOG, shadowed at <c>$7E:0068</c> reg_WOBJLOG: OBJ/colour window Boolean operations.</summary>
    public static readonly PpuLayoutCell ObjectColorLogic = Register(nameof(ObjectColorLogic), 0x2b, "SnesWindowLogic x2 (u8)");
    /// <summary>$212C TM, shadowed at <c>$7E:0069</c> reg_TM: the main-screen designation the main loop writes.</summary>
    public static readonly PpuLayoutCell MainScreen = Register(nameof(MainScreen), 0x2c, "SnesMainScreenLayers (u8)");
    /// <summary>
    /// $212C TM again, as <c>$7E:006A</c> gameplay_TM: the copy an accepted NMI takes for the
    /// gameplay IRQ, which re-programs TM below the HUD. Same register, different lifetime.
    /// </summary>
    public static readonly PpuLayoutCell GameplayMainScreen = new(nameof(GameplayMainScreen), PpuLayoutRegion.Register, 0x2c, 1,
        "SnesMainScreenLayers (u8), NMI-latched IRQ copy", MainScreen);
    /// <summary>$212D TS, shadowed at <c>$7E:006B</c> reg_TS: subscreen designation.</summary>
    public static readonly PpuLayoutCell Subscreen = Register(nameof(Subscreen), 0x2d, "SnesMainScreenLayers (u8)");
    /// <summary>$212E TMW, shadowed at <c>$7E:006C</c> reg_TMW: main-screen window admission.</summary>
    public static readonly PpuLayoutCell MainScreenWindow = Register(nameof(MainScreenWindow), 0x2e, "SnesMainScreenLayers (u8)");
    /// <summary>$212F TSW, shadowed at <c>$7E:006D</c> reg_TSW: subscreen window admission.</summary>
    public static readonly PpuLayoutCell SubscreenWindow = Register(nameof(SubscreenWindow), 0x2f, "SnesMainScreenLayers (u8)");

    /// <summary>The WRAM byte that caches <paramref name="register"/>.</summary>
    [PpuLayoutAccess]
    public static ushort ShadowAddress(PpuLayoutCell register) => register switch
    {
        _ when register == Window12Selection => GameplayWindowRegisterAddresses.Window12Selection,
        _ when register == Window34Selection => GameplayWindowRegisterAddresses.Window34Selection,
        _ when register == ObjectColorSelection => GameplayWindowRegisterAddresses.ObjectColorSelection,
        _ when register == FirstLeft => GameplayWindowRegisterAddresses.FirstLeft,
        _ when register == FirstRight => GameplayWindowRegisterAddresses.FirstRight,
        _ when register == SecondLeft => GameplayWindowRegisterAddresses.SecondLeft,
        _ when register == SecondRight => GameplayWindowRegisterAddresses.SecondRight,
        _ when register == BackgroundLogic => GameplayWindowRegisterAddresses.BackgroundLogic,
        _ when register == ObjectColorLogic => GameplayWindowRegisterAddresses.ObjectColorLogic,
        _ when register == MainScreen => GameplayWindowRegisterAddresses.MainScreen,
        _ when register == GameplayMainScreen => GameplayWindowRegisterAddresses.GameplayMainScreen,
        _ when register == Subscreen => GameplayWindowRegisterAddresses.Subscreen,
        _ when register == MainScreenWindow => GameplayWindowRegisterAddresses.MainScreenWindow,
        _ when register == SubscreenWindow => GameplayWindowRegisterAddresses.SubscreenWindow,
        _ => throw new ArgumentOutOfRangeException(nameof(register), register, "Not a gameplay window-cache register."),
    };

    /// <summary>Registers.</summary>
    private static PpuLayoutCell Register(string name, int number, string encoding) =>
        new(name, PpuLayoutRegion.Register, number, 1, encoding);
}
