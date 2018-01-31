﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Pawn : MonoBehaviour
{
    public bool selected;
    public Vector3 offset;
    private BoardManager bm;
    public Player player;
    public Box currentBox;
    public float speed;
    // Use this for initialization
    void Start()
    {
        bm = FindObjectOfType<BoardManager>();
        selected = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMouseDown()
    {
        bm.PawnSelected(gameObject.GetComponent<Pawn>());
    }

    public void Move(int boxindex1, int boxindex2)
    {
        Transform boxToMove=currentBox.transform;
        if (player == Player.player1)
        {
            boxToMove = bm.board1[boxindex1][boxindex2];
        }
        else if (player == Player.player2)
        {
            boxToMove = bm.board2[boxindex1][boxindex2];
        }
        PawnMovement(boxindex1, boxindex2, boxToMove);
        selected = false;
    }

    private void PawnMovement(int boxindex1, int boxindex2, Transform boxToMove)
    {
        if ((boxindex1 == currentBox.index1 + 1 || boxindex1 == currentBox.index1 - 1 || boxindex1 == currentBox.index1) && (boxindex2 == currentBox.index2 || boxindex2 == currentBox.index2+1 || boxindex2 == currentBox.index2-1))
        {
            transform.LookAt(new Vector3(boxToMove.position.x, transform.position.y, boxToMove.position.z));
            transform.Rotate(new Vector3(0, 90,0));
            //transform.position = boxToMove.position + offset;
            transform.DOMove(boxToMove.position + offset, speed);
            currentBox = boxToMove.GetComponent<Box>();
        }
    }
}
