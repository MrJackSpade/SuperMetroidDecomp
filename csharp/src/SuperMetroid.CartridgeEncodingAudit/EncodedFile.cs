using SuperMetroid.EnsureAnalyzer;

namespace SuperMetroid.CartridgeEncodingAudit;

/// <summary>One audited source file's occurrences and retained cartridge storage widths.</summary>
internal sealed record EncodedFile(
    string Path,
    string Project,
    IReadOnlyList<CartridgeEncodingOccurrence> Occurrences,
    IReadOnlyList<string> AddressInterpretingTypes,
    int StorageWidthMembers);
