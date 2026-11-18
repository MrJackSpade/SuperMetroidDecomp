using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What the room's eye doors (Tourian and Kraid's lair) consult (#1554): their setup, whether Samus is near, the shots,
/// sweat and smoke the eye spawns, its sounds, and the move of the destroyed eye onto its blue cap. The eye
/// (<see cref="EyeDoorEyeObject"/>) and its body and bottom (<see cref="EyeDoorPartObject"/>) are typed room objects.
/// </summary>
[RoomLifetime]
internal sealed class EyeDoorObjects
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects _enemies;
    /// <summary>Queues rejected-shot and launched-projectile sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the eye doors to room services and sound requests.</summary>
    internal EyeDoorObjects(SharedGameState system, IRoomSamusAccess samus, IRoomObjectEnemyEffects enemies, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _enemies = enemies;
        _sounds = sounds;
    }

    /// <summary>
    /// Places the eye of the eye door facing <paramref name="orientation"/> that locks <paramref name="door"/>, which takes
    /// its first look on its first update, and installs its closed collision blocks unless its door has been opened for
    /// good; the eye's first look then turns it into the ordinary blue cap.
    /// </summary>
    internal void SetupEyeDoorEye(RoomLevelData level, RoomObjectPlace place, EyeDoorOrientation orientation, LockedDoorArgument door)
    {
        Ensure.NotNull(place);
        var placedDoor = new PlacedDoor(Ensure.NotNull(door));
        place.Become(new EyeDoorEyeObject(orientation, placedDoor));
        if (placedDoor.IsOpenedForGood(_system.HasOpenedDoor))
            return;
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, place.Block, EyeDoorDefinitions.EyeCollision,
            EyeDoorDefinitions.EyeBehavior);
        WriteExtensionRows(level, place.Block, EyeDoorDefinitions.EyeExtensionRows);
    }

    /// <summary>
    /// Places the eye door's body or bottom, <paramref name="part"/>, in <paramref name="door"/>, which takes its first look
    /// on its first update, and installs its closed collision block unless its door has been opened for good; the part's
    /// first look then removes it.
    /// </summary>
    internal void SetupEyeDoorPart(RoomLevelData level, RoomObjectPlace place, EyeDoorPart part, LockedDoorArgument? door)
    {
        Ensure.NotNull(place);
        PlacedDoor? placedDoor = PlacedDoor.For(door);
        place.Become(new EyeDoorPartObject(part, placedDoor));
        if (placedDoor?.IsOpenedForGood(_system.HasOpenedDoor) == true)
            return;
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, place.Block, EyeDoorDefinitions.ClosedComponentCollision,
            EyeDoorDefinitions.ClosedComponentBehavior);
    }

    /// <summary>Queues the dud sound of a shot the eye shrugs off.</summary>
    internal void QueueRejectedShotSound() =>
        _sounds.Queue(EyeDoorDefinitions.RejectedShotSound, Priority: SoundRequestPriority.Normal);

    /// <summary>Whether Samus stands within <paramref name="reach"/> blocks across and up or down of the part.</summary>
    internal bool IsSamusNear(RoomObjectPlace place, Extent16<RoomBlocks> reach)
    {
        IRoomSamus samus = _samus.Current;
        int roomObjectX = place.Block.X;
        int roomObjectY = place.Block.Y;
        int deltaX = Math.Abs(RoomGridConversions.PixelToBlock(samus.Position.X.Whole) - roomObjectX);
        int deltaY = Math.Abs(RoomGridConversions.PixelToBlock(samus.Position.Y.Whole) - roomObjectY);
        return deltaX <= reach.X && deltaY <= reach.Y;
    }

    /// <summary>
    /// The eye of <paramref name="door"/> facing <paramref name="facing"/> fires its attack, which bursts once the door
    /// opens, and queues its sound.
    /// </summary>
    internal void Shoot(RoomObjectPlace place, EyeDoorOrientation facing, LockedDoor door)
    {
        _enemies.SpawnEyeDoorAttack(new EyeDoorAttackRequest(facing, Ensure.NotNull(place).Block, door));
        _sounds.Queue(
            EyeDoorDefinitions.ProjectileSound,
            Priority: SoundRequestPriority.Normal);
    }

    /// <summary>The eye facing <paramref name="facing"/> sweats.</summary>
    internal void SpawnSweat(RoomObjectPlace place, EyeDoorOrientation facing) =>
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.Sweat, facing);

    /// <summary>Two randomly placed smoke puffs.</summary>
    internal void SpawnTwoSmoke(RoomObjectPlace place, EyeDoorOrientation facing)
    {
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.ScatteredSmoke, facing);
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.ScatteredSmoke, facing);
    }

    /// <summary>One centred smoke puff.</summary>
    internal void SpawnSmoke(RoomObjectPlace place, EyeDoorOrientation facing) =>
        SpawnEyeDoorProjectile(place, EyeDoorProjectileDefinition.CenteredSmoke, facing);

    /// <summary>
    /// Moves the eye up one block and writes the blue cap facing
    /// <paramref name="orientation"/> with its extension rows.
    /// </summary>
    internal static void ConvertEyeToBlueDoor(RoomLevelData level, RoomObjectPlace place, EyeDoorOrientation orientation)
    {
        RoomBlockBehavior facing = RoomBlockBehavior.Reacting(
            BlockReactionDefinitions.BlueDoorCaps.ReactionFor(EyeDoorDraws.BlueCapFacing(orientation)));
        place.Block += new Offset16<RoomBlocks>(0, -1);
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
            level,
            place.Block,
            RoomCollisionType.ShootableBlock,
            facing);
        WriteExtensionRows(level, place.Block, EyeDoorDefinitions.BlueDoorExtensionCount);
    }

    /// <summary>
    /// Writes <paramref name="rows"/> vertical extension blocks below <paramref name="origin"/>, each pointing back
    /// up to the origin block.
    /// </summary>
    private static void WriteExtensionRows(RoomLevelData level, Position16<RoomBlocks> origin, int rows)
    {
        for (int row = 1; row <= rows; row++)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
                level,
                origin + new Offset16<RoomBlocks>(0, row),
                RoomCollisionType.VerticalExtension,
                RoomBlockBehavior.Extending(-row));
        }
    }

    /// <summary>Sends an eye-door sweat or smoke request with the eye's facing and source block to enemy services.</summary>
    private void SpawnEyeDoorProjectile(
        RoomObjectPlace place,
        EyeDoorProjectileDefinition definition,
        EyeDoorOrientation facing) =>
        _enemies.SpawnEyeDoorProjectile(new EyeDoorProjectileRequest(definition, facing, place.Block));
}
