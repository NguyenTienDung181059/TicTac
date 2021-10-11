﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{
    class Controller : MonoBehaviour
    {
        public bool playerTurn;

        private char playerChar = 'X';

        private char botChar = 'O';

        private char emptyChar = ' ';

        public static Controller controller;

        private bool gameOver;

        private int finalResult=-999;

        private int availableMove = 0;

        public GameObject x_Obj;

        public GameObject y_Obj;

        public char[,] arrBoard = new char[4, 4];

        public PieceBoard[,] arrPiece = new PieceBoard[4, 4];


        private void Start()
        {
            playerTurn = true;
            int count = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    arrBoard[i, j] = emptyChar;
                    arrPiece[i, j] = transform.GetChild(count).GetComponent<PieceBoard>();
                    count++;
                }
            }
        }

        private void Awake()
        {
            controller = this;
        }

        private void Update()
        {
            if (playerTurn && !gameOver)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var piece = CheckRayscat();
                    if (piece != null)
                    {
                        SetUpMove(piece.X, piece.Y);
                        SpawmCaroChar(piece);

                        CheckWin(piece);
                        LastResult(finalResult);
                        playerTurn = false;

                        BotMove();
                    }
                }
            }
        }

        private void SpawmCaroChar(PieceBoard piece)
        {
            piece.SetUpNewChar(playerTurn);
        }
        private PieceBoard CheckRayscat()
        {
            RaycastHit2D raycastHit2D;
            raycastHit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.one);

            if (raycastHit2D.collider != null)
            {
                PieceBoard piece = raycastHit2D.transform.GetComponent<PieceBoard>();
                if (!piece.hasChar)
                    return piece;
                else return null;
            }
            else return null;
        }

        public void SetUpMove(int _x, int _y)
        {
            if (playerTurn)
            {
                arrBoard[_x, _y] = playerChar;
            }
            else
            {
                arrBoard[_x, _y] = botChar;
            }

        }

        private void BotMove()
        {
            if (!gameOver)
            {
                int _x;
                int _y;
                ChooseMove(out _x, out _y);

                PieceBoard selectPiece = arrPiece[_x, _y];
                SetUpMove(selectPiece.X, selectPiece.Y);
                SpawmCaroChar(selectPiece);
                CheckWin(selectPiece);

                int check = finalResult;
                LastResult(check);
                playerTurn = true;
            }
        }

        private void LastResult(int check)
        {
           
            if (check == 1)
            {
                Debug.Log("Win");
                gameOver = true;
            }
            else if (check == 0)
            {
                Debug.Log("Tie");
                gameOver = true;
            }
        }

        private void ChooseMove(out int _x, out int _y)
        {
            _x = 0;
            _y = 0;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                    if (!arrPiece[x, y].hasChar)
                    {
                        arrPiece[x, y].hasChar = true;
                        _x = x;
                        _y = y;
                        return;
                    }
            }
        }

        public void CheckWin(PieceBoard curPiece)
        {
            for (int y = 0; y < 3; y++)
            {
                if (arrBoard[y, 0] == arrBoard[y, 1] && arrBoard[y, 0] == arrBoard[y, 2])
                {
                    if (arrBoard[y, 0] == playerChar)
                    {
                        Debug.Log("Doc");
                        finalResult = 1;
                    }
                    else if (arrBoard[y, 0] == botChar)
                    {
                        finalResult = -1;
                    }
                }
            }
            //Ngang
            for (int x = 0; x < 3; x++)
            {
                if (arrBoard[0, x] == arrBoard[1, x] && arrBoard[0, x] == arrBoard[2, x])
                {
                    if (arrBoard[0, x] == playerChar)
                    {
                        Debug.Log("Ngang");
                        finalResult = 1;
                    }
                    else if (arrBoard[0, x] == botChar)
                    {
                        finalResult = -1;
                    }
                }
            }

            //Cheo
            if (arrBoard[0, 0] == arrBoard[1, 1] && arrBoard[0, 0] == arrBoard[2, 2])
            {
                Debug.Log("Cheo1");
                if (arrBoard[1, 1] == playerChar)
                {
                    
                    finalResult = 1;
                }
                else if (arrBoard[1, 1] == botChar)
                {
                    finalResult = -1;
                }
            }

            if (arrBoard[0, 2] == arrBoard[1, 1] && arrBoard[0, 2] == arrBoard[2, 0])
            { Debug.Log("Cheo2");
                if (arrBoard[1, 1] == playerChar)
                {
                   
                    finalResult = 1;
                }
                else if (arrBoard[1, 1] == botChar)
                {
                    finalResult = -1;
                }
            }
            //Hoa
            if (availableMove == 9)
            {
                Debug.Log("Tie");
                finalResult = 0;
            }
            else return;

        }

        private char CheckCurrentPoint(Vector2 dir, PieceBoard start)
        {
            if (dir == Vector2.one)
            {
                if (arrBoard[start.X, start.Y] == arrBoard[start.X - 1, start.Y - 1] && arrBoard[start.X, start.Y] == arrBoard[start.X + 1, start.Y + 1])
                {
                    return arrBoard[start.X, start.Y];
                }
                else return emptyChar;
            }
            else if (dir == -Vector2.one)
            {
                if (arrBoard[start.X, start.Y] == arrBoard[start.X + 2, start.Y + 2] && arrBoard[start.X, start.Y] == arrBoard[start.X + 1, start.Y + 1])
                {
                    return arrBoard[start.X, start.Y];
                }
                else return emptyChar;
            }

            return emptyChar;
        }

        private Vector2 CheckDirection(int dir1X, int dir1Y, int dir2X, int dir2Y)
        {
            int disX = dir2X - dir1X;
            int disY = dir2X - dir1Y;
            if (disX == 1 || disY == 1)
            {
                return new Vector2(dir2X - dir1X, dir2Y - dir1Y);
            }
            else return new Vector2(0, 0);
        }
    }
}
