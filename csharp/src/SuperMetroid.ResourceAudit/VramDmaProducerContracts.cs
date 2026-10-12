namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed nonconstant producer bodies. A changed body must be re-reviewed, never silently skipped.</summary>
internal static class VramDmaProducerContracts
{
    internal static bool TryGet(string owner, string hash, out string? family)
    {
        family = (owner, hash) switch
        {
            // #627 reviewed: native records now match through the typed EscapeTimerTilePage table; still enqueues only the two installed pages.
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Assets.EscapeTimerTileAtlas.TryQueueNativeTransfer", "6C0779231792F33D50127AE5CB5D9EC517F124235A62C093B954CA18B1DD277F") => "timer",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Assets.GrappleTileAtlas.QueuePoint", "297DD83DB779E775F213058F544A6B8C60B9C1FA8077436C3B8C38A10D958830") => "grapple-point",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Assets.GrappleTileAtlas.QueueSegments", "46452EBDF1A2DB2C9F852D49EC81BFCCE4001C40EF030F0FEEE5DF8778CF60E8") => "grapple-segment",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Assets.ProjectileTrailAtlas.QueueTo", "8F3BFA7AD59D851120BDD276C6B514A9D4452927FAC9FC1D134A7CCA497D2CBC") => "trails",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresEmergencyText", "A59704F1B07199E7D8C219E9F212CF0FF0054C081BB639D0AE44D2B1DF3AB3EE") => "emergency",
            // #627 reviewed: the escape-timer source guard now asks EscapeTimerTilePages.IsSource; transfer routing unchanged.
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueNextCeresEscapeTransfer", "6FAB1BAF9C4205D7D0F2DB86E202B4D19C7F5630170BC2F3D74DA3830BBC6ED4") => "ceres",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresTransferList", "210C7434F1DA503B4CB02D48FB5CE6DA699AA7DAFFE141F583C809904B6AF0BE") => "ceres-japanese",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadSidehopperFrameVramTransfers", "EF3D714CDCC9C449DE6B22CC8679FCECFAC397308BF5AAF95E8B5ED2FB718580") => "corpse",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadTorizoFrameVramTransfers", "C89748C8FEEBBB64D323600228ECBB97DD89BC37368BF99B8F474DD6BD000D6A") => "dead-torizo",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.AdvanceKraidDeathBg3Transfer", "6496DF692487A123D26832590F8A2DC7BA1CE328E7A233660508ABD48C55BEA3") => "kraid",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueGraphicsUploads", "2C64F2B7BC45559D7CA1362930CE6EDE7BF953C6374FDE4F48793E321066B69A") => "enemies",
            // #627 re-pinned: only the gunship function write became the typed GunshipFunction.FireUpEngines.
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueGunshipTakeoffTiles", "38793C0CAA94D4826154247D6FC791CDEBCD4CF4992CEE52039D4FD874A185F9") => "gunship",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.RoomFxAnimatedTilesState.Step", "DBA98B7AA7364E53356FF5F085E846979519062D77DBCC4314A6E8D410ACEFD5") => "fx",
            // #628 reviewed: the OAM origin, layer-1 camera and Samus position became typed values; transfer source and count unchanged.
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.SamusArmCannonState.Draw", "71A8B36C2F02E0B82996FDE11D51D35560044BAB105157E4BE37B653A3715EF8") => "arm",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.SamusDeathSequenceState.QueueSegment", "36126ABB3E9A9AC238EBF5FB38BEAD10705383A6B972F6978751B074FC4DC960") => "death",
            // #628 reviewed: the OAM origins, layer-1 camera and grapple anchor/flare became typed values; transfer source and count unchanged.
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.SamusGrappleMovement.DrawConnectedBeam", "3A46FDAF849CD135A2F3083953A85A1C258FAD73DA5BFB03E86D0CB631C74E73") => "grapple-native",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.SamusProjectileSystem.QueueBeamTilesAndLoadPalette", "88C6638A97CCA42A88F03C6ECB9622FD9BFC013ADD394D524A07E3244B20074B") => "beam",
            // #627 re-pin: the chunk table parameter became ScrollingSkyChunkTable with the same two tables; sources and counts unchanged.
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.ScrollingSkyState.QueueTilemapRows", "D4A69DE535DAB85451E27199652458C96B21B079078C25AA5C4A9C464F3D443C") => "sky",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.TourianStatueSequence.StepTiles", "ECFD06A5B81EFA49A0A79006931D447A1A2683F0FE0B595F004E73D0B10038D5") => "statues",
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Game.WreckedShipTreadmillAnimatedTilesState.Step", "2C16DDAE96CCA50EA3B18A5548E934889989B587526D7424B20F1A329A9A7D36") => "treadmill",
            // #628 reviewed: the camera and BG1 offset are read from their typed positions and the PLM step takes the camera as one Position16<RoomPixels>; PLM VRAM requests unchanged.
            // #630 reviewed: the queued destination became a typed VramDmaDestination from a layout cell or native record; sources and counts unchanged.
            ("SuperMetroid.Core.Runtime.SuperMetroidRuntime.RunPlmHandlerCore", "BBB4E43AC1D0F7D481607B4280683DAE286EECDE903D679E0D7160F49D018FD4") => "plm",
            _ => null,
        };
        return family is not null;
    }
}
