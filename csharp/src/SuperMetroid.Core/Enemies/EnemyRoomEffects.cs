using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room effects enemy AI requests during a pass: the hardcoded PLM spawns of Botwoon, Spore Spawn, Crocomire,
/// Kraid, Mother Brain, the Shitroid and the Chozo statues, and the downward gates' projectile requests.
/// </summary>
internal sealed class EnemyRoomEffects
{
    /// <summary>Destination for Mother Brain's room palette and FX transitions.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Receives layer-blending defaults selected by a Mother Brain FX entry.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Starts the escape countdown on Mother Brain's requested frame.</summary>
    private readonly EscapeTimer _escapeTimer;
    /// <summary>Supplies Samus equipment and relinquishes the drained-phase timer handler.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides active level, area, FX pointer, and room scroll state.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Records boss bits and the Zebes timebomb event requested by the encounter.</summary>
    private readonly Bank80SystemState _system;

    /// <summary>Binds the room systems needed to consume enemy publications immediately after their AI pass.</summary>
    internal EnemyRoomEffects(EnemyRoomEffectsDependencies dependencies)
    {
        _cgram = dependencies.Cgram;
        _display = dependencies.Display;
        _escapeTimer = dependencies.EscapeTimer;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _system = dependencies.System;
    }

    /// <summary>
    /// Starts the Ceres escape timer: <c>$A6:C117</c> starts it on the same EnemyMain call that changes ceres_status
    /// from one to two.
    /// </summary>
    internal void StartCeresEscape() => _escapeTimer.RequestCeresStart();

    /// <summary>
    /// Applies the hardcoded PLMs and room effects that enemy AI requested during its pass.
    /// </summary>
    /// <remarks>
    /// Native actors call <c>Spawn_Hardcoded_PLM</c> ($84:83D7) and their other room writes
    /// inside EnemyMain, and each PLM's setup edits the level at once. Applying them right
    /// after the enemy pass lets Samus's movement later in the frame meet the edited
    /// terrain; Baby Metroid clears its invisible wall this way just before Samus runs
    /// through it.
    /// </remarks>
    internal void ApplyEnemyRequestedRoomEffects(EnemyRoomEffectRequests requests, RoomPlmSystem plms, RoomLayer3FxState roomLayer3Fx, RoomPaletteFxSystem roomPaletteFx, RoomEnemySystem enemies)
    {
        ApplyPendingBotwoonWallPlm(requests, plms);
        ApplyPendingSporeSpawnCeilingPlm(requests, plms);
        ApplyPendingCrocomireArenaPlms(requests, plms);
        ApplyPendingKraidPlms(requests, plms);
        ApplyPendingMotherBrainPlms(requests, plms, roomLayer3Fx, roomPaletteFx);
        ApplyPendingShitroidWallPlms(requests, plms);
        ApplyPendingChozoStatuePlms(plms, roomLayer3Fx, enemies);
    }

    /// <summary>
    /// Applies the cross-bank effect published by Botwoon's bank-$B3 AI through the shared
    /// bank-$84 PLM owner. This is called at both native producer sites: room initialization
    /// for an already-defeated boss and EnemyMain when the final body segment lands.
    /// </summary>
    internal void ApplyPendingBotwoonWallPlm(EnemyRoomEffectRequests requests, RoomPlmSystem plms)
    {
        if (requests.BotwoonWall is not PlmHeaderId header)
            return;
        if (_room.LevelData is null || _room.Camera is null)
        {
            throw new InvalidOperationException(
                "Botwoon published a wall PLM without an active room level and scroll grid.");
        }

        // SpawnHardcodedPLM silently returns when all native slots are occupied. Preserve
        // that allocator behavior; normal load begins from Reset's empty pool, while an
        // artificial full-pool audit must not gain a host-only exception or terrain edit.
        plms.BossRoomMutations.TrySpawnBotwoonWall(_room.LevelData, header);

        if (header == PlmHeaderId.ClearBotwoonWall)
        {
            // The already-defeated branch performs this 16-bit `$0101` store directly in
            // `$B3:959E`; unlike the live crumble PLM, it does not wait for instruction
            // `$84:AB51` during the first handler pass.
            _room.Camera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
            _room.Camera.Scrolls.SetLogicalState(new(1, 0), RoomScrollState.Blue);
        }
    }

    /// <summary>
    /// Applies Spore Spawn's bank-$A5 ceiling request through the shared bank-$84 PLM
    /// owner. Initialization publishes the clear entry for a defeated save; the live death
    /// reaction publishes the animated crumble entry on the exact collision frame.
    /// </summary>
    internal void ApplyPendingSporeSpawnCeilingPlm(EnemyRoomEffectRequests requests, RoomPlmSystem plms)
    {
        if (requests.SporeSpawnCeiling is not PlmSpawnRequest request)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Spore Spawn published a ceiling PLM without active room level data.");
        }
        if (request.Block != new Position16<RoomBlocks>(7, 30))
        {
            throw new InvalidDataException(
                $"Spore Spawn published non-cartridge ceiling coordinates " +
                $"({request.Block.X},{request.Block.Y}).");
        }

