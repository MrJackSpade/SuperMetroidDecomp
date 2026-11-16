namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What the Draygon eye does each update.</summary>
internal enum DraygonEyeAction
{
    /// <summary>The eye is inert.</summary>
    Inert,

    /// <summary>The eye tracks Samus the way Draygon faces (<see cref="DraygonEyeState.Facing"/>).</summary>
    Tracking,
}
