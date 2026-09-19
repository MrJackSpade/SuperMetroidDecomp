using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Provenance and selected-content identity for one playable installation. Source revision,
/// compiled engine build, and replaceable presentation are deliberately separate so future
/// replay/state compatibility can distinguish a mechanics change from an art or audio edit.
/// </summary>
public sealed record GameContentIdentity(
    int FormatVersion,
    string SourceCartridgeSha256,
    Guid CompiledDefinitionsBuildId,
    string AudioContentSha256,
    string MapContentSha256,
    string ProjectileContentSha256,
    string CompositeSha256)
{
    /// <summary>Version of this framed component-identity contract.</summary>
    public const int CurrentFormatVersion = 1;

    /// <summary>Builds an identity from the three validated catalogs selected by a host.</summary>
    public static GameContentIdentity Create(
        ExtractedAudioAssetCatalog audio,
        AreaMapPresentationCatalog maps,
        InstalledProjectilePresentation projectiles)
    {
        ArgumentNullException.ThrowIfNull(audio);
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentNullException.ThrowIfNull(projectiles);
        return Create(
            audio.ContentIdentity,
            maps.ContentIdentity,
            projectiles.SelectedSha256,
            typeof(SuperMetroidRuntime).Module.ModuleVersionId);
    }

    /// <summary>
    /// Builds the same identity from validated component digests. This is also the narrow test
    /// seam for proving that each independently replaceable domain invalidates the aggregate.
    /// </summary>
    public static GameContentIdentity Create(
        string audioContentSha256,
        string mapContentSha256,
        string projectileContentSha256,
        Guid compiledDefinitionsBuildId)
    {
        ValidateDigest(audioContentSha256, nameof(audioContentSha256));
        ValidateDigest(mapContentSha256, nameof(mapContentSha256));
        ValidateDigest(projectileContentSha256, nameof(projectileContentSha256));
        string source = SupportedCartridge.Sha256;
        ValidateDigest(source, nameof(SupportedCartridge.Sha256));

        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Append(hash, "SuperMetroid.GameContentIdentity");
        Append(hash, CurrentFormatVersion.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Append(hash, source);
        Append(hash, compiledDefinitionsBuildId.ToString("D"));
        Append(hash, audioContentSha256);
        Append(hash, mapContentSha256);
        Append(hash, projectileContentSha256);
        return new GameContentIdentity(
            CurrentFormatVersion,
            source,
            compiledDefinitionsBuildId,
            audioContentSha256,
            mapContentSha256,
            projectileContentSha256,
            Convert.ToHexString(hash.GetHashAndReset()));
    }

    /// <summary>Creates the ROM-free identity payload persisted by diagnostic artifacts.</summary>
    public GameContentIdentitySnapshot ToSnapshot() => new()
    {
        FormatVersion = FormatVersion,
        CompiledDefinitionsBuildId = CompiledDefinitionsBuildId,
        AudioContentSha256 = Convert.FromHexString(AudioContentSha256),
        MapContentSha256 = Convert.FromHexString(MapContentSha256),
        ProjectileContentSha256 = Convert.FromHexString(ProjectileContentSha256),
        CompositeSha256 = Convert.FromHexString(CompositeSha256),
    };

    /// <summary>
    /// Describes diagnostic-artifact compatibility drift without conflating selected installed content
    /// with the source-cartridge check performed by the host.
    /// </summary>
    public IReadOnlyList<string> GetCompatibilityWarnings(
        GameContentIdentitySnapshot? recorded,
        string artifactName)
    {
        if (recorded is null)
        {
            return [
                $"Legacy {artifactName} has no installed-content identity; " +
                "source-ROM compatibility was verified, but build and presentation drift cannot be identified.",
            ];
        }

        var warnings = new List<string>();
        if (recorded.FormatVersion != FormatVersion)
        {
            warnings.Add(
                $"Installed-content identity format differs: recording={recorded.FormatVersion}, " +
                $"current={FormatVersion}.");
        }
        if (recorded.CompiledDefinitionsBuildId != CompiledDefinitionsBuildId)
        {
            warnings.Add(
                $"Compiled gameplay definitions differ: recording={recorded.CompiledDefinitionsBuildId:D}, " +
                $"current={CompiledDefinitionsBuildId:D}.");
        }
        AddDigestWarning(warnings, "audio", recorded.AudioContentSha256, AudioContentSha256);
        AddDigestWarning(warnings, "map", recorded.MapContentSha256, MapContentSha256);
        AddDigestWarning(warnings, "projectile", recorded.ProjectileContentSha256, ProjectileContentSha256);

        // Component/build warnings already explain the aggregate mismatch. Retain an
        // aggregate-only guard so a malformed or differently framed identity never passes
        // merely because its visible components happen to agree.
        byte[] currentComposite = Convert.FromHexString(CompositeSha256);
        if (warnings.Count == 0 && !CryptographicOperations.FixedTimeEquals(
                recorded.CompositeSha256,
                currentComposite))
        {
            warnings.Add(
                $"Aggregate installed-content identity differs: " +
                $"recording={Convert.ToHexString(recorded.CompositeSha256)}, current={CompositeSha256}.");
        }
        return warnings;
    }

    private static void Append(IncrementalHash hash, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }

    private static void ValidateDigest(string digest, string parameterName)
    {
        if (digest is null || digest.Length != SHA256.HashSizeInBytes * 2 ||
            !digest.All(Uri.IsHexDigit))
        {
            throw new ArgumentException(
                "Content identities must be 64 hexadecimal SHA-256 characters.",
                parameterName);
        }
    }

    private static void AddDigestWarning(
        List<string> warnings,
        string component,
        byte[] recorded,
        string currentHex)
    {
        byte[] current = Convert.FromHexString(currentHex);
        if (!CryptographicOperations.FixedTimeEquals(recorded, current))
        {
            warnings.Add(
                $"Selected {component} content differs: recording={Convert.ToHexString(recorded)}, " +
                $"current={currentHex}.");
        }
    }
}
