namespace SuperMetroid.Core.Samus;

/// <summary>The installed Samus overread file: each byte range as base-64 text, in cartridge order.</summary>
/// <param name="Version">The schema revision, <see cref="SamusOverreadFormat.Version"/>.</param>
/// <param name="SpacetimeCopySource">The bytes the SpaceTime palette-copy overrun can read.</param>
/// <param name="SpeedBoosterGrappleCode">The grapple-handler code the Speed Booster palette overrun reads as colours.</param>
/// <param name="HudPrecedingPostureBytes">The bytes before the HUD's posture-transition flags.</param>
/// <param name="HudFollowingPostureBytes">The bytes after the HUD's posture-transition flags.</param>
/// <param name="TrailOverruns">What a trail reads past each placement list's records, by list name, as base-64 entries in frame order.</param>
public sealed record SamusOverreadDocument(
    int Version,
    string SpacetimeCopySource,
    string SpeedBoosterGrappleCode,
    string HudPrecedingPostureBytes,
    string HudFollowingPostureBytes,
    IReadOnlyDictionary<string, string> TrailOverruns);
