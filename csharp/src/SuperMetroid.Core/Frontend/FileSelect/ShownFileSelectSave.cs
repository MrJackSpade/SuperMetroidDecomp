using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>What one save slot's field shows (#1467): its health, its maximum, and its game time, each as the field shows it.</summary>
/// <param name="Health">The slot's health.</param>
/// <param name="MaxHealth">The slot's maximum health.</param>
/// <param name="Hours">The slot's game-time hours, at most 99.</param>
/// <param name="Minutes">The slot's game-time minutes, at most 99.</param>
internal sealed record ShownFileSelectSave(int Health, int MaxHealth, int Hours, int Minutes)
{
    /// <summary>The field of <paramref name="slot"/>, its time clamped to the two digits each part shows.</summary>
    internal static ShownFileSelectSave Of(SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(slot);
        return new(slot.Health, slot.MaxHealth, Math.Min(slot.GameTimeHours, FileSelectFieldDefinitions.MaximumTimePart),
            Math.Min(slot.GameTimeMinutes, FileSelectFieldDefinitions.MaximumTimePart));
    }
}
