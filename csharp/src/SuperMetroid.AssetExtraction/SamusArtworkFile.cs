using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>Retains the exact selected Samus filename across hash, JSON and codec admission.</summary>
/// <summary>Retains the selected file path and bytes through Samus artwork admission.</summary>
/// <param name="Path">Path of the stock manifest, stock resource, or selected override.</param>
/// <param name="Bytes">Original bytes used for hashing and decoding.</param>
internal sealed record SamusArtworkFile(string Path, byte[] Bytes)
{
    /// <summary>Exception-data key marking that an InvalidDataException already names its source file.</summary>
    private const string DiagnosticPathKey = "SuperMetroid.SamusArtworkFile.Path";

    /// <summary>Reads a Samus artwork file while retaining its path for later diagnostics.</summary>
    /// <param name="path">File path to read.</param>
    /// <returns>The path and bytes of the file.</returns>
    internal static SamusArtworkFile Read(string path) => new(path, File.ReadAllBytes(path));

    /// <summary>Reads stock bytes and verifies their SHA-256 before returning the retained file.</summary>
    /// <param name="path">Path of the stock resource.</param>
    /// <param name="expectedHash">Hash recorded by the stock manifest.</param>
    /// <returns>The validated stock file.</returns>
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

    /// <summary>Selects a same-named override when present, preserving stock otherwise.</summary>
    /// <param name="overrideDirectory">Optional directory containing user replacements.</param>
    /// <returns>The selected replacement or this stock file.</returns>
    internal SamusArtworkFile Select(string? overrideDirectory)
    {
        string? replacement = overrideDirectory is null ? null :
            System.IO.Path.Combine(overrideDirectory, System.IO.Path.GetFileName(Path));
        return replacement is not null && File.Exists(replacement) ? Read(replacement) : this;
    }

    /// <summary>Deserializes this file as strict JSON and adds its path to admission errors.</summary>
    /// <typeparam name="T">Document model to deserialize.</typeparam>
    /// <param name="options">Serializer settings for the artwork document.</param>
    /// <returns>The parsed document.</returns>
    internal T Json<T>(JsonSerializerOptions options) => Compile(stream =>
        JsonAssetDocument.Read<T>(stream, options, $"Samus artwork {Path}"));

    /// <summary>Runs a decoder over a read-only stream of the retained file bytes.</summary>
    /// <typeparam name="T">Value produced by the decoder.</typeparam>
    /// <param name="compile">Decoder that consumes the file stream.</param>
    /// <returns>The decoded value.</returns>
    internal T Compile<T>(Func<Stream, T> compile) => WithContext(() =>
    {
        using var stream = new MemoryStream(Bytes, writable: false);
        return compile(stream);
    });

    /// <summary>Adds this file's path to invalid-data errors unless a nested file already supplied context.</summary>
    /// <typeparam name="T">Value returned by the contextual operation.</typeparam>
    /// <param name="compile">Operation that may reject this file or a nested resource.</param>
    /// <returns>The operation's value.</returns>
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
