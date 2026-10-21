namespace SuperMetroid.ReachabilityAudit.SolutionLoading;

/// <summary>The compiled repository: every project of the solution with its error-free compilation.</summary>
internal sealed record LoadedSolution(string RepositoryRoot, IReadOnlyList<ProjectCompilation> Projects);
