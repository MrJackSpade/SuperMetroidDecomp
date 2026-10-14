namespace SuperMetroid.Core.Hardware;

/// <summary>Hardware ordering of window targets across W12SEL/W34SEL/WOBJSEL.</summary>
public enum SnesWindowTarget : byte
{
    /// <summary>BG1 target, using the low W12SEL nibble and WBGLOG bits 0 and 1.</summary>
    Bg1 = 0,
    /// <summary>BG2 target, using the high W12SEL nibble and WBGLOG bits 2 and 3.</summary>
    Bg2 = 1,
    /// <summary>BG3 target, using the low W34SEL nibble and WBGLOG bits 4 and 5.</summary>
    Bg3 = 2,
    /// <summary>BG4 target, using the high W34SEL nibble and WBGLOG bits 6 and 7.</summary>
    Bg4 = 3,
    /// <summary>OBJ target, using the low WOBJSEL nibble and WOBJLOG bits 0 and 1.</summary>
    Obj = 4,
    /// <summary>Color-window target, using the high WOBJSEL nibble and WOBJLOG bits 2 and 3.</summary>
    ColorMath = 5,
}
