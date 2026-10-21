using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>The enemy state and hooks the room main routines and scrolling read or run.</summary>
internal interface IEnemyRoomMainHooks
{
    /// <summary>The loaded Crocomire, if any.</summary>
    CrocomireEnemyState? Crocomire { get; }

    /// <summary>Crocomire's death sequence, if it has begun.</summary>
    CrocomireDeathState? CrocomireDeath { get; }

    /// <summary>Runs Spore Spawn's scrolling-finished hook ($90:9589) when it is installed.</summary>
    void RunSporeSpawnScrollingFinishedHook(ScrollBoundaryCamera camera);

    /// <summary>The Zebetites' camera-distance index.</summary>
    CameraDistanceMode ZebetiteCameraDistanceIndex { get; }
}
