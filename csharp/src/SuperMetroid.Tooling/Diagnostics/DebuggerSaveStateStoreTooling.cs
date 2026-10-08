using System.Security.Cryptography;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop;

/// <summary>Development-tool construction of <see cref="DebuggerSaveStateStore"/>; never linked by player hosts.</summary>
internal static class DebuggerSaveStateStoreTooling
{
    /// <summary>
    /// A store identified by a cartridge image's digest, beside the ROM file unless overridden.
    /// Player hosts use <see cref="DebuggerSaveStateStore.ForInstalledGame"/> instead.
    /// </summary>
    internal static DebuggerSaveStateStore ForCartridge(
        string romPath,
        ReadOnlySpan<byte> cartridgeRom,
        string? directoryOverride = null,
        SuperMetroidGameOptions? hostOptions = null,
        GameContentIdentity? contentIdentity = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        return new DebuggerSaveStateStore(
            directoryOverride is null
                ? Path.Combine(
                    Path.GetDirectoryName(Path.GetFullPath(romPath))
                        ?? throw new InvalidOperationException("ROM path has no parent directory."),
                    "debug-states")
                : Path.GetFullPath(directoryOverride),
            SHA256.HashData(cartridgeRom),
            hostOptions,
            contentIdentity);
    }
}
