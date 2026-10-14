namespace SuperMetroid.Core.Rendering;

/// <summary>Mutually exclusive X-ray blending routines selected by $91:D27F and bank $88.</summary>
public enum XrayRoomBlendMode
{
    /// <summary>$88:817B substitutes the hidden-block BG2 map inside the beam.</summary>
    RevealBlocks,
    /// <summary>$88:81A4 preserves room backgrounds and does not substitute hidden blocks.</summary>
    PreserveBackgrounds,
    /// <summary>$88:81DB preserves the Fireflea darkness subtraction outside the beam.</summary>
    Fireflea,
}
