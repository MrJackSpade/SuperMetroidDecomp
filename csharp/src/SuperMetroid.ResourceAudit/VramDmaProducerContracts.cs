namespace SuperMetroid.ResourceAudit;

/// <summary>Nonconstant producers and the finite transfer domain each one enqueues from.</summary>
internal static class VramDmaProducerContracts
{
    /// <summary>Maps a known runtime DMA producer to the finite artwork family
    /// whose source and transfer bounds the resource audit checks.</summary>
    internal static bool TryGet(string owner, out string? family)
    {
        family = owner switch
        {
            "SuperMetroid.Core.Assets.EscapeTimerTileAtlas.TryQueueNativeTransfer" => "timer",
            "SuperMetroid.Core.Assets.GrappleTileAtlas.QueuePoint" => "grapple-point",
            "SuperMetroid.Core.Assets.GrappleTileAtlas.QueueSegments" => "grapple-segment",
            "SuperMetroid.Core.Assets.ProjectileTrailAtlas.QueueTo" => "trails",
            "SuperMetroid.Core.Game.CeresRidleyBehavior.QueueCeresEmergencyText" => "emergency",
            "SuperMetroid.Core.Game.CeresRidleyBehavior.QueueNextCeresEscapeTransfer" => "ceres",
            "SuperMetroid.Core.Game.CeresRidleyBehavior.QueueCeresTransferList" => "ceres-japanese",
            "SuperMetroid.Core.Game.DeadSidehopperBehavior.QueueDeadSidehopperFrameVramTransfers" => "corpse",
            "SuperMetroid.Core.Game.DeadTorizoBehavior.QueueDeadTorizoFrameVramTransfers" => "dead-torizo",
            "SuperMetroid.Core.Game.KraidBehavior.AdvanceKraidDeathBg3Transfer" => "kraid",
            "SuperMetroid.Core.Game.EnemyPopulationLoader.QueueGraphicsUploads" => "enemies",
            "SuperMetroid.Core.Game.GunshipBehavior.QueueGunshipTakeoffTiles" => "gunship",
            "SuperMetroid.Core.Game.RoomFxAnimatedTilesState.Step" => "fx",
            "SuperMetroid.Core.Game.SamusArmCannonState.Draw" => "arm",
            "SuperMetroid.Core.Game.SamusDeathSequenceState.QueueSegment" => "death",
            "SuperMetroid.Core.Game.SamusGrappleMovement.DrawConnectedBeam" => "grapple-native",
            "SuperMetroid.Core.Game.SamusProjectileSystem.QueueBeamTilesAndLoadPalette" => "beam",
            "SuperMetroid.Core.Game.ScrollingSkyState.QueueTilemapRows" => "sky",
            "SuperMetroid.Core.Game.TourianStatueSequence.StepTiles" => "statues",
            "SuperMetroid.Core.Game.WreckedShipTreadmillAnimatedTilesState.Step" => "treadmill",
            "SuperMetroid.Core.Runtime.PlmFramePhase.RunPlmHandlerCore" => "plm",
            _ => null,
        };
        return family is not null;
    }
}
