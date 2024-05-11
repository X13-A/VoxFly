using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

#region GameManager Events
public class GameMenuEvent : SDD.Events.Event
{
}
public class GamePlayEvent : SDD.Events.Event
{
}
public class GamePauseEvent : SDD.Events.Event
{
}
public class GameResumeEvent : SDD.Events.Event
{
}
public class GameOverEvent : SDD.Events.Event
{
}
public class GameVictoryEvent : SDD.Events.Event
{
}
public class GameSettingsEvent : SDD.Events.Event
{
}
public class GameScoreEvent : SDD.Events.Event
{
	public float score { get; set; }
}
public class GameStatisticsChangedEvent : SDD.Events.Event
{
	public int eScore { get; set; }
	public float eCountDown { get; set; }
}
public class DisablePlayerEvent : SDD.Events.Event
{
}
public class UpdateGameScoreEvent : SDD.Events.Event
{
    public float score { get; set; }
}

public class GamePlayStartEvent : SDD.Events.Event
{
}

#endregion

#region PlayerManager Events
public class PlayerWorldGeneratorEvent : SDD.Events.Event
{
}
#endregion

#region MenuManager Events
public class EscapeButtonClickedEvent : SDD.Events.Event
{
}
public class PlayButtonClickedEvent : SDD.Events.Event
{
}
public class ReplayButtonClickedEvent : SDD.Events.Event
{
}
public class ResumeButtonClickedEvent : SDD.Events.Event
{
}
public class MainMenuButtonClickedEvent : SDD.Events.Event
{
}
public class NextLevelButtonClickedEvent : SDD.Events.Event
{
}
public class QuitButtonClickedEvent: SDD.Events.Event
{
}
public class SettingsButtonClickedEvent : SDD.Events.Event
{
}
public class ScoreButtonClickedEvent : SDD.Events.Event
{
}
#endregion

#region ScoreManager Events
public class UpdateScoreEvent : SDD.Events.Event
{
	public int score;
    public UpdateScoreEvent(int score)
    {
        this.score = score;
    }
}
public class UpdateScoresTextEvent : SDD.Events.Event
{
}
#endregion

#region PauseManager Events
public class PauseButtonClickedEvent : SDD.Events.Event
{
}
public class PausePlayerEvent : SDD.Events.Event
{
}
public class ResumePlayerEvent : SDD.Events.Event
{
}
#endregion

#region BeginTimer Events
public class FinishTimerEvent : SDD.Events.Event
{
}
#endregion

#region AudioManager Events
public class SoundMixEvent : SDD.Events.Event
{
    public float eSFXVolume;
    public float eGameplayVolume;
    public float eMenuVolume;
}

#region player events
public class PlaySoundEvent : SDD.Events.Event
{
    public string eNameClip;
    public bool eLoop;
}

public class StopSoundEvent : SDD.Events.Event
{
    public string eNameClip;
}
public class StopSoundAllEvent : SDD.Events.Event
{
    public bool eMute;
}

public class StopSoundByTypeEvent : SDD.Events.Event
{
    public string eType; // Check audioTypes list in AudioManager
}
#endregion
#endregion

#region Enemy Event
public class EnemyHasBeenHitEvent : SDD.Events.Event
{
	public GameObject eEnemy;
}
#endregion

#region Plane Event
public class PlaneIsInShadowEvent : SDD.Events.Event
{
	public bool eIsInShadow;
	public float eRayRate;
}

public class PlaneStateEvent : SDD.Events.Event
{
    public float eBurningPercent; // in % from 0 to 100
    public float eThrust;
}
public class PlaneInformationEvent : SDD.Events.Event
{
    public float eMinThrust;
    public float eMaxThrust;
}
public class PlaneInitializedEvent : SDD.Events.Event
{
    public plane plane;
}
#endregion

public class WinningPointsEvent : SDD.Events.Event
{
    public float ePoints;
}

#region Rendering Events
// Called every frame before the custom post processing starts
public class StartPostProcessingEvent : SDD.Events.Event
{
}

public class GBufferInitializedEvent : SDD.Events.Event
{
    public GBuffer gbuffer;
}
public class WorldGeneratedEvent : SDD.Events.Event
{
    public WorldGenerator generator;
}

public class GiveWorldGeneratorEvent : SDD.Events.Event
{
    public WorldGenerator generator;
}

public class ShadowMapInitializedEvent : SDD.Events.Event
{
    public ShadowMap shadowMap;
}

#endregion
public class DestroyEvent : SDD.Events.Event
{
}
