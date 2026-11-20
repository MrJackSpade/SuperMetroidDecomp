namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A displayed state kept apart from the live state (#2571): the accepted frame's boundary latches it with the
/// rest of the frame, for every such display the room holds, so the frame shows one coherent state.
/// </summary>
internal interface IDisplayLatch
{
    /// <summary>Latches the live state as the state the accepted frame shows.</summary>
    void LatchDisplay();
}
