using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Retains the exact selected Samus filename across hash, JSON and codec admission.</summary>
internal sealed record SamusArtworkFile(string Path, byte[] Bytes)
{
    /// <summary>Exception-data key marking the precise child file responsible for invalid artwork.</summary>
    private const string DiagnosticPathKey = "SuperMetroid.SamusArtworkFile.Path";

    /// <summary>Captures the selected file path and its bytes before compilation.</summary>
    internal static SamusArtworkFile Read(string path) => new(path, File.ReadAllBytes(path));

    /// <summary>Checks a stock artwork file against its pinned provenance hash.</summary>
    internal static SamusArtworkFile Stock(string path, string expectedHash)
    {
        SamusArtworkFile file = Read(path);
        return file.WithContext(() =>
        {
            if (!string.Equals(expectedHash, Convert.ToHexString(SHA256.HashData(file.Bytes)),
                StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Stock Samus artwork failed its provenance hash.");
            return file;
        });
    }

    /// <summary>Uses an existing same-named override file, otherwise retaining the current artwork.</summary>
    internal SamusArtworkFile Select(string? overrideDirectory)
    {
        string? replacement = overrideDirectory is null ? null :
            System.IO.Path.Combine(overrideDirectory, System.IO.Path.GetFileName(Path));
        return replacement is not null && File.Exists(replacement) ? Read(replacement) : this;
    }

    /// <summary>Deserializes selected artwork with a path-specific error context.</summary>
    internal T Json<T>(JsonSerializerOptions options) => Compile(stream =>
        JsonAssetDocument.Read<T>(stream, options, $"Samus artwork {Path}"));

    /// <summary>Passes an immutable in-memory view of the selected bytes to the artwork compiler.</summary>
    internal T Compile<T>(Func<Stream, T> compile) => WithContext(() =>
    {
        using var stream = new MemoryStream(Bytes, writable: false);
        return compile(stream);
    });

    /// <summary>Annotates an invalid artwork error once with the exact selected file path.</summary>
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
