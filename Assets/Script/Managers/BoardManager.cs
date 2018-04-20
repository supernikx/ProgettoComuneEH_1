﻿using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using PawnOutlineNameSpace;


public class BoardManager : MonoBehaviour
{

    public static BoardManager Instance;

    //variabili pubbliche
    public Transform[][] board1, board2;
    public PlayerElements player1Elements, player2Elements;
    public List<Pawn> pawns;
    [HideInInspector]
    public Pawn pawnSelected;
    [HideInInspector]
    public bool movementSkipped, superAttackPressed;
    public int pawnsToPlace;
    public int p1pawns, p2pawns;
    public int p1tiles, p2tiles;
    public Box[] boxesArray;
    public int placingsLeft;

    //managers
    [Header("Managers")]
    public TurnManager turnManager;
    public DraftManager draftManager;
    public UIManager uiManager;

    public bool factionChosen;
    public int factionID = 0;

    /// <summary>
    /// Funzioni che iscrivono/disiscrivono il boardmanager agli eventi appena viene abilitato/disabilitato
    /// </summary>
    private void OnEnable()
    {
        EventManager.OnPause += OnGamePause;
        EventManager.OnUnPause += OnGameUnPause;
    }
    private void OnDisable()
    {
        EventManager.OnPause -= OnGamePause;
        EventManager.OnUnPause -= OnGameUnPause;
    }

    #region Pause

    bool pause;

    /// <summary>
    /// Funzione che imposta la variabile pause a true stoppando il gioco
    /// </summary>
    private void OnGamePause()
    {
        pause = true;
    }

