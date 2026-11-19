namespace SuperMetroid.Core.Frontend.Title;

/// <summary>How the title sequence hands off to the frontend (#2562), which it keeps once it reaches it.</summary>
public enum TitleSequenceOutcome
{
    /// <summary>The title runs: it has handed off to nothing yet.</summary>
    Playing,
    /// <summary>The title's slow fade hands control to file select.</summary>
    FileSelect,
    /// <summary>The idle timeout's slow fade reaches the demo.</summary>
    Demo,
}
