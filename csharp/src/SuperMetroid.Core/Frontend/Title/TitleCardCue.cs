namespace SuperMetroid.Core.Frontend.Title;

/// <summary>What a title card does as it ends (#1826): start the next floor scene, or remove the card.</summary>
internal enum TitleCardCue
{
    /// <summary>Removes the card.</summary>
    RemoveCard,

    /// <summary>Starts the lower pan.</summary>
    StartLowerPan,

    /// <summary>Starts the upper pan.</summary>
    StartUpperPan,

    /// <summary>Starts the downward pan.</summary>
    StartDownwardPan,

    /// <summary>Starts the final zoom.</summary>
    StartFinalZoom,
}
