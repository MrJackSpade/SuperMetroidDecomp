namespace SuperMetroid.Core.Progression;

/// <summary>
/// Queries over Samus's equipment and beam flags. Their conversion to the native word is
/// <see cref="SamusEquipmentWordCodec"/>'s alone.
/// </summary>
public static class SamusEquipmentFlagExtensions
{
    /// <summary>Tests whether an item set contains at least one requested equipment bit.</summary>
    /// <param name="items">Collected or equipped items.</param>
    /// <param name="flags">One or more item bits to test.</param>
    /// <returns><see langword="true"/> when the two sets share any set bit.</returns>
    public static bool HasAny(this SamusEquipmentFlags items, SamusEquipmentFlags flags) => (items & flags) != SamusEquipmentFlags.None;

    /// <summary>Tests whether a beam set contains at least one requested beam component.</summary>
    /// <param name="beams">Collected or equipped beams.</param>
    /// <param name="flags">One or more beam bits to test.</param>
    /// <returns><see langword="true"/> when the two sets share any set bit.</returns>
    public static bool HasAny(this SamusBeamFlags beams, SamusBeamFlags flags) => (beams & flags) != SamusBeamFlags.None;
}
