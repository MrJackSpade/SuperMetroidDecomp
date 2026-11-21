namespace SuperMetroid.Tooling;

/// <summary>
/// The runtime's own assembly paths, which a tool's fixture compilations reference. Its member-lookup helpers went with the
/// rest of the game's reflection (#2602).
/// </summary>
public static class RequiredReflection
{

    /// <summary>The runtime's trusted platform assembly paths, which every .NET host publishes.</summary>
    public static string[] TrustedPlatformAssemblies() =>
        (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ??
            throw new InvalidOperationException("The .NET host published no trusted platform assemblies."))
        .Split(Path.PathSeparator);
}
