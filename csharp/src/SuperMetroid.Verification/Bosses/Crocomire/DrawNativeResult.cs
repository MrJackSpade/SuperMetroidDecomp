using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>The oam and vram of one draw native result.</summary>
/// <param name="Oam">The oam.</param>
/// <param name="Vram">The vram.</param>
internal readonly record struct DrawNativeResult(OamBuffer Oam, byte[] Vram);
