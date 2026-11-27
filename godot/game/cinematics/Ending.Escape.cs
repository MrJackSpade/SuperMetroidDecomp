using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The first part of the ending: out through the clouds, the planet blowing apart, the gunship flying clear, and the
/// mission's result typed over it.
/// </summary>
/// <remarks>
/// The data names the floor's pictures ("cloud_floor", "top_and_bottom_cloud_floor", "planet_floor", "gunship_floor"), the blast's ("planet_explosion",
/// "burst_glow", "finale_explosion", "finale_glow"), and the timelines "right_side_cloud", "left_side_cloud",
/// "top_cloud_pattern", "top_cloud_edge", "bottom_cloud_edge", "bottom_cloud_pattern", "exploding_planet" (which gives
/// the cue "silhouette"), "explosion_lava", "explosion_glow", "stars", "explosion_silhouette" (cue "burst"),
/// "explosion_stars" (cues "finale" and "end"), "explosion_afterglow", "animal_escape", and "digit_0" to "digit_9"
/// and "colon". The result's lines are its "result_texts": each by the name of its words ("ending_operation_was",
/// "ending_completed_successfully", "ending_clear_time"), with the font it is typed in and where its first
/// character's cell lies.
/// </remarks>
public partial class Ending
{
    /// <summary>How long a letter of the result takes, and how long the last letter of a word does.</summary>
    private const int LetterUpdates = 8, LastLetterUpdates = 15;

    /// <summary>How long each figure of the clear time takes to follow the one before, and how long the whole is shown.</summary>
    private const int FigureUpdates = 8, ClearTimeUpdates = 128;

    /// <summary>How many updates each step of the white the blast left takes to clear, once it has been held.</summary>
    private const int FlyawayWashPace = 8;

    /// <summary>Where the figures of the clear time go across the screen: hours, a colon, minutes.</summary>
    private static readonly int[] FigureX = [156, 164, 172, 180, 188];

