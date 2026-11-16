using System.Runtime.CompilerServices;

// The asynchronous cleanup runs this audit inside its own process on the solution it has already loaded, so its steps
// share one load (tools/cleanup/CodeFixRunner.cs, an app that takes its assembly name from that file).
[assembly: InternalsVisibleTo("CodeFixRunner")]
