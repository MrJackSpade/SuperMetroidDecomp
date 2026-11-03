using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Fx;

/// <summary>The state and bus of one construct blend load result.</summary>
/// <param name="State">The state.</param>
/// <param name="Bus">The bus.</param>
internal readonly record struct ConstructBlendLoadResult(RoomLayer3FxState State, CartridgeReadGuard Bus);
