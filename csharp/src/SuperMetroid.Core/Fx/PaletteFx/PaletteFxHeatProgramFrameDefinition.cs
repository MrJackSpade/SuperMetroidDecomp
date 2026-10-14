namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One timed fifteen-color record in a Samus-in-heat palette program.</summary>
public readonly record struct PaletteFxHeatProgramFrameDefinition(
    ushort InstructionPointer)
{
    /// <summary>The first live BGR555 presentation word after the duration.</summary>
    /// <remarks>Derived from the native duration word followed immediately by color data.
    /// The color values and repeated color-row relationships require their own review;
    /// this address calculation is not a disposition for their contents.</remarks>
    public ushort FirstColorPointer => unchecked((ushort)(InstructionPointer + sizeof(ushort)));
}
