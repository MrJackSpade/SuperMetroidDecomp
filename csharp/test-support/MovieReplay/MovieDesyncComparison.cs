using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.SamusNativeCodecs;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The comparison every converted playthrough replay makes after each checkpointed update: the gameplay-outcome
/// fields that reveal desynchronization, read from the port and from the native checkpoint. Its trace prints the
/// same port and native state field by field to diagnose a divergence.
/// </summary>
internal static class MovieDesyncComparison
{
    /// <summary>Prints port and native state at a converted update to diagnose the first replay divergence.</summary>
    internal static void TraceMovieSamus(SuperMetroidGame game, NativeCheckpointMemory memory, int update, ConvertedMovieUpdate step)
    {
        var samus = game.RuntimeForVerification?.Player.Samus;
        string Native(int address) => Word(memory, address).ToString("X4");
        Console.WriteLine($"trace {update}@{step.SourceFrame} in={step.Input:X4} state={GameStateNumberCodec.Encode(game.GameState):X2}/{Native(MovieDesyncMemory.GameState)} " +
            $"door={game.DoorTransitionPhaseForVerification}/{Native(0x099c)} rng={game.DispatcherRandomNumber:X4}/{Native(MovieDesyncMemory.Random)}");
        if (game.Session.ScreenAs<IntroCinematicState>() is { } intro)
            Console.WriteLine($"  intro port={intro.Phase} native cinematic={Native(0x1f51)}");
        if (game.Session.ScreenAs<GameOptionsMenuState>() is { } options)
            Console.WriteLine($"  options port phase={options.Phase} item={options.SelectedItem}");
        if (samus is null) return;
        Console.WriteLine($"  port   pose={(int)SamusPoseNumbers.Number(samus.Pose):X2} X={samus.Position.X.Whole:X4}.{samus.Kinematics.Position.X.Fraction:X4} Y={samus.Position.Y.Whole:X4}.{samus.Kinematics.Position.Y.Fraction:X4} " +
            $"vs={samus.Kinematics.YSpeed.Whole:X4}.{samus.Kinematics.YSpeed.Fraction:X4} total={samus.HorizontalSpeed.TotalSpeed.Whole:X4}.{samus.HorizontalSpeed.TotalSpeed.Fraction:X4} slope={(samus.Kinematics.PositionAdjustedBySlope ? 1 : 0)} base={samus.HorizontalSpeed.BaseSpeed.Whole:X4}.{samus.HorizontalSpeed.BaseSpeed.Fraction:X4} extra={samus.HorizontalSpeed.ExtraRunSpeed.Whole:X4}.{samus.HorizontalSpeed.ExtraRunSpeed.Fraction:X4} accel={SamusAccelerationModeCodec.ToNative(samus.HorizontalSpeed.AccelerationMode):X4} anim={samus.AnimationFrame:X4}/{samus.AnimationFrameTimer.RawValue:X4} ydir={samus.Kinematics.YDirection} drained={samus.Drained.Phase} movement={SamusPoseMetadata.ReadMovementKind(samus)}");
        Console.WriteLine($"  native pose={Native(MovieDesyncMemory.SamusPose)} X={Native(MovieDesyncMemory.SamusX)}.{Native(MovieDesyncMemory.SamusXFraction)} Y={Native(MovieDesyncMemory.SamusY)}.{Native(MovieDesyncMemory.SamusYFraction)} " +
            $"vs={Native(0x0b2e)}.{Native(0x0b2c)} total={Native(0x0dbc)}.{Native(0x0dbe)} slope={Native(0x0dba)} base={Native(0x0b46)}.{Native(0x0b48)} extra={Native(0x0b42)}.{Native(0x0b44)} accel={Native(0x0b4a)} anim={Native(0x0a96)}/{Native(0x0a94)}");
        Console.WriteLine($"  hurt port inv={samus.InvincibilityTimer.RawValue:X4} kb={samus.KnockbackTimer:X4} div={samus.XSpeedDivisor:X4} hp={samus.Health:X4}");
        if (game.CeresDestructionForVerification is { } boom)
            Console.WriteLine($"  cinematic port={boom.Phase} native={Native(0x1f51)}");
        if (game.Session.ScreenAs<PauseMenuState>() is { } pause)
            Console.WriteLine($"  pause port mode={pause.ScreenMode} cat={(int)pause.SelectedCategory} item={pause.SelectedItem} transition={pause.IsPageTransitionActive} " +
                $"held cur={ControllerWordCodec.ToJoypadWord(game.Session.GameplayRuntime.Controller1.Current):X4} new={ControllerWordCodec.ToJoypadWord(game.Session.GameplayRuntime.Controller1.NewlyPressed):X4} hp={ControllerWordCodec.ToJoypadWord(game.Session.GameplayRuntime.System.HeldInputPrevious):X4} tt={game.Session.GameplayRuntime.System.TimedHeldInputTimer.RawValue:X4} th={ControllerWordCodec.ToJoypadWord(game.Session.GameplayRuntime.System.TimedHeldInput):X4} tn={ControllerWordCodec.ToJoypadWord(game.Session.GameplayRuntime.System.NewlyTimedHeldInput):X4} tp={ControllerWordCodec.ToJoypadWord(game.Session.GameplayRuntime.System.TimedHeldInputPrevious):X4}");
        Console.WriteLine($"  nmi port={(game.RuntimeForVerification?.Clock.Counter ?? game.MenuNmiFrameCounterForVerification):X4} native={Native(0x05b6)}");
        if (game.RuntimeForVerification?.EscapeTimer is { } escape)
            Console.WriteLine($"  escape port={escape.Phase}/{escape.IsActive} {escape.Readout.Minutes:D2}:{escape.Readout.Seconds:D2}.{escape.Readout.Centiseconds:D2} " +
                $"native={Native(0x0943)} {memory[0x0947]:X2}:{memory[0x0946]:X2}.{memory[0x0945]:X2}");
        Console.WriteLine("  aerial port  " + game.RuntimeForVerification?.Player.LastAerialSamusMovement);
        if (game.RuntimeForVerification is { } projectileRuntime)
        {
            Console.WriteLine("  sproj port   " + string.Join(" ", projectileRuntime.Projectiles.Slots
                .Where(slot => !slot.Type.IsNone)
                .Select(slot => $"{slot.Place}:{slot.Type}@{slot.Position.X.Whole:X4}.{slot.Position.X.Fraction:X4}/{slot.Position.Y.Whole:X4} r{slot.Radius.X:X}/{slot.Radius.Y:X} v{slot.Velocity.X.InTwoHundredFiftySixthsOfPixel:X4} d{slot.Direction}{(slot.IsSpent ? " spent" : "")}")));
            Console.WriteLine("  sproj native " + string.Join(" ", Enumerable.Range(0, 5)
                .Where(slot => Native(0x0c18 + 2 * slot) != "0000")
                .Select(slot => $"{slot}:{Native(0x0c18 + 2 * slot)}@{Native(0x0b64 + 2 * slot)}.{Native(0x0b8c + 2 * slot)}/{Native(0x0b78 + 2 * slot)} r{Native(0x0bb4 + 2 * slot)}/{Native(0x0bc8 + 2 * slot)} v{Native(0x0bf0 + 2 * slot)} d{Native(0x0c04 + 2 * slot)}")));
        }
        Console.WriteLine("  music port   " + game.AudioForVerification.MusicQueueForVerification());
        Console.WriteLine($"  music native {Word(memory, 0x063b) / 2:X}/{Word(memory, 0x0639) / 2:X} t={Word(memory, 0x063f):X} d=" +
            string.Join(",", Enumerable.Range(0, 8).Select(slot => Word(memory, 0x0629 + 2 * slot).ToString("X"))));
        // Each library's waiting requests and handshake phase. The cartridge keeps each queue as a sixteen-position ring
        // ($0643 start, $0646 next, $0649 handler state), so its waiting count is the ring distance from start to next.
        Console.WriteLine("  sfx port   " + string.Join(" ", SoundEffectLibraries.InQueueOrder.Select(library =>
        {
            SoundQueueState q = game.AudioForVerification.SoundQueueForVerification(library);
            return $"{q.Waiting.Length:X}/{q.State}";
        })));
        Console.WriteLine("  sfx native " + string.Join(" ", Enumerable.Range(0, 3).Select(queue =>
            $"{(memory[0x0646 + queue] - memory[0x0643 + queue]) & 0x0f:X}/{SoundQueueState.PhaseFromNative(memory[0x0649 + queue])}")));
        if (game.RuntimeForVerification?.Enemies is { } traceEnemies)
        {
            // $1997 IDs, $1A4B X, $1A93 Y: one word per slot.
            var portProjectiles = traceEnemies.Projectiles.EnemyProjectiles.Select((projectile, slot) => new PortProjectile(projectile, slot))
                .Where(entry => entry.Projectile.IsActive)
                .Select(entry => $"{entry.Slot}:{EnemyProjectileNativeWords.Kinds.Word(entry.Projectile.Kind):X4}@{entry.Projectile.Position.X.Whole:X}/{entry.Projectile.Position.Y.Whole:X}" +
                    $"#{entry.Projectile.Animation?.GetType().Name}/{entry.Projectile.FrameHold.RawValue:X}");
            var nativeProjectiles = Enumerable.Range(0, 18).Where(slot => Word(memory, 0x1997 + 2 * slot) != 0)
                .Select(slot => $"{slot}:{Native(0x1997 + 2 * slot)}@{Word(memory, 0x1a4b + 2 * slot):X}/{Word(memory, 0x1a93 + 2 * slot):X}" +
                    $"#{Word(memory, 0x1b47 + 2 * slot):X4}/{Word(memory, 0x1b8f + 2 * slot):X}");
            Console.WriteLine("  eproj port   " + string.Join(" ", portProjectiles));
            Console.WriteLine("  eproj native " + string.Join(" ", nativeProjectiles));
            Console.WriteLine($"  ceres port={traceEnemies.RidleyEncounter.CeresStatus} native={Native(0x093f)} " +
                $"nmi port={game.GameplayRuntimeForVerification.Clock.Counter:X4} native-raw={Native(0x05b6)}");
            foreach (var actor in traceEnemies.SlotTable.Slots.Where(slot => slot.DefinitionId != EnemyDefinitionId.None))
            {
                // $0F86 properties, $0F92 instruction list pointer, $0F94 instruction timer.
                int slotBase = MovieDesyncMemory.EnemyBase + SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(actor.SlotIndex);
                Console.WriteLine($"  enemy {actor.SlotIndex} {actor.DefinitionId} " +
                    $"port timer={actor.FrameHold.RawValue:X4} prop={SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.EncodePropertyWord(actor):X}" +
                    $"pos={actor.Position.X.Whole:X4}.{actor.Position.X.Fraction:X4}/{actor.Position.Y.Whole:X4}.{actor.Position.Y.Fraction:X4} " +
                    $"native inst={Native(slotBase + 0x1a)}/{Native(slotBase + 0x1c)} prop={Native(slotBase + 0x0e)} " +
                    $"pos={Native(slotBase + 2)}.{Native(slotBase + 4)}/{Native(slotBase + 6)}.{Native(slotBase + 8)}");
            }
        }
        if (game.RuntimeForVerification?.Enemies.MotherBrainEncounter.MotherBrain?.BabyMetroid is { } baby)
        {
            // Baby Metroid cutscene actor: $0FA8+$40n function words; velocities at +2/+4.
            Console.WriteLine($"  baby port   pos={baby.Position.X.Whole:X4}.{baby.Position.X.Fraction:X4}/{baby.Position.Y.Whole:X4}.{baby.Position.Y.Fraction:X4} v={baby.Velocity.X.RawValue:X4}/{baby.Velocity.Y.RawValue:X4}");
            Console.WriteLine($"  baby native pos={Native(0x103a)}.{Native(0x103c)}/{Native(0x103e)}.{Native(0x1040)} v={Native(0x106a)}/{Native(0x106c)}");
        }
        if (game.RuntimeForVerification?.Enemies.RidleyEncounter.Ridley is { } ridley)
        {
            // $7E:2020: seven ten-word tail records; X/Y at +12/+14.
            string Port(int index) => $"{ridley.TailSegments[index].Position.X:X4}/{ridley.TailSegments[index].Position.Y:X4}";
            string NativeTail(int index) => $"{Native(0x2020 + index * 20 + 12)}/{Native(0x2020 + index * 20 + 14)}";
            Console.WriteLine("  tail port   " + string.Join(" ", Enumerable.Range(0, 7).Select(Port)) + $" fn={ridley.Function} tailfn={(int)ridley.TailFunctionIndex:X4} v={ridley.Velocity.X.RawValue:X4}/{ridley.Velocity.Y.RawValue:X4} getaway-frame={ridley.GetawayFrameIndex}");
            // $7E:8026 holds the getaway frame as a byte offset into its word-sized tables: twice the frame.
            Console.WriteLine("  tail native " + string.Join(" ", Enumerable.Range(0, 7).Select(NativeTail)) + $" fn={Native(0x0fa8)} tailfn={Native(0x2000)} v={Native(0x0faa)}/{Native(0x0fac)} getaway-frame={Word(memory, 0x8026) / 2}");
        }
    }

