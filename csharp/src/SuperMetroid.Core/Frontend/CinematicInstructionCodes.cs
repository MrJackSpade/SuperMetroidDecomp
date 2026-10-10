using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Validated decoding of bank-$8B cinematic instruction words into their closed per-owner
/// instruction sets. A list word with <see cref="CinematicCodePointers.InstructionCommandBit"/>
/// set names an executable entry point; each interpreter owner accepts only its own set.
/// </summary>
internal static class CinematicInstructionWords
{
    /// <summary>True when <paramref name="word"/> is a member of <typeparamref name="TInstruction"/>.</summary>
    public static bool TryDecode<TInstruction>(ushort word, out TInstruction instruction)
        where TInstruction : struct, Enum
    {
        instruction = Unsafe.BitCast<ushort, TInstruction>(word);
        return Enum.IsDefined(instruction);
    }

    /// <summary>Decodes <paramref name="word"/>, read at <paramref name="address"/>, or throws.</summary>
    public static TInstruction Decode<TInstruction>(ushort word, ushort address)
        where TInstruction : struct, Enum =>
        TryDecode(word, out TInstruction instruction)
            ? instruction
            : throw new InvalidDataException(
                $"Cinematic word $8B:{word:X4} at $8B:{address:X4} is not a {typeof(TInstruction).Name}.");
}

/// <summary>Shared bank-$8B cinematic-sprite interpreter instructions.</summary>
internal enum CinematicSpriteInstruction : ushort
{
    /// <summary><c>CinematicSpriteObject_Instruction_Delete</c> at $8B:9438.</summary>
    Delete = 0x9438,

    /// <summary><c>UNUSED_CinematicSpriteObject_Instruction_Sleep_8B9442</c> at $8B:9442.</summary>
    Sleep = 0x9442,

    /// <summary>Pre-instruction = [[Y]] at $8B:944C.</summary>
    SetPreInstruction = 0x944c,

    /// <summary><c>CinematicSpriteObject_Instruction_GotoY</c> at $8B:94BC.</summary>
    Goto = 0x94bc,

    /// <summary>Decrement timer and go to [[Y]] if non-zero at $8B:94C3.</summary>
    DecrementTimerAndGoto = 0x94c3,

    /// <summary>Timer = [[Y]] at $8B:94D6.</summary>
    SetTimer = 0x94d6,
}

/// <summary>Instructions of the intro's bank-$8C cinematic background-object lists.</summary>
internal enum CinematicBackgroundInstruction : ushort
{
    /// <summary><c>CinematicBGObject_Instruction_Delete</c> at $8B:9698.</summary>
    Delete = 0x9698,

    /// <summary><c>CinematicBGObject_Instruction_GotoY</c> at $8B:971E.</summary>
    Goto = 0x971e,

    /// <summary><c>Instruction_SetCaretToBlink</c> at $8B:ADD4.</summary>
    SetCaretToBlink = 0xadd4,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page1</c> at $8B:AE43.</summary>
    BeginPage1 = 0xae43,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page1</c> at $8B:AE5B.</summary>
    FinishPage1 = 0xae5b,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page2</c> at $8B:AE79.</summary>
    BeginPage2 = 0xae79,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page2</c> at $8B:AE91.</summary>
    FinishPage2 = 0xae91,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page3</c> at $8B:B074.</summary>
    BeginPage3 = 0xb074,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page3</c> at $8B:B08C.</summary>
    FinishPage3 = 0xb08c,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page4</c> at $8B:B0B3.</summary>
    BeginPage4 = 0xb0b3,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page4</c> at $8B:B0CB.</summary>
    FinishPage4 = 0xb0cb,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page5</c> at $8B:B19B.</summary>
    BeginPage5 = 0xb19b,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page5</c> at $8B:B1B3.</summary>
    FinishPage5 = 0xb1b3,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page6</c> at $8B:B228.</summary>
    BeginPage6 = 0xb228,

    /// <summary><c>Instruction_FinishIntro</c> at $8B:B240.</summary>
    FinishIntro = 0xb240,
}

/// <summary>Private instructions of the Ceres-destruction PLANET ZEBES title actor.</summary>
internal enum ZebesTitleInstruction : ushort
{
    /// <summary><c>Instruction_FadeInPlanetZebesText</c> at $8B:C9A5.</summary>
    FadeInText = 0xc9a5,

    /// <summary><c>Instruction_SpawnPlanetZebesSubtitleIfNeeded</c> at $8B:C9AF.</summary>
    SpawnJapaneseTextIfNeeded = 0xc9af,

