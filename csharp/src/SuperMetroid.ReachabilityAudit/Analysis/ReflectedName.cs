
namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>A member name reflection may look up, and the call site that names it.</summary>
/// <param name="Name">The name.</param>
/// <param name="Site">The site.</param>
internal readonly record struct ReflectedName(string Name, ReflectionSite Site);
