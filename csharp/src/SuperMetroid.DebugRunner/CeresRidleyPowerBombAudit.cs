using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Focused proof for Ceres Ridley's shared $A6:DFB2 power-bomb callback. Lower Norfair's
/// battle audit executes the same common no-death-check route; this fresh Ceres load keeps
/// its health/timer effects independent of the cinematic's 100-hit escape counter.
/// </summary>
internal static class CeresRidleyPowerBombAudit
{
    private const ushort CeresRidleyDefinition = 0xe13f;

    public static void Verify(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1230);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 128,
            YPosition = 112,
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
            isAreaBossDefeated: () => false,
            level: assets.LevelData,
            samus: samus);

        RoomEnemySlot body = enemies.Slots[0];
        RidleyEnemyState state = enemies.CeresRidley ?? throw new InvalidDataException(
            "Ceres Ridley power-bomb fixture did not allocate typed state.");
        ushort vulnerabilityPointer = body.Definition.VulnerabilityPointer != 0
            ? body.Definition.VulnerabilityPointer
            : (ushort)0xec1c;
        byte vulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + 15)));
        int expectedDamage = 100 * (vulnerability & 0x7f);
        if (body.EnemyDefinitionPointer != CeresRidleyDefinition ||
            body.Definition.PowerBombReactionPointer != 0xdfb2 || expectedDamage == 0)
        {
            throw new InvalidDataException(
                $"Ceres Ridley power-bomb header mismatch: definition=" +
                $"${body.EnemyDefinitionPointer:X4}, callback=" +
                $"${body.Definition.PowerBombReactionPointer:X4}, " +
                $"vulnerability=${vulnerability:X2}.");
        }

        ushort healthBefore = body.Health;
        ushort hitCounterBefore = state.HitCounter;
        int reactions = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            body.XPosition,
            body.YPosition,
            explosionRadius: 32,
            samus);
        ushort expectedHealth = expectedDamage >= healthBefore
            ? (ushort)0
            : unchecked((ushort)(healthBefore - expectedDamage));
        ushort expectedFlash = unchecked((ushort)(
            (body.HurtAiTime == 0 ? 4 : body.HurtAiTime) + 8));
        if (reactions != 1 || body.Health != expectedHealth ||
            body.InvincibilityTimer != 48 || body.FlashTimer != expectedFlash ||
            (body.AiHandlerBits & 0x0002) == 0 ||
            body.Properties.HasAny(EnemyProperties.Deleted) ||
            state.HitCounter != hitCounterBefore)
        {
            throw new InvalidDataException(
                $"Ceres Ridley $A6:DFB2 mismatch: reactions={reactions}, " +
                $"health={healthBefore}->{body.Health}/{expectedHealth}, " +
                $"invincibility/flash={body.InvincibilityTimer}/{body.FlashTimer}, " +
                $"AI=${body.AiHandlerBits:X4}, deleted=" +
                $"{body.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"hit counter={hitCounterBefore}->{state.HitCounter}.");
        }
    }
}