    /// <summary><c>Instruction_FadeOutPlanetZebesText</c> at $8B:C9BD.</summary>
    FadeOutText = 0xc9bd,

    /// <summary><c>Instruction_StartFlyingToZebes</c> at $8B:C9C7.</summary>
    StartFlyingToZebes = 0xc9c7,
}

/// <summary>Private instructions of the intro Metroid egg actor.</summary>
internal enum IntroEggInstruction : ushort
{
    /// <summary><c>Instruction_SpawnMetroidEggParticles</c> at $8B:A918.</summary>
    SpawnParticles = 0xa918,

    /// <summary><c>Instruction_StartIntroPage3</c> at $8B:B33E.</summary>
    StartIntroPage3 = 0xb33e,
}

/// <summary>Private instructions of the intro's confused baby Metroid actor.</summary>
internal enum ConfusedBabyInstruction : ushort
{
    /// <summary><c>Instruction_PlayBabyMetroid_Cry1</c> at $8B:A25B.</summary>
    PlayCry1 = 0xa25b,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry2</c> at $8B:A263.</summary>
    PlayCry2 = 0xa263,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry3</c> at $8B:A26B.</summary>
    PlayCry3 = 0xa26b,
}

/// <summary>Private instructions of the intro scientist-cutscene baby Metroid actor.</summary>
internal enum IntroScientistBabyInstruction : ushort
{
    /// <summary><c>Instruction_PlayBabyMetroid_Cry1</c> at $8B:A25B.</summary>
    PlayCry1 = 0xa25b,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry2</c> at $8B:A263.</summary>
    PlayCry2 = 0xa263,

    /// <summary><c>Instruction_PlayBabyMetroid_Cry3</c> at $8B:A26B.</summary>
    PlayCry3 = 0xa26b,

    /// <summary><c>Instruction_StartIntroPage4</c> at $8B:B346.</summary>
    StartIntroPage4 = 0xb346,

    /// <summary><c>Instruction_StartIntroPage5</c> at $8B:B34E.</summary>
    StartIntroPage5 = 0xb34e,
}

/// <summary>Private instructions of the intro flashback's Rinka spawner actor.</summary>
internal enum IntroRinkaSpawnerInstruction : ushort
{
    /// <summary><c>Instruction_Spawn_IntroRinkas_0_1</c> at $8B:BA21.</summary>
    SpawnRinkas0And1 = 0xba21,

    /// <summary><c>Instruction_Spawn_IntroRinkas_2_3</c> at $8B:BA36.</summary>
    SpawnRinkas2And3 = 0xba36,
}

/// <summary>Private instructions of an intro flashback Rinka actor.</summary>
internal enum IntroRinkaInstruction : ushort
{
    /// <summary><c>Instruction_StartMoving_IntroRinka</c> at $8B:B8C5.</summary>
    StartMoving = 0xb8c5,
}

/// <summary>Pre-instructions an intro flashback Rinka actor can select.</summary>
internal enum IntroRinkaPreInstruction : ushort
{
    /// <summary>No pre-instruction has been assigned yet.</summary>
    None = 0,

    /// <summary><c>RTS_8B93D9</c>, the shared cinematic-sprite no-op callback.</summary>
    NoOp = 0x93d9,

    /// <summary><c>PreInstruction_IntroRinka_Moving_HitsSamus</c> at $8B:B8D8.</summary>
    MovingHitsSamus = 0xb8d8,

    /// <summary><c>PreInstruction_IntroRinka_Moving_MissesSamus</c> at $8B:B93B.</summary>
    MovingMissesSamus = 0xb93b,
}

/// <summary>Pre-instructions of the intro's confused baby Metroid actor.</summary>
internal enum ConfusedBabyPreInstruction : ushort
{
    /// <summary><c>PreInstruction_CinematicSpriteObject_ConfusedBabyMetroid</c> at $8B:BA5E.</summary>
    WaitingForHatch = 0xba5e,

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Hatched</c> at $8B:BA73.</summary>
    Hatched = 0xba73,

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Idling</c> at $8B:BB0D.</summary>
    Idling = 0xbb0d,

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Dancing</c> at $8B:BB24.</summary>
    Dancing = 0xbb24,
}

/// <summary>Private instructions of the ending's Zebes-explosion and clear-time actors.</summary>
internal enum EndingSpriteInstruction : ushort
{
    /// <summary><c>Instruction_FadeOutZoomedOutExplodingZebes</c> at $8B:F284.</summary>
    FadeExplosionPalette = 0xf284,

