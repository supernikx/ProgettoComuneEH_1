﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    #region PauseEvent
    public delegate void PauseEvent();
    public static PauseEvent OnPause;
    public static PauseEvent OnUnPause;
    #endregion

    #region GameEvent
    public delegate void GameEvent();
    public static GameEvent OnGameEnd;
    #endregion

    #region Joystickevent
    public delegate void JoystickEvent();
    public static JoystickEvent OnJoystickDisconnected;
    public static JoystickEvent OnJoystickRiconnected;
    #endregion

}
