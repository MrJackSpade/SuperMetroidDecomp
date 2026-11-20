namespace Engine.Content;

/// <summary>An asset or a value in it is missing, misspelt or of the wrong kind. The message names the file and what is wrong.</summary>
public sealed class ContentException(string file, string problem) : Exception($"{file}: {problem}")
{
    /// <summary>The asset file, as a path within the assets tree.</summary>
    public string File { get; } = file;
}
