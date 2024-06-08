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
public class UpdateGameScoreEvent : SDD.Events.Event
{
    public float score { get; set; }
}

public class GamePlayStartEvent : SDD.Events.Event
{
}

public class SceneLoadedEvent : SDD.Events.Event
{
    public int scene;
}
public class UpdateScoreEvent : SDD.Events.Event
{
    public int score { get; set; }
}

#endregion

#region PlayerManager Events
public class RequestWorldGeneratorEvent : SDD.Events.Event
{
}

public class PlayerExplodedEvent : SDD.Events.Event
{
}

public class ExplosionEvent : SDD.Events.Event
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
public class SoundMixAllEvent : SDD.Events.Event
{
    public float? eSFXVolume;
    public float? eGameplayVolume;
    public float? eMenuVolume;
    public float? ePlaneVolume;
}

public class SoundMixSoundEvent : SDD.Events.Event
{
    public string eNameClip; // Check audioTypes list in AudioManager
    public float eVolume;
}

public class PlaneMixSoundEvent : SDD.Events.Event
{
    public float? eVolume;
    public float? ePitch;
}

#region player events
public class PlaySoundEvent : SDD.Events.Event
{
    public string eNameClip;
    public bool eLoop;
    public bool eCanStack;
    public bool eDestroyWhenFinished = false;
    public float ePitch = 1;
    public float eVolumeMultiplier = 1;
}

public class StopSoundEvent : SDD.Events.Event
{
    public string eNameClip;
}
public class StopSoundAllEvent : SDD.Events.Event
{ }

public class StopSoundByTypeEvent : SDD.Events.Event
{
    public string eType; // Check audioTypes list in AudioManager
}

public class MuteAllSoundEvent : SDD.Events.Event
{
    public bool eMute;
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
public class PlaneStateEvent : SDD.Events.Event
{
    public float? eBurningPercent;
    public float? eThrust;
    public bool? eIsInWater;
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

#region EnvrionnementManager Events
public class SetCloudDensityEvent : SDD.Events.Event
{
    public float eValue; // from 0 to 1 : 0 = black, 1 = full light
}

public class SetCloudCoverageEvent : SDD.Events.Event
{
    public float eValue; // from 0 to 1 : 0 = no clouds, 1 = full clouds
}

public class SetTurbulenceEvent : SDD.Events.Event
{
    public float eStrength;
    public float eScale;
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

public class ShadowMapInitializedEvent : SDD.Events.Event
{
    public ShadowMap shadowMap;
}

#endregion
public class DestroyEvent : SDD.Events.Event
{
}

#region Camera events
public class SwitchToFirstPersonEvent : SDD.Events.Event
{
}

public class SwitchToThirdPersonEvent : SDD.Events.Event
{
}

#endregion

#region Generation Events
public class WorldGeneratedEvent : SDD.Events.Event
{
    public WorldGenerator generator;
}

public class GiveWorldGeneratorEvent : SDD.Events.Event
{
    public WorldGenerator generator;
}

public class WorldConfigChangedEvent : SDD.Events.Event
{
}

#endregion