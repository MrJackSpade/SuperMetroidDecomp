using SuperMetroid.Core;
/// <summary>Locates the user's private cartridge image without requiring debug arguments.</summary>
static class PrivateRomPath
{
    /// <summary>Stores known file names.</summary>
    private static readonly string[] KnownFileNames =
    [
        "Super Metroid.smc",
        "Super Metroid.sfc",
        "sm.smc",
        "sm.sfc",
    ];

    /// <summary>Resolves.</summary>
    public static string Resolve(string[] arguments)
    {
        Ensure.NotNull(arguments);
        return arguments.Length switch
        {
            0 => FindAutomatically(),
            1 => Validate(arguments[0]),
            _ => throw new ArgumentException(
                "DesktopVerification ROM audits accept either no parameters or one private ROM path."),
        };
    }

    /// <summary>Finds automatically.</summary>
    private static string FindAutomatically()
    {
        // An environment override keeps the copyrighted cartridge image outside copied
        // workspaces while still making ordinary F5 execution parameter-free.
        string? environmentRom = Environment.GetEnvironmentVariable("SUPERMETROID_ROM");
        if (!string.IsNullOrWhiteSpace(environmentRom) && File.Exists(environmentRom))
            return Path.GetFullPath(environmentRom);

        foreach (string root in BuildBoundedSearchRoots())
        {
            foreach (string fileName in KnownFileNames)
            {
                string candidate = Path.Combine(root, fileName);
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }
        }

        throw new FileNotFoundException(
            "Could not locate the private Super Metroid ROM automatically. Put " +
            "'Super Metroid.smc' in the workspace, set SUPERMETROID_ROM, or pass the ROM " +
            "path as the sole argument.");
    }

    /// <summary>Validates.</summary>
    private static string Validate(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Private ROM does not exist: {fullPath}", fullPath);
        return fullPath;
    }

    /// <summary>Builds bounded search roots.</summary>
    private static List<string> BuildBoundedSearchRoots()
    {
        var roots = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAncestors(Environment.CurrentDirectory, roots, seen);
        AddAncestors(AppContext.BaseDirectory, roots, seen);
        return roots;
    }

    /// <summary>Adds ancestors.</summary>
    private static void AddAncestors(
        string startingDirectory,
        List<string> roots,
        HashSet<string> seen)
    {
        DirectoryInfo? directory = new(Path.GetFullPath(startingDirectory));
        while (directory is not null)
        {
            if (seen.Add(directory.FullName))
                roots.Add(directory.FullName);
            directory = directory.Parent;
        }
    }
}

