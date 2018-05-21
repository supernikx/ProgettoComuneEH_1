﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public bool SoundActive;
    public AudioSource GeneralAudioSource;

    [Header("Clips Menu")]
    public AudioClip buttonselection;
    public AudioClip selectnextbutton;
    public AudioClip togglebutton;
    public AudioClip factionslected;

    private void Start()
    {
        SoundActive = true;
    }

    public void ActiveDeactiveSound()
    {
        SoundActive = !SoundActive;
    }

    public void ButtonSelection()
    {
        if (SoundActive)
        {
            if (buttonselection != null)
            {
                GeneralAudioSource.clip = buttonselection;
                GeneralAudioSource.Play();
            }
        }
    }

    public void SelectNextButton()
    {
        if (SoundActive)
        {
            if (selectnextbutton != null)
            {
                GeneralAudioSource.clip = selectnextbutton;
                GeneralAudioSource.Play();
            }
        }
    }

    public void ToggleButton()
    {
        if (SoundActive)
        {
            if (togglebutton != null)
            {
                GeneralAudioSource.clip = togglebutton;
                GeneralAudioSource.Play();
            }
        }
    }

    public void FactionSelected()
    {
        if (SoundActive)
        {
            if (factionslected != null)
            {
                GeneralAudioSource.clip = factionslected;
                GeneralAudioSource.Play();
            }
        }
    }
}
