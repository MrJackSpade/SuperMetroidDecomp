namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Nonconstant producers and the finite transfer domain each one enqueues from.</summary>
internal static class VramDmaProducerContracts
{
    /// <summary>Maps a known runtime DMA producer to the finite artwork family
    /// whose source and transfer bounds the resource audit checks.</summary>
    internal static bool TryGet(string owner, out string? family)
    {
        family = owner switch
        {
            "SuperMetroid.Core.Areas.Escape.Artwork.EscapeTimerTileAtlas.TryQueueNativeTransfer" => "timer",
            "SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas.QueuePoint" => "grapple-point",
            "SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas.QueueSegments" => "grapple-segment",
            "SuperMetroid.Core.Samus.Artwork.ProjectileTrailAtlas.QueueTo" => "trails",
            "SuperMetroid.Core.Bosses.Ridley.CeresRidleyBehavior.QueueCeresEmergencyText" => "emergency",
            "SuperMetroid.Core.Bosses.Ridley.CeresRidleyBehavior.QueueNextCeresEscapeTransfer" => "ceres",
            "SuperMetroid.Core.Bosses.Ridley.CeresRidleyBehavior.QueueCeresTransferList" => "ceres-japanese",
            "SuperMetroid.Core.Bosses.Kraid.KraidBehavior.AdvanceKraidDeathBg3Transfer" => "kraid",
            "SuperMetroid.Core.Enemies.EnemyPopulationLoader.QueueGraphicsUploads" => "enemies",
            "SuperMetroid.Core.Enemies.Gunship.GunshipBehavior.QueueGunshipTakeoffTiles" => "gunship",
            "SuperMetroid.Core.Fx.RoomFxAnimatedTilesState.Step" => "fx",
            "SuperMetroid.Core.Samus.Drawing.SamusArmCannonState.Draw" => "arm",
            "SuperMetroid.Core.Samus.Death.SamusDeathSequenceState.QueueSegment" => "death",
            "SuperMetroid.Core.Samus.Grapple.SamusGrappleMovement.DrawConnectedBeam" => "grapple-native",
            "SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.QueueBeamTilesAndLoadPalette" => "beam",
            "SuperMetroid.Core.Rooms.Scrolling.ScrollingSkyState.QueueTilemapRows" => "sky",
            "SuperMetroid.Core.Areas.Tourian.TourianStatueSequence.StepTiles" => "statues",
            "SuperMetroid.Core.Enemies.WreckedShip.WreckedShipTreadmillAnimatedTilesState.Step" => "treadmill",
            "SuperMetroid.Core.Rooms.Plms.PlmFramePhase.RunPlmHandlerCore" => "plm",
            _ => null,
        };
        return family is not null;
    }
}
