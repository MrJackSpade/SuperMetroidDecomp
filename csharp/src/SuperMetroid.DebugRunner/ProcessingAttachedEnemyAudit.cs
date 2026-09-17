using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Exercises the three retail enemy families named by the Processing technique with every
/// room actor overlapping Samus on the shared one-in-eight drain-sound frame.
/// </summary>
internal static class ProcessingAttachedEnemyAudit
{
    public static int Run(string rom)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        VerifyFamily(bus, ProcessingAttachedEnemyDefinitions.GreenBrinstarBeetomsRoom,
            ProcessingAttachedEnemyDefinitions.Beetom, expectedActors: 4, "Beetom");
        VerifyFamily(bus, ProcessingAttachedEnemyDefinitions.TourianMetroidRoom,
            ProcessingAttachedEnemyDefinitions.Metroid, expectedActors: 4, "Metroid");
        VerifyFamily(bus, ProcessingAttachedEnemyDefinitions.ColosseumRoom,
            ProcessingAttachedEnemyDefinitions.Mochtroid, expectedActors: 8, "Mochtroid");
        Console.WriteLine(
            "Processing attached enemies: all retail actors publish independent library-three drain calls; the native Max6 ring retains six.");
        return 0;
    }

    private static void VerifyFamily(
        SuperMetroidAddressSpace bus,
        ushort roomPointer,
        ushort enemyDefinition,
        int expectedActors,
        string family)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            // Native eligibility uses a signed compare against 30; keep the fixture in the
            // retail health domain rather than wrapping that comparison with a forged value.
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 128,
            YPosition = 128,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);

        RoomEnemySlot[] actors = enemies.Slots.Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == enemyDefinition)
            .ToArray();
        if (actors.Length != expectedActors)
        {
            throw new InvalidDataException(
                $"{family} processing fixture loaded {actors.Length} target actors, expected {expectedActors}.");
        }

        EnemySoundRequest[] requests = [];
        for (int phase = 0; phase < 16 && requests.Length == 0; phase++)
        {
            foreach (RoomEnemySlot actor in actors)
            {
                actor.XPosition = samus.XPosition;
                actor.YPosition = samus.YPosition;
            }
            enemies.StepFrame(
                0,
                0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            // Resolve every actor through the ordinary collision dispatcher so no actor can
            // hide behind the whole-list method's deliberate early return. EnemyMain above
            // advances the shared phase and clears the preceding frame's publications.
            foreach (RoomEnemySlot actor in actors)
            {
                samus.InvincibilityTimer = 0;
                actor.XPosition = samus.XPosition;
                actor.YPosition = samus.YPosition;
                ushort nativeIndex = checked((ushort)(actor.SlotIndex * RoomEnemySystem.NativeSlotSize));
                if (!enemies.ResolveOrdinarySamusContact(
                        samus, controllerInput: 0, assets.LevelData, nativeIndex))
                {
                    throw new InvalidDataException(
                        $"{family} slot {actor.SlotIndex} did not overlap in the processing fixture.");
                }
            }
            requests = enemies.SoundRequests.ToArray();
        }
        if (requests.Length != expectedActors || requests.Any(request =>
                request.SoundEffect != SoundEffectLibrary3Sounds.AttachedEnemyDrain ||
                request.MaximumQueued != 6 || request.SoundSuppressed))
        {
            throw new InvalidDataException(
                $"{family} produced {requests.Length} drain requests; expected {expectedActors} independent library-three $2D/Max6 calls.");
        }

        var audio = new CartridgeAudioState();
        audio.AdvanceFrame(bus, default);
        foreach (EnemySoundRequest request in requests)
            audio.QueueSoundAndGetAccumulator(
                request.SoundEffect, request.MaximumQueued, request.SoundSuppressed);
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        byte[] reads = (byte[])typeof(CartridgeAudioState)
            .GetField("_soundReadPositions", fields)!.GetValue(audio)!;
        byte[] writes = (byte[])typeof(CartridgeAudioState)
            .GetField("_soundWritePositions", fields)!.GetValue(audio)!;
        int occupancy = (writes[2] - reads[2]) & AudioRomData.Queues.SoundIndexMask;
        if (occupancy != Math.Min(expectedActors, 6))
        {
            throw new InvalidDataException(
                $"{family} library-three ring retained {occupancy} requests, expected {Math.Min(expectedActors, 6)}.");
        }

        Console.WriteLine(
            $"ATTACHED family={family} room=${roomPointer:X4} actors={expectedActors} requests={requests.Length} retained={occupancy}");
    }
}

internal static class ProcessingAttachedEnemyDefinitions
{
    /// <summary>Retail room $9FE5 contains four Beetoms and no other enemy family.</summary>
    public const ushort GreenBrinstarBeetomsRoom = 0x9fe5;
    /// <summary>Retail room $DAE1 contains four ordinary Metroids plus four Rinkas.</summary>
    public const ushort TourianMetroidRoom = 0xdae1;
    /// <summary>Retail room $D72A contains eight Mochtroids and no other enemy family.</summary>
    public const ushort ColosseumRoom = 0xd72a;
    /// <summary>Enemy header $A0:E87F is Beetom.</summary>
    public const ushort Beetom = 0xe87f;
    /// <summary>Enemy header $A0:DD7F is the ordinary Tourian Metroid.</summary>
    public const ushort Metroid = 0xdd7f;
    /// <summary>Enemy header $A0:D8FF is Mochtroid.</summary>
    public const ushort Mochtroid = 0xd8ff;
}
