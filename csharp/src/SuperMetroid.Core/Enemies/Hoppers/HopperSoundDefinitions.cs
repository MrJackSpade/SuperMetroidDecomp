namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The library-two sounds the Sidehoppers' programs queue.</summary>
internal static class HopperSoundDefinitions
{
    /// <summary>The hop sound $5D, queued as a Sidehopper's jumping program starts.</summary>
    internal static readonly SoundEffectId Hop = Library2Sound.SidehopperJump;

    /// <summary>The landing sound $5E, queued as a Sidehopper's landed program starts.</summary>
    internal static readonly SoundEffectId Land = Library2Sound.SidehopperLanding;
}
