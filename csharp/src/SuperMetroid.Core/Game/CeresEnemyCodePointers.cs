namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A6 code pointers consumed by translated Ceres enemy dispatchers.</summary>
internal static class CeresEnemyCodePointers
{
    /// <summary><c>Function_CeresDoor_HandleEarthquakeDuringEscape</c> at $A6:F76B.</summary>
    public const ushort Function_CeresDoor_HandleEarthquakeDuringEscape = 0xf76b;

    /// <summary><c>Function_CeresDoor_HandleEarthquakeDuringEscapeInRidleysRoom</c> at $A6:F770.</summary>
    public const ushort Function_CeresDoor_HandleEarthquakeDuringEscapeInRidleysRoom = 0xf770;

    /// <summary><c>Function_CeresDoor_RidleyEscapeMode7Wall</c> at $A6:F7A5.</summary>
    public const ushort Function_CeresDoor_RidleyEscapeMode7Wall = 0xf7a5;

    /// <summary><c>UpdateBabyMetroidPosition_CarriedInArms</c> at $A6:BE9C.</summary>
    public const ushort UpdateBabyMetroidPosition_CarriedInArms = 0xbe9c;

    /// <summary><c>DropBabyMetroid</c> at $A6:BECA.</summary>
    public const ushort DropBabyMetroid = 0xbeca;

    /// <summary><c>BabyMetroidDropped</c> at $A6:BEDC.</summary>
    public const ushort BabyMetroidDropped = 0xbedc;

    /// <summary><c>UpdateBabyMetroidPosition_CarriedInFeet</c> at $A6:BEB3.</summary>
    public const ushort UpdateBabyMetroidPosition_CarriedInFeet = 0xbeb3;

    /// <summary><c>RTS_A6BF19</c> at $A6:BF19.</summary>
    public const ushort RTS_A6BF19 = 0xbf19;

    /// <summary><c>Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX</c> at $A6:BFC9.</summary>
    public const ushort Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX = 0xbfc9;

    /// <summary><c>Instruction_BabyMetroidCutscene_UpdateColors</c> at $A6:BFE1.</summary>
    public const ushort Instruction_BabyMetroidCutscene_UpdateColors = 0xbfe1;

    /// <summary><c>Instruction_BabyMetroidCutscene_GotoXIfNotFalling</c> at $A6:BFF2.</summary>
    public const ushort Instruction_BabyMetroidCutscene_GotoXIfNotFalling = 0xbff2;

    /// <summary><c>Instruction_BabyMetroidCutscene_GotoX</c> at $A6:BFF8.</summary>
    public const ushort Instruction_BabyMetroidCutscene_GotoX = 0xbff8;

    /// <summary><c>UNUSED_Instruction_RidleyCeres_GotoYIfNotHoldingBaby_A6E4EE</c> at $A6:E4EE.</summary>
    public const ushort RidleyGotoIfNotHoldingBaby = 0xe4ee;
    /// <summary><c>UNUSED_Instruction_RidleyCeres_GotoYIfHoldingBaby_A6E4F8</c> at $A6:E4F8.</summary>
    public const ushort RidleyGotoIfHoldingBaby = 0xe4f8;
    /// <summary><c>Instruction_Ridley_GotoYIfNotFacingLeft</c> at $A6:E517.</summary>
    public const ushort RidleyGotoIfNotFacingLeft = 0xe517;
    /// <summary><c>Instruction_Ridley_MoveRidleyWithArgsInY</c> at $A6:E51F.</summary>
    public const ushort MoveRidley = 0xe51f;
    /// <summary><c>Instruction_Ridley_SetDirectionToLeft_UpdateTailParts</c> at $A6:E71C.</summary>
    public const ushort FaceRidleyLeft = 0xe71c;
    /// <summary><c>Instruction_Ridley_SetDirectionToForwardTurning</c> at $A6:E727.</summary>
    public const ushort FaceRidleyForward = 0xe727;
    /// <summary><c>Instruction_Ridley_SetDirectionToRight_UpdateTailParts</c> at $A6:E72F.</summary>
    public const ushort FaceRidleyRight = 0xe72f;
    /// <summary><c>FireLeadsFireball</c> at $A6:E904.</summary>
    public const ushort FireLeadingRidleyFireball = 0xe904;
    /// <summary><c>Instruction_CeresSteam_SetToIntangibleAndInvisible</c> at $A6:F11D.</summary>
    public const ushort HideCeresSteam = 0xf11d;
    /// <summary><c>Instruction_CeresSteam_DecActivationTimer_Decide_GotoYOrY2</c> at $A6:F127.</summary>
    public const ushort StepCeresSteamActivationTimer = 0xf127;
    /// <summary><c>Inst_CeresDoor_GotoYIfSamusIsNotWithing30Pixels</c> at $A6:F63E.</summary>
    public const ushort CeresDoorGotoIfSamusIsDistant = 0xf63e;
    /// <summary><c>Instruction_CeresDoor_SetAsIntangible</c> at $A6:F68B.</summary>
    public const ushort MakeCeresDoorIntangible = 0xf68b;
    /// <summary><c>Instruction_CeresDoor_SetAsTangible</c> at $A6:F695.</summary>
    public const ushort MakeCeresDoorTangible = 0xf695;
    /// <summary><c>Instruction_CeresDoor_SetAsVisible</c> at $A6:F6B3.</summary>
    public const ushort ShowCeresDoor = 0xf6b3;

}
