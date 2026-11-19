using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon grabbed or released Samus (#2562): the grab and the release discard the pose changes the input phase already
/// chose for her old pose, including the no-input fallback kept beside them.
/// </summary>
public sealed record DraygonHoldChanged : EnemyPassEvent;
