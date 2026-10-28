namespace SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>
/// Reports a shared fixture's one-time load as its own timed entry (#1430). A process loads the repository installation,
/// its artwork and the sheet compositions once, inside whichever suite first asks for them; timed as their own entries,
/// those loads keep their own time and allocation lines and budget markers instead of being charged to that suite.
/// </summary>
internal static class SharedFixtureTiming
{
    /// <summary>
    /// Times one named load, or is null in hosts that time nothing. The verifier installs its suite timer, which runs
    /// the load nested under the suite that asked for it.
    /// </summary>
    internal static Action<string, Action>? Timer { get; set; }

    /// <summary>Runs <paramref name="load"/> once, through <see cref="Timer"/> when one is installed.</summary>
    /// <param name="name">The fixture the load builds, as its timed entry names it.</param>
    /// <param name="load">Builds the fixture.</param>
    internal static T Load<T>(string name, Func<T> load) where T : notnull
    {
        if (Timer is not { } timer)
            return load();
        T? result = default;
        timer(name, () => result = load());
        return result ?? throw new InvalidOperationException($"Shared fixture {name} did not load.");
    }
}
