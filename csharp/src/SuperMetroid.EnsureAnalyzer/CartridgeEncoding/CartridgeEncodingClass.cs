namespace SuperMetroid.EnsureAnalyzer;

/// <summary>The six #641 audit classes. The scanner proposes one per occurrence and names its basis.</summary>
internal enum CartridgeEncodingClass
{
    /// <summary>A count, distance, duration, velocity or other ordinary quantity written in hexadecimal.</summary>
    NumericQuantity,
    /// <summary>A member of a mutually exclusive domain: a case label, comparison constant or plain enum member.</summary>
    DomainIdentity,
    /// <summary>A bit, mask or shift: composable bits when the cartridge treats them so.</summary>
    ComposableBits,
    /// <summary>Encoded instruction or definition words held in an opaque integral array.</summary>
    StructuredInstruction,
    /// <summary>A cartridge address, code pointer or address-based dispatch.</summary>
    CartridgeAddress,
    /// <summary>No rule establishes the meaning; it stays at the boundary until one does.</summary>
    Unknown,
}
