namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Samus Eater plant's hold.</summary>
internal static class SamusEaterPreInstructions
{
    /// <summary><c>$84:AC89</c>: the plant holds Samus at its saved position.</summary>
    internal static PlmPreInstruction Hold { get; } = new(
        "Samus Eater hold", static (slot, context) => context.Plms.SamusEater.RunSamusEaterPreInstruction(slot));
}
