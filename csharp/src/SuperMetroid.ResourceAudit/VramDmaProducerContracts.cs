namespace SuperMetroid.ResourceAudit;

/// <summary>Nonconstant producers and the finite transfer domain each one enqueues from.</summary>
internal static class VramDmaProducerContracts
{
    internal static bool TryGet(string owner, out string? family)
    {
        family = owner switch
        {
            "SuperMetroid.Core.Assets.EscapeTimerTileAtlas.TryQueueNativeTransfer" => "timer",
            "SuperMetroid.Core.Assets.GrappleTileAtlas.QueuePoint" => "grapple-point",
            "SuperMetroid.Core.Assets.GrappleTileAtlas.QueueSegments" => "grapple-segment",
            "SuperMetroid.Core.Assets.ProjectileTrailAtlas.QueueTo" => "trails",
            "SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresEmergencyText" => "emergency",
            "SuperMetroid.Core.Game.RoomEnemySystem.QueueNextCeresEscapeTransfer" => "ceres",
            "SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresTransferList" => "ceres-japanese",
            "SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadSidehopperFrameVramTransfers" => "corpse",
            "SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadTorizoFrameVramTransfers" => "dead-torizo",
            "SuperMetroid.Core.Game.RoomEnemySystem.AdvanceKraidDeathBg3Transfer" => "kraid",
            "SuperMetroid.Core.Game.RoomEnemySystem.QueueGraphicsUploads" => "enemies",
            "SuperMetroid.Core.Game.RoomEnemySystem.QueueGunshipTakeoffTiles" => "gunship",
            "SuperMetroid.Core.Game.RoomFxAnimatedTilesState.Step" => "fx",
            "SuperMetroid.Core.Game.SamusArmCannonState.Draw" => "arm",
            "SuperMetroid.Core.Game.SamusDeathSequenceState.QueueSegment" => "death",
            "SuperMetroid.Core.Game.SamusGrappleMovement.DrawConnectedBeam" => "grapple-native",
            "SuperMetroid.Core.Game.SamusProjectileSystem.QueueBeamTilesAndLoadPalette" => "beam",
            "SuperMetroid.Core.Game.ScrollingSkyState.QueueTilemapRows" => "sky",
            "SuperMetroid.Core.Game.TourianStatueSequence.StepTiles" => "statues",
            "SuperMetroid.Core.Game.WreckedShipTreadmillAnimatedTilesState.Step" => "treadmill",
            "SuperMetroid.Core.Runtime.SuperMetroidRuntime.RunPlmHandlerCore" => "plm",
            _ => null,
        };
        return family is not null;
    }
}
