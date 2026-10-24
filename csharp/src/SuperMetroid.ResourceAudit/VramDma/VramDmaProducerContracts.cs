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
            "SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas.QueuePoint" => "grapple-point",
            "SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas.QueueSegments" => "grapple-segment",
            "SuperMetroid.Core.Samus.Artwork.ProjectileTrailAtlas.QueueTo" => "trails",
            "SuperMetroid.Core.Fx.RoomFxAnimatedTilesState.Step" => "fx",
            "SuperMetroid.Core.Samus.Drawing.SamusArmCannonState.Draw" => "arm",
            "SuperMetroid.Core.Samus.Grapple.SamusGrappleMovement.DrawConnectedBeam" => "grapple-native",
            "SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.QueueBeamTilesAndLoadPalette" => "beam",
            "SuperMetroid.Core.World.TourianStatueSequence.StepTiles" => "statues",
            "SuperMetroid.Core.Fx.WreckedShipTreadmillAnimatedTilesState.Step" => "treadmill",
            "SuperMetroid.Core.World.PlmFramePhase.RunPlmHandlerCore" => "plm",
            _ => null,
        };
        return family is not null;
    }
}
