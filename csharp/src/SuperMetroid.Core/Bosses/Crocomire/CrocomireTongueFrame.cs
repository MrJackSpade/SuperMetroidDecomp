namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The nine bank-$A4 extended frames selected by Crocomire's tongue actor.</summary>
internal enum CrocomireTongueFrame : ushort
{
    /// <summary>$A4:C65E ExtendedSpritemap_Crocomire_10, first one-component fight frame.</summary>
    Fight0 = 0xc65e,
    /// <summary>$A4:C668 ExtendedSpritemap_Crocomire_11, second fight frame.</summary>
    Fight1 = 0xc668,
    /// <summary>$A4:C672 ExtendedSpritemap_Crocomire_12, third fight frame.</summary>
    Fight2 = 0xc672,
    /// <summary>$A4:C67C ExtendedSpritemap_Crocomire_13, fourth fight frame.</summary>
    Fight3 = 0xc67c,
    /// <summary>$A4:CACE ExtendedSpritemap_Crocomire_2D, first melting pose.</summary>
    MeltingPose0 = 0xcace,
    /// <summary>$A4:CAD8 ExtendedSpritemap_Crocomire_2E, second melting pose.</summary>
    MeltingPose1 = 0xcad8,
    /// <summary>$A4:CAE2 ExtendedSpritemap_Crocomire_2F, third melting pose.</summary>
    MeltingPose2 = 0xcae2,
    /// <summary>$A4:CAEC ExtendedSpritemap_Crocomire_30, fourth melting pose.</summary>
    MeltingPose3 = 0xcaec,
    /// <summary>$A4:CAF6 ExtendedSpritemap_Crocomire_31, final melting pose.</summary>
    MeltingPose4 = 0xcaf6,
}
