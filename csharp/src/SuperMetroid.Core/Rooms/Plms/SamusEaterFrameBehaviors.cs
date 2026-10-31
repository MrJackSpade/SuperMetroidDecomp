namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Samus Eater plant's hold.</summary>
internal static class SamusEaterFrameBehaviors
{
    /// <summary>The plant holds Samus at its saved position.</summary>
    internal static PlmFrameBehavior Hold { get; } = new(
        "Samus Eater hold", static (slot, context) => context.Plms.SamusEater.RunSamusEaterFrameBehavior(slot));
}
