using System.Runtime.CompilerServices;

// The dependency-free verification executable checks exact internal cartridge layouts.
// Production hosts still see only the public game-facing API.
[assembly: InternalsVisibleTo("SuperMetroid.Verification")]
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DiagnosticsVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