    /// <summary><c>Instruction_CineSpriteObjectSpawnZebesExplosionSilhouette</c> at $8B:F295.</summary>
    SpawnExplosionSilhouette = 0xf295,

    /// <summary><c>Instruction_CinematicSpriteObject_StartZebesExplosion</c> at $8B:F2B7.</summary>
    StartZebesExplosion = 0xf2b7,

    /// <summary><c>Instruction_ZebesExplosionFinale</c> at $8B:F2FA.</summary>
    ExplosionFinale = 0xf2fa,

    /// <summary><c>Instruction_EndZebesExplosion</c> at $8B:F32B.</summary>
    EndZebesExplosion = 0xf32b,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnCompletedSuccessfully</c> at $8B:F3B0.</summary>
    SpawnCompletedText = 0xf3b0,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnClearTime</c> at $8B:F3CE.</summary>
    SpawnClearTime = 0xf3ce,

    /// <summary><c>Instruction_CineSpriteObject_SpawnClearTime_Hours_TensDigit</c> at $8B:F41B.</summary>
    SpawnHoursTens = 0xf41b,

    /// <summary><c>Instruction_CineSpriteObject_SpawnClearTime_Hours_OnesDigit</c> at $8B:F424.</summary>
    SpawnHoursUnits = 0xf424,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnClearTime_Colon</c> at $8B:F42D.</summary>
    SpawnColon = 0xf42d,

    /// <summary><c>Inst_CineSpriteObject_SpawnClearTime_Minutes_TensDigit</c> at $8B:F436.</summary>
    SpawnMinutesTens = 0xf436,

    /// <summary><c>Inst_CineSpriteObject_SpawnClearTime_Minutes_OnesDigit</c> at $8B:F43F.</summary>
    SpawnMinutesUnits = 0xf43f,

    /// <summary><c>Instruction_CinematicSpriteObject_TransitionToCredits</c> at $8B:F448.</summary>
    TransitionToCredits = 0xf448,
}

/// <summary>Private instruction of the ending's Super Metroid logo actors.</summary>
internal enum EndingLogoInstruction : ushort
{
    /// <summary><c>Instruction_GreyOutSuperMetroidIcon</c> at $8B:F25E, starting the logo crossfade.</summary>
    GreyOut = 0xf25e,
}

/// <summary>Private instructions of the ending reward gesture actor.</summary>
internal enum EndingRewardGestureInstruction : ushort
{
    /// <summary><c>Instruction_CinematicSpriteObject_SpawnSuitlessSamusJump</c> at $8B:F51D.</summary>
    SpawnSuitlessJump = 0xf51d,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnSuitedSamusJump</c> at $8B:F554.</summary>
    SpawnSuitedJump = 0xf554,
}

/// <summary>Private instructions of the ending reward jump body and head actors.</summary>
internal enum EndingRewardJumpInstruction : ushort
{
    /// <summary><c>Instruction_CineSpriteObject_PositionSuitedHeadToPrepareJump</c> at $8B:F597.</summary>
    PrepareHead = 0xf597,

    /// <summary><c>Instruction_CinematicSpriteObject_PositionSamusHeadToJump</c> at $8B:F5BA.</summary>
    LaunchHead = 0xf5ba,

    /// <summary><c>Instruction_CinematicSpriteObject_SamusShootsScreen</c> at $8B:F604.</summary>
    Shoot = 0xf604,

    /// <summary><c>Instruction_CinematicSpriteObject_MakeEndingSamusJump</c> at $8B:F651.</summary>
    Launch = 0xf651,
}

/// <summary>Commands of the title sequence's bank-$8B text-sequence lists.</summary>
internal enum TitleSequenceInstruction : ushort
{
    /// <summary><c>CinematicSpriteObject_Instruction_Delete</c> at $8B:9438.</summary>
    Delete = 0x9438,

    /// <summary><c>Instruction_TriggerTitleSequenceScene0</c> at $8B:9CE1.</summary>
    TriggerScene0 = 0x9ce1,

    /// <summary><c>Instruction_TriggerTitleSequenceScene1</c> at $8B:9D5D.</summary>
    TriggerScene1 = 0x9d5d,

    /// <summary><c>Instruction_TriggerTitleSequenceScene2</c> at $8B:9DD6.</summary>
    TriggerScene2 = 0x9dd6,

    /// <summary><c>Instruction_TriggerTitleSequenceScene3</c> at $8B:9E58.</summary>
    TriggerScene3 = 0x9e58,
}
