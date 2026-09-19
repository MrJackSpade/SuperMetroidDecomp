namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Installed-content identity captured beside a deterministic controller recording.
/// </summary>
/// <remarks>
/// The source cartridge digest remains a separate field on
/// <see cref="ControllerInputRecording"/>. These values identify the compiled gameplay
/// definitions and the replaceable presentation catalogs selected by the host, allowing a
/// replay consumer to explain which part of an installation differs without embedding any
/// copyrighted content.
/// </remarks>
public sealed record ControllerRecordingContentIdentity
{
    /// <summary>Version of the framed installed-content identity contract.</summary>
    public required int FormatVersion { get; init; }

    /// <summary>Module build ID containing the compiled cartridge definitions.</summary>
    public required Guid CompiledDefinitionsBuildId { get; init; }

    /// <summary>SHA-256 of the selected validated audio catalog.</summary>
    public required byte[] AudioContentSha256 { get; init; }

    /// <summary>SHA-256 of the selected validated map-presentation catalog.</summary>
    public required byte[] MapContentSha256 { get; init; }

    /// <summary>SHA-256 of the selected validated projectile-presentation catalog.</summary>
    public required byte[] ProjectileContentSha256 { get; init; }

    /// <summary>SHA-256 aggregate over source, build, and selected component identities.</summary>
    public required byte[] CompositeSha256 { get; init; }
}
