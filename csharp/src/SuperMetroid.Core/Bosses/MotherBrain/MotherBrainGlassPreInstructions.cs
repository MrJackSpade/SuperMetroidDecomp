using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass hit counter.</summary>
internal static class MotherBrainGlassPreInstructions
{
    /// <summary><c>$84:D1E6</c>: missile hits advance the glass's hit count.</summary>
    internal static PlmPreInstruction Hit { get; } = new(
        "Mother Brain glass hit",
        static (slot, _) => MotherBrainGlassPlms.RunMotherBrainGlassPreInstruction(slot));
}
