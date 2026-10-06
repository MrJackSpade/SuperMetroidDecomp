namespace SuperMetroid.Core.Game;

/// <summary>Native program dispatch for bank-$B4 room sprite objects.</summary>
internal static class RoomSpriteObjectDefinitions
{
    /// <summary>Mutually exclusive entry programs selected by CreateSpriteAtPos at $B4:BC26.</summary>
    private enum ProgramEntry : ushort
    {
        /// <summary>$B4:BE5A, <c>UNUSED_InstList_SpriteObject_0_BeamCharge_B4BE5A</c>; object selector $00.</summary>
        BeamCharge = 0xbe5a,
        /// <summary>$B4:BE6C, <c>UNUSED_InstList_SpriteObject_1_MBElbowChargeParticles_B4BE6C</c>; object selector $01.</summary>
        MBElbowChargeParticles = 0xbe6c,
        /// <summary>$B4:BE86, <c>UNSUED_InstList_SpriteObject_2_MBElbowChargeEnergy_B4BE86</c>; object selector $02.</summary>
        MBElbowChargeEnergy = 0xbe86,
        /// <summary>$B4:BEA4, <c>InstList_SpriteObject_3_SmallExplosion</c>; object selector $03.</summary>
        SmallExplosion = 0xbea4,
        /// <summary>$B4:BEBE, <c>UNUSED_InstList_SpriteObject_4_BombExplosion_B4BEBE</c>; object selector $04.</summary>
        BombExplosion = 0xbebe,
        /// <summary>$B4:BED4, <c>UNUSED_InstList_SpriteObject_5_BeamTrail_B4BED4</c>; object selector $05.</summary>
        BeamTrail = 0xbed4,
        /// <summary>$B4:BEEA, <c>InstList_SpriteObject_6_DudShot</c>; object selector $06.</summary>
        DudShot = 0xbeea,
        /// <summary>$B4:BF04, <c>UNUSED_InstList_SpriteObject_7_PowerBomb_B4BF04</c>; object selector $07.</summary>
        PowerBomb = 0xbf04,
        /// <summary>$B4:BF12, <c>UNUSED_InstList_SpriteObject_8_ElevatorPad_B4BF12</c>; object selector $08.</summary>
        ElevatorPad = 0xbf12,
        /// <summary>$B4:BF1C, <c>InstList_SpriteObject_9_SmallDudShot</c>; object selector $09.</summary>
        SmallDudShot = 0xbf1c,
        /// <summary>$B4:BF32, <c>InstList_SpriteObject_A_SpacePirateLandingDustCloud</c>; object selector $0A.</summary>
        SpacePirateLandingDustCloud = 0xbf32,
        /// <summary>$B4:BF44, <c>UNUSED_InstList_SpriteObject_B_EyeDoorSweatDrop_B4BF44</c>; object selector $0B.</summary>
        EyeDoorSweatDrop = 0xbf44,
        /// <summary>$B4:BF56, <c>InstList_SpriteObject_C_Smoke</c>; object selector $0C.</summary>
        Smoke = 0xbf56,
        /// <summary>$B4:BF8E, <c>UNUSED_InstList_SpriteObject_D_SmallEnergyDrop_B4BF8E</c>; object selector $0D.</summary>
        SmallEnergyDrop = 0xbf8e,
        /// <summary>$B4:BFA0, <c>UNUSED_InstList_SpriteObject_E_BigEnergyDrop_B4BFA0</c>; object selector $0E.</summary>
        BigEnergyDrop = 0xbfa0,
        /// <summary>$B4:BFB2, <c>UNUSED_InstList_SpriteObject_F_Bomb_B4BFB2</c>; object selector $0F.</summary>
        Bomb = 0xbfb2,
        /// <summary>$B4:BFC4, <c>UNUSED_InstList_SpriteObject_10_WeirdSmallEnergyDrop_B4BFC4</c>; object selector $10.</summary>
        WeirdSmallEnergyDrop = 0xbfc4,
        /// <summary>$B4:BFD2, <c>UNUSED_InstList_SpriteObject_11_RockParticles_B4BFD2</c>; object selector $11.</summary>
        RockParticles = 0xbfd2,
        /// <summary>$B4:C014, <c>InstList_SpriteObject_12_ShortBigDustCloud</c>; object selector $12.</summary>
        ShortBigDustCloud = 0xc014,
        /// <summary>$B4:C026, <c>UNUSED_InstList_SpriteObject_13_ShortBigDustCloudBeam_B4C026</c>; object selector $13.</summary>
        ShortBigDustCloudBeam = 0xc026,
        /// <summary>$B4:C040, <c>UNUSED_InstList_SpriteObject_14_ShortBigDustCloudBeam_B4C040</c>; object selector $14.</summary>
        ShortBigDustCloudBeam14 = 0xc040,
        /// <summary>$B4:C05E, <c>InstList_SpriteObject_15_BigDustCloud</c>; object selector $15.</summary>
        BigDustCloud = 0xc05e,
        /// <summary>$B4:C080, <c>UNUSED_InstList_SpriteObject_16_WeirdLongBeam_B4C080</c>; object selector $16.</summary>
        WeirdLongBeam = 0xc080,
        /// <summary>$B4:C0FE, <c>UNUSED_InstList_SpriteObject_17_WeirdLongFlickerBeam_B4C0FE</c>; object selector $17.</summary>
        WeirdLongFlickerBeam = 0xc0fe,
        /// <summary>$B4:C10C, <c>InstList_SpriteObject_18_ShortDraygonBreathBubbles</c>; object selector $18.</summary>
        ShortDraygonBreathBubbles = 0xc10c,
        /// <summary>$B4:C132, <c>UNSUED_InstList_SpriteObject_19_SaveStationElectricity</c>; object selector $19.</summary>
        SaveStationElectricity = 0xc132,
        /// <summary>$B4:C154, <c>UNUSED_InstList_SpriteObject_1A_ExpandingVerticalGate_B4C154</c>; object selector $1A.</summary>
        ExpandingVerticalGate = 0xc154,
        /// <summary>$B4:C176, <c>UNUSED_InstList_SpriteObject_1B_ContractingVerticalGate</c>; object selector $1B.</summary>
        ContractingVerticalGate = 0xc176,
        /// <summary>$B4:BF68, <c>UNUSED_InstList_SpriteObject_1C_ElevatorPad_B4BF68</c>; object selector $1C.</summary>
        ElevatorPad1C = 0xbf68,
        /// <summary>$B4:BF74, <c>InstList_SpriteObject_1D_BigExplosion</c>; object selector $1D.</summary>
        BigExplosion = 0xbf74,
        /// <summary>$B4:C198, <c>UNUSED_InstList_SpriteObject_1E_B4C198</c>; object selector $1E.</summary>
        Unused1E = 0xc198,
        /// <summary>$B4:C1AC, <c>UNUSED_InstList_SpriteObject_1F_B4C1AC</c>; object selector $1F.</summary>
        Unused1F = 0xc1ac,
        /// <summary>$B4:C1C0, <c>UNUSED_InstList_SpriteObject_20_B4C1C0</c>; object selector $20.</summary>
        Unused20 = 0xc1c0,
        /// <summary>$B4:C1D4, <c>UNUSED_InstList_SpriteObject_21_B4C1D4</c>; object selector $21.</summary>
        Unused21 = 0xc1d4,
        /// <summary>$B4:C1E8, <c>UNUSED_InstList_SpriteObject_22_B4C1E8</c>; object selector $22.</summary>
        Unused22 = 0xc1e8,
        /// <summary>$B4:C1FC, <c>UNUSED_InstList_SpriteObject_23_B4C1FC</c>; object selector $23.</summary>
        Unused23 = 0xc1fc,
        /// <summary>$B4:C210, <c>UNUSED_InstList_SpriteObject_24_B4C210</c>; object selector $24.</summary>
        Unused24 = 0xc210,
        /// <summary>$B4:C224, <c>UNUSED_InstList_SpriteObject_25_B4C224</c>; object selector $25.</summary>
        Unused25 = 0xc224,
        /// <summary>$B4:C238, <c>UNUSED_InstList_SpriteObject_26_B4C238</c>; object selector $26.</summary>
        Unused26 = 0xc238,
        /// <summary>$B4:C258, <c>UNUSED_InstList_SpriteObject_27_B4C258</c>; object selector $27.</summary>
        Unused27 = 0xc258,
        /// <summary>$B4:C2A0, <c>UNUSED_InstList_SpriteObject_28_B4C2A0</c>; object selector $28.</summary>
        Unused28 = 0xc2a0,
        /// <summary>$B4:C2BC, <c>UNUSED_InstList_SpriteObject_29_B4C2BC</c>; object selector $29.</summary>
        Unused29 = 0xc2bc,
        /// <summary>$B4:C304, <c>UNUSED_InstList_SpriteObject_2A_B4C304</c>; object selector $2A.</summary>
        Unused2A = 0xc304,
        /// <summary>$B4:C30A, <c>InstList_SpriteObject_2B_PuromiBody</c>; object selector $2B.</summary>
        PuromiBody = 0xc30a,
        /// <summary>$B4:C33E, <c>InstList_SpriteObject_2C_PuromiRightExplosion</c>; object selector $2C.</summary>
        PuromiRightExplosion = 0xc33e,
        /// <summary>$B4:C35C, <c>InstList_SpriteObject_2D_PuromiLeftExplosion</c>; object selector $2D.</summary>
        PuromiLeftExplosion = 0xc35c,
        /// <summary>$B4:C37A, <c>InstList_SpriteObject_2E_PuromiSplash</c>; object selector $2E.</summary>
        PuromiSplash = 0xc37a,
        /// <summary>$B4:BE54, <c>UNUSED_InstList_SpriteObject_2F_B4BE54</c>; object selector $2F.</summary>
        Unused2F = 0xbe54,
        /// <summary>$B4:C390, <c>InstList_SpriteObject_30_FallingSparkTrail</c>; object selector $30.</summary>
        FallingSparkTrail = 0xc390,
        /// <summary>$B4:C3A2, <c>UNSUED_InstList_SpriteObject_31_MetroidInsides_B4C3A2</c>; object selector $31.</summary>
        MetroidInsides = 0xc3a2,
        /// <summary>$B4:C3BA, <c>InstList_SpriteObject_32_MetroidElectricity</c>; object selector $32.</summary>
        MetroidElectricity = 0xc3ba,
        /// <summary>$B4:C436, <c>UNUSED_InstList_SpriteObject_33_B4C436</c>; object selector $33.</summary>
        Unused33 = 0xc436,
        /// <summary>$B4:C4B6, <c>InstList_SpriteObject_34_MetroidShell</c>; object selector $34.</summary>
        MetroidShell = 0xc4b6,
        /// <summary>$B4:C536, <c>UNUSED_InstList_SpriteObject_35_B4C536</c>; object selector $35.</summary>
        Unused35 = 0xc536,
        /// <summary>$B4:C5B2, <c>UNUSED_InstList_SpriteObject_36_B4C5B2</c>; object selector $36.</summary>
        Unused36 = 0xc5b2,
        /// <summary>$B4:C5C6, <c>InstList_SpriteObject_37_EnemyShot</c>; object selector $37.</summary>
        EnemyShot = 0xc5c6,
        /// <summary>$B4:C5D8, <c>InstList_SpriteObject_38_YappingMawBaseFacingDown</c>; object selector $38.</summary>
        YappingMawBaseFacingDown = 0xc5d8,
        /// <summary>$B4:C5DE, <c>InstList_SpriteObject_39_YappingMawBaseFacingUp</c>; object selector $39.</summary>
        YappingMawBaseFacingUp = 0xc5de,
        /// <summary>$B4:C5E4, <c>UNUSED_InstList_SpriteObject_3A_B4C5E4</c>; object selector $3A.</summary>
        Unused3A = 0xc5e4,
        /// <summary>$B4:C608, <c>InstList_SpriteObject_3B_EvirFacingLeft</c>; object selector $3B.</summary>
        EvirFacingLeft = 0xc608,
        /// <summary>$B4:C61C, <c>InstList_SpriteObject_3C_EvirFacingRight</c>; object selector $3C.</summary>
        EvirFacingRight = 0xc61c,
        /// <summary>$B4:BE24, <c>InstList_SpriteObject_3D_DraygonFoamingAtTheMouth</c>; object selector $3D.</summary>
        DraygonFoamingAtTheMouth = 0xbe24,
    }

