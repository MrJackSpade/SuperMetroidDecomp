namespace SuperMetroid.Core.Frontend.Title;

/// <summary>What a title card does as it ends (#1826): start the next floor scene, or remove the card.</summary>
/// <remarks>The values are arbitrary (#1416).</remarks>
internal enum TitleCardCue
{
    /// <summary>Removes the card.</summary>
    RemoveCard = 1,

    /// <summary>Starts the lower pan.</summary>
    StartLowerPan,

    /// <summary>Starts the upper pan.</summary>
    StartUpperPan,

    /// <summary>Starts the downward pan.</summary>
    StartDownwardPan,

    /// <summary>Starts the final zoom.</summary>
    StartFinalZoom,
}
