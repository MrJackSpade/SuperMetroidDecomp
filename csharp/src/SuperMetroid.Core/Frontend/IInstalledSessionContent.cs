namespace SuperMetroid.Core.Frontend;

/// <summary>The installed content a host supplies to every session it composes: immutable, host-owned and shared.</summary>
public interface IInstalledSessionContent
{
    /// <summary>The installed gameplay and frontend catalogs.</summary>
    GameArtwork Artwork { get; }

    /// <summary>The installed audio clips the session's playback state binds, whether or not samples reach a device.</summary>
    ClipAudioLibrary AudioClips { get; }
}
