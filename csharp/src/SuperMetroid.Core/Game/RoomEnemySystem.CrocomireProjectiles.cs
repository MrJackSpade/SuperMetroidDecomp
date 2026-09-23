using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile $8F8F fired by Crocomire's open-mouth volley.</summary>
public sealed partial class RoomEnemySystem
{
    // These three tables are indexed by the physical bank-$86 projectile slot. Crocomire
    // deliberately clears the pool and then allocates eight actors from slot 17 downward,
    // so entries 10..17 produce the wall's authored fan rather than eight identical shards.
    /// <summary>
    /// $86:91C3-$91E6, eighteen unsigned X-acceleration increments indexed
    /// by physical projectile slot 0..17. All words match the pinned NTSC
    /// J/U v1.0 ROM: slots 0..1 are zero, slots 2..9 contain
    /// $0FF0,$0EE0,$0CC0,$0AA0,$0880,$0660,$0440,$0220, and slots 10..17
    /// repeat that profile. The death transition clears the pool and spawns
    /// spike pieces in slots 17..10, so these last eight values are reached.
    /// Retain the tuned profile: its first step needs an exception in an
    /// arithmetic generator, and such a generator obscures slot indexing.
    /// $91E7 starts the distinct acceleration-cap table.
    /// </summary>
    private static readonly ushort[] CrocomireSpikeAccelerationDelta =
    [
        0x0000, 0x0000, 0x0ff0, 0x0ee0, 0x0cc0, 0x0aa0,
        0x0880, 0x0660, 0x0440, 0x0220, 0x0ff0, 0x0ee0,
        0x0cc0, 0x0aa0, 0x0880, 0x0660, 0x0440, 0x0220,
    ];

    /// <summary>
    /// $86:91E7-$920A, eighteen unsigned X-acceleration caps indexed by
    /// physical projectile slot 0..17. Every pinned NTSC J/U v1.0 ROM word
    /// exactly equals the corresponding $86:91C3 acceleration increment
    /// shifted left four bits; even the largest $0FF0 becomes only $FF00.
    /// This proves the bounded cap algorithm for all 18 slots, including
    /// the ordinary spike-wall spawns in slots 17..10. The live path still
    /// applies an unsigned cap after adding the increment. $920B begins
    /// the separate maximum-velocity table.
    /// </summary>
    private static readonly ushort[] CrocomireSpikeMaximumAcceleration =
    [
        0x0000, 0x0000, 0xff00, 0xee00, 0xcc00, 0xaa00,
        0x8800, 0x6600, 0x4400, 0x2200, 0xff00, 0xee00,
        0xcc00, 0xaa00, 0x8800, 0x6600, 0x4400, 0x2200,
    ];

    /// <summary>
    /// $86:920B-$922E stores eighteen little-endian words indexed by physical
    /// projectile slot 0..17, but native $86:913F/$9144 reads only each low
    /// byte in 8-bit A mode; all stock high bytes are zero. The compiled
    /// bytes match the pinned NTSC J/U v1.0 ROM. Slots 0..1 are zero,
    /// 2..9 are 4,4,3,3,2,2,1,1, and the reached spike-wall slots 10..17
    /// are 6,5,4,3,2,2,1,1. Retain the nonuniform active velocity caps as
    /// authored tuning; a rule for the unused earlier profile does not
    /// explain the active one. $922F begins a different routine.
    /// </summary>
    private static readonly byte[] CrocomireSpikeMaximumVelocity =
    [
        0, 0, 4, 4, 3, 3, 2, 2, 1, 1, 6, 5, 4, 3, 2, 2, 1, 1,
    ];