    /// <summary>Plays one update of the escape, the explosion and the flight away.</summary>
    private void StepEscape()
    {
        ScreenFade fade = Fade();
        switch (_phase)
        {
            case Phase.WaitingForEscapeMusic:
                // The clouds fade in once the music asked for has all been carried out.
                if (Session.Sounds.HasQueuedMusic)
                    break;
                fade.FadeIn(FadeUpdates);
                _phase = Phase.FadingInSideClouds;
                break;

            case Phase.FadingInSideClouds:
                StepAtmosphere(2);
                StepActors();
                fade.Step();
                if (fade.IsDone)
                    _phase = Phase.SideClouds;
                break;

            case Phase.SideClouds:
            case Phase.TopAndBottomClouds:
                StepAtmosphere(_phase == Phase.SideClouds ? 2 : 3);
                StepActors();
                if (_floorSize < 384)
                    break;
                fade.FadeOut(FadeUpdates);
                _phase++;
                break;

            case Phase.FadingOutSideClouds:
                StepAtmosphere(2);
                StepActors();
                fade.Step();
                if (!fade.IsDone)
                    break;
                SetUpTopAndBottomClouds();
                fade.FadeIn(FadeUpdates);
                _phase = Phase.FadingInTopAndBottomClouds;
                break;

            case Phase.FadingInTopAndBottomClouds:
                StepAtmosphere(3);
                StepActors();
                fade.Step();
                if (fade.IsDone)
                    _phase = Phase.TopAndBottomClouds;
                break;

            case Phase.FadingOutTopAndBottomClouds:
                StepAtmosphere(3);
                StepActors();
                fade.Step();
                if (!fade.IsDone)
                    break;
                SetUpExplosion();
                fade.FadeIn(FadeUpdates);
                _phase = Phase.FadingInExplosion;
                break;

            case Phase.FadingInExplosion:
            case Phase.ExplosionCrossfade:
                StepExplosionCrossfade();
                StepActors();
                if (_phase == Phase.FadingInExplosion)
                {
                    fade.Step();
                    if (fade.IsDone)
                        _phase = Phase.ExplosionCrossfade;
                }

                break;

            case Phase.ExplosionHold:
                StepActors();
                if (--_wait <= 0)
                    _phase = Phase.ExplosionAnimation;
                break;

            case Phase.ExplosionAnimation:
                StepActors();
                break;

            case Phase.WaitingForCreditsMusic:
                StepActors();
                if (--_wait > 0)
                    break;
                Session.Sounds.RequestMusic("");
                Compatibility.CartridgeSounds.Of(Session).AskMusicSet("credits");
                Session.Sounds.RequestMusic(Music.Credits, MusicWait);
                _phase = Phase.WaitingForCreditsMusicToCome;
                break;

            case Phase.WaitingForCreditsMusicToCome:
                // The gunship is seen flying away once the music asked for has all been carried out.
                StepActors();
                if (Session.Sounds.HasQueuedMusic)
                    break;
                SetUpPlanetEscape();
                _phase = Phase.PlanetEscapeFast;
                break;

            case Phase.PlanetEscapeFast:
                StepActors();
                StepWhiteOut();
                _floorAngle -= 4;
                ShakeGunship();
                _floorSize -= 8;
                if (_floorSize >= 1456)
                    break;
                (_slowShake, _shake) = (true, 0);
                _phase = Phase.PlanetEscapeSlow;
                break;

            case Phase.PlanetEscapeSlow:
                StepActors();
                StepWhiteOut();
                if (Wrapped(_floorAngle) != 224)
                    _floorAngle--;
                ShakeGunship();
                _floorSize -= 2;
                if (_floorSize >= 1184)
                    break;
                _planetSpeed = 0.5;
                _phase = Phase.PlanetEscapeAccelerating;
                if (AnimalsEscaped)
                {
                    // The animals' pod is seen leaving too.
                    Actor pod = Spawn("animal_escape", "animal_escape", 128, 128, Motion.AnimalPod);
                    Move(pod);
                    pod.Sprite.Step();
                }

                break;

            case Phase.PlanetEscapeAccelerating:
                StepActors();
                StepWhiteOut();
                _planetSpeed -= 1.0 / 256;
                _floorX += _planetSpeed;
                if (_floorSize < 384 && _frame % 4 == 0 && Wrapped(_floorAngle) != 16)
                    _floorAngle += 2;
                if (_floorSize >= 32)
                {
                    _floorSize -= 4;
                    break;
                }

                BeginTyping("ending_operation_was");
                _phase = Phase.CompletionText;
                break;

            case Phase.CompletionText:
                StepActors();
                StepCompletionText();
                break;

            case Phase.FadingOutToCredits:
                StepActors();
                fade.Step();
                if (fade.IsDone)
                    SetUpCredits();
                break;

            default:
                throw new InvalidOperationException($"The ending's escape has no phase {_phase}.");
        }
    }

    /// <summary>The cues the ending's timelines give.</summary>
    private void OnCue(string cue)
    {
        switch (cue)
        {
            case "silhouette":
                Spawn("explosion_silhouette", "explosion_silhouette", 128, 128, Motion.Still);
                (_backdrop ?? throw NotEntered()).Color = Colors.White;
                break;

            case "burst":
                // The planet goes: its blast fills the screen, and the stars are seen past it.
                (_backdrop ?? throw NotEntered()).Color = Colors.Black;
                (_floor ?? throw NotEntered()).Visible = false;
                ShowExplosion("planet_explosion", "burst_glow");
                Spawn("explosion_stars_right", "explosion_stars", 128, 128, Motion.FlyawayStars, behind: true);
                Spawn("explosion_stars_left", "stars", -128, 128, Motion.FlyawayStars, behind: true).Released = true;
                break;

            case "finale":
                ShowExplosion("finale_explosion", "finale_glow");

                // The afterglow plays ahead of the starfields, so it first plays on the next update.
                Actor afterglow = Spawn("explosion_afterglow", "explosion_afterglow", 128, 128, Motion.FinaleExit);
                _actors.Remove(afterglow);
                _actors.Insert(_actors.FindIndex(actor => actor.Motion == Motion.FlyawayStars), afterglow);
                break;

            case "end":
                // The screen goes white, and stays so until the gunship is seen flying out of it.
                BeginWhiteWash(FlyawayWashPace);
                (_explosionPicture ?? throw NotEntered()).Visible = false;
                (_explosionGlow ?? throw NotEntered()).Visible = false;
                foreach (Actor stars in _actors.Where(actor => actor.Motion == Motion.FlyawayStars))
                    stars.Released = true;
                (_phase, _wait) = (Phase.WaitingForCreditsMusic, 120);
                break;

            default:
                throw new InvalidOperationException($"A timeline of the ending gives the cue \"{cue}\", which it does not know.");
        }
    }

