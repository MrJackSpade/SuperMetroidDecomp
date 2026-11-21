using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SuperMetroid.Verification")]
[assembly: InternalsVisibleTo("SuperMetroid.IntegrationVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.ResourceAudit")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
[assembly: InternalsVisibleTo("SuperMetroid.Tooling")]

// #2602: every type's debugger-state members are written by the compiler.
[assembly: SuperMetroid.Core.StateGraphAssembly]
