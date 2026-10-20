namespace SuperMetroid.Core.Runtime;

/// <summary>How the gameplay frame being finished began.</summary>
public enum GameplayFrameEntry
{
    /// <summary>An ordinary frame.</summary>
    Fresh,

    /// <summary>A frame resumed after a message box.</summary>
    ResumedAfterMessageBox,
}
