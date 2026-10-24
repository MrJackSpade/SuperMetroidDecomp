using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Diagnostics.Sessions;

/// <summary>The content a host loaded from its installation, supplied to every session it composes.</summary>
/// <param name="Artwork">The installed gameplay and frontend catalogs.</param>
/// <param name="AudioClips">The selected audio clips.</param>
public sealed record InstalledSessionContent(GameArtwork Artwork, ClipAudioCatalog AudioClips) : IInstalledSessionContent;
