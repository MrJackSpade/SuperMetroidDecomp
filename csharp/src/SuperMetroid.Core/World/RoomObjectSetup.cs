using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>
/// The setup a placed room object's family runs as the room's population places it, once the object has taken its place
/// (#2288). Each placement's kind names its family's setup (<see cref="RoomObjectSetups"/>), which the population loader
/// calls directly.
/// </summary>
internal abstract class RoomObjectSetup
{
    /// <summary>Sets up the object <paramref name="context"/>'s placement placed in its place.</summary>
    internal abstract void SetUp(RoomObjectSetupContext context);

    /// <summary>A coloured door's setup: its cap in its colour and facing, locked by its placement's door.</summary>
    internal sealed class ColoredDoor(ColoredDoorColor color, ColoredDoorOrientation orientation) : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            DoorObjects.SetupColoredDoor(context.Room.Level, context.Place, color, orientation,
                context.Placement.Argument as LockedDoorArgument);
    }

    /// <summary>A grey door's setup, the Bomb Torizo's among them, opened by its placement's condition.</summary>
    internal sealed class GreyDoor(GreyDoorVariant variant) : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            GreyDoorObjects.SetupGreyDoor(context.Room.Level, context.Place, variant, context.Placement.Argument as GreyDoorArgument);
    }

    /// <summary>A scroll trigger's or scroll extension's setup.</summary>
    internal sealed class Scroll(RoomObjectKind kind) : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            ScrollObjects.SetupScroll(context.Room.Level, context.Place, kind, context.Placement.ScrollChange);
    }

    /// <summary>A permanent item's setup: its kind shown as it is placed, unless its placement's item is already collected.</summary>
    internal sealed class Collectible(InWorldCollectibleKind kind, CollectiblePresentation presentation) : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            context.Families.CollectibleObjects.SetupCollectible(context.Room, context.Place, kind, presentation,
                context.Placement.Argument as CollectedItemArgument);
    }

    /// <summary>An eye door's part's setup.</summary>
    internal sealed class EyeDoor(RoomObjectKind part) : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            context.Families.EyeDoors.SetupEyeDoor(context.Room.Level, context.Place, part, context.Placement.Argument as LockedDoorArgument);
    }

    /// <summary>A Draygon wall cannon's setup: its facing and whether it starts destroyed, controlling its placement's cannon.</summary>
    internal sealed class DraygonCannon(DraygonCannonSetup setup) : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            DraygonCannonObjects.SetupDraygonCannon(context.Room.Level, context.Place, setup, context.Placement.Cannon
                ?? throw new InvalidDataException($"Draygon cannon placement {context.Placement.Kind} names no Draygon wall cannon."));
    }

    /// <summary>A station's setup, a save station saving at the load station its placement names.</summary>
    internal sealed class Station(StationKind kind) : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            StationObjects.SetupStation(context.Room.Level, context.Room.System, context.Room.Area, context.Place, kind,
                (context.Placement.Argument as SaveStationArgument)?.Station);
    }

    /// <summary>An elevator platform's setup.</summary>
    internal sealed class ElevatorPlatform : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            StationObjects.SetupElevatorPlatform(context.Room.Level, context.Place);
    }

    /// <summary>Mother Brain's glass's setup.</summary>
    internal sealed class MotherBrainGlass : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            context.Families.MotherBrainGlass.SetupMotherBrainGlass(context.Room.Level, context.Place);
    }

    /// <summary>The Bomb Torizo statue's hand's setup, by whether the Torizo is beaten.</summary>
    internal sealed class BombTorizoHand : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            context.Families.BombTorizoHand.SetupBombTorizoHand(context.Place, context.Bosses);
    }

    /// <summary>The setup of the Metroid room's clearance, by whether the placement's room's Metroids are cleared.</summary>
    internal sealed class MetroidRoomClearance : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            TourianObjects.SetupMetroidsCleared(context.Place, context.Placement.Argument as MetroidRoomArgument);
    }

    /// <summary>The Speed Booster's escape's setup.</summary>
    internal sealed class SpeedBoosterEscape : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            context.Families.SpeedBoosterEscape.SetupSpeedBoosterEscape(context.Place);
    }

    /// <summary>The Wrecked Ship attic's setup, which changes nothing: the object only holds its place.</summary>
    internal sealed class WreckedShipAttic : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) => context.Place.Become(new WreckedShipAtticObject());
    }

    /// <summary>Maridia's glass tube's setup.</summary>
    internal sealed class NoobTube : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            context.Families.NoobTube.SetupNoobTube(context.Room.Level, context.Place);
    }

    /// <summary>The setup of the gate Mother Brain's escape room shuts behind Samus: a passable block, shut.</summary>
    internal sealed class MotherBrainEscapeRoomGate : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context)
        {
            DoorObjects.SetupPassableBlock(context.Room.Level, context.Place);
            context.Place.Become(new MotherBrainEscapeGateObject(MotherBrainEscapeGateDefinitions.Shut));
        }
    }

    /// <summary>A downward gate's setup.</summary>
    internal sealed class DownwardGate : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            context.Families.Gates.SetupDownwardGate(context.Room.Level, context.Place);
    }

    /// <summary>A downward gate's shot block's setup, triggering its placement's gate.</summary>
    internal sealed class DownwardGateShotBlock : RoomObjectSetup
    {
        /// <inheritdoc/>
        internal override void SetUp(RoomObjectSetupContext context) =>
            GateObjects.SetupDownwardGateShotBlock(context.Room.Level, context.Place, context.Placement.Argument as GateTriggerArgument);
    }
}
