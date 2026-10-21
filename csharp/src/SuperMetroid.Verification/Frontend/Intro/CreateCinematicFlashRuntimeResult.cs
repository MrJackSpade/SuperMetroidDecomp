using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>The bus, runtime and samus of one create cinematic flash runtime result.</summary>
/// <param name="Bus">The bus.</param>
/// <param name="Runtime">The runtime.</param>
/// <param name="Samus">The samus.</param>
internal readonly record struct CreateCinematicFlashRuntimeResult(SuperMetroidAddressSpace Bus, SuperMetroidRuntime Runtime, SamusState Samus);
