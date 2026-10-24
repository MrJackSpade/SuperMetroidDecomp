using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's entrance: the opening delay and its dance.</summary>
[RoomLifetime]
internal sealed class DraygonIntro
{
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Updates the body and eye palette bands used by Draygon's presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonIntro(EnemyRoomBinding room, RoomSpriteObjectPool spriteObjects, EnemyPresentation presentation, DraygonTurrets turrets)
    {
        _room = room;
        _spriteObjects = spriteObjects;
        _presentation = presentation;
        _turrets = turrets;
    }

    /// <summary>Ports the opening 256-frame delay at <c>$A5:871B</c>.</summary>
    internal void RunDraygonIntroInitialDelay(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (state.FunctionTimer.RawValue < 256)
        {
            if (state.FunctionTimer.IsZero)
            {
                RoomEnemyDefinition evir = RoomEnemyDefinitionCatalog.Get(EnemyDefinitionId.Evir);
                if ((evir.TileDataSize & short.MaxValue) !=
                    DraygonIntroPresentationDefinitions.EvirTilesByteCount)
                    throw new InvalidDataException(
                        "Draygon opening Evir sheet no longer has the compiled transfer length.");
                if (_room.Artwork?.TryResolve(evir.TileDataAddress,
                        DraygonIntroPresentationDefinitions.EvirTilesByteCount, out _) != true)
                    throw new InvalidDataException(
                        "Installed Evir graphics are missing for Draygon's opening transfer.");
                _presentation.ShowCharacters(DraygonIntroPresentationDefinitions.EvirTiles);
                state.IntroEvirGraphicsLoaded = true;

                // CreateSpriteAtPos searches descending, so these become native indexes
                // $3E,$3C,$3A,$38—the exact four slots later traversed by $A5:A13E.
                for (int actor = 0; actor < 4; actor++)
                {
                    if (_spriteObjects.SpawnRoomSpriteObject(
                            new(16, 384),
                            RoomSpriteObjectKind.DraygonIntroEvir,
                            EnemyPaletteBits.Palette7) is not null)
                    {
                        state.IntroEvirsSpawned++;
                    }
                }
            }
            state.FunctionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
            return;
        }

        state.Function = DraygonAiFunction.IntroDance;
        state.FunctionTimer = WrappingCountdown.Start(0);
        state.LeftSideReset = state.Body.Position.Whole;
        state.RightSideReset = state.LeftSideReset + DraygonBehavior.DraygonRightSideResetDistance;
        state.Body.Position = state.Body.Position.WithWhole(state.LeftSideReset);
        state.SwoopAcceleration = DraygonBehavior.DraygonInitialSwoopAcceleration;
    }

    /// <summary>Ports the 1,232-frame Evir dance at <c>$A5:878B/A13E</c>.</summary>
    internal void RunDraygonIntroDance(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (state.FunctionTimer.RawValue >= DraygonIntroDanceDefinitions.DurationFrames)
        {
            state.Function = DraygonAiFunction.SwoopRightSetup;
            state.FunctionTimer = WrappingCountdown.Start(0);
            return;
        }

        // The native routine processes only object indexes $3E..$38. Its latency lookup
        // unexpectedly uses the first four of eight entries, producing 128 inert frames;
        // retain that retail quirk rather than choosing the seemingly intended entries.
        for (int slotIndex = 31; slotIndex >= 28; slotIndex--)
        {
            RoomSpriteObjectSlot sprite = _spriteObjects.Slots[slotIndex];
            int streamIndex = state.FightIntroDanceIndex +
                DraygonIntroDanceDefinitions.MovementLatencyForSlot(slotIndex);
            if (streamIndex < 0 || !sprite.IsActive)
                continue;

            DraygonIntroMovement movement =
                DraygonIntroDanceDefinitions.ResolveMovement(checked((ushort)streamIndex));

            if (movement.DeletesSprite)
            {
                sprite.Clear();
                continue;
            }

            sprite.Position += movement.Delta;
        }

        state.FightIntroDanceIndex += DraygonIntroDanceDefinitions.StreamIndexAdvance;
        state.IntroDanceFrames++;
        state.FunctionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
    }
}
