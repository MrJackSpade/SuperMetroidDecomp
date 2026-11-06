using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall-turret PLMs: header identification and the cannon state Draygon's grab fight drives.</summary>
[RoomLifetime]
internal sealed class DraygonCannonPlms : IPlmFamilySource
{
    /// <summary>Registers the DraygonCannon family, which has no step of its own: its slots take the shared path.</summary>
    public void AddPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(PlmFamily.DraygonCannon, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomPlmEnemyEffects? _enemies;

    /// <summary>Connects cannon PLM instruction handling to the active room's enemy services.</summary>
    internal DraygonCannonPlms(IRoomPlmEnemyEffects? enemies)
    {
        _enemies = enemies;
    }

    /// <summary>Whether <paramref name="header"/> is one of the room's wall cannons: the live right, destroyed right or live left cannon.</summary>
    internal static bool IsDraygonCannonHeader(PlmHeaderId header) => DraygonCannonDefinitions.Setups.ContainsKey(header);

    /// <summary>The cannon a room's cannon PLM sets up: which way it faces and whether it starts destroyed, or null when <paramref name="header"/> is no cannon.</summary>
    internal static DraygonCannonSetup? SetupOf(PlmHeaderId header) =>
        DraygonCannonDefinitions.Setups.TryGetValue(header, out DraygonCannonSetup setup) ? setup : null;

    /// <summary>Captures cannon orientation and the wall cannon it controls, installing shootable terrain unless the cannon starts destroyed.</summary>
    internal static void SetupDraygonCannonSlot(RoomLevelData level, PlmSlot slot, DraygonCannonSetup setup, DraygonWallCannon cannon)
    {
        DraygonCannonOrientation orientation = setup.Orientation;
        slot.Family = new DraygonCannonPlmState(cannon);
        if (setup.Start is DraygonCannonStart.Destroyed)
        {
            // A destroyed cannon's setup only seeds its hit counter; its program begins at the destruction step,
            // which writes the destroyed terrain.
            slot.HitCounter = DraygonCannonDefinitions.HitsToDestroy;
            slot.StartAnimation(new DraygonCannonPlmAnimation(DraygonCannonPrograms.DestroyProgram(orientation)));
            return;
        }

        slot.HitCounter = 0;
        slot.StartAnimation(new DraygonCannonPlmAnimation(DraygonCannonPrograms.StartProgram(orientation)));
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
    internal static void RunDraygonCannonFrameBehavior(PlmSlot slot)
    {
        DraygonCannonPlmState state = slot.Family as DraygonCannonPlmState
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
    internal static bool CountHit(PlmSlot slot)
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
        slot.FrameBehavior = PlmFrameBehaviors.Inert;
        return true;
    }

    /// <summary>Disables the matching cannon firing word and changes both room blocks to destroyed terrain.</summary>
    /// <summary>Destroys the cannon: disables its firing word and writes its destroyed terrain.</summary>
    internal void DamageDraygonCannon(
        RoomLevelData level,
        PlmSlot slot)
    {
        DraygonCannonPlmState state = (slot.Family as DraygonCannonPlmState)
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
        RoomTerrainBlock original = level.GetPlmCollisionBlockByIndex(blockIndex).Terrain;
        level.SetPlmTerrain(
            blockIndex,
            original.WithCollision(terrain.Type));
        level.SetPlmBehavior(blockIndex, terrain.Behavior);
    }

    /// <summary>Per-slot wall cannon identity, destruction state, and queued hit flag.</summary>
    internal sealed class DraygonCannonPlmState(
        DraygonWallCannon cannon) : PlmFamilyState, IShotTriggeredPlm
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.DraygonCannon;

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
