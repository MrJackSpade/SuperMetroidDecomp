namespace SuperMetroid.Core.Game;

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

/// <summary>The two bank-$A4 hitbox lists the tongue frames refer to.</summary>
internal enum CrocomireTongueHitboxList : ushort
{
    /// <summary>$A4:CBB3 Hitbox_Crocomire_9, empty fight-tongue hitbox list.</summary>
    Fight = 0xcbb3,
    /// <summary>$A4:CC3B Hitbox_Crocomire_11, empty melting-tongue hitbox list.</summary>
    Melting = 0xcc3b,
}

/// <summary>Native physical component of a selected Crocomire tongue frame.</summary>
internal readonly record struct CrocomireTongueCollisionComponent(
CrocomireTongueHitboxList HitboxPointer);

/// <summary>
/// All nine extended frames selected by Crocomire's independently scheduled
/// tongue actor. The four fight frames refer to the empty $A4:CBB3 hitbox
/// list; the five melting frames refer to the empty $A4:CC3B list. Their OAM
/// pointers are deliberately absent so presentation can be edited separately.
/// </summary>
internal static class CrocomireTongueCollisionDefinitions
{
    /// <summary>Native bank containing the selected Crocomire extended frames.</summary>
    internal const byte Bank = 0xa4;

    internal static int FrameCount => 9;

    /// <summary>Four fight frames followed by five melting frames. The native
    /// count word and one eight-byte component give a ten-byte frame stride.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 4
            ? (int)CrocomireTongueFrame.Fight0 + 10 * index
            : (int)CrocomireTongueFrame.MeltingPose0 + 10 * (index - 4));
    }

    internal static bool HasFrame(ushort frame) => Enum.IsDefined((CrocomireTongueFrame)frame);

    /// <summary>Fight frames share a single physical component. Melting frame
    /// identities select their pose-specific offsets and common empty hitbox list.
    /// OAM/artwork identity remains separate from this physical geometry.</summary>
    internal static CrocomireTongueCollisionComponent ComponentAt(ushort frame) =>
        ComponentAt(ClosedNativeWords.Decode<CrocomireTongueFrame>(frame, "Crocomire tongue frame with compiled collision"));

    internal static CrocomireTongueCollisionComponent ComponentAt(CrocomireTongueFrame frame) => frame switch
    {
        CrocomireTongueFrame.Fight0 or CrocomireTongueFrame.Fight1 or
            CrocomireTongueFrame.Fight2 or CrocomireTongueFrame.Fight3 => new(CrocomireTongueHitboxList.Fight),
        CrocomireTongueFrame.MeltingPose0 or CrocomireTongueFrame.MeltingPose1 or CrocomireTongueFrame.MeltingPose2 or
            CrocomireTongueFrame.MeltingPose3 or CrocomireTongueFrame.MeltingPose4 => new(CrocomireTongueHitboxList.Melting),
        _ => throw new InvalidOperationException($"Undefined CrocomireTongueFrame {frame}."),
    };

    internal static int HitboxCountAt(ushort list) =>
        HitboxCountAt(ClosedNativeWords.Decode<CrocomireTongueHitboxList>(list, "compiled Crocomire tongue hitbox list"));

    internal static int HitboxCountAt(CrocomireTongueHitboxList list) => list switch
    {
        CrocomireTongueHitboxList.Fight or CrocomireTongueHitboxList.Melting => 0,
        _ => throw new InvalidOperationException($"Undefined CrocomireTongueHitboxList {list}."),
    };
}
