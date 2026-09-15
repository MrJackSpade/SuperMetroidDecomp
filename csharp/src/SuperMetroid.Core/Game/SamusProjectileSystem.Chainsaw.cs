using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Invalid-beam callbacks reached by cartridge table overreads.</summary>
public sealed partial class SamusProjectileSystem
{
    [NonSerialized]
    private GameplayWindowRegisterCache? chainsawWindowRegisters;

    /// <summary>
    /// Literal cached PPU-register owner corrupted by the misaligned Chainsaw callback.
    /// The cache is presentation hardware, not durable gameplay state; a restored debugger
    /// state begins from the ordinary layer-blending defaults until producers rewrite it.
    /// </summary>
    public GameplayWindowRegisterCache ChainsawWindowRegisters
    {
        get
        {
            if (chainsawWindowRegisters is not null)
                return chainsawWindowRegisters;

            chainsawWindowRegisters = new GameplayWindowRegisterCache();
            chainsawWindowRegisters.InitializeWindowAndScreenSelection();
            return chainsawWindowRegisters;
        }
    }

    /// <summary>
    /// Executes the `$90:B0AC` entry reached by uncharged beam combination thirteen.
    /// The first callback inherits Y=$0034 from SetInitialProjectileSpeed. Every later
    /// callback inherits the prior one-frame animation record address. The Chainsaw list
    /// uses only one-frame records, so the current next-record pointer identifies that
    /// previous consumed record without inventing a persistent CPU register owner.
    /// </summary>
    private void RunChainsawWindowStoreThenPowerBombPreInstruction(
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles)
    {
        ushort inheritedY = slot.SpritemapPointer == 0
            ? unchecked((ushort)(slot.PackedType.BeamCombinationIndex *
                SamusProjectileRomData.Beams.InitialSpeedRowBytes))
            : unchecked((ushort)(slot.InstructionPointer -
                SamusProjectileRomData.Instructions.AnimationRecordByteCount));
        ChainsawWindowRegisters.WriteWord(
            unchecked((ushort)(GameplayWindowRegisterAddresses.Window12Selection +
                slot.NativeByteIndex)),
            inheritedY);

        PowerBombFuseStep fuse = SamusPowerBombFuse.Step(
            slot.Variable,
            slot.InstructionPointer,
            sharedProjectiles.PowerBombExplosion.Flag);
        slot.Variable = fuse.Timer;
        slot.InstructionPointer = fuse.InstructionPointer;

        if (fuse.SpawnExplosion)
            sharedProjectiles.PowerBombExplosion.Spawn(slot.XPosition, slot.YPosition);
        if (fuse.DeleteProjectile)
            ClearProjectile(slot);
    }
}
