namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block (<see cref="BombSpecialBlockReactionDefinitions.Responses"/>).</summary>
internal enum SpecialBlockBombResponse
{
    /// <summary>The crumble block's reveal, by the crumble's size.</summary>
    CrumbleReveal,

    /// <summary>The Speed Booster block's reveal.</summary>
    SpeedBlockReveal,

    /// <summary>A room object that does nothing but hold its place for an update.</summary>
    Nothing,
}
