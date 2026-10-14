namespace SuperMetroid.Core.Game;

/// <summary>The four bank-$A5 hitbox lists Draygon's BG2 and OAM frames refer to.</summary>
internal enum DraygonHitboxList : ushort
{
    /// <summary>$A5:AA95, Draygon's first four-rectangle body hitbox list.</summary>
    FirstBody = 0xaa95,
    /// <summary>$A5:AAC7, the empty hitbox list used while the body cannot be hit.</summary>
    Empty = 0xaac7,
    /// <summary>$A5:ABAB, Draygon's second four-rectangle body hitbox list.</summary>
    SecondBody = 0xabab,
    /// <summary>$A5:ABDD, the second empty hitbox list used by mirrored OAM frames.</summary>
    OtherEmpty = 0xabdd,
}
