namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Debugger-visible result of bank-$94's post-horizontal slope correction.</summary>
public readonly record struct SlopeAlignmentResult(
    ushort YPosition,
    bool Adjusted);
