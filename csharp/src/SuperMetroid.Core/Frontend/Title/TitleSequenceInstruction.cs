namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The commands that end a title card: start the next floor scene, or remove the card.</summary>
/// <remarks>The values are arbitrary (#1416); verification's TitleNativeWords names the cartridge routine of each.</remarks>
internal enum TitleSequenceInstruction
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
