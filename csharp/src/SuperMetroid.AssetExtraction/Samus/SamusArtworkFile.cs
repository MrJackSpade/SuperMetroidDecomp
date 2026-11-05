using System.Text.Json;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Retains the exact installed Samus filename across JSON and codec admission.</summary>
internal sealed record SamusArtworkFile(string Path, byte[] Bytes)
{
    /// <summary>Exception-data key marking the precise child file responsible for invalid artwork.</summary>
    private const string DiagnosticPathKey = "SuperMetroid.SamusArtworkFile.Path";

    /// <summary>Captures the installed file path and its bytes before compilation.</summary>
    internal static SamusArtworkFile Read(string path) => new(path, File.ReadAllBytes(path));

    /// <summary>Deserializes installed artwork with a path-specific error context.</summary>
    internal T Json<T>(JsonSerializerOptions options) => Compile(stream =>
        JsonAssetDocument.Read<T>(stream, options, $"Samus artwork {Path}"));

    /// <summary>Passes an immutable in-memory view of the installed bytes to the artwork compiler.</summary>
    internal T Compile<T>(Func<Stream, T> compile) => WithContext(() =>
    {
        using var stream = new MemoryStream(Bytes, writable: false);
        return compile(stream);
    });

    /// <summary>Annotates an invalid artwork error once with the exact installed file path.</summary>
    internal T WithContext<T>(Func<T> compile)
    {
        try { return compile(); }
        // A body manifest composes several files. Preserve the precise child's
        // diagnostic rather than falsely blaming the outer manifest as well.
        catch (InvalidDataException error) when (!error.Data.Contains(DiagnosticPathKey))
        {
            var contextual = new InvalidDataException($"Invalid Samus artwork {Path}: {error.Message}", error);
            contextual.Data[DiagnosticPathKey] = Path;
            throw contextual;
        }
    }
}
