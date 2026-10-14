namespace SuperMetroid.Core.Rendering;

/// <summary>Inserts a Mode 7 plane at an explicit position in the OBJ priority ladder.</summary>
/// <param name="Registers">Unmodified integer projection and overflow controls.</param>
/// <param name="SubtractObjSubscreen">Subtracts the winning OBJ from BG1.</param>
/// <param name="AddBg1Subscreen">Owns BG1/OBJ main selection and adds BG1 to eligible main pixels, without halving. Backdrop and OBJ palettes zero through three are excluded.</param>
public sealed record Mode7RenderLayer(Mode7RenderRegisters Registers, bool SubtractObjSubscreen = false,
    bool AddBg1Subscreen = false) : RenderLayer;
