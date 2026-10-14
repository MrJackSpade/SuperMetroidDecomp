namespace SuperMetroid.Core.Game;

/// <summary>One four-byte <c>PalFxDef</c> record from bank <c>$8D</c>.</summary>
/// <param name="SetupCallback">Native setup callback invoked when the object acquires a slot.</param>
/// <param name="InitialInstructionList">Initial mixed palette-animation program.</param>
internal readonly record struct RoomPaletteFxDefinition(
    PaletteFxSetup SetupCallback,
    ushort InitialInstructionList);
