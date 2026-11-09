namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The panel the post-credits text shows (#1555).</summary>
internal enum EndingPostCreditsPanel
{
    /// <summary>No panel.</summary>
    None,
    /// <summary>The producer and development credits, shown from the waiting backdrop's end until the copyright replaces them.</summary>
    Result,
    /// <summary>The copyright, shown from the reward's first half until the white flash clears it.</summary>
    Copyright,
}
