using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Stoke;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Stoke's walking and attack animations and the cues they hand to main AI and the projectile system.</summary>
internal static class StokeBehaviorVerification
{
    /// <summary>
    /// Each walking animation sets main AI's direction and function and loops on time; each attack spits a fireball in
    /// its own direction and then walks again in that direction.
    /// </summary>
    internal static void VerifyStokeBehavior(SuperMetroidAddressSpace rom)
    {
        foreach ((StokeDirection direction, StokeProgram walk, StokeProgram loop, StokeProgram attack, StokeAiFunction function, ushort fireball) in new[]
                 {
                     (StokeDirection.Left, StokeProgram.MovingLeft, StokeProgram.MovingLeftLoop, StokeProgram.AttackingLeft, StokeAiFunction.MovingLeft, (ushort)0),
                     (StokeDirection.Right, StokeProgram.MovingRight, StokeProgram.MovingRightLoop, StokeProgram.AttackingRight, StokeAiFunction.MovingRight, (ushort)1),
                 })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, StokeEnemyState state) = Create(rom, direction == StokeDirection.Left ? StokeDirection.Right : StokeDirection.Left);
            state.Program = AnimationProgramCursor<StokeProgram>.Start(walk);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(StokeAnimations.Programs, loop, StokeFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(StokeAnimations.Programs, loop)), $"Stoke {walk} loop");
            AssertEqual(direction, state.Direction, $"Stoke {walk} cue sets the direction");
            AssertEqual(function, state.Function, $"Stoke {walk} cue sets the walking function");

            (enemies, slot, state) = Create(rom, direction);
            state.Function = StokeAiFunction.IdleDuringAttack;
            state.Program = AnimationProgramCursor<StokeProgram>.Start(attack);
            ushort[] attackFrames = StokeAnimationsConstants.Spritemaps(attack).ToArray();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(attackFrames, [16, 16]),
                EnemyInstructionRunner.ShownFrames(enemies, slot, 32), $"Stoke {attack} poses");
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(
                candidate => candidate.Kind == RoomEnemyProjectileKind.StokeProjectile);
            AssertEqual(fireball, projectile.DirectionParameter, $"Stoke {attack} fireball direction");
            EnemyInstructionRunner.ShownFrames(enemies, slot, 1);
            AssertEqual(function, state.Function, $"Stoke {attack} walks again");
        }
        Console.WriteLine("  Stoke behavior: both walks set direction and function and loop on time; both attacks spit their fireball and walk again.");
    }

    /// <summary>A Stoke initialized facing <paramref name="direction"/>.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, StokeEnemyState State) Create(SuperMetroidAddressSpace rom, StokeDirection direction)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Stoke;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = StokeFrameToolingDefinitions.Bank };
        slot.Parameter1 = (ushort)direction;
        slot.Position = slot.Position.WithWhole(new(128, 128));
        enemies.StokeBehavior.InitializeStoke(slot);
        return (enemies, slot, enemies.StokeStates[0] ?? throw new InvalidOperationException("Stoke has no state."));
    }

    /// <summary>Runs the Stoke behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--stoke-instruction-mechanics")]
    internal static int StokeBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStokeBehavior), () => VerifyStokeBehavior(rom));
        Suite(nameof(StokeAnimationsVerification.VerifyStokeAnimations), () => StokeAnimationsVerification.VerifyStokeAnimations(rom));
        return 0;
    }
}
