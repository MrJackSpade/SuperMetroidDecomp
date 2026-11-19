using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>What one save slot's field shows (#1467): its health, its maximum, and its game time, each as the field shows it.</summary>
internal sealed record ShownFileSelectSave
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="health">The slot's health.</param>
    /// <param name="maxHealth">The slot's maximum health.</param>
    /// <param name="hours">The slot's game-time hours, at most 99.</param>
    /// <param name="minutes">The slot's game-time minutes, at most 99.</param>
    private ShownFileSelectSave(int health, int maxHealth, int hours, int minutes)
    {
        Health = health;
        MaxHealth = maxHealth;
        Hours = hours;
        Minutes = minutes;
    }

    /// <summary>The slot's health.</summary>
    public int Health { get; init; }

    /// <summary>The slot's maximum health.</summary>
    public int MaxHealth { get; init; }

    /// <summary>The slot's game-time hours, at most 99.</summary>
    public int Hours { get; init; }

    /// <summary>The slot's game-time minutes, at most 99.</summary>
    public int Minutes { get; init; }

    /// <summary>The field of <paramref name="slot"/>, its time clamped to the two digits each part shows.</summary>
    internal static ShownFileSelectSave Of(SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(slot);
        return new(slot.Health, slot.MaxHealth, Math.Min(slot.GameTime.Hours, FileSelectFieldDefinitions.MaximumTimePart),
            Math.Min(slot.GameTime.Minutes, FileSelectFieldDefinitions.MaximumTimePart));
    }
}
