using System.Runtime.CompilerServices;

// The diagnostic executable verifies GPU render targets without exposing COM in Core.
[assembly: InternalsVisibleTo("SuperMetroid.RenderVerification")]
