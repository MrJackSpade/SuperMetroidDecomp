namespace SuperMetroid.AssetExtraction.Saves;

/// <summary>The definitions a save can name, declared on first use.</summary>
internal sealed class DefinitionIndex
{
    /// <summary>The index every save shares.</summary>
    internal static DefinitionIndex Shared { get; } = new();

    /// <summary>The number of definitions, which production reads.</summary>
    internal int Count => Declared.Count;

    /// <summary>The definitions, declared once.</summary>
    internal Declarations Declared => field ??= new(["Power", "Ice"], 2);

    /// <summary>The declared definitions.</summary>
    /// <param name="Names">Each definition's name.</param>
    /// <param name="Count">How many there are.</param>
    internal sealed record Declarations(string[] Names, int Count);
}