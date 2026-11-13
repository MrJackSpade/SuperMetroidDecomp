namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The steps that end a title card: start the next floor scene, or remove the card.</summary>
/// <remarks>The values are arbitrary (#1416).</remarks>
internal enum TitleSequenceScriptStep
{
    /// <summary>Removes the card.</summary>
    Delete = 1,

    /// <summary>Starts the lower pan.</summary>
    TriggerScene0,

    /// <summary>Starts the upper pan.</summary>
    TriggerScene1,

    /// <summary>Starts the downward pan.</summary>
    TriggerScene2,

    /// <summary>Starts the final zoom.</summary>
    TriggerScene3,
}
