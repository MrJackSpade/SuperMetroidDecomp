namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The two hitbox lists the tongue frames refer to; each member names the bank-$A4 list it replaced (#1413: the values are arbitrary).</summary>
internal enum CrocomireTongueHitboxList
{
    /// <summary>$A4:CBB3 Hitbox_Crocomire_9, empty fight-tongue hitbox list.</summary>
    Fight = 1,
    /// <summary>$A4:CC3B Hitbox_Crocomire_11, empty melting-tongue hitbox list.</summary>
    Melting,
}
