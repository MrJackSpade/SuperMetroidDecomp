using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>One four-byte <c>PalFxDef</c> record from bank <c>$8D</c>.</summary>
/// <param name="SetupCallback">Native setup callback invoked when the object acquires a slot.</param>
/// <param name="InitialProgram">The program the object starts.</param>
internal readonly record struct RoomPaletteFxDefinition(
    PaletteFxSetup SetupCallback,
    PaletteFxProgram InitialProgram);
