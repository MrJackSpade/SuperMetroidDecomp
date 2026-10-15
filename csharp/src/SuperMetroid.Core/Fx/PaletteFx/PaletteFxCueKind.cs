namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>What a palette-FX program asks of its object.</summary>
public enum PaletteFxCueKind
{
    /// <summary><c>$8D:C655</c>: later records write from this CGRAM byte index.</summary>
    SetColorIndex,
    /// <summary><c>$8D:C5D4</c>: installs a pre-instruction.</summary>
    SetPreInstruction,
    /// <summary><c>$8D:C5DD</c>: clears the pre-instruction.</summary>
    ClearPreInstruction,
    /// <summary><c>$8D:F1C6</c>: publishes the Norfair heat palette index.</summary>
    SetHeatPaletteIndex,
    /// <summary><c>$8D:C673</c>: queues a library-2 sound.</summary>
    QueueSoundLibrary2,
}
