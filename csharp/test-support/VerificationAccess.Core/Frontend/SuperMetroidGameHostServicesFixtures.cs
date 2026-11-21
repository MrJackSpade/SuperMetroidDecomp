namespace SuperMetroid.Core.Frontend;

/// <summary>Host services a check runs a session against (#2570): the game's own, with the services the check replaces.</summary>
internal static class SuperMetroidGameHostServicesFixtures
{
    /// <summary>The game's own services with each one given here in its place.</summary>
    internal static SuperMetroidGameHostServices With(
        IDoorMusicLoadFrames? doorMusicLoadFrames = null,
        IRoomLoadProgress? roomLoadProgress = null,
        ISoundAcceptanceTiming<SoundEffectLibrary, SoundEffectId>? soundAcceptance = null,
        IGameplayFramePublication? gameplayFrames = null)
    {
        SuperMetroidGameHostServices standard = SuperMetroidGameHostServices.Default;
        return new SuperMetroidGameHostServices(doorMusicLoadFrames ?? standard.DoorMusicLoadFrames, roomLoadProgress ?? standard.RoomLoadProgress, soundAcceptance ?? standard.SoundAcceptance, gameplayFrames ?? standard.GameplayFrames);
    }
}
