namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed nonconstant producer bodies. A changed body must be re-reviewed, never silently skipped.</summary>
internal static class VramDmaProducerContracts
{
    internal static bool TryGet(string owner, string hash, out string? family)
    {
        family = (owner, hash) switch
        {
            ("SuperMetroid.Core.Assets.EscapeTimerTileAtlas.TryQueueNativeTransfer", "7F18166A4783DBEEFEC65767EDA4EA74DDEB0F8961C4BDEB4247A1F2FFFA57C8") => "timer",
            ("SuperMetroid.Core.Assets.GrappleTileAtlas.QueuePoint", "A2C82B42691558AB64B9927BB959BD43BDA58A64C27A998B69109A4DDED2299C") => "grapple-point",
            ("SuperMetroid.Core.Assets.GrappleTileAtlas.QueueSegments", "4F3C3E18D4968EF278B1423ADA955413E7F0EF739943E6E2A2C310B1F4C32077") => "grapple-segment",
            ("SuperMetroid.Core.Assets.ProjectileTrailAtlas.QueueTo", "AC33CFB7DD73737706097E3EE4FDF69AA3F192F309F06EE9AD1ED08EE950EB12") => "trails",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresEmergencyText", "54A804A7863688F277F41969841AFCF4B6F39E0A3AB348AC38B9F8E4E58A750C") => "emergency",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueNextCeresEscapeTransfer", "194B6B287A8395FE96F576F435DC0AFEC94FA0FAED1BD5B15070BB7922D9F9A9") => "ceres",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueCeresTransferList", "A24FB0E2FF85491412E0521924419D6AEE6A0199BD978BD10E45EC123DAB01B9") => "ceres-japanese",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadSidehopperFrameVramTransfers", "7262A6E682A2CA5E114299556A10E169E1AC256BE9AE74FDC3F71A653890A6E0") => "corpse",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueDeadTorizoFrameVramTransfers", "C38E4E76EC0C8172695329818A80151767AC4D67412EBE7962AF0C9203BEE66F") => "dead-torizo",
            ("SuperMetroid.Core.Game.RoomEnemySystem.AdvanceKraidDeathBg3Transfer", "D22EF4DF07D73003473126C8FBB09CE03DF6BC26521AB970B5F71AEA4087312C") => "kraid",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueGraphicsUploads", "CF1DA21493A234AACBF7191BBD0264A72F282D3224338BF0C019FFD46F2543A0") => "enemies",
            ("SuperMetroid.Core.Game.RoomEnemySystem.QueueGunshipTakeoffTiles", "09170F3259C9B6D9D6C8DE016CCCF4B2A554BA3DEF536BEBC66FF9C0C23097B1") => "gunship",
            ("SuperMetroid.Core.Game.RoomFxAnimatedTilesState.Step", "74D7F0380AB8B028C6809093235F87837DFD62D7794A242BEFA09C99182049CD") => "fx",
            ("SuperMetroid.Core.Game.SamusArmCannonState.Draw", "71A7789688B774B2A32EB48B793BE93363C555C6C461C1FEC7240A4D59B923D7") => "arm",
            ("SuperMetroid.Core.Game.SamusDeathSequenceState.QueueSegment", "2EA2B64CC751F333B40AED2D5CD4E4FB7B6261003A1882E42E0CF34234C2EB4D") => "death",
            ("SuperMetroid.Core.Game.SamusGrappleMovement.DrawConnectedBeam", "08658BA90E0B432295CA0E4C8F2F72C0BF5F3E4C6F2FC78D0931AEBBB999CE51") => "grapple-native",
            ("SuperMetroid.Core.Game.SamusProjectileSystem.QueueBeamTilesAndLoadPalette", "9400C1F1F3EB941C04621BC070CE56B89828DFFFD4A90A89DD48495304C9700B") => "beam",
            // #627 re-pin: the chunk table parameter became ScrollingSkyChunkTable with the same two tables; sources and counts unchanged.
            ("SuperMetroid.Core.Game.ScrollingSkyState.QueueTilemapRows", "A2AD8385A17B2882557B10624010100C822C3E1F6065403D06698A5C6B73C32B") => "sky",
            ("SuperMetroid.Core.Game.TourianStatueSequence.StepTiles", "91776F442E0E3B352859873875ED9CC1EEF2DE5B7AE0404EF11E0B7B3FA74322") => "statues",
            ("SuperMetroid.Core.Game.WreckedShipTreadmillAnimatedTilesState.Step", "3326DF0E41031BBC1A775D7CB9AE16CD0E0DEACDBFF496AD2482229F046152CE") => "treadmill",
            ("SuperMetroid.Core.Runtime.SuperMetroidRuntime.RunPlmHandlerCore", "440774579B4AA06B249473C51A59FECC7580D0C35E5316933E1E361582FC27ED") => "plm",
            _ => null,
        };
        return family is not null;
    }
}