    /// <summary>Reads a little-endian WRAM word from a native movie checkpoint.</summary>
    internal static ushort Word(NativeCheckpointMemory memory, int address) => memory.Word(address);

    /// <summary>
    /// Compares the gameplay-outcome fields that reveal desynchronization. Door loading
    /// is atomic in the port, so room-owned state is compared only once gameplay resumes.
    /// </summary>
    internal static List<string> CompareMovieDesyncState(SuperMetroidGame game, NativeCheckpointMemory memory)
    {
        var mismatches = new List<string>();
        void Check(string name, int actual, int address)
        {
            ushort expected = Word(memory, address);
            if (actual != expected) mismatches.Add($"{name}: native={expected:X4} port={actual:X4}");
        }

        Check("Game state", GameStateNumberCodec.Encode(game.GameState), MovieDesyncMemory.GameState);
        bool doorTransition = game.GameState is SuperMetroidGameState.HitDoorBlock or
            SuperMetroidGameState.BeginningDoorTransition or SuperMetroidGameState.DoorTransition;
        if (!doorTransition)
            Check("RNG", game.DispatcherRandomNumber, MovieDesyncMemory.Random);

        // Native loading leaves Samus half-initialized across its NMI waits; the port
        // loads atomically. The following state compares the completed result.
        if (game.GameState is SuperMetroidGameState.SetUpNewGame or SuperMetroidGameState.LoadingGameData)
            return mismatches;
        var runtime = game.RuntimeForVerification;
        if (runtime?.Player.Samus is not { } samus) return mismatches;
        Check("Samus X", samus.Position.X.Whole, MovieDesyncMemory.SamusX);
        Check("Samus X fraction", samus.Kinematics.Position.X.Fraction, MovieDesyncMemory.SamusXFraction);
        Check("Samus Y", samus.Position.Y.Whole, MovieDesyncMemory.SamusY);
        Check("Samus Y fraction", samus.Kinematics.Position.Y.Fraction, MovieDesyncMemory.SamusYFraction);
        Check("Samus health", SamusCountWords.Encode(samus.Health), MovieDesyncMemory.Health);
        Check("Samus max health", SamusCountWords.Encode(samus.MaxHealth), MovieDesyncMemory.MaxHealth);
        Check("Equipped items", samus.EquippedItems.ToEquipmentWord(), MovieDesyncMemory.EquippedItems);
        Check("Collected items", samus.CollectedItems.ToEquipmentWord(), MovieDesyncMemory.CollectedItems);
        Check("Equipped beams", samus.EquippedBeams.ToEquipmentWord(), MovieDesyncMemory.EquippedBeams);
        Check("Collected beams", samus.CollectedBeams.ToEquipmentWord(), MovieDesyncMemory.CollectedBeams);
        Check("Missiles", SamusCountWords.Encode(samus.Missiles), MovieDesyncMemory.Missiles);
        Check("Max missiles", SamusCountWords.Encode(samus.MaxMissiles), MovieDesyncMemory.MaxMissiles);
        Check("Super missiles", SamusCountWords.Encode(samus.SuperMissiles), MovieDesyncMemory.SuperMissiles);
        Check("Max super missiles", SamusCountWords.Encode(samus.MaxSuperMissiles), MovieDesyncMemory.MaxSuperMissiles);
        Check("Power bombs", SamusCountWords.Encode(samus.PowerBombs), MovieDesyncMemory.PowerBombs);
        Check("Max power bombs", SamusCountWords.Encode(samus.MaxPowerBombs), MovieDesyncMemory.MaxPowerBombs);
        Check("Reserve capacity", SamusCountWords.Encode(samus.MaxReserveEnergy), MovieDesyncMemory.MaxReserve);
        Check("Reserve energy", SamusCountWords.Encode(samus.ReserveEnergy), MovieDesyncMemory.Reserve);
        if (game.GameState != SuperMetroidGameState.MainGameplay) return mismatches;

        Check("Samus pose", SamusPoseNumbers.Number(samus.Pose), MovieDesyncMemory.SamusPose);
        Check("Room", runtime.Room.LoadedRoom.Pointer, MovieDesyncMemory.Room);
        Check("Camera X", runtime.Room.LoadedCamera.PixelPosition.X, MovieDesyncMemory.CameraX);
        Check("Camera Y", runtime.Room.LoadedCamera.PixelPosition.Y, MovieDesyncMemory.CameraY);
        Check("Camera X fraction", runtime.Room.LoadedCamera.Position.X.Fraction, MovieDesyncMemory.CameraXSubposition);
        Check("Camera Y fraction", runtime.Room.LoadedCamera.Position.Y.Fraction, MovieDesyncMemory.CameraYSubposition);
        Check("Accepted NMI", checked((ushort)runtime.Clock.Counter), MovieDesyncMemory.NmiCounter);
        if (runtime.Enemies.RidleyEncounter.Ridley is { } ridley)
            for (int index = 0; index < ridley.TailSegments.Length; index++)
            {
                int tail = MovieDesyncMemory.RidleyTailSegments + index * MovieDesyncMemory.RidleyTailSegmentStride;
                // Inactive records hold stale words the cartridge never reads.
                bool nativeActive = (Word(memory, tail) & 0x8000) != 0;
                if (nativeActive != ridley.TailSegments[index].Active)
                    mismatches.Add($"Ridley tail {index} active: native={nativeActive} port={ridley.TailSegments[index].Active}");
                if (!nativeActive && !ridley.TailSegments[index].Active) continue;
                Check($"Ridley tail {index} X", ridley.TailSegments[index].Position.X, tail + MovieDesyncMemory.RidleyTailXOffset);
                Check($"Ridley tail {index} Y", ridley.TailSegments[index].Position.Y, tail + MovieDesyncMemory.RidleyTailYOffset);
            }
        var projectiles = runtime.Enemies.Projectiles.EnemyProjectiles;
        if (projectiles.Count != MovieDesyncMemory.EnemyProjectileSlots)
            throw new InvalidOperationException($"Port has {projectiles.Count} enemy-projectile slots.");
        // The fresh-Ceres pad ($86:A387) and concealer ($86:A395) live in their own arrival
        // model, which owns native slots $22 and $20 while they exist; the pool leaves them free.
        var arrival = runtime.Room.CeresElevatorArrival;
        void CheckArrivalSlot(int slot, bool active, ushort id, ushort y)
        {
            int offset = 2 * slot;
            if (projectiles[slot].IsActive)
                mismatches.Add($"Enemy projectile {slot} is allocated while the Ceres arrival owns it");
            Check($"Enemy projectile {slot} ID", active ? id : (ushort)0, MovieDesyncMemory.EnemyProjectileIds + offset);
            if (!active) return;
            Check($"Enemy projectile {slot} X", arrival.PadPosition.X, MovieDesyncMemory.EnemyProjectileX + offset);
            Check($"Enemy projectile {slot} Y", y, MovieDesyncMemory.EnemyProjectileY + offset);
        }
        bool arrivalLive = arrival is { IsComplete: false };
        if (arrivalLive)
        {
            CheckArrivalSlot(17, (arrival ?? throw new InvalidOperationException("Verification requires arrival.")).PadActive, 0xa387, arrival.PadPosition.Y);
            CheckArrivalSlot(16, arrival.PlatformActive, 0xa395, arrival.PlatformPosition.Y);
        }
        for (int slot = 0; slot < (arrivalLive ? 16 : projectiles.Count); slot++)
        {
            // The port's projectile kind is the native bank-$86 header pointer.
            var projectile = projectiles[slot];
            int offset = 2 * slot;
            Check($"Enemy projectile {slot} ID", projectile.IsActive ? EnemyProjectileNativeWords.Kinds.Word(projectile.Kind) : (ushort)0,
                MovieDesyncMemory.EnemyProjectileIds + offset);
            if (!projectile.IsActive) continue;
            // $86:8097 leaves the whole X word unwritten and the n00b-tube bubble's init only
            // stores its X in Var1; $86:D8DF copies it on its first run. Until then native X
            // is whatever the aliased RAM held (slot 3's X is CinematicFrameCounter $1A51).
            // The bubble has no contact radius, so that residue only places one drawn frame.
            bool bubbleXUnwritten = projectile.Kind == SuperMetroid.Core.Enemies.RoomEnemyProjectileKind.NoobTubeReleasedAirBubble &&
                Word(memory, MovieDesyncMemory.EnemyProjectileX + offset) !=
                Word(memory, MovieDesyncMemory.EnemyProjectileVar1 + offset);
            if (!bubbleXUnwritten)
                Check($"Enemy projectile {slot} X", projectile.Position.X.Whole, MovieDesyncMemory.EnemyProjectileX + offset);
            Check($"Enemy projectile {slot} Y", projectile.Position.Y.Whole, MovieDesyncMemory.EnemyProjectileY + offset);
        }
        foreach (var actor in runtime.Enemies.SlotTable.Slots)
        {
            int address = MovieDesyncMemory.EnemyBase + SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(actor.SlotIndex);
            string owner = $"Enemy {actor.SlotIndex}";
            Check(owner + " identity", SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(actor.DefinitionId), address + MovieDesyncMemory.EnemyIdentityOffset);
            if (actor.DefinitionId == EnemyDefinitionId.None) continue;
            Check(owner + " X", actor.Position.X.Whole, address + MovieDesyncMemory.EnemyXOffset);
            Check(owner + " X fraction", actor.Position.X.Fraction, address + MovieDesyncMemory.EnemyXFractionOffset);
            Check(owner + " Y", actor.Position.Y.Whole, address + MovieDesyncMemory.EnemyYOffset);
            Check(owner + " Y fraction", actor.Position.Y.Fraction, address + MovieDesyncMemory.EnemyYFractionOffset);
            Check(owner + " health", checked((ushort)actor.Health), address + MovieDesyncMemory.EnemyHealthOffset);
        }
        return mismatches;
    }
}
