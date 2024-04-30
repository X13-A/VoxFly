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
public class GameStatisticsChangedEvent : SDD.Events.Event
{
	public int eScore { get; set; }
	public float eCountDown { get; set; }
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
    public float eBurningRate; // in % from 0 to 100
	public float eAltitude;
}
#endregion

public class WinningPointsEvent : SDD.Events.Event
{
    public float ePoints;
}