    private void SetUpSideClouds()
    {
        ClearActors();
        Spawn("side_cloud", "right_side_cloud", 320, 192, Motion.SideCloudFromRight);
        Spawn("side_cloud", "left_side_cloud", -64, 64, Motion.SideCloudFromLeft);
        Spawn("side_cloud", "right_side_cloud", 320, 448, Motion.SideCloudFromRight);
        Spawn("side_cloud", "left_side_cloud", -64, -192, Motion.SideCloudFromLeft);
        // From the clouds until the planet has gone, the floor repeats beyond its edges.
        ShowFloorPicture("cloud_floor", AtmosphereCentre, repeats: true);
        SetFloor(0, 0, 64, 32);
    }

    private void SetUpTopAndBottomClouds()
    {
        ClearActors();
        Spawn("cloud", "top_cloud_pattern", 128, -96, Motion.CloudDown);
        Spawn("cloud", "top_cloud_edge", 128, -32, Motion.CloudDown);
        Spawn("cloud", "bottom_cloud_edge", 128, 288, Motion.CloudUp);
        Spawn("cloud", "bottom_cloud_pattern", 128, 352, Motion.CloudUp);
        ShowFloorPicture("top_and_bottom_cloud_floor", AtmosphereCentre, repeats: true);
        SetFloor(0, 0, 64, 32);
    }

    private void SetUpExplosion()
    {
        ClearActors();
        Spawn("exploding_planet", "exploding_planet", 128, 128, Motion.Still);
        Spawn("explosion_lava", "explosion_lava", 128, 128, Motion.ExplosionLayer);
        Spawn("explosion_glow", "explosion_glow", 128, 128, Motion.ExplosionLayer);
        Spawn("explosion_stars", "stars", 128, 128, Motion.ExplosionLayer, behind: true);
        ShowFloorPicture("planet_floor", AtmosphereCentre, repeats: true);
        SetFloor(0, 0, 64, 0);
        _crossfade = 0;
        ShowPlanetFade();
    }

    /// <summary>The planet and its lava come in over what lay behind them, a step every other update, as the view draws back.</summary>
    private void StepExplosionCrossfade()
    {
        _floorSize += 4;
        _crossfade++;
        ShowPlanetFade();
        if (_crossfade >= 64)
            (_phase, _wait) = (Phase.ExplosionHold, 16);
    }

    private void ShowPlanetFade()
    {
        float shown = Math.Min(1f, _crossfade / 2 / 32f);
        foreach (Actor actor in _actors.Where(actor => actor.Role is "exploding_planet" or "explosion_lava"))
            actor.Sprite.Tint(new Color(1, 1, 1, shown));
        (_floor ?? throw NotEntered()).Modulate = new Color(1, 1, 1, 1 - shown);
    }

    private void ShowExplosion(string picture, string glow)
    {
        Sprite2D shown = _explosionPicture ?? throw NotEntered(), glowing = _explosionGlow ?? throw NotEntered();
        shown.Texture = Session.Content.LoadTexture(Data().Text(picture));
        glowing.Texture = Session.Content.LoadTexture(Data().Text(glow));
        (shown.Visible, glowing.Visible) = (true, true);
    }

