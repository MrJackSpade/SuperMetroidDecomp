using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>The first and second of one verify intro trail png case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyIntroTrailPngCase(SuperMetroidGame First, IntroCinematicState Second);