    /// <summary>
    /// $B4:BDA8..BE23, SpriteObject_DrawInst_Pointers: select the initial program
    /// for the native object kind. Unused native kinds remain valid dispatch cases.
    /// Program timing and artwork are independent definitions.
    /// </summary>
    internal static ushort InstructionPointer(RoomSpriteObjectKind kind) =>
        (ushort)((ushort)kind switch
        {
            0x00 => ProgramEntry.BeamCharge,
            0x01 => ProgramEntry.MBElbowChargeParticles,
            0x02 => ProgramEntry.MBElbowChargeEnergy,
            0x03 => ProgramEntry.SmallExplosion,
            0x04 => ProgramEntry.BombExplosion,
            0x05 => ProgramEntry.BeamTrail,
            0x06 => ProgramEntry.DudShot,
            0x07 => ProgramEntry.PowerBomb,
            0x08 => ProgramEntry.ElevatorPad,
            0x09 => ProgramEntry.SmallDudShot,
            0x0a => ProgramEntry.SpacePirateLandingDustCloud,
            0x0b => ProgramEntry.EyeDoorSweatDrop,
            0x0c => ProgramEntry.Smoke,
            0x0d => ProgramEntry.SmallEnergyDrop,
            0x0e => ProgramEntry.BigEnergyDrop,
            0x0f => ProgramEntry.Bomb,
            0x10 => ProgramEntry.WeirdSmallEnergyDrop,
            0x11 => ProgramEntry.RockParticles,
            0x12 => ProgramEntry.ShortBigDustCloud,
            0x13 => ProgramEntry.ShortBigDustCloudBeam,
            0x14 => ProgramEntry.ShortBigDustCloudBeam14,
            0x15 => ProgramEntry.BigDustCloud,
            0x16 => ProgramEntry.WeirdLongBeam,
            0x17 => ProgramEntry.WeirdLongFlickerBeam,
            0x18 => ProgramEntry.ShortDraygonBreathBubbles,
            0x19 => ProgramEntry.SaveStationElectricity,
            0x1a => ProgramEntry.ExpandingVerticalGate,
            0x1b => ProgramEntry.ContractingVerticalGate,
            0x1c => ProgramEntry.ElevatorPad1C,
            0x1d => ProgramEntry.BigExplosion,
            0x1e => ProgramEntry.Unused1E,
            0x1f => ProgramEntry.Unused1F,
            0x20 => ProgramEntry.Unused20,
            0x21 => ProgramEntry.Unused21,
            0x22 => ProgramEntry.Unused22,
            0x23 => ProgramEntry.Unused23,
            0x24 => ProgramEntry.Unused24,
            0x25 => ProgramEntry.Unused25,
            0x26 => ProgramEntry.Unused26,
            0x27 => ProgramEntry.Unused27,
            0x28 => ProgramEntry.Unused28,
            0x29 => ProgramEntry.Unused29,
            0x2a => ProgramEntry.Unused2A,
            0x2b => ProgramEntry.PuromiBody,
            0x2c => ProgramEntry.PuromiRightExplosion,
            0x2d => ProgramEntry.PuromiLeftExplosion,
            0x2e => ProgramEntry.PuromiSplash,
            0x2f => ProgramEntry.Unused2F,
            0x30 => ProgramEntry.FallingSparkTrail,
            0x31 => ProgramEntry.MetroidInsides,
            0x32 => ProgramEntry.MetroidElectricity,
            0x33 => ProgramEntry.Unused33,
            0x34 => ProgramEntry.MetroidShell,
            0x35 => ProgramEntry.Unused35,
            0x36 => ProgramEntry.Unused36,
            0x37 => ProgramEntry.EnemyShot,
            0x38 => ProgramEntry.YappingMawBaseFacingDown,
            0x39 => ProgramEntry.YappingMawBaseFacingUp,
            0x3a => ProgramEntry.Unused3A,
            0x3b => ProgramEntry.EvirFacingLeft,
            0x3c => ProgramEntry.EvirFacingRight,
            0x3d => ProgramEntry.DraygonFoamingAtTheMouth,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind,
                "Room sprite object number must be zero through $3D."),
        });
}
