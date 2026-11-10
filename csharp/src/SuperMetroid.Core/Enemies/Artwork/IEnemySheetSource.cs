namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The pre-coloured artwork each room population's enemies draw from (#1392, #1599).</summary>
public interface IEnemySheetSource
{
    /// <summary>The artwork a room that places no enemies draws its spawned objects from: each owner's standard variant.</summary>
    PopulationArtwork Standard { get; }

    /// <summary>The artwork the enemies <paramref name="population"/> places draw from.</summary>
    /// <exception cref="InvalidDataException">No artwork is installed for the population.</exception>
    PopulationArtwork For(RoomEnemyPopulationDefinition population);
}
