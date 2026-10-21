using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>The program and entry of one cinematic sprite native list addresses entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct CinematicSpriteNativeListAddressesEntry(CinematicSpriteProgram Program, ushort Entry);
