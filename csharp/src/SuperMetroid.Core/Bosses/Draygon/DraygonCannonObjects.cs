using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall-turret PLMs: header identification and the cannon state Draygon's grab fight drives.</summary>
[RoomLifetime]
internal sealed class DraygonCannonObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the DraygonCannon family, which has no step of its own: its slots take the shared path.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.DraygonCannon, static (_, _) => RoomObjectStepOutcome.Shared);
    }

    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;

    /// <summary>Connects cannon PLM instruction handling to the active room's enemy services.</summary>
    internal DraygonCannonObjects(IRoomObjectEnemyEffects? enemies)
    {
        _enemies = enemies;
    }

    /// <summary>Whether <paramref name="header"/> is one of the room's wall cannons: the live right, destroyed right or live left cannon.</summary>
    internal static bool IsDraygonCannonHeader(RoomObjectHeaderId header) => DraygonCannonDefinitions.Setups.ContainsKey(header);

    /// <summary>The cannon a room's cannon PLM sets up: which way it faces and whether it starts destroyed, or null when <paramref name="header"/> is no cannon.</summary>
    internal static DraygonCannonSetup? SetupOf(RoomObjectHeaderId header) =>
        DraygonCannonDefinitions.Setups.TryGetValue(header, out DraygonCannonSetup setup) ? setup : null;

    /// <summary>Captures cannon orientation and the wall cannon it controls, installing shootable terrain unless the cannon starts destroyed.</summary>
    internal static void SetupDraygonCannonSlot(RoomLevelData level, RoomObjectSlot slot, DraygonCannonSetup setup, DraygonWallCannon cannon)
    {
        DraygonCannonOrientation orientation = setup.Orientation;
        slot.Family = new DraygonCannonObjectState(cannon);
        if (setup.Start is DraygonCannonStart.Destroyed)
        {
            // A destroyed cannon's setup only seeds its hit counter; its program begins at the destruction step,
            // which writes the destroyed terrain.
            slot.HitCounter = DraygonCannonDefinitions.HitsToDestroy;
            slot.StartAnimation(new DraygonCannonObjectAnimation(DraygonCannonPrograms.DestroyProgram(orientation)));
            return;
        }

        slot.HitCounter = 0;
        slot.StartAnimation(new DraygonCannonObjectAnimation(DraygonCannonPrograms.StartProgram(orientation)));
        WriteDraygonCannonTerrain(
            level,
            slot.BlockIndex,
            DraygonCannonDefinitions.LiveCannon);
        WriteDraygonCannonTerrain(
            level,
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonDefinitions.LiveCannonExtension);
    }

    /// <summary>Accepts pending missile hits, treating a Super Missile as enough damage to reach the destruction threshold.</summary>
    internal static void RunDraygonCannonFrameBehavior(RoomObjectSlot slot)
    {
        DraygonCannonObjectState state = slot.Family as DraygonCannonObjectState
            ?? throw new InvalidOperationException("A Draygon cannon's hit ran without its cannon state.");
        if (!state.HasPendingHit)
            return;

        SamusProjectileFamily? family = slot.ProjectileHit?.Family;
        if (family == SamusProjectileFamily.SuperMissile)
            slot.HitCounter = DraygonCannonDefinitions.SuperMissileHitCounterSeed;
        else if (family != SamusProjectileFamily.Missile)
            return;

        slot.ProjectileHit = null;
        state.HasPendingHit = false;
        slot.WakeAtLink();
    }

    /// <summary>
    /// The hit-count step on the cannon's hit counter: counts a hit and reports whether it destroys the cannon. The
    /// destroying hit marks the counter spent and removes the missile wake.
    /// </summary>
    internal static bool CountHit(RoomObjectSlot slot)
    {
        // Setup clears the counter and a Super Missile seeds it at 119; the destroying hit stops the counting, so the
        // count never reaches the byte's wrap.
        int next = slot.HitCounter + 1;
        if (next < DraygonCannonDefinitions.HitsToDestroy)
        {
            slot.HitCounter = next;
            return false;
        }
        slot.HitCounter = ushort.MaxValue;
        slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
        return true;
    }

    /// <summary>Disables the matching cannon firing word and changes both room blocks to destroyed terrain.</summary>
    /// <summary>Destroys the cannon: disables its firing word and writes its destroyed terrain.</summary>
    internal void DamageDraygonCannon(
        RoomLevelData level,
        RoomObjectSlot slot)
    {
        DraygonCannonObjectState state = (slot.Family as DraygonCannonObjectState)
            ?? throw new InvalidOperationException("Draygon cannon damage reached a slot without cannon state.");
        (_enemies ?? throw new InvalidOperationException(
            "Draygon cannon bytecode has no wall-cannon disabler.")).DisableDraygonCannon(
                state.Cannon);
        WriteDraygonCannonTerrain(
            level,
            slot.BlockIndex,
            DraygonCannonDefinitions.DestroyedCannon);
        WriteDraygonCannonTerrain(
            level,
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonDefinitions.DestroyedCannon);
        state.Destroyed = true;
    }

    /// <summary>Changes a cannon block's collision kind and behavior while preserving its appearance.</summary>
    private static void WriteDraygonCannonTerrain(
        RoomLevelData level,
        int blockIndex,
        DraygonCannonTerrain terrain)
    {
        RoomTerrainBlock original = level.GetObjectCollisionBlockByIndex(blockIndex).Terrain;
        level.SetObjectTerrain(
            blockIndex,
            original.WithCollision(terrain.Type));
        level.SetObjectBehavior(blockIndex, terrain.Behavior);
    }

    /// <summary>Per-slot wall cannon identity, destruction state, and queued hit flag.</summary>
    internal sealed class DraygonCannonObjectState(
        DraygonWallCannon cannon) : RoomObjectFamilyState, IShotTriggeredObject
    {
        /// <inheritdoc/>
        internal override RoomObjectFamily Family => RoomObjectFamily.DraygonCannon;

        /// <summary>The cannon whose firing Draygon stops when this physical cannon is destroyed, decoded from the room's PLM argument at setup.</summary>
        public DraygonWallCannon Cannon { get; } = cannon;
        /// <summary>Marks that this cannon's terrain and firing control have been disabled.</summary>
        public bool Destroyed { get; set; }
        /// <summary>Indicates a projectile collision awaiting missile-family filtering.</summary>
        public bool HasPendingHit { get; set; }

        /// <inheritdoc/>
        public void RecordShot() => HasPendingHit = true;
    }
}