        // SpawnHardcodedPLM silently drops the request if all forty native slots are full.
        plms.BossRoomMutations.TrySpawnSporeSpawnCeiling(_room.LevelData, request.Header);
    }

    /// <summary>
    /// Applies Crocomire's bank-$A4 hardcoded arena mutations through the shared bank-$84
    /// PLM owner. Consuming the full frame-local list preserves native publication order
    /// and fixed-pool exhaustion behavior.
    /// </summary>
    internal void ApplyPendingCrocomireArenaPlms(EnemyRoomEffectRequests requests, RoomPlmSystem plms)
    {
        if (requests.CrocomirePlms.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Crocomire published an arena PLM without an active room level.");
        }

        foreach (PlmSpawnRequest request in requests.CrocomirePlms)
        {
            // SpawnHardcodedPLM silently loses a request when all forty native slots are
            // occupied. TrySpawnCrocomireArenaMutation deliberately mirrors that result.
            plms.BossRoomMutations.TrySpawnCrocomireArenaMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Kraid's bank-$A7 hardcoded ceiling/platform calls into the shared bank-$84
    /// PLM pool before the current frame's handler pass.
    /// </summary>
    internal void ApplyPendingKraidPlms(EnemyRoomEffectRequests requests, RoomPlmSystem plms)
    {
        if (requests.KraidPlms.Count == 0)
            return;
        if (_room.LevelData is null)
            throw new InvalidOperationException("Kraid published room PLMs without active level data.");

        foreach (PlmSpawnRequest request in requests.KraidPlms)
        {
            plms.BossRoomMutations.TrySpawnKraidRoomMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Mother Brain's bank-$A9 hardcoded-PLM requests to the shared bank-$84
    /// owner. The enemy state publishes requests in cartridge call order; consuming them
    /// here retains the native descending-slot allocation and one-frame execution seam.
    /// </summary>
    internal void ApplyPendingMotherBrainPlms(EnemyRoomEffectRequests requests, RoomPlmSystem plms, RoomLayer3FxState roomLayer3Fx, RoomPaletteFxSystem roomPaletteFx)
    {
        if (requests.MotherBrainDeathStep is { } death)
        {
            foreach (PaletteFxObject definition in death.EscapePaletteFxRequests)
                roomPaletteFx.SpawnDefinition(definition, _player.IntroducedSamus.EquippedItems);
            if (death.TimerHandlingEnableRequested)
                _player.IntroducedSamus.Drained.RelinquishTimerHackHandler();
            if (death.MotherBrainEscapeTimerStartRequested)
                _escapeTimer.RequestMotherBrainStart(); // The runtime already owns per-frame timer processing/drawing.
            if (death.MotherBrainBossBitRequested)
                _system.SetBossBits(_room.LoadedRoom.AreaIndex, BossBits.AreaMiniBoss);
            if (death.ZebesTimebombEventRequested)
                _system.SetEvent(EventNumber.ZebesTimebombSet);
        }
        if (requests.MotherBrainFxEntry > 0)
        {
            // The pending native LoadFxEntry call is applied once, on the pass that requested it. Reapplying it
            // each frame would reset its delay and prevent the acid from moving.
            RoomFxDefinition fx = _room.LoadedRoom.State.Fx
                ?? throw new InvalidOperationException("Mother Brain requested an FX entry in a room without FX.");
            _display.LayerBlendingDefaultConfig = roomLayer3Fx.ApplyEntry(_cgram,
                fx.Records[requests.MotherBrainFxEntry]);
        }
        if (requests.MotherBrainPlms.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Mother Brain published a room PLM without active room level data.");
        }

        foreach (PlmSpawnRequest request in requests.MotherBrainPlms)
        {
            plms.BossRoomMutations.TrySpawnMotherBrainMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Shitroid's bank-$A9 hardcoded wall requests to the shared bank-$84 PLM
    /// pool at both producer seams: enemy initialization and the end of each enemy pass.
    /// </summary>
    internal void ApplyPendingShitroidWallPlms(EnemyRoomEffectRequests requests, RoomPlmSystem plms)
    {
        if (requests.ShitroidPlms.Count == 0)
            return;
        if (_room.LevelData is null)
        {
            throw new InvalidOperationException(
                "Shitroid published wall PLMs without active room level data.");
        }

        foreach (PlmSpawnRequest request in requests.ShitroidPlms)
        {
            plms.BossRoomMutations.TrySpawnShitroidWallMutation(
                _room.LevelData,
                request.Block,
                request.Header);
        }
    }

    /// <summary>
    /// Binds transient Chozo services and consumes pending bank-$AA terrain publications.
    /// Also runs before movement so restored states cannot collide before their pending
    /// initialization setup has installed the hand block.
    /// </summary>
    internal void ApplyPendingChozoStatuePlms(RoomPlmSystem plms, RoomLayer3FxState roomLayer3Fx, RoomEnemySystem enemies)
    {
        plms.ChozoStatue.BindChozoStatueContext(enemies, roomLayer3Fx);
        if (_room.LevelData is not null)
            enemies.ChozoStatueBehavior.ApplyPendingChozoStatuePlms(_room.LevelData, plms.ChozoStatue);
    }

    /// <summary>Transfers bank-$84 downward-gate spawn/wake work into bank $86.</summary>
    internal void ApplyPendingDownwardGateProjectileRequests(RoomPlmSystem plms, RoomEnemySystem enemies)
    {
        if (_room.LevelData is null)
            throw new InvalidOperationException("Gate projectile work requires active room level data.");

        foreach (DownwardGateProjectileRequest request in plms.Gates.TakeDownwardGateProjectileRequests())
        {
            if (_player.Samus?.Xray.AreEnemyProjectilesSuspended == true)
                continue;
            enemies.DownwardGateBehavior.ApplyDownwardGateProjectileRequest(request, _room.LevelData.Size.X);
        }
    }
}