    /// <summary>Ports <c>EprojInit_CrocomireProjectile</c> at $86:9023.</summary>
    private void SpawnCrocomireProjectile(RoomEnemySlot body, ushort spawnParameter)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireProjectile,
            graphicsIndex: EnemyPaletteBits.Palette5);
        projectile.XVelocity = 0xfe00;
        projectile.YVelocity = 1;
        projectile.XPosition = unchecked((ushort)(body.XPosition - 32));
        projectile.YPosition = unchecked((ushort)(body.YPosition - 16));
        projectile.GeneralTimer = 0;
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.DirectionParameter = spawnParameter;
    }

    /// <summary>Ports the one-frame vector setup at $86:906B.</summary>
    private void StartCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        // The native setup performs one horizontal collision move and deliberately ignores
        // carry. It also writes the graphics word of physical projectile slot zero rather
        // than the current slot; retain that shipped indexing bug for debugger parity.
        MoveProjectileAxis(projectile, level, horizontal: true);
        _enemyProjectiles[0].GraphicsIndex = EnemyPaletteBits.Palette5;
        projectile.GeneralTimer = unchecked((ushort)(
            projectile.GeneralTimer + projectile.XVelocity));

        // Spawn parameters are 2,4,...,18. The last index reads one word beyond the declared
        // nine-word gradient table. The compiled definition includes those exact setup
        // instruction bytes; do not clamp the final shot to the last authored gradient.
        short gradient = CrocomireProjectileRomData.GradientForSpawnParameter(projectile.DirectionParameter);
        byte angle = CalculateCartridgeAngle(-64, gradient);
        // The native angle starts at up, not right: Y uses negative cosine.
        (projectile.XVelocity, projectile.YVelocity) =
            CalculateCrocomireProjectileVelocity(angle);
        projectile.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CrocomiresProjectile_Fired;
    }

    /// <summary>Ports <c>sub_8690B3</c>: delete on the first horizontal/vertical wall hit.</summary>
    private void RunCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true) ||
            MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
        }
    }

    private static (ushort X, ushort Y) CalculateCrocomireProjectileVelocity(byte angle)
    {
        // Preserve the two native word shifts, including negative two's-complement bits.
        return (
            unchecked((ushort)(EnemyTrigonometryTables.SignedSine(angle) *
                CrocomireProjectileRomData.VelocityMultiplier)),
            unchecked((ushort)(EnemyTrigonometryTables.SignedNegativeCosineWord(angle) *
                CrocomireProjectileRomData.VelocityMultiplier)));
    }

    /// <summary>Ports <c>Crocomire_Func_52</c> and initializer $86:9286.</summary>
    private void SpawnNextCrocomireBridgeFragment(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        ushort cursor = death.BridgeFragmentCursor;
        if (unchecked((short)(cursor - 22)) >= 0)
            return;

        // The second SpawnEprojWithGfx argument is the same unaligned 0,2,...,20 cursor.
        // Its copied graphics word is immaterial: the definition initializer immediately
        // replaces it with $0400. Preserve the observable X table and cursor progression.
        death.BridgeFragmentCursor = unchecked((ushort)(cursor + 2));
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
            graphicsIndex: 0);
        projectile.XPosition = CrocomireBridgeFragmentDefinitions.XPosition(cursor);
        projectile.YPosition = 187;
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.XVelocity = 0;
        projectile.YVelocity = unchecked((ushort)((_nextRandom!() & 0x003f) + 64));
        projectile.GraphicsIndex = 0x0400;
    }

    /// <summary>
    /// Ports the eight descending allocations at $A4:99B6 and initializer
    /// $86:90CF. Its Y-position table at $86:9105-$9114 has eight unsigned
    /// words, all exactly $0038 + $0010*i for i=0..7 in the pinned NTSC
    /// J/U v1.0 ROM. Native even byte offsets 0..14 correspond to physical
    /// projectile slots 10..17; the checked managed index is slot minus 10.
    /// Clearing the pool before this call makes the spawn order Y=$A8 down
    /// to $38. $86:9115 starts the next routine, not a ninth table word.
    /// </summary>
    private void SpawnCrocomireSpikeWallPieces()
    {
        CrocomireEnemyState state = _crocomire ??
            throw new InvalidOperationException("Crocomire spike wall spawned without an owner.");
        ushort graphicsIndex = unchecked((ushort)(
            state.Body.VramTilesIndex | state.Body.PaletteIndex));

        // The initializer indexes Y by native projectile index minus $14. Clearing the pool
        // immediately before this call guarantees slots 17..10 and Y=$38,$48,...,$A8.
        for (int count = 0; count < 8; count++)
        {
            RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
            if (projectile is null)
                return;

            InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.CrocomireSpikeWallPieces,
                graphicsIndex);
            int yTableIndex = projectile.SlotIndex - 10;
            if ((uint)yTableIndex >= 8)
            {
                throw new InvalidDataException(
                    $"Crocomire spike wall occupied unexpected projectile slot {projectile.SlotIndex}.");
            }

            projectile.XPosition = 528;
            projectile.YPosition = unchecked((ushort)(0x0038 + yTableIndex * 0x0010));
            projectile.XVelocity = 0;
            projectile.YVelocity = 0xfffb;
            projectile.Variable0 = 0;
            projectile.Variable1 = 0x8800;
            projectile.XSubposition = 0;
            projectile.YSubposition = 0;
        }
    }

    /// <summary>Ports the byte-precise slot-dependent shard motion at $86:9115.</summary>
    private void RunCrocomireSpikeWallPiece(RoomEnemyProjectileSlot projectile)
    {
        int slot = projectile.SlotIndex;
        ushort maximumAcceleration = CrocomireSpikeMaximumAcceleration[slot];
        ushort acceleration = projectile.Variable0;
        if (acceleration != maximumAcceleration)
        {
            acceleration = unchecked((ushort)(
                acceleration + CrocomireSpikeAccelerationDelta[slot]));
            if (acceleration >= maximumAcceleration)
                acceleration = maximumAcceleration;
        }
        projectile.Variable0 = acceleration;

        // The 65816 switches to 8-bit A here. The high acceleration byte is added to the
        // fractional velocity byte; carry advances the signed integral velocity byte, which
        // is then capped by this physical slot's maximum.
        int velocityFractionSum = (projectile.XVelocity & 0x00ff) + (acceleration >> 8);
        byte velocityFraction = unchecked((byte)velocityFractionSum);
        byte velocityWhole = unchecked((byte)(
            (projectile.XVelocity >> 8) + (velocityFractionSum >> 8)));
        byte maximumVelocity = CrocomireSpikeMaximumVelocity[slot];
        if (unchecked((sbyte)(velocityWhole - maximumVelocity)) >= 0)
            velocityWhole = maximumVelocity;
        projectile.XVelocity = unchecked((ushort)((velocityWhole << 8) | velocityFraction));

        // Move X with the same split-byte math. Only the high subposition byte participates;
        // its low byte remains untouched, exactly like EnemyProjectile_XSubPositions+1,X.
        int subpositionSum = (projectile.XSubposition >> 8) + velocityFraction;
        projectile.XSubposition = unchecked((ushort)(
            (projectile.XSubposition & 0x00ff) | ((subpositionSum & 0xff) << 8)));
        int wholeDelta = unchecked((sbyte)velocityWhole) + (subpositionSum >> 8);
        projectile.XPosition = unchecked((ushort)(projectile.XPosition + wholeDelta));

        // Y velocity and position are ordinary signed 16.16 pairs, with $3000 acceleration.
        uint velocity = unchecked(((uint)projectile.YVelocity << 16) | projectile.Variable1);
        velocity = unchecked(velocity + 0x00003000u);
        projectile.YVelocity = unchecked((ushort)(velocity >> 16));
        projectile.Variable1 = unchecked((ushort)velocity);
        uint position = unchecked(((uint)projectile.YPosition << 16) | projectile.YSubposition);
        position = unchecked(position + velocity);
        projectile.YPosition = unchecked((ushort)(position >> 16));
        projectile.YSubposition = unchecked((ushort)position);

        if (projectile.YPosition < 0x00a8)
            return;

        ushort impactX = projectile.XPosition;
        ushort impactY = projectile.YPosition;
        int nativeIndex = projectile.SlotIndex * 2;
        projectile.Clear();
        if ((nativeIndex & 2) == 0)
            LastCrocomireSoundEffect = 0x0029;
        SpawnRoomGraphicsDustExplosion(impactX, impactY, animationIndex: 0x0015);
        LastCrocomireSoundEffect = 0x0025;
    }

    /// <summary>Ports bridge-fragment vertical collision and gravity at $86:92BA.</summary>
    private void RunCrocomireBridgeFragment(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
            return;
        }

        projectile.YVelocity = unchecked((ushort)(
            (projectile.YVelocity + 24) & 0x3fff));
    }
}
