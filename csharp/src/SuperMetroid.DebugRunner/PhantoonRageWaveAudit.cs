using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

/// <summary>Checks the literal retail rage-loop bounds, then the complete production Super Missile branch.</summary>
internal static class PhantoonRageWaveAudit
{
    public static int Run(string rom)
    {
        // The odd loop decrements before comparing with eight, so eight itself
        // still spawns. Read the actual cartridge operands, not port constants.
        var fixture = PhantoonMaterializationAudit.CreateEncounter(rom);
        var bus = fixture.AddressSpace;
        ushort ReadOperand(int opcodeAddress, byte opcode)
        {
            if (bus.ReadByte(opcodeAddress) != opcode) throw new InvalidDataException("Unexpected rage operand opcode.");
            return (ushort)(bus.ReadByte(opcodeAddress + 1) | bus.ReadByte(opcodeAddress + 2) << 8);
        }
        int evenFirst = ReadOperand(0xa7d8bb, 0xa0);
        int oddFirst = ReadOperand(0xa7d8d0, 0xa0);
        int oddLast = ReadOperand(0xa7d8e1, 0xc0);
        int rounds = ReadOperand(0xa7d8f4, 0xc9);
        int interval = ReadOperand(0xa7d8f9, 0xa9);
        var expectedSound = new EnemySoundRequest(SoundEffectId.FromCartridge(
            SoundEffectLibrary.Library3, (byte)ReadOperand(0xa7d8e6, 0xa9)), 6);
        var callback = typeof(RoomEnemySystem).GetMethod("RunPhantoonRage", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var failures = new List<string>();
        for (ushort round = 0; round < rounds; round++)
        {
            var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
            var boss = runtime.Enemies.Phantoon!;
            boss.Body.VariableE = 1;
            boss.Eye!.VariableF = round;
            int before = runtime.Enemies.SoundRequests.Count;
            callback.Invoke(runtime.Enemies, [boss.Body, boss]);
            var flames = RageFlames(runtime.Enemies);
            int first = (round & 1) == 0 ? evenFirst : oddFirst;
            int last = (round & 1) == 0 ? 0 : oddLast;
            ushort[] expectedAngles = Enumerable.Range(last, first - last + 1)
                .Select(i => (ushort)bus.ReadByte(EnemyRomTablePointers.Phantoon.FlameAngleBytes + i)).Order().ToArray();
            if (!flames.Select(p => p.Variable0).Order().SequenceEqual(expectedAngles))
                failures.Add($"Round {round}: spawned {flames.Length} flames; ROM requires {expectedAngles.Length} including direction {last}.");
            if (runtime.Enemies.SoundRequests.Count != before + 1 || runtime.Enemies.SoundRequests[^1] != expectedSound)
                failures.Add($"Round {round}: missing/wrong native wave sound.");
        }
        if (failures.Count != 0) throw new InvalidDataException(string.Join(Environment.NewLine, failures));

        // Let production AI enter and leave rage. Only projectile contact is
        // constructed; no timers, AI phases, or flame slots are forced here.
        var encounter = PhantoonMaterializationAudit.CreateEncounter(rom);
        var state = encounter.Enemies.Phantoon!;
        var samus = encounter.Samus!;
        samus.SelectedHudItem = 2;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        bool hit = false;
        int entered = -1;
        var waveFrames = new List<int>();
        for (int frame = 0; frame < 3600; frame++)
        {
            ushort oldPhase = state.Body.VariableF;
            ushort oldRound = state.Eye!.VariableF;
            encounter.StepFrame(0);
            if (!hit && state.Body.VariableF == (ushort)PhantoonAiFunction.EyeTracksSamus &&
                !state.Body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
                PhantoonHitFadeAudit.TryFindEyeContact(encounter.Enemies, state.Body, out ushort x, out ushort y))
            {
                var fired = encounter.Projectiles.TryFireMissile(encounter.AddressSpace, samus,
                    (ushort)SnesButton.X, 0, encounter.BombProjectiles);
                if (fired.Slot is not { } index) throw new InvalidDataException("Could not initialize Super Missile.");
                encounter.Projectiles.Slots[index].XPosition = x;
                encounter.Projectiles.Slots[index].YPosition = y;
                encounter.Enemies.ResolvePhantoonProjectileHits(encounter.AddressSpace, encounter.Projectiles, encounter.BombProjectiles);
                if (state.AcceptedProjectileHits != 1 || state.LastProjectileDamage != 600)
                    throw new InvalidDataException("Fixture did not trigger the production 600-damage hit.");
                hit = true;
            }
            if (!hit) continue;
            if (oldPhase != (ushort)PhantoonAiFunction.Enraged && state.Body.VariableF == (ushort)PhantoonAiFunction.Enraged)
                entered = frame;
            if (oldPhase != (ushort)PhantoonAiFunction.Enraged || oldRound == state.Eye.VariableF) continue;
            int wave = waveFrames.Count;
            var flames = RageFlames(encounter.Enemies);
            int expectedCount = (wave & 1) == 0 ? evenFirst + 1 : oddFirst - oddLast + 1;
            if (flames.Length != expectedCount)
                throw new InvalidDataException($"Encounter wave {wave}: {flames.Length} live flames, expected {expectedCount}.");
            int expectedFrame = wave == 0 ? entered + ReadOperand(0xa7d8a2, 0xa9) : waveFrames[^1] + interval;
            if (frame != expectedFrame) throw new InvalidDataException($"Wave {wave} frame {frame}, expected {expectedFrame}.");
            if (encounter.Enemies.SoundRequests.Count(s => s == expectedSound) != 1)
                throw new InvalidDataException($"Encounter wave {wave}: missing or duplicate sound publication.");
            Console.WriteLine($"Rage wave {wave}: frame {frame}, {flames.Length} flames, one native sound request.");
            waveFrames.Add(frame);
            if (state.Body.VariableF == (ushort)PhantoonAiFunction.FadeOutAfterRage) break;
        }
        if (waveFrames.Count != rounds) throw new InvalidDataException($"Only observed {waveFrames.Count}/{rounds} rage waves.");
        Console.WriteLine("Rage loop bounds, angle population, eight-wave cadence and wave sound publication match cartridge operands.");
        return 0;
    }

    private static RoomEnemyProjectileSlot[] RageFlames(RoomEnemySystem enemies) => enemies.EnemyProjectiles
        .Where(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame &&
            p.PreInstruction == EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Enraged).ToArray();
}
