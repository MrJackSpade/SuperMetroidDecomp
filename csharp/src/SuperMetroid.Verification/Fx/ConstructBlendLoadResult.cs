using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Fx;

/// <summary>The state, bus and cgram of one construct blend load result.</summary>
/// <param name="State">The state.</param>
/// <param name="Bus">The bus.</param>
/// <param name="Cgram">The cgram.</param>
internal readonly record struct ConstructBlendLoadResult(RoomLayer3FxState State, CartridgeReadGuard Bus, GameplayColours Cgram);
