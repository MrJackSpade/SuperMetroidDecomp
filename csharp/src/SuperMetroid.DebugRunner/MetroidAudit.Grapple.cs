using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class MetroidAudit
{
    private static void VerifyGrappleDamageRules(SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        foreach (ushort health in new ushort[] { 500, 1 })
        foreach (bool frozen in new[] { false, true })
        {
            var loaded = Load(bus, room, assets);
            var actor = loaded.Enemies.Slots[0];
            StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
            actor.Health = health;
            actor.FrozenTimer = frozen ? (ushort)400 : (ushort)0;
            if (actor.Definition.GrappleAiPointer != EnemyAiCodePointers.BankA0.GrappleCancel)
                throw new InvalidDataException("Retail Metroid header must cancel grapple, never select kill.");
            var contact = loaded.Enemies.ResolveGrappleEndpoint(actor.XPosition, actor.YPosition);
            if (!contact.Collided || contact.Reaction != GrappleEnemyReaction.Cancel)
                throw new InvalidDataException("Live Metroid collision failed to cancel grapple.");
            StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
            if (actor.Health != health || actor.EnemyDefinitionPointer != MetroidDefinition ||
                actor.Properties.HasAny(EnemyProperties.Deleted) || loaded.Enemies.EnemiesKilled != 0)
                throw new InvalidDataException("Grapple damaged or deleted a true Metroid.");
        }

        // Both reported crash rooms contain Mochtroids. Use their unmodified population
        // headers, not a synthetic enemy whose grapple reaction we selected ourselves.
        foreach (ushort roomPointer in MetroidGrappleAuditReferences.MochtroidRooms)
        {
            var mockRoom = CartridgeRoomHeader.Load(bus, roomPointer);
            var mockAssets = CartridgeRoomAssets.Load(bus, mockRoom);
            var loaded = Load(bus, mockRoom, mockAssets);
            var actor = loaded.Enemies.Slots.First(e =>
                e.EnemyDefinitionPointer == MetroidGrappleAuditReferences.MochtroidHeader);
            ushort cameraY = (ushort)Math.Clamp(actor.YPosition - 128, 0,
                Math.Max(0, mockRoom.HeightInScreens * 256 - 224));
            void StepAtActor() => loaded.Enemies.StepFrame(CameraFor(mockRoom, actor), cameraY,
                false, loaded.Samus, level: mockAssets.LevelData);
            StepAtActor();
            if (actor.Definition.GrappleAiPointer != EnemyAiCodePointers.BankA0.GrappleKill)
                throw new InvalidDataException("Retail Mochtroid must select grapple kill.");
            var contact = loaded.Enemies.ResolveGrappleEndpoint(actor.XPosition, actor.YPosition);
            if (!contact.Collided || contact.Reaction != GrappleEnemyReaction.Kill)
                throw new InvalidDataException("Live Mochtroid collision failed to select native grapple kill.");
            loaded.Samus.Grapple.Phase = GrapplePhase.Firing;
            StepAtActor();
            if (loaded.Enemies.EnemiesKilled != 1 || loaded.Samus.Grapple.Phase != GrapplePhase.Dropped)
                throw new InvalidDataException("Mochtroid grapple kill did not dispatch native death/cleanup.");
        }
        Console.WriteLine("Grapple damage: true Metroids survive at 500/1 HP, frozen or unfrozen; both reported Maridia rooms contain grapple-killable Mochtroids.");
    }
}

/// <summary>Retail room/header identities used to distinguish the two enemy families.</summary>
internal static class MetroidGrappleAuditReferences
{
    /// <summary>$A0:D8FF EnemyHeaders_Mochtroid, whose grapple AI is Common_GrappleAI_KillEnemy.</summary>
    internal const ushort MochtroidHeader = 0xd8ff;
    /// <summary>$8F:D72A Colosseum and $8F:D913 Halfie Climb, the rooms in the reported grapple crash logs.</summary>
    internal static ReadOnlySpan<ushort> MochtroidRooms => [0xd72a, 0xd913];
}
