using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The room objects of the opening cinematic's collision rooms, which have none (#2049). The discovery and Mother Brain
/// flashback rooms hold only air, solid and sloped blocks, so neither the flashback Samus's movement nor her shots ever
/// touch a block whose reaction reaches a room object; reaching one fails, naming what was reached.
/// </summary>
internal sealed class IntroCollisionRoomObjects : ISamusRoomObjects
{
    /// <summary>The one set of room objects every intro collision room shares: none.</summary>
    internal static IntroCollisionRoomObjects Instance { get; } = new();

    /// <summary>Creates the intro rooms' empty set of room objects.</summary>
    private IntroCollisionRoomObjects()
    {
    }

    /// <inheritdoc/>
    public BlockReactionObjects BlockReactions => throw Reached(nameof(BlockReactions));

    /// <inheritdoc/>
    public IGlobalEventFlags Events => throw Reached(nameof(Events));

    /// <inheritdoc/>
    public RoomObjectSoundQueue Sounds => throw Reached(nameof(Sounds));

    /// <inheritdoc/>
    public IRoomObjectDrawing Drawing => throw Reached(nameof(Drawing));

    /// <inheritdoc/>
    public CollectibleObjects CollectibleObjects => throw Reached(nameof(CollectibleObjects));

    /// <inheritdoc/>
    public StationObjects Stations => throw Reached(nameof(Stations));

    /// <inheritdoc/>
    public DoorObjects DoorObjects => throw Reached(nameof(DoorObjects));

    /// <inheritdoc/>
    public EyeDoorObjects EyeDoors => throw Reached(nameof(EyeDoors));

    /// <inheritdoc/>
    public GreyDoorObjects GreyDoors => throw Reached(nameof(GreyDoors));

    /// <inheritdoc/>
    public GateObjects Gates => throw Reached(nameof(Gates));

    /// <inheritdoc/>
    public SamusEaterObjects SamusEater => throw Reached(nameof(SamusEater));

    /// <inheritdoc/>
    public ScrollObjects Scrolls => throw Reached(nameof(Scrolls));

    /// <inheritdoc/>
    public ProjectileTriggerObjects ProjectileTriggers => throw Reached(nameof(ProjectileTriggers));

    /// <inheritdoc/>
    public QuicksandObjects Quicksand => throw Reached(nameof(Quicksand));

    /// <inheritdoc/>
    public IChozoStatueHand ChozoStatueHand => throw Reached(nameof(ChozoStatueHand));

    /// <inheritdoc/>
    public void DeleteObject(RoomObjectPlace place) => throw Reached(nameof(DeleteObject));

    /// <summary>The failure of a touch that reached <paramref name="family"/>, which the intro's rooms do not have.</summary>
    private static InvalidOperationException Reached(string family) =>
        new($"The opening cinematic's collision rooms hold no room objects, but a touched block reached {family}.");
}
