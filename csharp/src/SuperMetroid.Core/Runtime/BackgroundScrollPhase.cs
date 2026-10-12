using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The background scroll phase: scroll registers from the camera, background tilemap streaming, and layer-1
/// position adoption.
/// </summary>
internal sealed class BackgroundScrollPhase
{
    /// <summary>Stores background scroll.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Stores room.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Stores vram.</summary>
    private readonly SnesVram _vram;

    /// <summary>Initializes a new background scroll phase instance.</summary>
    internal BackgroundScrollPhase(BackgroundScrollState backgroundScroll, ActiveRoomState room, SnesVram vram)
    {
        _backgroundScroll = backgroundScroll;
        _room = room;
        _vram = vram;
    }

    /// <summary>
    /// Copies the translated layer-1 camera into <c>$80:A3AB</c> scroll bookkeeping and
    /// returns the exact BG update calls selected for this frame.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> UpdateBackgroundScrollingFromCamera(bool timeIsFrozen = false)
    {
        if (_room.Camera is null)
            throw new InvalidOperationException("A room camera must be initialized before BG scrolling.");

        _backgroundScroll.Layer1Position = _room.Camera.PixelPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = _backgroundScroll.StepScrolling(timeIsFrozen);

        // Both native block producers begin with the same `$0783` test at
        // `$80:A9DE/$80:AB78`. Ceres's elevator door sets that Mode-7 IRQ word, so crossing
        // a 16-pixel camera boundary still updates all layer/register bookkeeping above but
        // performs no ordinary Mode-1 row or column expansion. Apart from corrupting Mode-7
        // VRAM, retaining these requests made the bottom-right 17th row sample walk beyond
        // Ceres's logical BG2 allocation and turned harmless native overfetch into a host
        // exception.
        if (_room.ActiveDoor?.UsesCeresElevatorMode7 == true)
        {
            _room.LastBackgroundUpdateCount = 0;
            return Array.Empty<BackgroundUpdateRequest>();
        }

        _room.LastBackgroundUpdateCount = requests.Count;
        return requests;
    }

    /// <summary>
    /// Expands and commits one ordered set of bank-$80 scrolling requests to the live
    /// tilemap ring buffer. Gameplay camera tracking and the door IRQ are separate native
    /// callers of the same row/column producers; keeping their consumer shared prevents a
    /// transition from updating only the scroll registers while silently dropping its DMA.
    /// </summary>
    internal void ExecuteBackgroundStreamRequests(
        IReadOnlyList<BackgroundUpdateRequest> requests,
        string owner)
    {
        Ensure.NotNull(requests);
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        if (_room.BackgroundStreamer is null)
            throw new InvalidOperationException($"{owner} has no active background streamer.");

        // Ceres's elevator door keeps the ordinary block-coordinate mirrors moving while
        // its IRQ owns Mode 7. Native `$0783` suppresses the corresponding BG1/BG2 DMA;
        // preserve that gate for both gameplay and door-transition callers.
        if (_room.ActiveDoor?.UsesCeresElevatorMode7 == true)
        {
            _room.LastBackgroundUpdateCount = 0;
            return;
        }

        _room.LastBackgroundUpdateCount = requests.Count;
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = _room.BackgroundStreamer.Build(request)
                ?? throw new InvalidOperationException(
                    $"{owner} unexpectedly requested ordinary streaming while Mode 7 is active.");
            update.ExecuteTo(_vram);
        }
    }

    /// <summary>Adopts the layer-one position whose fraction words survived the previous runtime.</summary>
    internal void AdoptLayer1Position(FixedPosition<RoomPixels> retained) => _room.InheritedLayer1Position = retained;
}
