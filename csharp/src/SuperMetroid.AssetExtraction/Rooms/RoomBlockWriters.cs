using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectCollectibleDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Every writer of blocks into a running room the installation holds the pictures of (#1636), read from the installed room
/// object catalogs and X-ray reveal, each with the room states whose installed data directly holds what starts it. Every
/// block a room object writes comes from one of these catalogs: the game's room-object drawing draws only their installed
/// frames and the block pairs a collectible that shows its own picture writes under it, a block reaction's setup writes
/// only the installed setup blocks, and every other terrain write keeps the block already there or is drawn over in the
/// same update before it shows. A writer counts for a room state only where that state's terrain holds the reaction that
/// starts it, its population places the room object or enemy that draws it, its setup or main routine spawns it, or a door
/// into its room closes it behind Samus; nothing follows what a writer goes on to trigger. A block a room shows that no
/// writer counted for its tileset fails as the room resolves its pictures, naming the sheet and the block.
/// </summary>
internal static class RoomBlockWriters
{
    /// <summary>The writers the installed <paramref name="catalogs"/> and <paramref name="xray"/> reveal hold the blocks of.</summary>
    /// <exception cref="InvalidDataException">An installed frame or X-ray reveal has no rule that draws it.</exception>
    internal static IReadOnlyList<RoomBlockWriter> Of(RoomObjectVisualCatalogSet catalogs, InstalledXrayReveals xray)
    {
        Ensure.NotNull(catalogs);
        Ensure.NotNull(xray);
        return
        [
            // The block reactions the room's own terrain starts, and the blocks their setups write.
            .. PerFrame(catalogs.ShotBlock.Images, ShotBlockFrameWrittenIn),
            new(FrameBlocks(catalogs.GrappleBlock.Images), state => state.HoldsBreakableGrappleBlocks),
            .. PerFrame(catalogs.LinkedRestore.Images, LinkedRestoreWrittenIn),
            new(FrameBlocks(catalogs.SpeedBooster.Images), state => state.HoldsSpeedBoosterBlocks),
            new(FrameBlocks(catalogs.SamusEater.Images), state => state.HoldsSamusEaterPlants),
            .. catalogs.SetupBlocks.All.Select(setup => new RoomBlockWriter(                new HashSet<int> { setup.Value.Block }, SetupBlockWrittenIn(setup.Key))),
            // The blue door caps a shootable cap opens, and those closing behind Samus as she comes through a door.
            .. PerFrame(catalogs.BlueDoor.Images, BlueDoorFrameWrittenIn),
            // The room objects the population places.
            .. PerFrame(catalogs.ColoredDoor.Images, ColoredDoorFrameWrittenIn),
            // The grey caps, and the cleared opening every door cap of a facing shows once it opens (#1636).
            .. PerFrame(catalogs.GreyDoor.Images, GreyDoorFrameWrittenIn),
            new(FrameBlocks(catalogs.EyeDoor.Images), Placing(EyeDoorPartDefinitions.Parts.Keys)),
            new(FrameBlocks(catalogs.DownwardGate.Images), state => state.HoldsDownwardGateTriggers ||
                state.Headers.Contains(RoomObjectKind.DownwardGate) || state.Headers.Contains(RoomObjectKind.DownwardGateShotBlock)),
            .. PerFrame(catalogs.Collectible.Images, CollectibleFrameWrittenIn),
            new(BlockPairBlocks(), state => state.PlacesCollectible(static (kind, _) => CollectibleObjects.ShowsOwnPicture(kind))),
            .. PerFrame(catalogs.Station.Images, StationFrameWrittenIn),
            new(FrameBlocks(catalogs.ElevatorPlatform.Images), Placing([RoomObjectKind.ElevatorPlatform])),
            new(FrameBlocks(catalogs.BombTorizoHand.Images), Placing([RoomObjectKind.BombTorizoHand])),
            new(FrameBlocks(catalogs.DraygonCannon.Images), Placing(DraygonCannonDefinitions.Setups.Keys)),
            new(FrameBlocks(catalogs.MotherBrainGlass.Images), Placing([RoomObjectKind.MotherBrainGlass])),
            new(FrameBlocks(catalogs.EscapeGate.Images), state =>
                state.Headers.Contains(RoomObjectKind.MotherBrainEscapeRoomGate) ||
                state.Entrances.Any(door => door.Orientation.Closing == DoorClosingBehavior.EscapeGateCloses)),
            new(FrameBlocks(catalogs.NoobTube.Images), Placing([RoomObjectKind.NoobTube])),
            // The enemies the population places, which change their own rooms.
            new(FrameBlocks(catalogs.Kraid.Images), Holding([EnemyDefinitionId.Kraid])),
            new(FrameBlocks(catalogs.Crocomire.Images), Holding([EnemyDefinitionId.Crocomire])),
            new(FrameBlocks(catalogs.MotherBrainFakeDeath.Images), Holding([EnemyDefinitionId.MotherBrainBody])),
            new(FrameBlocks(catalogs.ChozoStatue.Images), Holding([EnemyDefinitionId.Chozo])),
            new(FrameBlocks(catalogs.SporeSpawnCeiling.Images), Holding([EnemyDefinitionId.SporeSpawn])),
            new(FrameBlocks(catalogs.BotwoonWall.Images), Holding([EnemyDefinitionId.Botwoon])),
            new(FrameBlocks(catalogs.PhantoonDoor.Images), Holding([EnemyDefinitionId.PhantoonBody])),
            // What the state's setup and main routines spawn.
            new(FrameBlocks(catalogs.TourianAccess.Images),
                state => state.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations),
            .. PerFrame(catalogs.RevealAndWall.Images, RevealOrWallWrittenIn),
            new(FrameBlocks(catalogs.MaridiaElevatube.Images), state => state.MainCallback == RoomMainCallback.MaridiaElevatube),
            // The X-ray reveal: each reveal where the terrain holds the block it copies over, each tank where one is placed,
            // the extension exit where an extension can lead out, and a room's overlay only there.
            .. XrayRevealWriters(xray),
            .. TankKinds.Select(tank => new RoomBlockWriter(new HashSet<int> { xray.Overlays.TankMetatile(tank).Block },
                state => state.PlacesCollectible((kind, _) => kind == tank))),
            new(new HashSet<int> { xray.ExtensionExitBlock }, state => state.HoldsExtensions),
            .. xray.Overlays.rooms.Select(overlay => new RoomBlockWriter(                BlocksOf(overlay.Value.Select(tile => tile.Appearance)), state => Equals(state.XrayOverlay, overlay.Key))),
        ];
    }

    /// <summary>
    /// Whether a shot block's frame <paramref name="draw"/> is written in a room state: a breakup wherever a block breaks up
    /// (a reaction of the terrain, a shot-block collectible, Botwoon's wall or a Chozo statue's crumbling), a restoration
    /// wherever a shot block grows back. The terrain's reactions include those a Wave shot's scan past the room's ceiling
    /// sets off where it reads halfway through a block (#1650): Crocomire's arena breaks and restores respawning shot
    /// blocks that way at blocks (99, 1) and (100, 1), whose installed terrain is solid.
    /// </summary>
    /// <exception cref="InvalidDataException">The draw is not a shot block's breakup or restoration.</exception>
    private static Func<InventoriedRoomState, bool> ShotBlockFrameWrittenIn(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> { Frame.Restore: true } => static state => state.HoldsRespawningShotBlocks,
        RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> => static state => state.HoldsBreakingBlocks ||
            state.PlacesCollectible(static (_, presentation) => presentation == CollectiblePresentation.ShotBlock) ||
            state.Enemies.Contains(EnemyDefinitionId.Botwoon) || state.Enemies.Contains(EnemyDefinitionId.Chozo),
        _ => throw new InvalidDataException($"Shot-block frame {draw} is not a shot block's breakup or restoration."),
    };

    /// <summary>Whether a linked restoration <paramref name="draw"/> is written in a room state: a bomb block's where bomb blocks lie, a crumble block's where crumble blocks lie.</summary>
    /// <exception cref="InvalidDataException">The draw is neither family's restoration.</exception>
    private static Func<InventoriedRoomState, bool> LinkedRestoreWrittenIn(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<BombBlockRestoreDraw> => static state => state.HoldsBombBlocks,
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> => static state => state.HoldsCrumbleBlocks,
        _ => throw new InvalidDataException($"Linked restoration frame {draw} is neither a bomb block's nor a crumble block's."),
    };

    /// <summary>Whether the setup block <paramref name="block"/> is written in a room state: where what sets it up lies or arrives.</summary>
    /// <exception cref="InvalidDataException">The setup block has no writer.</exception>
    private static Func<InventoriedRoomState, bool> SetupBlockWrittenIn(RoomObjectSetupBlock block) => block switch
    {
        RoomObjectSetupBlock.BombBlockParent => static state => state.HoldsBombBlocks,
        RoomObjectSetupBlock.CrumbleBlockParent => static state => state.HoldsCrumbleBlocks,
        RoomObjectSetupBlock.SpeedBoosterBlockParent => static state => state.HoldsSpeedBoosterBlocks,
        RoomObjectSetupBlock.ShotBlockParent => static state => state.HoldsRespawningShotBlocks,
        RoomObjectSetupBlock.PowerBombBlockParent => static state => state.HoldsPowerBombBlocks,
        RoomObjectSetupBlock.SuperMissileBlockParent => static state => state.HoldsSuperMissileBlocks,
        RoomObjectSetupBlock.CritterWallStruck => BuildsEscapeAnimalWall,
        RoomObjectSetupBlock.TreadmillIdle or RoomObjectSetupBlock.TreadmillRunning => static state => state.Entrances.Any(static door =>
            door.SetupCode is DoorSetupCode.WreckedShipEntranceStartsTreadmillRightwards or DoorSetupCode.WreckedShipEntranceStartsTreadmillLeftwards),
        _ => throw new InvalidDataException($"Room-object setup block {block} has no writer."),
    };

    /// <summary>
    /// Whether a blue door cap's frame <paramref name="draw"/> is written in a room state: where its terrain holds a blue cap
    /// of the frame's facing, or a door into its room closes a blue cap of that facing behind Samus.
    /// </summary>
    /// <exception cref="InvalidDataException">The draw is not a blue door cap's frame.</exception>
    private static Func<InventoriedRoomState, bool> BlueDoorFrameWrittenIn(RoomObjectDraw draw)
    {
        if (!BlueDoorCaps.TryDescribe(draw, out BlueDoorCaps.Draw cap))
            throw new InvalidDataException($"Blue door frame {draw} is not a blue door cap's frame.");
        ColoredDoorOrientation facing = cap.Orientation;
        return state => state.HoldsBlueDoorCap(facing) ||
            state.Entrances.Any(door => DoorClosingObjects.GetDefinition(door.Orientation)?.BlueCapFacing == facing);
    }

    /// <summary>
    /// Whether a grey door cap's frame <paramref name="draw"/> is written in a room state. A grey cap's closing images are
    /// written where a grey door of the facing is placed. The cleared opening is the image every door cap of a facing shows
    /// once it opens: it is written wherever a blue, coloured or grey cap of that facing can open, or a door into the room
    /// closes a cap of that facing behind Samus (#1636).
    /// </summary>
    /// <exception cref="InvalidDataException">The draw is not a grey door cap's frame.</exception>
    private static Func<InventoriedRoomState, bool> GreyDoorFrameWrittenIn(RoomObjectDraw draw)
    {
        if (!GreyDoorCaps.TryDescribe(draw, out GreyDoorCaps.Draw cap))
            throw new InvalidDataException($"Grey door frame {draw} is not a grey door cap's frame.");
        ColoredDoorOrientation facing = cap.Orientation;
        Func<InventoriedRoomState, bool> grey = Placing(DoorObjectKindDefinitions.GreyDoors.Where(door => door.Value == facing).Select(door => door.Key));
        if (!cap.IsCleared)
            return grey;
        Func<InventoriedRoomState, bool> coloured = Placing(DoorObjectKindDefinitions.ColoredDoors
            .Where(door => door.Value.Orientation == facing).Select(door => door.Key));
        return state => grey(state) || coloured(state) || state.HoldsBlueDoorCap(facing) ||
            state.Entrances.Any(door => DoorClosingObjects.GetDefinition(door.Orientation)?.BlueCapFacing == facing);
    }

    /// <summary>Whether a station's frame <paramref name="draw"/> is written in a room state: where a station of the kind that draws it is placed.</summary>
    /// <exception cref="InvalidDataException">The draw is not a station's frame.</exception>
    private static Func<InventoriedRoomState, bool> StationFrameWrittenIn(RoomObjectDraw draw)
    {
        if (draw is not RoomObjectFrameDraw<StationDraw> { Frame: var station })
            throw new InvalidDataException($"Station frame {draw} is not a station's frame.");
        return Placing(station switch
        {
            StationDraw.MapFrame0 or StationDraw.MapFrame1 or StationDraw.MapFrame2 or StationDraw.MapRightRetracted or
                StationDraw.MapRightExtended or StationDraw.MapLeftRetracted or StationDraw.MapLeftExtended => [RoomObjectKind.MapStation],
            StationDraw.EnergyFrame0 or StationDraw.EnergyFrame1 or StationDraw.EnergyFrame2 => [RoomObjectKind.EnergyStation],
            StationDraw.MissileFrame0 or StationDraw.MissileFrame1 or StationDraw.MissileFrame2 => [RoomObjectKind.MissileStation],
            StationDraw.SaveIdle or StationDraw.SaveActive or StationDraw.SaveAlternate => [RoomObjectKind.SaveStation],
            StationDraw.ResourceRightRetracted or StationDraw.ResourceLeftRetracted or StationDraw.ResourceRightExtended or
                StationDraw.ResourceLeftExtended => [RoomObjectKind.EnergyStation, RoomObjectKind.MissileStation],
            _ => throw new InvalidDataException($"Station frame {draw} is of no station."),
        });
    }

    /// <summary>Whether a coloured door cap's frame <paramref name="draw"/> is written in a room state: where a door of its colour is placed.</summary>
    /// <exception cref="InvalidDataException">The draw is not a coloured door cap's frame.</exception>
    private static Func<InventoriedRoomState, bool> ColoredDoorFrameWrittenIn(RoomObjectDraw draw)
    {
        if (!ColoredDoorCaps.TryDescribe(draw, out ColoredDoorCaps.Draw cap))
            throw new InvalidDataException($"Coloured door frame {draw} is not a coloured door cap's frame.");
        return Placing(DoorObjectKindDefinitions.ColoredDoors.Where(door => door.Value.Color == cap.Color).Select(door => door.Key));
    }

    /// <summary>
    /// Whether a collectible's frame <paramref name="draw"/> is written in a room state: where a collectible is placed that
    /// draws it, by its presentation, or for a tank's frames by its kind.
    /// </summary>
    /// <exception cref="InvalidDataException">The draw is not a collectible's frame.</exception>
    private static Func<InventoriedRoomState, bool> CollectibleFrameWrittenIn(RoomObjectDraw draw)
    {
        if (draw is not RoomObjectFrameDraw<Draw> { Frame: var frame })
            throw new InvalidDataException($"Collectible frame {draw} is not a collectible's frame.");
        switch (frame.Group)
        {
            case DrawGroup.Empty:
                return static state => state.PlacesCollectible(static (_, _) => true);
            case DrawGroup.Orb:
            case DrawGroup.OrbBurst:
                return static state => state.PlacesCollectible(static (_, presentation) => presentation == CollectiblePresentation.ChozoOrb);
            case DrawGroup.ShotReveal:
                return static state => state.PlacesCollectible(static (_, presentation) => presentation == CollectiblePresentation.ShotBlock);
            case DrawGroup.Tank:
                int framesPerTank = CollectibleObjectDraws.CountOf(DrawGroup.Tank) / TankKinds.Length;
                InWorldCollectibleKind tank = TankKinds[frame.Index / framesPerTank];
                return state => state.PlacesCollectible((kind, _) => kind == tank);
            case DrawGroup.OwnPicture:
                return static state => state.PlacesCollectible(static (kind, _) => CollectibleObjects.ShowsOwnPicture(kind));
            default:
                throw new InvalidDataException($"Collectible frame {draw} is of no collectible draw group.");
        }
    }

    /// <summary>
    /// Whether a reveal or wall's frame <paramref name="draw"/> is written in a room state: a bombed reveal where the block
    /// a bomb reveals lies, the escape passage and the escape shaft's wall where the state's setup spawns them, and the
    /// rescue wall where the state builds it.
    /// </summary>
    /// <exception cref="InvalidDataException">The draw is not a reveal or wall's frame.</exception>
    private static Func<InventoriedRoomState, bool> RevealOrWallWrittenIn(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.CrumbleSingle } => static state => state.HoldsCrumbleBlocks,
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.PowerBomb } => static state => state.HoldsPowerBombBlocks,
        RoomObjectFrameDraw<RoomObjectBombedRevealDraw> { Frame: RoomObjectBombedRevealDraw.SuperMissile } => static state => state.HoldsSuperMissileBlocks,
        RoomObjectFrameDraw<CrateriaMainstreetEscapePassageObjectDefinitions.Draw> =>
            static state => state.SetupCallback == RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen,
        RoomObjectFrameDraw<OldTourianEscapeShaftWallObjectDefinitions.Draw> =>
            static state => state.SetupCallback == RoomSetupCallback.AutoDestroyWallDuringEscape,
        RoomObjectFrameDraw<EscapeAnimalDraw> => BuildsEscapeAnimalWall,
        _ => throw new InvalidDataException($"Reveal and wall frame {draw} is no reveal's or wall's."),
    };

    /// <summary>Whether a room state builds the escape animals' rescue wall: its setup turns the wall into shot blocks, or its terrain holds it.</summary>
    private static bool BuildsEscapeAnimalWall(InventoriedRoomState state) =>
        state.SetupCallback == RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape || state.HoldsEscapeAnimalWall;

    /// <summary>
    /// One writer for each X-ray reveal: the blocks it copies over the room's, as many as its shape copies, written where the
    /// state's terrain holds a block of its collision kind and reaction; Brinstar's own reveals only in Brinstar.
    /// </summary>
    /// <exception cref="InvalidDataException">An installed reveal has no rule that draws it.</exception>
    private static IEnumerable<RoomBlockWriter> XrayRevealWriters(InstalledXrayReveals xray)
    {
        foreach ((XrayRevealVisualKey key, XrayRevealVisualBlocks visual) in xray.blocks)
        {
            XrayRevealCommand command = XrayRevealTable.Find(key.Type, RoomBlockBehavior.Reacting(key.Reaction))
                ?? throw new InvalidDataException($"X-ray visual {key.Type}/reaction {key.Reaction} has no reveal rule.");
            bool brinstar = command == XrayRevealCommand.CopyBrinstar;
            var blocks = new HashSet<int> { visual.TopLeft };
            if (command is XrayRevealCommand.CopyWide or XrayRevealCommand.CopySquare)
                blocks.Add(visual.TopRight);
            if (command is XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare)
                blocks.Add(visual.BottomLeft);
            if (command == XrayRevealCommand.CopySquare)
                blocks.Add(visual.BottomRight);
            yield return new(blocks,
                state => state.Holds(key.Type, key.Reaction) && (!brinstar || state.Area == AreaId.Brinstar));
        }
    }

    /// <summary>One writer for each of a room object's installed frames, written where <paramref name="writtenIn"/> says of the frame.</summary>
    private static IEnumerable<RoomBlockWriter> PerFrame<TFrame>(RoomObjectFrameImages<TFrame> images,
        Func<TFrame, Func<InventoriedRoomState, bool>> writtenIn) where TFrame : notnull =>
        images.All.Select(frame => new RoomBlockWriter(BlocksOf(frame.Value), writtenIn(frame.Key)));

    /// <summary>
    /// The blocks a collectible that shows its own picture writes under it, a block pair's frames in turn, which a room shows
    /// where the picture does not cover them (#1538).
    /// </summary>
    private static HashSet<int> BlockPairBlocks() =>
        [.. Enumerable.Range(OwnPictureFirstBlock, CollectibleObjectDraws.CountOf(DrawGroup.OwnPicture))];

    /// <summary>The blocks <paramref name="shown"/> show, each once.</summary>
    private static HashSet<int> BlocksOf(IEnumerable<RoomBlockAppearance> shown) => [.. shown.Select(block => block.Block)];

    /// <summary>Every block one installed frame shows.</summary>
    private static HashSet<int> BlocksOf(RoomObjectFrameImage image) => [.. image.Cells.Values.Select(shown => shown.Block)];

    /// <summary>Written in a room state whose population places one of <paramref name="headers"/>.</summary>
    private static Func<InventoriedRoomState, bool> Placing(IEnumerable<RoomObjectKind> headers)
    {
        HashSet<RoomObjectKind> placed = [.. headers];
        return state => state.Headers.Overlaps(placed);
    }

    /// <summary>Written in a room state whose population holds one of <paramref name="enemies"/>.</summary>
    private static Func<InventoriedRoomState, bool> Holding(IEnumerable<EnemyDefinitionId> enemies)
    {
        HashSet<EnemyDefinitionId> held = [.. enemies];
        return state => state.Enemies.Overlaps(held);
    }

    /// <summary>Every block any of a room object's installed frames shows.</summary>
    private static HashSet<int> FrameBlocks<TFrame>(RoomObjectFrameImages<TFrame> images) where TFrame : notnull =>
        [.. images.All.Values.SelectMany(image => image.Cells.Values).Select(shown => shown.Block)];
}
