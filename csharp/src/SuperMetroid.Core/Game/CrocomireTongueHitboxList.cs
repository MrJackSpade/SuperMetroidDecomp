namespace SuperMetroid.Core.Game;

/// <summary>The two bank-$A4 hitbox lists the tongue frames refer to.</summary>
internal enum CrocomireTongueHitboxList : ushort
{
    /// <summary>$A4:CBB3 Hitbox_Crocomire_9, empty fight-tongue hitbox list.</summary>
    Fight = 0xcbb3,
    /// <summary>$A4:CC3B Hitbox_Crocomire_11, empty melting-tongue hitbox list.</summary>
    Melting = 0xcc3b,
}
