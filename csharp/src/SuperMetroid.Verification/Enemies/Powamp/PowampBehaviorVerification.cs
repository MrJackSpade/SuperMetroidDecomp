using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Powamp's initial animations and the balloon phase its AI derives from the typed cursor.</summary>
internal static class PowampBehaviorVerification
{
    /// <summary>
    /// The real initializers start the balloon deflated and the body on its slow pulse. At every step of the balloon's
    /// animations, the phase the AI reads matches the native pointer it stands for: that sequence's first list plus four
    /// bytes per shown pose.
    /// </summary>
    internal static void VerifyPowampBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot balloon = enemies.SlotTable.Slots[0];
        balloon.Bind(EnemyDefinitionId.Powamp, balloon.Definition);
        balloon.Bind(balloon.DefinitionId, default(RoomEnemyDefinition));
        balloon.SetSpawnParameter1(EnemyDefinitionId.Powamp, 1);
        enemies.PowampBehavior.InitializePowamp(balloon);
        RoomEnemySlot body = enemies.SlotTable.Slots[1];
        body.Bind(EnemyDefinitionId.Powamp, body.Definition);
        body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
        body.SetSpawnSettings(new PowampSpawnSettings(PowampPart.Body, GrappleTravelDistance: 0));
        enemies.PowampBehavior.InitializePowamp(body);
        AssertEqual(AnimationProgramCursor<PowampProgram>.Start(PowampProgram.BalloonDeflated), balloon.RequireSpeciesState<PowampEnemyState>().Program,
            "the balloon starts deflated");
        AssertEqual(AnimationProgramCursor<PowampProgram>.Start(PowampProgram.BodySlow), body.RequireSpeciesState<PowampEnemyState>().Program,
            "the body starts on its slow pulse");

        Dictionary<PowampProgram, ushort[]> steps = NativeEnemyProgramOracle.StepAddresses(CartridgeImportSource.Require(rom), PowampNativeListAddresses.Bank,
            PowampNativeListAddresses.Entries, PowampAnimations.Programs, PowampFrameToolingDefinitions.Spritemap, _ => null,
            (word, _, _, at) => throw new InvalidDataException($"{at}: the Powamp lists use no owner instruction ${word:X4}."));
        foreach (PowampProgram program in new[] { PowampProgram.BalloonInflate0, PowampProgram.BalloonInflate1, PowampProgram.BalloonStartSinking, PowampProgram.BalloonDeflated })
        for (int position = 0; position < steps[program].Length; position++)
        {
            (bool sinking, int shown) = PowampBehavior.PowampBalloonPhase(new AnimationProgramCursor<PowampProgram>(program, position));
            ushort sequence = NativeEnemyProgramOracle.EntryOf(PowampNativeListAddresses.Entries, sinking ? PowampProgram.BalloonStartSinking : PowampProgram.BalloonInflate0);
            AssertEqual(steps[program][position], (ushort)(sequence + 4 * shown), $"Powamp balloon {program} step {position} phase");
        }
        Console.WriteLine("  Powamp behavior: both initial animations and every balloon step's native phase pass.");
    }

    /// <summary>Runs the Powamp behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--powamp-instruction-mechanics")]
    internal static int PowampBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPowampBehavior), () => VerifyPowampBehavior(rom));
        Suite(nameof(PowampAnimationsVerification.VerifyPowampAnimations), () => PowampAnimationsVerification.VerifyPowampAnimations(rom));
        return 0;
    }
}
