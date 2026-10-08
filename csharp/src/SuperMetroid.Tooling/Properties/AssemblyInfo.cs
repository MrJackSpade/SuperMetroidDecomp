using System.Runtime.CompilerServices;

// Development tools and the verification executables that exercise them; never linked by player hosts.
[assembly: InternalsVisibleTo("SuperMetroid.ResourceAudit")]
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
[assembly: InternalsVisibleTo("SuperMetroid.Verification")]
[assembly: InternalsVisibleTo("SuperMetroid.IntegrationVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