    /// <summary>
    /// Funzione che imposta la variabile pause a false facendo ripartire il gioco
    /// </summary>
    private void OnGameUnPause()
    {
        pause = false;
    }

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
        draftManager = FindObjectOfType<DraftManager>();
        turnManager = FindObjectOfType<TurnManager>();
        uiManager = FindObjectOfType<UIManager>();
    }

    void Start()
    {
        placingsLeft = 1;
        pawnsToPlace = 8;
        movementSkipped = false;
        superAttackPressed = false;
        pause = false;
        pawns = FindObjectsOfType<Pawn>().ToList();
        boxesArray = FindObjectsOfType<Box>();
        int i = 0;
        foreach (Pawn pawn in pawns)
        {
            if (pawns[i].player == Player.player1)
            {
                p1pawns++;
                i++;
            }
            else if (pawns[i].player == Player.player2)
            {
                p2pawns++;
                i++;
            }
        }
    }

    #region Movement

    /// <summary>
    /// Funzione che obbliga il giocatore a muoversi durante la fase di check non deselezionando mai la pedina finchè non si è mossa in una delle caselle disponibili
    /// </summary>
    /// <param name="boxclicked"></param>
    private void Movement(Box boxclicked, bool checkphase)
    {
        if ((pawnSelected.player == Player.player1 && boxclicked.board == 1 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn) || (pawnSelected.player == Player.player2 && boxclicked.board == 2 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn))
        {
            if (CheckFreeBox(boxclicked) && pawnSelected.CheckMovementPattern(boxclicked))
            {
                if (checkphase)
                {
                    pawnSelected.OnMovementEnd += OnMovementCheckEnd;
                }
                else
                {
                    pawnSelected.OnMovementEnd += OnMovementEnd;
                }
                pawnSelected.Move(boxclicked);
            }
            else
            {
                CustomLogger.Log("Casella non valida");
            }
        }
        else
        {
            CustomLogger.Log("Casella non valida");
        }
    }

    private void OnMovementCheckEnd()
    {
        pawnSelected.OnMovementEnd -= OnMovementCheckEnd;
        CustomLogger.Log(pawnSelected.player + " si è mosso");
        pawnSelected.randomized = false;
        DeselectPawn();
        turnManager.CurrentTurnState = TurnManager.PlayTurnState.check;
    }

    private void OnMovementEnd()
    {
        CustomLogger.Log(pawnSelected.player + " si è mosso");
        pawnSelected.OnMovementEnd -= OnMovementEnd;
        pawnSelected.ShowAttackPattern();
        turnManager.CurrentTurnState = TurnManager.PlayTurnState.attack;
    }

    /// <summary>
    /// Funzione che teletrasporta la pawnselected alla box passata come paramentro se rispetta i requisiti richiesti
    /// </summary>
    /// <param name="boxclicked"></param>
    private void PlacingTeleport(Box boxclicked)
    {
        if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.placing && turnManager.CurrentTurnState == TurnManager.PlayTurnState.placing)
        {
            if (pawnSelected.player == Player.player1 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn && boxclicked.board == 1 && boxclicked.index1 == 3 && boxclicked.free)
            {
                Debug.Log(boxclicked);
                pawnSelected.gameObject.transform.position = boxclicked.gameObject.transform.position;
                pawnSelected.currentBox = boxclicked;
                pawnSelected.currentBox.free = false;
                DeselectPawn();
                pawnsToPlace--;
                placingsLeft--;
                if (placingsLeft == 0 || pawnsToPlace == 0)
                {
                    turnManager.CurrentPlayerTurn = TurnManager.PlayerTurn.P2_turn;
                    placingsLeft = 2;
                }
            }
            else if (pawnSelected.player == Player.player2 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn && boxclicked.board == 2 && boxclicked.index1 == 3 && boxclicked.free)
            {
                Debug.Log(boxclicked);
                pawnSelected.gameObject.transform.position = boxclicked.gameObject.transform.position;
                pawnSelected.currentBox = boxclicked;
                pawnSelected.currentBox.free = false;
                DeselectPawn();
                pawnsToPlace--;
                placingsLeft--;
                if (placingsLeft == 0 || pawnsToPlace == 0)
                {
                    turnManager.CurrentPlayerTurn = TurnManager.PlayerTurn.P1_turn;
                    placingsLeft = 2;
                }
            }
        }
    }

    #endregion

    //identifica la zona di codice con le funzioni pubbliche
    #region API

    public void MagicChosen()
    {
        if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.faction)
        {
            factionID = 1;
            factionChosen = true;
            turnManager.CurrentMacroPhase = TurnManager.MacroPhase.draft;
        }
    }

    public void ScienceChosen()
    {
        if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.faction)
        {
            factionID = 2;
            factionChosen = true;
            turnManager.CurrentMacroPhase = TurnManager.MacroPhase.draft;
        }
    }

    #region Attack

    /// <summary>
    /// Funzione che toglie il marchio di Kill a tutte le pedine
    /// </summary>
    public void UnmarkKillPawns()
    {
        foreach (Pawn p in pawns)
        {
            if (p.killMarker)
            {
                p.projections[p.activePattern].SetActive(false);
                //Color finalColor = Color.white * Mathf.LinearToGammaSpace(0.25f);
                //p.projections[p.activePattern].GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", finalColor);
                p.killMarker = false;
            }
        }
    }

    /// <summary>
    /// Funzione che richiama la funzione Attack della pawnselected e se avviene l'attacco passa il turno
    /// </summary>
    /// <param name="boxclicked"></param>
    public void Attack(bool superAttack)
    {
        if (pawnSelected != null && !superAttackPressed && !pause)
        {
            if (pawnSelected.CheckAttackPattern())
            {
                pawnSelected.OnAttackEnd += OnAttackEnd;
                pawnSelected.AttackBehaviour(superAttack);
            }
            else
            {
                Debug.Log("nope");
            }
        }
    }

    public void OnAttackEnd()
    {
        pawnSelected.OnAttackEnd -= OnAttackEnd;
        CustomLogger.Log(pawnSelected.player + " ha attaccato");
        turnManager.ChangeTurn();
    }

    /// <summary>
    /// Funzione che prende in input una pedina con il bool killMarker=true e la uccide
    /// </summary>
    /// <param name="pawnToKill"></param>
    public void KillPawnMarked(Pawn pawnToKill)
    {
        UnmarkKillPawns();
        pawnToKill.OnDeathEnd += pawnSelected.OnPawnKilled;
        pawnToKill.KillPawn();       
    }

    private void OnPawnKilled(Pawn pawnKilled)
    {      
        pawnKilled.OnDeathEnd -= OnPawnKilled;
        DeselectPawn();
        turnManager.CurrentTurnState = TurnManager.PlayTurnState.check;
    }

    #endregion

    #region Check

    /// <summary>
    /// Controlla se una pedina si trova su una casella non walkable la obbliga a muoversi
    /// </summary>
    public void CheckPhaseControll()
    {
        if (turnManager.CurrentTurnState == TurnManager.PlayTurnState.check)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (!pawns[i].currentBox.walkable)
                {
                    if (CheckFreeBoxes(pawns[i]))
                    {
                        CustomLogger.Log(pawns[i] + " è in casella !walkable");
                        PawnSelected(pawns[i]);
                        if (pawns[i].randomized)
                        {
                            return;
                        }
                        pawns[i].RandomizePattern();
                        return;
                    }
                    else
                    {
                        CustomLogger.Log(pawns[i] + " non ha caselle libere adiacenti");
                        turnManager.CurrentTurnState = TurnManager.PlayTurnState.animation;
                        pawns[i].OnDeathEnd += OnPawnKilled;
                        pawns[i].KillPawn();
                        return;
                    }
                }
            }
            DeselectPawn();
            turnManager.CurrentTurnState = TurnManager.PlayTurnState.movement;
        }
    }

    /// <summary>
    /// Controlla se sono presenti delle pedine da scegliere (bianche/nere) e ritorna true se ci sono o false se non ci sono
    /// </summary>
    /// <returns></returns>
    public bool CheckPawnToChoose()
    {
        bool foundPawn = false;
        foreach (Pawn p in pawns)
        {
            if (p.activePattern == 4 || p.activePattern == 5)
            {
                foundPawn = true;
            }
        }
        return foundPawn;
    }

    /// <summary>
    /// Funzione che controlla se la casella che gli è stata passata in input è già occupata da un altro player o se non è walkable
    /// se è libera ritorna true, altrimenti se è occupata ritorna false
    /// </summary>
    /// <param name="boxclicked"></param>
    /// <returns></returns>
    private bool CheckFreeBox(Box boxclicked)
    {
        if (boxclicked.walkable && boxclicked.free)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Funzione che controlla se la casella che gli è stata passata in input è già occupata da un altro player o se non è walkable
    /// se è libera ritorna true, altrimenti se è occupata ritorna false
    /// </summary>
    /// <param name="boxclicked"></param>
    /// <returns></returns>
    private bool CheckFreeBoxes(Pawn pawnToCheck)
    {
        Transform[][] boardToUse;
        Box currentBox = pawnToCheck.currentBox;
        if (pawnToCheck.player == Player.player1)
        {
            boardToUse = board1;
        }
        else
        {
            boardToUse = board2;
        }

        for (int index1 = 0; index1 < boardToUse.Length; index1++)
        {
            for (int index2 = 0; index2 < boardToUse[0].Length; index2++)
            {
                if ((index1 == currentBox.index1 + 1 || index1 == currentBox.index1 - 1 || index1 == currentBox.index1) && (index2 == currentBox.index2 || index2 == currentBox.index2 + 1 || index2 == currentBox.index2 - 1)
                    && boardToUse[index1][index2].GetComponent<Box>() != currentBox && CheckFreeBox(boardToUse[index1][index2].GetComponent<Box>()))
                {
                    return true;
                }
            }
        }
        CustomLogger.Log("Non c'è una casella libera");
        return false;
    }

    public bool CheckAllAttackPattern()
    {
        foreach (Pawn p in pawns)
        {
            if (p.CheckAttackPattern() && ((p.player == Player.player1 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn) || (p.player == Player.player2 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn)))
            {
                Debug.Log("è possibil eseguire un attacco");
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Pawn

    /// <summary>
    /// Funzione che imposta la variabile pawnSelected a null, prima reimposta il colore della pedina a quello di default e imposta a false il bool selected
    /// </summary>
    public void DeselectPawn()
    {
        if (pawnSelected != null)
        {
            if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.game)
            {
                pawnSelected.DisableMovementBoxes();
                pawnSelected.DisableAttackPattern();
                pawnSelected.ForceMoveProjection(!(turnManager.CurrentTurnState == TurnManager.PlayTurnState.movement));
            }
            pawnSelected.projections[pawnSelected.activePattern].SetActive(false);
            pawnSelected.selected = false;
            pawnSelected = null;

        }
    }

    /// <summary>
    /// Funzione che imposta nella variabile pawnSelected l'oggetto Pawn passato in input, solo se la pedina selezionata appartiene al giocatore del turno in corso e se la fase del turno e quella di movimento
    /// prima di impostarla chiama la funzione DeselectPawn per resettare l'oggetto pawnSelected precedente
    /// </summary>
    /// <param name="selected"></param>
    public void PawnSelected(Pawn selected)
    {
        if (!pause)
        {
            switch (turnManager.CurrentMacroPhase)
            {
                case TurnManager.MacroPhase.placing:
                    if (turnManager.CurrentTurnState == TurnManager.PlayTurnState.placing)
                    {
                        Debug.Log("In Macro Fase Placing");
                        if (((turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn && selected.player == Player.player1) || (turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn && selected.player == Player.player2)) && !selected.currentBox)
                        {
                            if (pawnSelected != null)
                            {
                                DeselectPawn();
                            }
                            selected.selected = true;
                            pawnSelected = selected;
                            pawnSelected.projections[pawnSelected.activePattern].SetActive(true);
                        }
                    }
                    break;
                case TurnManager.MacroPhase.game:
                    if (pawnSelected == null && turnManager.CurrentTurnState == TurnManager.PlayTurnState.check)
                    {
                        selected.selected = true;
                        pawnSelected = selected;
                        pawnSelected.projections[pawnSelected.activePattern].SetActive(true);
                        pawnSelected.ShowMovementBoxes();
                    }
                    else if ((turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn && selected.player == Player.player1 || turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn && selected.player == Player.player2) && movementSkipped && !superAttackPressed && turnManager.CurrentTurnState == TurnManager.PlayTurnState.attack)
                    {
                        if (pawnSelected != null)
                        {
                            DeselectPawn();
                        }
                        selected.selected = true;
                        pawnSelected = selected;
                        pawnSelected.projections[pawnSelected.activePattern].SetActive(true);
                        pawnSelected.ShowAttackPattern();
                    }
                    else if ((turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn && selected.player == Player.player1 || turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn && selected.player == Player.player2) && turnManager.CurrentTurnState == TurnManager.PlayTurnState.movement)
                    {
                        if (pawnSelected != null)
                        {
                            DeselectPawn();
                        }
                        selected.selected = true;
                        pawnSelected = selected;
                        pawnSelected.projections[pawnSelected.activePattern].SetActive(true);
                        pawnSelected.ShowAttackPattern();
                        pawnSelected.ShowMovementBoxes();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Funzione che setta il pattern delle pedine a seconda della scelta fatta nella fase di draft
    /// </summary>
    public void SetPawnsPattern()
    {
        int j = 0;
        int k = 0;
        for (int i = 0; i < pawns.Count; i++)
        {
            if (pawns[i].player == Player.player1)
            {
                pawns[i].ChangePattern(draftManager.p1_pawns_picks[j]);
                j++;
            }
            else if (pawns[i].player == Player.player2)
            {
                pawns[i].ChangePattern(draftManager.p2_pawns_picks[k]);
                k++;
            }
        }
    }

    /// <summary>
    /// Imposta la pedina di cui bisogna scegliere il pattern in base al turno del giocatore
    /// </summary>
    public void SetPawnToChoose()
    {
        bool foundPawn = false;
        if (turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn)
        {
            foreach (Pawn p in pawns)
            {
                if ((p.activePattern == 4 || p.activePattern == 5) && p.player == Player.player1)
                {
                    pawnSelected = p;
                    pawnSelected.projections[pawnSelected.activePattern].SetActive(true);
                    foundPawn = true;
                    CustomLogger.Log("trovata una nel p1");
                    break;
                }
            }
        }
        else if (turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn)
        {
            foreach (Pawn p in pawns)
            {
                if ((p.activePattern == 4 || p.activePattern == 5) && p.player == Player.player2)
                {
                    pawnSelected = p;
                    pawnSelected.projections[pawnSelected.activePattern].SetActive(true);
                    foundPawn = true;
                    CustomLogger.Log("trovata una nel p2");
                    break;
                }
            }
        }
        if (foundPawn)
            return;
        turnManager.ChangeTurn();
        CustomLogger.Log("Cambio turno");
    }

    /// <summary>
    /// Funzione che imposta il pattern della selectedPawn con il valore passato in input (usata quando viene premuto il pulsante del rispettivo colore)
    /// </summary>
    /// <param name="patternIndex"></param>
    public void ChoosePawnPattern(int patternIndex)
    {
        pawnSelected.ChangePattern(patternIndex);
        if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.placing)
        {
            DeselectPawn();
            turnManager.ChangeTurn();
        }
        else if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.game)
        {
            uiManager.choosingUi.SetActive(false);
            turnManager.CurrentTurnState = TurnManager.PlayTurnState.check;
        }
    }

    #endregion

    /// <summary>
    /// Funzione che salta la fase d'attacco del player corrente e passa il turno
    /// </summary>
    public void ButtonFunctions()
    {
        if (!pause)
        {
            switch (turnManager.CurrentTurnState)
            {
                case TurnManager.PlayTurnState.movement:
                    movementSkipped = true;
                    if (pawnSelected != null)
                    {
                        pawnSelected.MoveProjection(pawnSelected.currentBox);
                        pawnSelected.DisableMovementBoxes();
                    }
                    turnManager.CurrentTurnState = TurnManager.PlayTurnState.attack;
                    CustomLogger.Log("Hai saltato il movimento");
                    break;
                case TurnManager.PlayTurnState.attack:
                    turnManager.ChangeTurn();
                    CustomLogger.Log("Hai saltato l'attacco");
                    break;
                default:
                    break;
            }
        }
    }

    public void BoxOver(Box boxover)
    {
        if ((turnManager.CurrentTurnState == TurnManager.PlayTurnState.check || turnManager.CurrentTurnState == TurnManager.PlayTurnState.movement) && pawnSelected != null)
        {
            if ((pawnSelected.player == Player.player1 && boxover.board == 1 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P1_turn) || (pawnSelected.player == Player.player2 && boxover.board == 2 && turnManager.CurrentPlayerTurn == TurnManager.PlayerTurn.P2_turn))
            {
                if (!pawnSelected.MoveProjection(boxover))
                {
                    pawnSelected.MoveProjection(pawnSelected.currentBox);
                }
            }
        }
    }

    /// <summary>
    /// Funzione che viene chiamata quando si clicca una casella e la si riceve in input, si controlla che fase del turno è e si passano le informazioni della casella alle funzioni interessate
    /// </summary>
    /// <param name="boxclicked"></param>
    public void BoxClicked(Box boxclicked)
    {
        if (pawnSelected != null && !pause)
        {
            if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.placing)
            {
                if (turnManager.CurrentTurnState == TurnManager.PlayTurnState.placing)
                {
                    PlacingTeleport(boxclicked);
                }
            }
            else if (turnManager.CurrentMacroPhase == TurnManager.MacroPhase.game)
            {
                switch (turnManager.CurrentTurnState)
                {
                    case TurnManager.PlayTurnState.choosing:
                        CustomLogger.Log("Devi prima scegliere il pattern");
                        break;
                    case TurnManager.PlayTurnState.animation:
                        Debug.Log("Animazione in corso");
                        break;
                    case TurnManager.PlayTurnState.check:
                        Movement(boxclicked,true);
                        break;
                    case TurnManager.PlayTurnState.movement:
                        Movement(boxclicked,false);
                        break;
                    case TurnManager.PlayTurnState.attack:
                        CustomLogger.Log("Clicca il pulsante Attack se c'è una pedina in range");
                        break;
                    default:
                        DeselectPawn();
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Funzione che gestisce le condizioni di vittoria
    /// </summary>
    public void WinCondition()
    {

        if (p1pawns > p2pawns)
        {
            uiManager.winScreen.SetActive(true);
            uiManager.gameResult.text = "Magic wins by having more pawns! \n" + "The game ended in " + turnManager.numberOfTurns + " turns.";
        }
        else if (p2pawns > p1pawns)
        {
            uiManager.winScreen.SetActive(true);
            uiManager.gameResult.text = "Science wins by having more pawns! \n" + "The game ended in " + turnManager.numberOfTurns + " turns.";
        }
        else if (p1pawns == p2pawns)
        {
            foreach (Box box in boxesArray)
            {
                if (box.board == 1)
                {
                    p1tiles++;
                }
                else if (box.board == 2)
                {
                    p2tiles++;
                }
            }


            if (p1tiles > p2tiles)
            {
                uiManager.winScreen.SetActive(true);
                uiManager.gameResult.text = "Magic wins by destroying more tiles! \n" + "The game ended in " + turnManager.numberOfTurns + " turns.";
            }
            else if (p2tiles > p1tiles)
            {
                uiManager.winScreen.SetActive(true);
                uiManager.gameResult.text = "Science wins by destroying more tiles! \n" + "The game ended in " + turnManager.numberOfTurns + " turns.";
            }
            else if (p1tiles == p2tiles)
            {
                uiManager.winScreen.SetActive(true);
                uiManager.gameResult.text = "DRAW! Both players had the same amount of pawns and destroyed the same amount of tiles! \n" + "The game ended in " + turnManager.numberOfTurns + " turns.";
            }
        }
    }

    #endregion
}

//enumeratore che contiene i player possibili
public enum Player
{
    player1, player2
}
