using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Bosses.Crocomire.Artwork;

/// <summary>The characters, state and guard of one run installed result.</summary>
/// <param name="Characters">The characters.</param>
/// <param name="State">The state.</param>
/// <param name="Guard">The guard.</param>
internal readonly record struct RunInstalledResult(byte[] Characters, CrocomireDeathState State, CartridgeReadGuard Guard);
