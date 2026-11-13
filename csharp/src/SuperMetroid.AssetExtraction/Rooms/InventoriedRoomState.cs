using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// What of one room state decides the blocks it can show (#1636): the tileset whose block sheet pictures them, the blocks
/// its level data places where the room shows it, and what its installed data holds that starts a writer of blocks: the
/// reactions its terrain selects, including those a Wave shot's scan past the room's ceiling sets off where it reads
/// halfway through a block (#1650), the room objects and enemies its populations place, its setup and main routines, the
/// doors that lead into it and its X-ray overlay. A writer counts for the state only when one of these holds what starts
/// it; nothing here follows what a writer goes on to trigger.
/// </summary>
/// <param name="Tileset">The tileset the state loads, whose block sheet pictures the blocks it shows.</param>
/// <param name="Area">The area the state's room lies in.</param>
/// <param name="Placed">
/// Every block the state's level data places where the room shows it: in its foreground, and in its background where the
/// state's background shows its level data.
/// </param>
/// <param name="Terrain">How each block of the state's installed terrain collides and the reaction it selects, each once.</param>
/// <param name="StraddledShots">
/// The shootable collision kinds and reactions a Wave shot's scan past the room's ceiling sets off in the state's installed
/// terrain where it reads halfway through a block (<see cref="StraddledShotReactions"/>), each once. The shot is a beam, so
/// a block among them that rejects a beam starts nothing.
/// </param>
/// <param name="Headers">The room objects the state's population places.</param>
/// <param name="Enemies">The enemies the state's population places.</param>
/// <param name="SetupBehavior">The behaviour the state runs when its room loads.</param>
/// <param name="MainBehavior">The room behaviour the state runs each frame.</param>
/// <param name="Entrances">The doors that lead into the state's room.</param>
/// <param name="XrayOverlay">The X-ray reveal's overlay of the state, or null where it has none.</param>
internal sealed record InventoriedRoomState(TilesetDefinition Tileset, AreaId Area, IReadOnlySet<int> Placed,
    IReadOnlySet<RoomTerrainReaction> Terrain, IReadOnlySet<RoomTerrainReaction> StraddledShots, IReadOnlySet<RoomObjectKind> Headers,
    IReadOnlySet<EnemyDefinitionId> Enemies, RoomSetupBehavior SetupBehavior, RoomMainBehavior MainBehavior,
    IReadOnlySet<DoorDefinition> Entrances, XrayRoomOverlayDefinition? XrayOverlay)
{
    /// <summary>The shot-block reactions whose blocks any weapon breaks and that grow back.</summary>
    private static readonly HashSet<ShotBlockReaction> RespawningShotBreaks = [ShotBlockReaction.RespawningBreak];

    /// <summary>The shot-block reactions whose blocks a Power Bomb breaks.</summary>
    private static readonly HashSet<ShotBlockReaction> PowerBombBreaks =
        [ShotBlockReaction.RespawningPowerBombBreak, ShotBlockReaction.PermanentPowerBombBreak];

    /// <summary>The shot-block reactions whose blocks a Super Missile breaks.</summary>
    private static readonly HashSet<ShotBlockReaction> SuperMissileBreaks =
        [ShotBlockReaction.RespawningSuperMissileBreak, ShotBlockReaction.PermanentSuperMissileBreak];

    /// <summary>The shot-block reactions whose blocks a weapon breaks: any break, a Power Bomb's or a Super Missile's.</summary>
    private static readonly HashSet<ShotBlockReaction> ShotBreaks =
        [ShotBlockReaction.RespawningBreak, ShotBlockReaction.PermanentBreak, .. PowerBombBreaks, .. SuperMissileBreaks];

    /// <summary>
    /// The shot-block reactions a beam breaks, the only ones a Wave shot's straddled read sets off: a Power Bomb's and a
    /// Super Missile's blocks reject a beam.
    /// </summary>
    private static readonly HashSet<ShotBlockReaction> BeamBreaks = [ShotBlockReaction.RespawningBreak, ShotBlockReaction.PermanentBreak];

    /// <summary>Whether the state's room lies in Ceres, whose rooms load no common room elements and place no room objects.</summary>
    internal bool InCeres => Area == AreaId.Ceres;

    /// <summary>Whether the state draws its level's blocks: every state but the Ceres elevator shaft's, whose screen is its Mode 7 floor (#1647).</summary>
    internal bool DrawsLevel => !RoomTurningFloors.ShowsShaftFloor(MainBehavior);

    /// <summary>
    /// Whether a block of the state's terrain collides as <paramref name="collision"/> and selects <paramref name="reaction"/>,
    /// the reaction that kind reads; null for a kind that reads none, which any block of the kind holds.
    /// </summary>
    internal bool Holds(RoomCollisionType collision, Enum? reaction) =>
        Terrain.Any(block => block.Collision == collision && Equals(block.Behavior.ReactionUnder(collision), reaction));

    /// <summary>Whether the state's terrain holds a shot block that any weapon breaks and that grows back, or a Wave shot's straddled read sets one off.</summary>
    internal bool HoldsRespawningShotBlocks => HoldsShotBlocks(RespawningShotBreaks);

    /// <summary>
    /// Whether the state's terrain holds a shot block a weapon breaks: any break, a Power Bomb's or a Super Missile's; or a
    /// Wave shot's straddled read sets off a break.
    /// </summary>
    internal bool HoldsBreakableShotBlocks => HoldsShotBlocks(ShotBreaks);

    /// <summary>Whether the state's terrain holds a block a Power Bomb breaks.</summary>
    internal bool HoldsPowerBombBlocks => HoldsShotBlocks(PowerBombBreaks);

    /// <summary>Whether the state's terrain holds a block a Super Missile breaks.</summary>
    internal bool HoldsSuperMissileBlocks => HoldsShotBlocks(SuperMissileBreaks);

    /// <summary>Whether the state's terrain holds a bomb block: a bombable block selecting one of the eight bomb-block reactions.</summary>
    internal bool HoldsBombBlocks => Terrain.Any(block =>
        block.Collision is RoomCollisionType.BombableBlock or RoomCollisionType.BombableAir && BombableReactions.IsBreak(block.Behavior.Bombable));

    /// <summary>Whether the state's terrain holds a crumble block: a special block whose reaction selects one of the crumble blocks.</summary>
    internal bool HoldsCrumbleBlocks => Terrain.Any(block =>
        block.Collision == RoomCollisionType.SpecialBlock && block.Behavior.TryGetContactCrumble(out _));

    /// <summary>
    /// Whether the state's terrain holds a Speed Booster block: a special block whose reaction selects one in the state's
    /// area, or that a bomb reveals as one, or a special-air block whose area reaction's collision reaction is one.
    /// </summary>
    internal bool HoldsSpeedBoosterBlocks => Terrain.Any(block =>
        (block.Collision == RoomCollisionType.SpecialBlock &&
            (SpeedBoosterBlocks.TryResolve(block.Behavior, Area, out _) ||
                (!BombSpecialBlockReactions.IsArea(block.Behavior.Special) && block.Behavior.Special != SpecialBlockReaction.PastTable &&
                    BombSpecialBlockReactions.ChangeFor(block.Behavior.Special) == SpeedBoosterBlockObjectDefinitions.BombReveal))) ||
        (IsAreaSpecialAir(block) &&
            SpecialAirReactions.CollisionOf(Area, block.Behavior.SpecialAir) is SpecialAirCollisionReaction.BrinstarSlowRespawningSpeedBlock
                or SpecialAirCollisionReaction.BrinstarSlowPermanentSpeedBlock or SpecialAirCollisionReaction.DachoraRespawningSpeedBlock
                or SpecialAirCollisionReaction.PermanentSpeedBlock));

    /// <summary>Whether the state's terrain holds a Grapple block that breaks under the beam.</summary>
    internal bool HoldsBreakableGrappleBlocks => Terrain.Any(block => block.Collision == RoomCollisionType.GrappleBlock &&
        block.Behavior.Grapple is GrappleReaction.RespawningBreakableAnchor or GrappleReaction.PermanentBreakableAnchor);

    /// <summary>
    /// Whether the state's terrain holds a shootable block whose reaction selects a blue-door cap facing
    /// <paramref name="facing"/>, or a Wave shot's straddled read sets one off, as a beam opens a blue cap.
    /// </summary>
    internal bool HoldsBlueDoorCap(ColoredDoorOrientation facing) => Terrain.Concat(StraddledShots).Any(block =>
        block.Collision is RoomCollisionType.ShootableBlock or RoomCollisionType.ShootableAir &&
        block.Behavior.TryGetBlueDoorOrientation(out ColoredDoorOrientation cap) && cap == facing);

    /// <summary>
    /// Whether the state's terrain holds a shootable block whose reaction selects a downward gate's trigger, or a Wave
    /// shot's straddled read sets one off, as a beam triggers a gate.
    /// </summary>
    internal bool HoldsDownwardGateTriggers => Terrain.Concat(StraddledShots).Any(block =>
        block.Collision is RoomCollisionType.ShootableBlock or RoomCollisionType.ShootableAir &&
        block.Behavior.TryGetDownwardGateTrigger(out _));

    /// <summary>Whether the state's terrain holds the escape animals' rescue wall.</summary>
    internal bool HoldsEscapeAnimalWall =>
        Terrain.Contains(new(EscapeAnimalWallDefinitions.OriginCollisionType, EscapeAnimalWallDefinitions.ReactionBehavior));

    /// <summary>Whether the state's terrain holds a special-air block whose area reaction's inside reaction is a Brinstar plant that bites Samus.</summary>
    internal bool HoldsSamusEaterPlants => Terrain.Any(block => IsAreaSpecialAir(block) &&
        SpecialAirReactions.InsideOf(Area, block.Behavior.SpecialAir) is SpecialAirInsideReaction.BrinstarFloorPlant
            or SpecialAirInsideReaction.BrinstarCeilingPlant);

    /// <summary>
    /// Whether <paramref name="block"/> is a special-air block selecting one of its area's special-air reactions. Only those
    /// reach the area's reactions: the game reads a special-air block with a common reaction through the reactions every
    /// area shares (the conveyors and the scroll trigger), never through its area's.
    /// </summary>
    private static bool IsAreaSpecialAir(RoomTerrainReaction block) =>
        block.Collision == RoomCollisionType.SpecialAir && SpecialAirReactions.IsAreaTableReaction(block.Behavior.SpecialAir);

    /// <summary>Whether the state's terrain holds a spike block an enemy breaks.</summary>
    internal bool HoldsEnemyBreakableBlocks => Terrain.Any(block => block.Collision == RoomCollisionType.SpikeBlock &&
        block.Behavior.Spike is not (SpikeReaction.PastEnemyTable or SpikeReaction.AreaPastEnemyTable) &&
        EnemyBreakableTerrain.IsEnemyBreakable(block.Behavior));

    /// <summary>Whether the state's terrain holds a block that collides as another along a row or a column.</summary>
    internal bool HoldsExtensions => Terrain.Any(block =>
        block.Collision is RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension);

    /// <summary>
    /// Whether the state's terrain holds a block whose reaction breaks it up, or a Wave shot's straddled read sets one off:
    /// a shot block a weapon breaks, a bomb block, a crumble block, a Speed Booster block or a block an enemy breaks.
    /// </summary>
    internal bool HoldsBreakingBlocks => HoldsBreakableShotBlocks || HoldsBombBlocks || HoldsCrumbleBlocks || HoldsSpeedBoosterBlocks ||
        HoldsEnemyBreakableBlocks;

    /// <summary>Whether the state's population places a collectible whose kind and presentation <paramref name="placed"/> accepts.</summary>
    internal bool PlacesCollectible(Func<InWorldCollectibleKind, CollectiblePresentation, bool> placed)
    {
        Ensure.NotNull(placed);
        foreach (RoomObjectKind header in Headers)
        {
            if (CollectibleObjects.TryIdentifyPermanentCollectible(header, out InWorldCollectibleKind kind, out CollectiblePresentation presentation) &&
                placed(kind, presentation))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Whether the state's terrain holds a shootable block whose reaction the shot-block dispatcher handles as one of
    /// <paramref name="kinds"/>, or a Wave shot's straddled read sets off one of them that a beam breaks.
    /// </summary>
    private bool HoldsShotBlocks(HashSet<ShotBlockReaction> kinds) =>
        Terrain.Any(block => SelectsShotBlock(block, kinds)) ||
        StraddledShots.Any(block => SelectsShotBlock(block, kinds) && SelectsShotBlock(block, BeamBreaks));

    /// <summary>Whether <paramref name="block"/> collides as shootable and its reaction is a shot block of one of <paramref name="kinds"/>.</summary>
    private static bool SelectsShotBlock(RoomTerrainReaction block, HashSet<ShotBlockReaction> kinds) =>
        block.Collision is RoomCollisionType.ShootableBlock or RoomCollisionType.ShootableAir &&
        ShotBlockReactions.Of(block.Behavior.Shootable) is { } reaction && kinds.Contains(reaction);

    /// <summary>
    /// What of <paramref name="state"/> of <paramref name="room"/> decides the blocks it can show: <paramref name="level"/>
    /// is what its level data shows, <paramref name="terrain"/> its installed terrain, <paramref name="straddledShots"/> the
    /// shootable reactions a Wave shot's straddled read sets off in that terrain (<see cref="StraddledShotReactions"/>) and
    /// <paramref name="entrances"/> the doors that lead into the room. Its background's blocks are shown only where its
    /// background scrolls over its level data, as the room streams it; otherwise the background shows a library background
    /// or nothing of the level.
    /// </summary>
    internal static InventoriedRoomState Of(RoomDefinition room, RoomStateSettings state, RoomLevelArtwork level,
        InstalledRoomLevel terrain, IReadOnlySet<RoomTerrainReaction> straddledShots, IEnumerable<DoorDefinition> entrances)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(state);
        Ensure.NotNull(level);
        Ensure.NotNull(terrain);
        Ensure.NotNull(straddledShots);
        Ensure.NotNull(entrances);
        var placed = new HashSet<int>();
        foreach (RoomBlockAppearance block in level.Foreground.Span)
            placed.Add(block.Block);
        if (state.BackgroundScrollModes.Source == RoomBackgroundSource.Level)
        {
            foreach (RoomBlockAppearance block in level.Background.Span)
                placed.Add(block.Block);
        }
        var reactions = new HashSet<RoomTerrainReaction>();
        for (int index = 0; index < terrain.Foreground.Length; index++)
            reactions.Add(new(terrain.Foreground[index].Collision, terrain.Behaviors[index]));
        var headers = new HashSet<RoomObjectKind>();
        foreach (RoomObjectPlacement placement in state.ObjectPopulation.Placements.Span)
            headers.Add(placement.Kind);
        var enemies = new HashSet<EnemyDefinitionId>();
        foreach (RoomEnemyPopulationRecord record in state.EnemyPopulation.Records.Span)
            enemies.Add(record.DefinitionId);
        return new InventoriedRoomState(RoomTilesets.Get(state.GraphicsSet), room.AreaIndex, placed, reactions, straddledShots, headers, enemies,
            state.SetupBehavior, state.MainBehavior, entrances.ToHashSet(), state.XrayOverlay);
    }
}
