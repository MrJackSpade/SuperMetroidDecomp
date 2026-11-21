namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// What the equipment selector shows at one step of its installed animation (#1596, #2359): the frame of the installed
/// pause sheet each equipment column's selector shows, the suits and boots sharing one.
/// </summary>
/// <param name="Reserve">The reserve column's frame.</param>
/// <param name="Beam">The beam column's frame.</param>
/// <param name="Equipment">The suit and boot columns' frame.</param>
public readonly record struct PauseSelectorLook(string Reserve, string Beam, string Equipment)
{
    /// <summary>The frame the selector shows over each equipment category.</summary>
    private static IdentityTable<PauseEquipmentCategory, Func<PauseSelectorLook, string>> Frames => field ??= new(new Dictionary<PauseEquipmentCategory, Func<PauseSelectorLook, string>>
    {
        [PauseEquipmentCategory.Reserves] = static look => look.Reserve,
        [PauseEquipmentCategory.Beams] = static look => look.Beam,
        [PauseEquipmentCategory.Suits] = static look => look.Equipment,
        [PauseEquipmentCategory.Boots] = static look => look.Equipment,
    });

    /// <summary>The frame the selector of <paramref name="category"/>'s column shows.</summary>
    /// <exception cref="InvalidOperationException">The category is undefined.</exception>
    internal string FrameOf(PauseEquipmentCategory category) => Frames[category](this);
}
