namespace SuperMetroid.Verification.Harness;

/// <summary>The read and identity of one loaded Ceres actor layout.</summary>
/// <typeparam name="TRole">The scene's actor roles.</typeparam>
/// <typeparam name="T">The scene's placement type.</typeparam>
/// <param name="Read">The loaded layout's placement of a role.</param>
/// <param name="Identity">The loaded layout's content identity.</param>
internal readonly record struct LookupStream5Load<TRole, T>(Func<TRole, T> Read, string Identity)
    where TRole : struct, Enum;
