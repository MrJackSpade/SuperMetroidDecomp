namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive Mode 7 overflow operations shared by display packets and GPU constants.</summary>
public enum Mode7OverflowMode : byte
{
    /// <summary>M7SEL=$80: samples beyond the 1024-pixel map are transparent.</summary>
    Transparent = 0,
    /// <summary>M7SEL=$C0: use character zero outside the map, retaining the pixel's low three coordinates.</summary>
    CharacterZero = 1,
    /// <summary>M7SEL bit seven clear: wrap both tile-map coordinates to ten bits.</summary>
    Wrap = 2,
}

/// <summary>Converts retained Mode 7 controls to one mutually exclusive sampling operation.</summary>
public static class Mode7OverflowPolicy
{
    /// <summary>When wrapping is enabled, M7SEL's character-fill bit has no effect.</summary>
    public static Mode7OverflowMode FromRegisters(Mode7RenderRegisters registers) =>
        registers.WrapOutsideMap ? Mode7OverflowMode.Wrap : registers.FillOutsideWithCharacterZero
            ? Mode7OverflowMode.CharacterZero : Mode7OverflowMode.Transparent;
}
