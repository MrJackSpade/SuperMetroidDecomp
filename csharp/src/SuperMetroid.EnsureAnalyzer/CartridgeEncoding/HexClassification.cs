
namespace SuperMetroid.EnsureAnalyzer.CartridgeEncoding;

/// <summary>How a hexadecimal literal is classified, and why.</summary>
/// <param name="Class">The class.</param>
/// <param name="Basis">The basis.</param>
internal readonly record struct HexClassification(CartridgeEncodingClass Class, string Basis);
