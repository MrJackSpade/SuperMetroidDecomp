using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where one native list starts in one colour effect animation that plays it (#1739).</summary>
/// <param name="Animation">The animation.</param>
/// <param name="FirstStep">The animation's step the list's first beat is.</param>
/// <param name="Entry">The list's native entry.</param>
internal readonly record struct PaletteFxNativeListAddressesEntry(ColourEffectAnimation Animation, int FirstStep, ushort Entry);
