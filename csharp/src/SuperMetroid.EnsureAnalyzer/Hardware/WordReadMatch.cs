using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>One little-endian word rebuilt from two adjacent byte reads of a canonical memory source.</summary>
/// <param name="Expression">The whole expression to replace, including a covering cast or <c>unchecked</c>.</param>
/// <param name="Receiver">The byte source, as written.</param>
/// <param name="ByteMethod">The byte read: ReadCartridgeByte or ReadWorkRamByte.</param>
/// <param name="Address">The low byte's address.</param>
/// <param name="Progression">How the high byte's address follows.</param>
/// <param name="OtherOperands">Further operands OR'd into the same expression, such as a bank.</param>
/// <param name="ViaAddressSpace">True when the bytes came through the address space's import capability, which the
/// canonical read names explicitly with <c>CartridgeImportSource.Require</c>.</param>
/// <param name="Long">True for a 24-bit value: a canonical word and the byte after it.</param>
public sealed record WordReadMatch(
    ExpressionSyntax Expression,
    ExpressionSyntax Receiver,
    string ByteMethod,
    ExpressionSyntax Address,
    WordReadProgression Progression,
    ImmutableArray<ExpressionSyntax> OtherOperands,
    bool ViaAddressSpace,
    bool Long = false)
{
    /// <summary>The receiver of the canonical read as source text.</summary>
    public string CanonicalReceiver => ViaAddressSpace ? $"CartridgeImportSource.Require({Receiver})" : Receiver.ToString();

    /// <summary>
    /// The canonical word operation for this byte source and progression, or null when the progression is
    /// ambiguous or no caller has needed that operation yet (the SRAM word). The linear cartridge long lives in
    /// test support, since only verification oracles read longs on the linear bus.
    /// </summary>
    public string? CanonicalMethod => (ByteMethod, Progression, Long) switch
    {
        ("ReadCartridgeByte", WordReadProgression.Linear, false) => "ReadCartridgeWord",
        ("ReadCartridgeByte", WordReadProgression.FixedBank, false) => "ReadCartridgeWordFixedBank",
        ("ReadCartridgeByte", WordReadProgression.Linear, true) => "ReadCartridgeLong",
        ("ReadCartridgeByte", WordReadProgression.FixedBank, true) => "ReadCartridgeLongFixedBank",
        ("ReadWorkRamByte", WordReadProgression.Linear, false) => "ReadWorkRamWord",
        ("ReadWorkRamByte", WordReadProgression.FixedBank, false) => "ReadWorkRamWordFixedBank",
        _ => null,
    };
}
