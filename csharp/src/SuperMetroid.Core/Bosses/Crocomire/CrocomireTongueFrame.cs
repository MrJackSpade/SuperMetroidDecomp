namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The nine frames Crocomire's tongue shows; each member names the bank-$A4 frame it replaced (#1413: the values are arbitrary).</summary>
internal enum CrocomireTongueFrame
{
    /// <summary>$A4:C65E ExtendedSpritemap_Crocomire_10, first one-component fight frame.</summary>
    Fight0 = 1,
    /// <summary>$A4:C668 ExtendedSpritemap_Crocomire_11, second fight frame.</summary>
    Fight1,
    /// <summary>$A4:C672 ExtendedSpritemap_Crocomire_12, third fight frame.</summary>
    Fight2,
    /// <summary>$A4:C67C ExtendedSpritemap_Crocomire_13, fourth fight frame.</summary>
    Fight3,
    /// <summary>$A4:CACE ExtendedSpritemap_Crocomire_2D, first melting pose.</summary>
    MeltingPose0,
    /// <summary>$A4:CAD8 ExtendedSpritemap_Crocomire_2E, second melting pose.</summary>
    MeltingPose1,
    /// <summary>$A4:CAE2 ExtendedSpritemap_Crocomire_2F, third melting pose.</summary>
    MeltingPose2,
    /// <summary>$A4:CAEC ExtendedSpritemap_Crocomire_30, fourth melting pose.</summary>
    MeltingPose3,
    /// <summary>$A4:CAF6 ExtendedSpritemap_Crocomire_31, final melting pose.</summary>
    MeltingPose4,
}
