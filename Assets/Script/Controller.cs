using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Script
{
    class Controller : MonoBehaviour
    {
        public bool playerTurn;

        private char playerChar = 'X';

        private char botChar = 'O';

        private char emptyChar = ' ';

        public static Controller controller;

        public event Action<string> onGameOver;

        private bool gameOver;
        [SerializeField]
        private int finalResult=-999;

        private int availableMove = 0;

        public GameObject x_Obj;

        public GameObject y_Obj;

        public char[,] arrBoard = new char[5, 5];

        public PieceBoard[,] arrPiece = new PieceBoard[5, 5];

        public enum DirectionPoint { TopLeft, TopRight, BotLeft, BotRight, MiddleRow,MiddleColumn,Central,None }
        private void Start()
        {
            playerTurn = true;
            int count = 0;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
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

                        //CheckWin(piece);

                        //Check 5x5
                        var point = CheckCurrentPoint(piece);
                        CheckWinV2(point, piece);

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
                // CheckWin(selectPiece);

                //Check 5x5
                var point = CheckCurrentPoint(selectPiece);
                CheckWinV2(point, selectPiece);

                LastResult(finalResult);
                playerTurn = true;
            }
        }

        private void LastResult(int check)
        {
           
            if (check == 1)
            {
                Debug.Log("Win");
                gameOver = true;
                onGameOver?.Invoke("Someone Won!");
            }
            else if (check == 0)
            {
                Debug.Log("Tie");
                gameOver = true;
                onGameOver?.Invoke("Tie!");
            }
        }

        private void ChooseMove(out int _x, out int _y)
        {
            _x = 0;
            _y = 0;
            while (arrPiece[_x, _y].hasChar)
            {
                _x = UnityEngine.Random.Range(0, 5);
                _y = UnityEngine.Random.Range(0, 5);
            }
            arrPiece[_x, _y].hasChar = true;

            //for (int x = 0; x < 5; x++)
            //{
            //    for (int y = 0; y < 5; y++)
            //        if (!arrPiece[x, y].hasChar)
            //        {
            //            arrPiece[x, y].hasChar = true;
            //            _x = x;
            //            _y = y;
            //            return;
            //        }
            //}
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

        private DirectionPoint CheckCurrentPoint(PieceBoard start)
        {
            if (start.X < 2 && start.Y < 2)
            {
                return DirectionPoint.BotLeft;
            }
            else if (start.X < 2 && start.Y > 2)
            {
                return DirectionPoint.TopLeft;
            }
            else if (start.X > 2 && start.Y < 2)
            {
                return DirectionPoint.BotRight;
            }
            else if (start.X > 2 && start.Y > 2)
            {
                return DirectionPoint.TopRight;
            }
            else if (start.Y == 2)
            {
                if (start.X != 2)
                    return DirectionPoint.MiddleRow;
                else return DirectionPoint.Central;
            }
            else if (start.X == 2)
            {
                if (start.Y != 2)
                    return DirectionPoint.MiddleColumn;
                else return DirectionPoint.Central;
            }
            else return DirectionPoint.None;
        }

        private void CheckWinV2(DirectionPoint point,PieceBoard piece)
        {
            if (availableMove == 25)
                return;

            switch(point)
            {
                case DirectionPoint.BotLeft:
                case DirectionPoint.TopRight:
                    {
                        int count;
                        count = ColumnAndRow(piece);
                        if (count == 5)
                        {
                            finalResult = 1;
                            return;
                        }
                        //right diagonal
                        for (int d = 0; d < 5; d++)
                        {
                            if (arrBoard[piece.X, piece.Y] == arrBoard[d, d])
                            {
                                count++;
                            }
                            else
                            {
                                count = 0;
                                break;
                            }
                        }

                        if (count == 5)
                        {
                            finalResult= 1;
                        }
                        else finalResult= - 999;

                        return;
                    }
                case DirectionPoint.TopLeft:
                case DirectionPoint.BotRight:
                    {
                        int count;
                        count = ColumnAndRow(piece);
                        if (count == 5)
                        {
                            finalResult = 1;
                             return;
                        }

                        //left diagonal
                        int tmp=4;
                        for (int d = 0; d < 5; d++)
                        {
                            if (arrBoard[piece.X, piece.Y] == arrBoard[d, tmp])
                            {
                                tmp--;
                                count++;
                            }
                            else
                            {
                                count = 0;
                                break;
                            }
                        }

                        if (count == 5)
                        {
                            finalResult= 1;
                        }
                        else finalResult= - 999;
                        break;

                    }
                case DirectionPoint.Central:
                    {
                        int count = ColumnAndRow(piece);
                        if (count == 5)
                        {
                            finalResult= 1;
                        }
                        else finalResult= - 999;
                        break;
                    }
                case DirectionPoint.MiddleColumn:
                    {
                        int count = ColumnAndRow(piece);
                        if (count == 5)
                        {
                            finalResult= 1;
                        }
                        else finalResult= - 999;
                        break;
                    }
                case DirectionPoint.MiddleRow:
                    {
                        int count = ColumnAndRow(piece);
                        if (count == 5)
                        {
                            finalResult= 1;
                        }
                        else finalResult= - 999;
                        break;
                    }
            }

            return;
        }
    private int ColumnAndRow(PieceBoard piece)
        {
            //vertical
            int count = 0;
            for (int y = 0; y < 5; y++)
            {
                if (arrBoard[piece.X, piece.Y] == arrBoard[piece.X, y])
                {
                    count++;
                }
                else
                {
                    count = 0;
                    break;
                }
            }
            if (count == 5)
                return count;

            //horizontal
            for (int x = 0; x < 5; x++)
            {
                if (arrBoard[piece.X, piece.Y] == arrBoard[x, piece.Y])
                {
                    count++;
                }
                else
                {
                    count = 0;
                    break;
                }
            }
            return count;
        }
        public void RestartGame()
        {
            SceneManager.LoadScene(0);
        }
    }
}
