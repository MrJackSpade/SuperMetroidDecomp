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
            ("SuperMetroid.Core.Game.RoomFxAnimatedTilesState.Step", "4901159D85A9738784F367B77A2F8BE84AB580691AF088314CABD721C7578F42") => "fx",
            ("SuperMetroid.Core.Game.SamusArmCannonState.Draw", "32BFF91D4CF096F166DDB79A036A6955847E6E5912C7A19DFEFA6CADBCF0B075") => "arm",
            ("SuperMetroid.Core.Game.SamusDeathSequenceState.QueueSegment", "2EA2B64CC751F333B40AED2D5CD4E4FB7B6261003A1882E42E0CF34234C2EB4D") => "death",
            ("SuperMetroid.Core.Game.SamusGrappleMovement.DrawConnectedBeam", "ED286145DF732B7DA580A2C2156B96B047BB4E6BC81A540B7219A66FB464B6A1") => "grapple-native",
            ("SuperMetroid.Core.Game.SamusProjectileSystem.QueueBeamTilesAndLoadPalette", "9400C1F1F3EB941C04621BC070CE56B89828DFFFD4A90A89DD48495304C9700B") => "beam",
            ("SuperMetroid.Core.Game.ScrollingSkyState.QueueTilemapRows", "B26BA7F6ACF3E0A9E88D49A30A4EC87A879555D305F29051C4CE093CDDD857CF") => "sky",
            ("SuperMetroid.Core.Game.TourianStatueSequence.StepTiles", "A496EC2EA70B7EB01C086114C5CD53E2477AF385D98058682A337E19D96E31EE") => "statues",
            ("SuperMetroid.Core.Game.WreckedShipTreadmillAnimatedTilesState.Step", "D8D40445DA38388CC8684F8E72ED8DB2891798317DBBE3D12439AAE23D157E13") => "treadmill",
            ("SuperMetroid.Core.Runtime.SuperMetroidRuntime.RunPlmHandlerCore", "440774579B4AA06B249473C51A59FECC7580D0C35E5316933E1E361582FC27ED") => "plm",
            _ => null,
        };
        return family is not null;
    }
}
