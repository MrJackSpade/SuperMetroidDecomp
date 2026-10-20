using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The loader hooks over Rinka and the pre-Phantoon room.</summary>
/// <param name="rinka">Rinka.</param>
/// <param name="prePhantoonRoom">The pre-Phantoon room.</param>
internal sealed class EnemyLoaderHooks(RinkaBehavior rinka, PrePhantoonRoomBehavior prePhantoonRoom) : IEnemyLoaderHooks
{
    /// <inheritdoc/>
    public bool HasDeferredRinkaLoaderTimeCameraReads => rinka.HasDeferredLoaderTimeCameraReads;

    /// <inheritdoc/>
    public void CompleteRinkaLoaderTimeCameraReads(Func<int, bool> loaderInitializedSlot, Position16<RoomPixels> camera) =>
        rinka.CompleteLoaderTimeCameraReads(loaderInitializedSlot, camera);

    /// <inheritdoc/>
    public void SpawnPrePhantoonRoomProjectile(BackgroundScrollState backgroundScroll) => prePhantoonRoom.SpawnPrePhantoonRoomProjectile(backgroundScroll);
}
