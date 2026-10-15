using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The misaligned Chainsaw callback: it stores into the shared cached window registers ($60..$6D) before running the power-bomb pre-instruction.</summary>
internal sealed class ChainsawWindow
{
    /// <summary>Projectile animation owner used when the misaligned callback clears its slot.</summary>
    private readonly ProjectileAnimation _animation;

    /// <summary>Connects the shared cached-window side effect to projectile animation cleanup.</summary>
    internal ChainsawWindow(ProjectileAnimation animation)
    {
        _animation = animation;
    }

    /// <summary>
    /// Connects the split projectile translation to the runtime's single cached-register
    /// owner. The cartridge has one WRAM <c>$60..$6D</c> region shared by HDMA, projectiles,
    /// NMI, and the gameplay IRQ; retaining a projectile-private copy would make the
    /// Chainsaw store invisible to the actual displayed frame.
    /// </summary>
    internal void BindGameplayWindowRegisters(GameplayWindowRegisterCache registers) =>
        ChainsawWindowRegisters = Ensure.NotNull(registers);

    /// <summary>
    /// Literal cached PPU-register owner corrupted by the misaligned Chainsaw callback.
    /// The cache is presentation hardware, not durable gameplay state; a restored debugger
    /// state begins from the ordinary layer-blending defaults until producers rewrite it.
    /// </summary>
    [field: NonSerialized]
    public GameplayWindowRegisterCache ChainsawWindowRegisters
    {
        get
        {
            if (field is not null)
                return field;

            field = new GameplayWindowRegisterCache();
            field.InitializeWindowAndScreenSelection();
            return field;
        }

        private set;
    }

    /// <summary>
    /// Executes the `$90:B0AC` entry reached by uncharged beam combination thirteen.
    /// The first callback inherits Y=$0034 from SetInitialProjectileSpeed. Every later
    /// callback inherits the prior one-frame animation record address. The Chainsaw list
    /// uses only one-frame records, so the current next-record pointer identifies that
    /// previous consumed record without inventing a persistent CPU register owner.
    /// </summary>
    internal void RunChainsawWindowStoreThenPowerBombPreInstruction(
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        ushort inheritedY = slot.SpritemapPointer == 0
            ? unchecked((ushort)(slot.PackedType.BeamCombination.TableIndex *
                SamusProjectileRomData.Beams.InitialSpeedRowBytes))
            : unchecked((ushort)(slot.InstructionPointer -
                SamusProjectileRomData.Instructions.AnimationRecordByteCount));
        ChainsawWindowRegisters.StoreProjectileIndexedWord(slot.NativeByteIndex, inheritedY);

        PowerBombFuseStep fuse = SamusPowerBombFuse.Step(slot.Variable, sharedProjectiles.PowerBombExplosion.Flag);
        slot.Variable = fuse.Timer;
        if (fuse.FastAnimation)
            slot.Cursor = SamusPowerBombFuse.FastAnimation(slot.Cursor);

        if (fuse.SpawnExplosion)
            sharedProjectiles.PowerBombExplosion.Spawn(slot.Position.Whole);
        if (fuse.DeleteProjectile)
            _animation.ClearProjectile(slot);

        // `$90:B0AE` always falls through from PowerBomb_Func3 to `$94:9CAC`, even
        // when the zero-timer/no-flag branch just cleared the projectile. The latter
        // case consequently dispatches with the cleared type word, while both paths
        // use the global Power Bomb centre/radius rather than the Chainsaw's position.
        // Preserve the inclusive edge traversal and its synchronous PLM mutations.
        var reactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            level,
            sharedProjectiles.PowerBombExplosion.Position,
            sharedProjectiles.PowerBombExplosion.ExplosionRadius,
            reactions,
            roomPlms,
            samus.LiquidPhysics.AreaIndex,
            slot.Type);
    }
}
