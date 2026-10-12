using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>Retains the exact selected Samus filename across hash, JSON and codec admission.</summary>
internal sealed record SamusArtworkFile(string Path, byte[] Bytes)
{
    private const string DiagnosticPathKey = "SuperMetroid.SamusArtworkFile.Path";

    internal static SamusArtworkFile Read(string path) => new(path, File.ReadAllBytes(path));

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

    internal SamusArtworkFile Select(string? overrideDirectory)
    {
        string? replacement = overrideDirectory is null ? null :
            System.IO.Path.Combine(overrideDirectory, System.IO.Path.GetFileName(Path));
        return replacement is not null && File.Exists(replacement) ? Read(replacement) : this;
    }

    internal T Json<T>(JsonSerializerOptions options) => Compile(stream =>
        JsonAssetDocument.Read<T>(stream, options, $"Samus artwork {Path}"));

    internal T Compile<T>(Func<Stream, T> compile) => WithContext(() =>
    {
        using var stream = new MemoryStream(Bytes, writable: false);
        return compile(stream);
    });

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
