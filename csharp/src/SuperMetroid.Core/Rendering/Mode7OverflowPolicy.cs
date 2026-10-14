namespace SuperMetroid.Core.Rendering;

/// <summary>Converts retained Mode 7 controls to one mutually exclusive sampling operation.</summary>
public static class Mode7OverflowPolicy
{
    /// <summary>When wrapping is enabled, M7SEL's character-fill bit has no effect.</summary>
    public static Mode7OverflowMode FromRegisters(Mode7RenderRegisters registers) =>
        registers.WrapOutsideMap ? Mode7OverflowMode.Wrap : registers.FillOutsideWithCharacterZero
            ? Mode7OverflowMode.CharacterZero : Mode7OverflowMode.Transparent;
}
