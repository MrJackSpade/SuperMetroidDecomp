using System.Runtime.CompilerServices;

// Host-private APIs are shared without exposing reflective graph loading as a general
// application API. Keep the legacy namespace because it is present in saved type names.
[assembly: InternalsVisibleTo("SuperMetroid.Desktop")]
[assembly: InternalsVisibleTo("SuperMetroid.Android")]
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
[assembly: InternalsVisibleTo("SuperMetroid.IntegrationVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.Verification")]
// Development-only tool library: what only tools read lives there, outside the shipped assemblies.
[assembly: InternalsVisibleTo("SuperMetroid.Tooling")]
