namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A5 functions the Draygon eye runs from its enemy variable A.</summary>
internal enum DraygonEyeFunction : ushort
{
    /// <summary><c>RTS_A5804B</c> at $A5:804B: the eye is inert.</summary>
    Inert = 0x804b,

    /// <summary><c>Function_DraygonEye_FacingLeft</c> at $A5:C48D.</summary>
    FacingLeft = 0xc48d,

    /// <summary><c>Function_DraygonEye_FacingRight</c> at $A5:C513.</summary>
    FacingRight = 0xc513,
}
