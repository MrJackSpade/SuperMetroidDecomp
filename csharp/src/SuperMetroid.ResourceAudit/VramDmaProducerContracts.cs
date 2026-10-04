namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed nonconstant producer bodies. A changed body must be re-reviewed, never silently skipped.</summary>
internal static class VramDmaProducerContracts
{
    internal static bool TryGet(string owner, string hash, out string? family)
    {
        family = (owner, hash) switch
        {
            ("SuperMetroid.Core.Assets.EscapeTimerTileAtlas.TryQueueNativeTransfer", "3D33F292A4BDC801445DF1ECAADFADE391B699B565E9824519028B934847FD69") => "timer",
            ("SuperMetroid.Core.Assets.GrappleTileAtlas.QueuePoint", "51FD3523C83AC0F7159834EBF24F5C57F8FFE133F04CC435BECAA0BBB63F9E5B") => "grapple-point",
            ("SuperMetroid.Core.Assets.GrappleTileAtlas.QueueSegments", "CDDBFE8D49F1FF228E23D173D018E81ACF9A5CEC8B203D6F4867A4487A1C2BB1") => "grapple-segment",
            ("SuperMetroid.Core.Assets.ProjectileTrailAtlas.QueueTo", "D67BEF4DF0B694CE731D10819694CA083F2F7A70F0DA7685B8FD06A145142563") => "trails",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresEmergencyText", "54845A9326C3FDD9CEA747BC5AD0926E6C680F4293A7BEE53868FE9EB2387EFB") => "emergency",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueNextCeresEscapeTransfer", "866A0DAE14C487627F4E1110BA4072DF4473FDE792F469E75E186494CD0CE762") => "ceres",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresTransferList", "F8C3E67A0A65E2F32C669B2AE8D5E9F32EE812FDB1456AF65FF7AFA10862B8EA") => "ceres-japanese",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadSidehopperFrameVramTransfers", "70D137F69FBC4FDD589B443A18B8D13DADF2BBFF5ACA8A8D6E873946F21204C0") => "corpse",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadTorizoFrameVramTransfers", "98A082B3F67EDCF9AD3807EB52E140A214BA1013AFAB0B55D68FCAA96181559A") => "dead-torizo",
            ("SuperMetroid.Core.Game.RoomEnemySystem.AdvanceKraidDeathBg3Transfer", "54415DBBB2343700E2F073A26AC780E70C69CCA4CEB7E9B845973BF2133D17E6") => "kraid",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueGraphicsUploads", "E1595BA3332F4AD449CE762A289278B82F55E720F157DB9AD2138E792D3CD90D") => "enemies",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueGunshipTakeoffTiles", "3DBBE2D7D64F7FE12212A3A7CA7CC0DB8DD83EF33BE5706D78264A1E5A3B17B0") => "gunship",
            ("SuperMetroid.Core.Game.RoomFxAnimatedTilesState.Step", "A39ED99F8074CF4BD992B88F053BD568B99846E03C50BEF08234409AB7C4C650") => "fx",
            ("SuperMetroid.Core.Game.SamusArmCannonState.Draw", "44F9986C48E392A4DD5B0D1C2356A80DF41B94649AA1370532D47D355B8EC5C0") => "arm",
            ("SuperMetroid.Core.Game.SamusDeathSequenceState.QueueSegment", "E7F50B67C5D86A9D0242F99335B319A965C8589022818DD7C9798E641CFCE6E0") => "death",
            ("SuperMetroid.Core.Game.SamusGrappleMovement.DrawConnectedBeam", "4DCF557953A345538F57F81AB57E82DA12109426667B095E3CB3417121969D7C") => "grapple-native",
            ("SuperMetroid.Core.Game.SamusProjectileSystem.QueueBeamTilesAndLoadPalette", "44395416D8BC86CA9A616F6F13F218F9E604706012EA4300740448FC8BF08661") => "beam",
            ("SuperMetroid.Core.Game.ScrollingSkyState.QueueTilemapRows", "58C422A0EA358CBE71A98743138A8B881A5C2A5E6B4209804421A4EFFDB469D3") => "sky",
            ("SuperMetroid.Core.Game.TourianStatueSequence.StepTiles", "139EA43A460DE03401FBC47FEA36679BD22DE5A9E8A71AACC2ECA7F3D881F748") => "statues",
            ("SuperMetroid.Core.Game.WreckedShipTreadmillAnimatedTilesState.Step", "5C3CA2467811334271B351B1D30B75FF09ACCB7EE075D257DADF6C4432B9F918") => "treadmill",
            ("SuperMetroid.Core.Runtime.SuperMetroidRuntime.RunPlmHandlerCore", "C9AC194B9874AC7384B6A9C92334BC7FD2E81A9084C5BBA3FA16415FC074ADA5") => "plm",
            ("SuperMetroid.Core.Runtime.SuperMetroidRuntime.InitializeLandingSiteViewport", "303066E06CE0E17F21CFF6C72A78D6AF73D44057498BAFC4020E03CAFAD12483") => "landing",
            _ => null,
        };
        return family is not null;
    }
}
