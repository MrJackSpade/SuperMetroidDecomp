namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Samus Eater plant's hold.</summary>
internal static class SamusEaterFrameBehaviors
{
    /// <summary>The plant holds Samus at its saved position.</summary>
    internal static RoomObjectFrameBehavior Hold { get; } = new(
        "Samus Eater hold", static (slot, context) => context.RoomObjects.SamusEater.RunSamusEaterFrameBehavior(slot));
}
