namespace SuperMetroid.Core.Game;

/// <summary>Native physical component of a selected Crocomire tongue frame.</summary>
/// <param name="HitboxPointer">Pointer to the native hitbox list used by this tongue component.</param>
internal readonly record struct CrocomireTongueCollisionComponent(
ushort HitboxPointer);

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

    /// <summary>$A4:C65E ExtendedSpritemap_Crocomire_10, first of four
    /// one-component fight frames, each ten bytes long.</summary>
    private const ushort FightFrameStart = 0xc65e;
    /// <summary>$A4:CACE ExtendedSpritemap_Crocomire_2D, first melting pose.</summary>
    private const ushort MeltingPose0 = 0xcace;
    /// <summary>$A4:CAD8 ExtendedSpritemap_Crocomire_2E, second melting pose.</summary>
    private const ushort MeltingPose1 = MeltingPose0 + 10;
    /// <summary>$A4:CAE2 ExtendedSpritemap_Crocomire_2F, third melting pose.</summary>
    private const ushort MeltingPose2 = MeltingPose0 + 20;
    /// <summary>$A4:CAEC ExtendedSpritemap_Crocomire_30, fourth melting pose.</summary>
    private const ushort MeltingPose3 = MeltingPose0 + 30;
    /// <summary>$A4:CAF6 ExtendedSpritemap_Crocomire_31, final melting pose.</summary>
    private const ushort MeltingPose4 = MeltingPose0 + 40;
    /// <summary>$A4:CBB3 Hitbox_Crocomire_9, empty fight-tongue hitbox list.</summary>
    private const ushort FightHitboxes = 0xcbb3;
    /// <summary>$A4:CC3B Hitbox_Crocomire_11, empty melting-tongue hitbox list.</summary>
    private const ushort MeltingHitboxes = 0xcc3b;

    /// <summary>Gets the number of extended tongue frames represented by the collision catalog.</summary>
    internal static int FrameCount => 9;

    /// <summary>Four fight frames followed by five melting frames. The native
    /// count word and one eight-byte component give a ten-byte frame stride.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 4 ? FightFrameStart + 10 * index : MeltingPose0 + 10 * (index - 4));
    }

    /// <summary>Checks whether a frame pointer identifies one of the compiled fight or melting tongue poses.</summary>
    /// <param name="frame">Bank-local extended-spritemap pointer to classify.</param>
    /// <returns><see langword="true"/> when the pointer is one of the nine supported frame entries.</returns>
    internal static bool HasFrame(ushort frame) => IsFightFrame(frame) ||
        frame is MeltingPose0 or MeltingPose1 or MeltingPose2 or MeltingPose3 or MeltingPose4;

    /// <summary>Recognizes the four ten-byte-stride extended spritemaps used during Crocomire's fight tongue sequence.</summary>
    /// <param name="frame">Bank-local extended-spritemap pointer to classify.</param>
    /// <returns><see langword="true"/> when the pointer is aligned to one of the fight-frame entries.</returns>
    private static bool IsFightFrame(ushort frame)
    {
        int offset = frame - FightFrameStart;
        return offset >= 0 && offset <= 30 && offset % 10 == 0;
    }

    /// <summary>Fight frames share a single physical component. Melting frame
    /// identities select their pose-specific offsets and common empty hitbox list.
    /// OAM/artwork identity remains separate from this physical geometry.</summary>
    internal static CrocomireTongueCollisionComponent ComponentAt(ushort frame)
    {
        if (IsFightFrame(frame)) return new(FightHitboxes);
        return frame switch
        {
            MeltingPose0 => new(MeltingHitboxes),
            MeltingPose1 => new(MeltingHitboxes),
            MeltingPose2 => new(MeltingHitboxes),
            MeltingPose3 => new(MeltingHitboxes),
            MeltingPose4 => new(MeltingHitboxes),
            _ => throw new InvalidDataException(
                $"Crocomire tongue frame $A4:{frame:X4} has no compiled collision."),
        };
    }

    /// <summary>Returns the number of components in a supported tongue hitbox list.</summary>
    /// <param name="list">Bank-local hitbox-list pointer selected by a tongue frame.</param>
    /// <returns>Zero for the compiled empty fight and melting lists.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a hitbox list represented by this catalog.</exception>
    internal static int HitboxCountAt(ushort list) => list switch
    {
        FightHitboxes or MeltingHitboxes => 0,
        _ => throw new InvalidDataException(
            $"Crocomire tongue hitbox list $A4:{list:X4} is not compiled."),
    };
}
