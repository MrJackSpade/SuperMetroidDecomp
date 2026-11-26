namespace Engine.Content;

/// <summary>
/// How a game's assets are made from a file the player supplies, for an exported game that carries none. A game whose
/// definition gives one (<see cref="Core.GameDefinition.AssetsMaker"/>) is set up by <see cref="Core.AssetsSetup"/>,
/// which asks for the file, keeps it and has this make the assets: the first time the game starts, and again whenever
/// the assets there were made by another <see cref="Version"/>.
/// </summary>
public abstract class AssetsMaker
{
    /// <summary>What the player is asked for, in a few words: shown on the screen and as the file chooser's title.</summary>
    public abstract string Asks { get; }

    /// <summary>The file names the player may choose among, as the file chooser takes them: "*.bin", say.</summary>
    public abstract IReadOnlyList<string> Filters { get; }

    /// <summary>
    /// What tells this maker's assets from another version's: assets made under another are made again before the
    /// game begins. Any text, the same for as long as what is made is the same.
    /// </summary>
    public abstract string Version { get; }

    /// <summary>
    /// Makes the assets from the file at <paramref name="source"/> into <paramref name="folder"/>, telling
    /// <paramref name="steps"/> how far it has come as often as work is done. It is called off the main thread and
    /// may take long. When it returns the folder holds every asset; when it throws, the folder holds what it held or
    /// is gone, never a part of a making.
    /// </summary>
    /// <param name="source">The file the player chose, kept as a plain file.</param>
    /// <param name="folder">The folder the game reads its assets from (<see cref="ContentLibrary.InstalledFolder"/>).</param>
    /// <param name="steps">Told how far the making has come, from whichever thread did the work.</param>
    public abstract void Make(string source, string folder, IProgress<MakingProgress> steps);
}
