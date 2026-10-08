using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SuperMetroid.Verification")]
[assembly: InternalsVisibleTo("SuperMetroid.IntegrationVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.ResourceAudit")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
// Development-only tool library: what only tools read lives there, outside the shipped assemblies.
[assembly: InternalsVisibleTo("SuperMetroid.Tooling")]
