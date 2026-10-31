using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>One four-byte <c>PalFxDef</c> record.</summary>
/// <param name="SetupCallback">Setup callback invoked when the object acquires a slot.</param>
/// <param name="InitialProgram">The program the object starts.</param>
internal readonly record struct RoomPaletteFxDefinition(
    PaletteFxSetup SetupCallback,
    PaletteFxProgram InitialProgram);
