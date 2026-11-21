using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SuperMetroid.Verification")]
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.IntegrationVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.EnsureVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
[assembly: InternalsVisibleTo("SuperMetroid.AssetExtraction")]
[assembly: InternalsVisibleTo("SuperMetroid.ResourceAudit")]
[assembly: InternalsVisibleTo("SuperMetroid.Tooling")]

// #2602: every type's debugger-state members are written by the compiler; the static members' objects are definition catalogs.
[assembly: SuperMetroid.Core.StateGraphAssembly(Catalogs = true)]
