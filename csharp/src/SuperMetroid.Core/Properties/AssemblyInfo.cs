using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SuperMetroid.Verification")]
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.MovieVideo")]
[assembly: InternalsVisibleTo("SuperMetroid.IntegrationVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
[assembly: InternalsVisibleTo("SuperMetroid.AssetExtraction")]
[assembly: InternalsVisibleTo("SuperMetroid.ResourceAudit")]
[assembly: InternalsVisibleTo("SuperMetroid.Tooling")]

[assembly: SuperMetroid.Core.StateGraphAssembly(Catalogs = true)]
