namespace SuperMetroid.Core.Frontend;

/// <summary>What a sequence's current step runs on (#2554).</summary>
internal enum PhaseGameplayNeed
{
    /// <summary>Nothing of gameplay: a frontend screen, or a step that creates or releases the runtime.</summary>
    Nothing,
    /// <summary>The session's gameplay runtime, which may not have loaded a room.</summary>
    Runtime,
    /// <summary>The session's gameplay runtime and its loaded room.</summary>
    LoadedRoom,
}
