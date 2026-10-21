using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Harness;

/// <summary>The bus, runtime, samus and game of one import native snapshot result.</summary>
/// <param name="Bus">The bus.</param>
/// <param name="Runtime">The runtime.</param>
/// <param name="Samus">The samus.</param>
/// <param name="Game">The game.</param>
internal readonly record struct ImportNativeSnapshotResult(CartridgeImportAddressSpace Bus, SuperMetroidRuntime Runtime, SamusState Samus, SuperMetroidGame Game);
