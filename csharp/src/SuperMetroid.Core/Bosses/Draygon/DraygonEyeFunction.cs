namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What the Draygon eye does each update; each member names the bank-$A5 routine it replaced (#1413: the values are arbitrary).</summary>
internal enum DraygonEyeFunction
{
    /// <summary><c>RTS_A5804B</c> at $A5:804B: the eye is inert.</summary>
    Inert = 1,

    /// <summary><c>Function_DraygonEye_FacingLeft</c> at $A5:C48D.</summary>
    FacingLeft,

    /// <summary><c>Function_DraygonEye_FacingRight</c> at $A5:C513.</summary>
    FacingRight,
}
