namespace SuperMetroid.Core.Rooms;

/// <summary>The four bank-$84 rescue-wall draw lists, valued by native address.</summary>
internal enum EscapeAnimalDraw : ushort
{
    /// <summary>$84:925B, DrawInst_CrittersEscapeBlock_0: three $8053 solid break frames.</summary>
    Frame0 = 0x925b,
    /// <summary>$84:9265, DrawInst_CrittersEscapeBlock_1: three $8054 solid break frames.</summary>
    Frame1 = 0x9265,
    /// <summary>$84:926F, DrawInst_CrittersEscapeBlock_2: three $8055 solid break frames.</summary>
    Frame2 = 0x926f,
    /// <summary>$84:9279, DrawInst_CrittersEscapeBlock_3: three $80FF blank solid words.</summary>
    Blank = 0x9279,
}
