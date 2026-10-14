using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Restored runtime owners, file metadata and any compatible build-identity warnings.</summary>
internal readonly record struct DebuggerSaveStateLoadResult(
    SuperMetroidAddressSpace AddressSpace,
    SuperMetroidGame Game,
    DebuggerSaveStateMetadata Metadata,
    IReadOnlyList<string> Warnings);
