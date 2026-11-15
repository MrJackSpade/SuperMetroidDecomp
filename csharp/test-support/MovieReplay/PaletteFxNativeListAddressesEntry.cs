using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where one native list starts in one colour effect that plays it (#1739, #1982).</summary>
/// <param name="Effect">The effect, whose steps the list's beats are.</param>
/// <param name="FirstStep">The effect's step the list's first beat is.</param>
/// <param name="Entry">The list's native entry.</param>
/// <param name="SetsUp">
/// Whether the list only sets its effect up, naming the colours and condition the effect's definition holds, and so holds
/// no step.
/// </param>
internal readonly record struct PaletteFxNativeListAddressesEntry(ColourEffectId Effect, int FirstStep, ushort Entry, bool SetsUp = false);
