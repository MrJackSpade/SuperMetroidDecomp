namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The neighbour the <see cref="PaletteFxPreInstruction.InspectAdjacentSlot"/> pre-instruction inspects.</summary>
internal static class PaletteFxSlotInspectionDefinitions
{
    /// <summary>
    /// How many slots below its own the pre-instruction looks: it deletes its object when that slot holds one. The
    /// cartridge reads the slot's own offset from the handler's enable word, which lands this many slots down the
    /// object list (<c>$8D:F621</c>).
    /// </summary>
    internal const int InspectedSlotDistance = 2;
}
