namespace SuperMetroid.Core.Gameplay;

/// <summary>What a tester inventory is applied to.</summary>
public enum TesterInventoryTarget
{
    /// <summary>A newly created Samus, once.</summary>
    NewSamus,

    /// <summary>An inventory just restored from a save, every time.</summary>
    RestoredInventory,
}
