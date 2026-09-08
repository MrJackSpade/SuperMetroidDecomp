using System.Runtime.CompilerServices;

// Host-private APIs are shared without exposing reflective graph loading as a general
// application API. Keep the legacy namespace because it is present in saved type names.
[assembly: InternalsVisibleTo("SuperMetroid.Desktop")]
[assembly: InternalsVisibleTo("SuperMetroid.Android")]
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
[assembly: InternalsVisibleTo("SuperMetroid.DiagnosticsVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
