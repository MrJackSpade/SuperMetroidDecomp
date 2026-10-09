/// <summary>Locates the user's private cartridge image without requiring debug arguments.</summary>
static class PrivateRomPath
{
    /// <summary>Conventional cartridge image names checked beneath the bounded automatic-search roots.</summary>
    private static readonly string[] KnownFileNames =
    [
        "Super Metroid.smc",
        "Super Metroid.sfc",
        "sm.smc",
        "sm.sfc",
    ];

    /// <summary>Resolves the private ROM path from zero or one command-line argument, using bounded discovery when omitted.</summary>
    /// <param name="arguments">Application arguments: empty for automatic discovery or a single explicit ROM path.</param>
    /// <returns>An absolute path to an existing cartridge image.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">More than one argument was supplied.</exception>
    /// <exception cref="FileNotFoundException">The explicit path is missing or automatic discovery found no known filename.</exception>
    public static string Resolve(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return arguments.Length switch
        {
            0 => FindAutomatically(),
            1 => Validate(arguments[0]),
            _ => throw new ArgumentException(
                "DesktopVerification ROM audits accept either no parameters or one private ROM path."),
        };
    }

    /// <summary>Uses the environment override when valid, otherwise searches known ROM filenames beneath current and application directories.</summary>
    /// <returns>The absolute path of the first existing cartridge image found.</returns>
    /// <exception cref="FileNotFoundException">No override or candidate file identifies an existing cartridge image.</exception>
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

    /// <summary>Converts an explicit ROM path to an absolute path and verifies that the file exists.</summary>
    /// <param name="path">User-supplied cartridge image path.</param>
    /// <returns>The normalized absolute path.</returns>
    /// <exception cref="FileNotFoundException">The normalized path does not exist.</exception>
    private static string Validate(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Private ROM does not exist: {fullPath}", fullPath);
        return fullPath;
    }

    /// <summary>Collects unique ancestor directories for the current working directory and application base directory.</summary>
    /// <returns>Search roots ordered from each starting directory outward, with case-insensitive duplicates removed.</returns>
    private static List<string> BuildBoundedSearchRoots()
    {
        var roots = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAncestors(Environment.CurrentDirectory, roots, seen);
        AddAncestors(AppContext.BaseDirectory, roots, seen);
        return roots;
    }

    /// <summary>Adds a starting directory and its parents once each to the shared search-root list.</summary>
    /// <param name="startingDirectory">Directory from which parent traversal begins.</param>
    /// <param name="roots">Ordered roots being assembled for automatic filename search.</param>
    /// <param name="seen">Case-insensitive set used to avoid duplicate paths across starting directories.</param>
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

