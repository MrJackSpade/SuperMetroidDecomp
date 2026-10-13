using System.Runtime.CompilerServices;

// The diagnostic executable verifies GPU render targets without exposing COM in Core.
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
// The debug runner reads rendered frames back for snapshot comparison.
[assembly: InternalsVisibleTo("SuperMetroid.DebugRunner")]
// The desktop verifier checks the worker's presentation state through the shared verification access.
[assembly: InternalsVisibleTo("SuperMetroid.DesktopVerification")]
