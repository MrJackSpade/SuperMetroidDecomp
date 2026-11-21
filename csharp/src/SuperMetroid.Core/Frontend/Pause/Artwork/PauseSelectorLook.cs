namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// What the equipment selector shows at one step of its installed animation (#1596, #2359): the frame of the installed
/// pause sheet each equipment column's selector shows, the suits and boots sharing one.
/// </summary>
/// <param name="Reserve">The reserve column's frame.</param>
/// <param name="Beam">The beam column's frame.</param>
/// <param name="Equipment">The suit and boot columns' frame.</param>
public readonly partial record struct PauseSelectorLook(string Reserve, string Beam, string Equipment)
{
    /// <summary>The frame the selector of <paramref name="category"/>'s column shows.</summary>
    /// <exception cref="InvalidOperationException">The category is undefined.</exception>
    internal string FrameOf(PauseEquipmentCategory category) => category switch
    {
        PauseEquipmentCategory.Reserves => Reserve,
        PauseEquipmentCategory.Beams => Beam,
        PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots => Equipment,
        _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
    };
}
