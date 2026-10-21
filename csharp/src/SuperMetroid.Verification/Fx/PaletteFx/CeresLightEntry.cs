using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>The owner and definition pointer of one ceres light entry.</summary>
/// <param name="Owner">The owner.</param>
/// <param name="DefinitionPointer">The definition pointer.</param>
internal readonly record struct CeresLightEntry(CeresCinematicLightPaletteFxProgramOwner Owner, ushort DefinitionPointer);
