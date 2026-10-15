namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The five bank-$A2 Choot falling-path streams, valued by native address.</summary>
internal enum ChootFallingPath : ushort
{
    /// <summary>$A2:D84C, normal falling motion.</summary>
    Normal = 0xd84c,
    /// <summary>$A2:D976, wide falling motion.</summary>
    Wide = 0xd976,
    /// <summary>$A2:DAA0, very wide falling motion.</summary>
    VeryWide = 0xdaa0,

    /// <summary>
    /// <c>$A2:DBCA-$A2:DD41</c>: the normal path with ten extra copies of
    /// normal frame 27 at physical frames 28..37 and normal frame 62 at
    /// physical frames 73..82. Physical frame 93 remains the terminator.
    /// All 188 physical words match this expansion in the pinned NTSC J/U
    /// v1.0 ROM; <c>$DD42</c> is the separate loop Y distance. The bounded
    /// mapping is applied by <see cref="ChootFallingPathDefinitions"/>.
    /// Investigation: #625 / #658.
    /// </summary>
    Slow = 0xdbca,

    /// <summary>
    /// <c>$A2:DD44-$A2:DF5B</c>: the normal path with thirty extra copies of
    /// normal frame 27 at physical frames 28..57 and normal frame 62 at
    /// physical frames 93..122. Physical frame 133 is the terminator.
    /// All 268 physical words match this expansion in the pinned NTSC J/U
    /// v1.0 ROM; <c>$DF5C</c> is the separate loop Y distance. The bounded
    /// mapping is applied by <see cref="ChootFallingPathDefinitions"/>.
    /// Investigation: #625 / #659.
    /// </summary>
    VerySlow = 0xdd44,
}