    /// <summary>Among the clouds the view turns a step every other update and draws back all the while.</summary>
    private void StepAtmosphere(int widening)
    {
        if (_frame % 2 == 0)
            _floorAngle--;
        _floorSize += widening;
    }

    private void SetUpPlanetEscape()
    {
        (_backdrop ?? throw NotEntered()).Color = Colors.Black;
        // From the gunship's flight away on, nothing is shown beyond the floor's edges.
        ShowFloorPicture("gunship_floor", GunshipCentre, repeats: false);
        TurningFloor gunship = _floor ?? throw NotEntered();
        gunship.Modulate = Colors.White;

        // The gunship passes in front of everything else from here on.
        gunship.GetParent().MoveChild(gunship, -1);
        SetFloor(-72, -104, 3072, -112);
        (_slowShake, _shake, _whiteHold) = (false, 0, 192);
    }

    /// <summary>
    /// The white the blast left is held a while and then clears, a step on the first update after the hold and every
    /// eighth update after that.
    /// </summary>
    private void StepWhiteOut()
    {
        if (_whiteHold > 0)
        {
            _whiteHold--;
            return;
        }

        StepWhiteWash();
    }

    private void ShakeGunship()
    {
        double[] shake = _slowShake ? SlowShake : FastShake;
        _floorX += shake[_shake];
        _shake = (_shake + 1) % shake.Length;
    }

    private static int Wrapped(int angle) => ((angle % 256) + 256) % 256;

    /// <summary>Starts typing one of the result's lines, its first letter on the next update.</summary>
    private void BeginTyping(string words)
    {
        // Each line of the result is typed in its own place, the ones before it staying.
        PixelText label = _resultTexts[words];
        label.VisibleCharacters = 0;
        label.Visible = true;
        (_typing, _typed, _typeWait, _digitsShown) = (words, 0, 1, 0);
    }

    /// <summary>Shows the next letter of the line being typed, a space coming with it, and how long it shows before the next.</summary>
    private void TypeResultLetter()
    {
        PixelText label = _resultTexts[_typing];
        string words = label.Text;
        _typed++;
        while (_typed < words.Length && char.IsWhiteSpace(words[_typed - 1]))
            _typed++;
        label.VisibleCharacters = _typed;
        bool endsWord = _typed == words.Length || char.IsWhiteSpace(words[_typed]);
        _typeWait = endsWord ? LastLetterUpdates : LetterUpdates;
    }

    /// <summary>
    /// Types the result: "the operation was", then "completed successfully" in its place, then the clear time and its
    /// figures one after another; and when that has been shown a while, the screen fades to the credits.
    /// </summary>
    private void StepCompletionText()
    {
        if (--_typeWait > 0)
            return;
        if (_typed < _resultTexts[_typing].Text.Length)
        {
            TypeResultLetter();
            return;
        }

        // A line's last letter has had its while: the next line shows its first letter on this same update.
        switch (_typing)
        {
            case "ending_operation_was":
                BeginTyping("ending_completed_successfully");
                TypeResultLetter();
                break;

            case "ending_completed_successfully":
                BeginTyping("ending_clear_time");
                TypeResultLetter();
                break;

            default:
                if (_digitsShown < FigureX.Length)
                {
                    string[] figures =
                    [
                        $"digit_{ClearHours / 10 % 10}", $"digit_{ClearHours % 10}", "colon", $"digit_{ClearMinutes / 10}", $"digit_{ClearMinutes % 10}",
                    ];
                    // A figure shows its first look on the update it is put up.
                    Spawn("clear_time_figure", figures[_digitsShown], FigureX[_digitsShown], 160, Motion.Still).Sprite.Step();
                    _digitsShown++;
                    _typeWait = _digitsShown < FigureX.Length ? FigureUpdates : ClearTimeUpdates;
                    break;
                }

                Fade().FadeOut(FadeUpdates);
                _phase = Phase.FadingOutToCredits;
                break;
        }
    }
}
