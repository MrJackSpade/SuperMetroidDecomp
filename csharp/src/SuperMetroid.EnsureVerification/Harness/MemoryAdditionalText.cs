using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.EnsureVerification.Harness;

/// <summary>An in-memory analyzer additional file, so fixtures supply configuration without filesystem access.</summary>
internal sealed class MemoryAdditionalText(string path, string content) : AdditionalText
{
    /// <summary>Gets the synthetic additional-file path the analyzer matches by name.</summary>
    public override string Path { get; } = path;

    /// <summary>Returns the fixture content.</summary>
    public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(content);
}
