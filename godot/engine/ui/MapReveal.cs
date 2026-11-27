namespace Engine.Ui;

/// <summary>What a map shows beyond the cells the player has explored and the cells their progress has revealed.</summary>
public enum MapReveal
{
    /// <summary>Nothing more: the map as the game is played.</summary>
    AsPlayed,

    /// <summary>Every cell the area's own map would reveal, whether or not the player has found it.</summary>
    Charted,

    /// <summary>Every cell there is, the secret ones among them.</summary>
    Everything,
}
