using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Script
{
    class Controller : MonoBehaviour
    {
        public bool playerTurn;

        private const int Inf = 252525;

        private char playerChar = 'X';

        private char botChar = 'O';

        public int maxDepth;

        private char emptyChar = ' ';
        [SerializeField]
        private PieceBoard lastPiece;

        public static Controller controller;

        public event Action<string> onGameOver;
        public event Action onGameRestart;
        public bool gameOver;
        [SerializeField]
        private int finalResult = -Inf;
        [SerializeField]
        private int availableMove = 0;

        public GameObject x_Obj;

        public GameObject y_Obj;

        public char[,] arrBoard = new char[5, 5];

        public PieceBoard[,] arrPiece = new PieceBoard[5, 5];

        private static int aiPoint;
        private static int humanPoint;
        [SerializeField]
        private Text aiText;
        [SerializeField]
        private Text humanText; 
        public enum DirectionPoint { TopLeft, TopRight, BotLeft, BotRight, MiddleRow, MiddleColumn, Central, None }
        private void Start()
        {
            playerTurn = true;
            int count = 0;
            gameOver = true;
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
                    HandleRayscat();
                }
            }
          //  TEST();
        }
        private void HandleRayscat()
        {
            var piece = CheckRayscat();
            if (piece != null)
            {
                SetUpMove(piece.X, piece.Y);
                SpawmCaroChar(piece);

                //CheckWin(piece);

                //Check 5x5
                var point = CheckCurrentPoint(piece);
                //  CheckWinV2(point, piece);

                
                LastResult(CheckWin());
                playerTurn = false;

                if (!gameOver)
                    AIMovement();
                    //BotMove();

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
            availableMove++;
        }

        private void BotMove()
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
                //CheckWinV2(point, selectPiece);
                LastResult(CheckWin());
                playerTurn = true;
        }
        private void AIMovement()
        {
            int _x = Inf;
            int _y = Inf;

            if (availableMove <= 1)
            {
                ChooseMove(out _x, out _y);
            }
            else
            {
                int bestScore = -Inf;
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        if (arrBoard[x, y] == emptyChar)
                        {
                            arrBoard[x, y] = botChar;
                            availableMove++;
                            int score = Minimax(arrBoard, 0, false, x, y);
                            arrBoard[x, y] = emptyChar;
                            availableMove--;
                            if (score > bestScore)
                            {
                                bestScore = score;
                                _x = x;
                                _y = y;
                            }
                        }
                    }
                }
            }
            arrBoard[_x, _y] = botChar;
            PieceBoard selectPiece = arrPiece[_x, _y];
            SetUpMove(_x, _y);
            SpawmCaroChar(selectPiece);
         
            LastResult(CheckWin());
            playerTurn = true;
        }
        private int Minimax(char[,] checkBoard, int depth, bool isMaximum, int horizon, int vertical)
        {
            //  PieceBoard piece= new PieceBoard(horizon,vertical);
            // CheckWinV2(CheckCurrentPoint(piece), piece);
            if (depth == maxDepth)
            {
                if (isMaximum)
                   return 1;
                else return -1;
            }

            int tmp = CheckWin();
            if (tmp == 0)
            {
                return tmp; 
            }
            else if (tmp == 1)
            {
                return -10;
            }
            else if (tmp == -1)
            {
                return 10 - depth;
            }
          //  Debug.Log("depth " + depth);

            if (isMaximum)
            {
                int bestScore = -Inf;
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        if (checkBoard[x, y] == emptyChar)
                        {
                            checkBoard[x, y] = botChar;
                            availableMove++;
                            depth++;
                            int curScore = Minimax(checkBoard, depth, false, x, y);
                            depth--;
                            checkBoard[x, y] = emptyChar;
                            availableMove--;
                            bestScore = Math.Max(bestScore, curScore);
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = Inf;
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        if (checkBoard[x, y] == emptyChar)
                        {
                            checkBoard[x, y] = playerChar;
                            availableMove++;
                            depth++;
                            int curScore = Minimax(checkBoard, depth, true, x, y);
                            depth--;
                            checkBoard[x, y] = emptyChar;
                            availableMove--;
                            bestScore = Math.Min(bestScore, curScore);
                        }
                    }
                }
                return bestScore;
            }

        }
        private void LastResult(int check)
        {

            if (check == 1)
            {
                gameOver = true;
                humanPoint++;
                humanText.text = "YOU: " + humanPoint.ToString();
                onGameOver?.Invoke("Human Won!");
            }
            else if (check == 0)
            {
                gameOver = true;
                onGameOver?.Invoke("Tie!");
            }
            else if (check == -1)
            {
                gameOver = true;
                aiPoint++;
                aiText.text = "AI: " + aiPoint.ToString();
                onGameOver?.Invoke("AI Won!");
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

        public int CheckWin()
        {
            // && arrBoard[y, 0]== arrBoard[y, 5] && arrBoard[y, 0]== arrBoard[y, 4]
            for (int y = 0; y < 5; y++)
            {
                if (arrBoard[y, 0] == arrBoard[y, 1] && arrBoard[y, 0] == arrBoard[y, 2]
                   && arrBoard[y, 0] == arrBoard[y, 3] && arrBoard[y, 0] == arrBoard[y, 4])
                {
                    if (arrBoard[y, 0] == playerChar)
                    {
                        return 1;
                    }
                    else if (arrBoard[y, 0] == botChar)
                    {
                        return -1;
                    }
                }
            }
            //Ngang
            // && arrBoard[0, x] == arrBoard[5, x] && arrBoard[0, x] == arrBoard[4, x]
            for (int x = 0; x < 5; x++)
            {
                if (arrBoard[0, x] == arrBoard[1, x] && arrBoard[0, x] == arrBoard[2, x]
                  && arrBoard[0, x] == arrBoard[3, x] && arrBoard[0, x] == arrBoard[4, x])
                {
                    if (arrBoard[0, x] == playerChar)
                    {
                        return 1;
                    }
                    else if (arrBoard[0, x] == botChar)
                    {
                        return -1;
                    }
                }
            }
            // && arrBoard[0, 0]== arrBoard[5, 5] && arrBoard[0, 0]== arrBoard[4, 4]
            //Cheo
            if (arrBoard[0, 0] == arrBoard[1, 1] && arrBoard[0, 0] == arrBoard[2, 2]
               && arrBoard[0, 0] == arrBoard[3, 3] && arrBoard[0, 0] == arrBoard[4, 4])
            {
                if (arrBoard[1, 1] == playerChar)
                {

                    return 1;
                }
                else if (arrBoard[1, 1] == botChar)
                {
                    return -1;
                }
            }

            //arrBoard[0, 2] == arrBoard[1, 1] && arrBoard[0, 2] == arrBoard[2, 0]
            if (arrBoard[0, 4] == arrBoard[1, 3] && arrBoard[0, 4] == arrBoard[2, 2]
                && arrBoard[0, 4] == arrBoard[3, 1] && arrBoard[0, 4] == arrBoard[4, 0])
            {
                if (arrBoard[2, 2] == playerChar)
                {

                    return 1;
                }
                else if (arrBoard[2, 2] == botChar)
                {
                    return -1;
                }
            }
            //Hoa
            if (availableMove == 25)
            {
                return 0;
            }
            else
            {
                return Inf;
            }
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

        //private void CheckWinV2(DirectionPoint point,PieceBoard piece)
        //{
        //    switch(point)
        //    {
        //        case DirectionPoint.BotLeft:
        //        case DirectionPoint.TopRight:
        //            {
        //                int count;
        //                count = ColumnAndRow(piece);
        //                if (count == 5)
        //                {
        //                    finalResult = 1;
        //                    return;
        //                }
        //                //right diagonal
        //                for (int d = 0; d < 5; d++)
        //                {
        //                    if (arrBoard[piece.X, piece.Y] == arrBoard[d, d])
        //                    {
        //                        count++;
        //                    }
        //                    else
        //                    {
        //                        count = 0;
        //                        break;
        //                    }
        //                }

        //                if (count == 5)
        //                {
        //                    finalResult= 1;
        //                }
        //                else finalResult= - 55;

        //                break;
        //            }
        //        case DirectionPoint.TopLeft:
        //        case DirectionPoint.BotRight:
        //            {
        //                int count;
        //                count = ColumnAndRow(piece);
        //                if (count == 5)
        //                {
        //                    finalResult = 1;
        //                     return;
        //                }

        //                //left diagonal
        //                int tmp=4;
        //                for (int d = 0; d < 5; d++)
        //                {
        //                    if (arrBoard[piece.X, piece.Y] == arrBoard[d, tmp])
        //                    {
        //                        tmp--;
        //                        count++;
        //                    }
        //                    else
        //                    {
        //                        count = 0;
        //                        break;
        //                    }
        //                }

        //                if (count == 5)
        //                {
        //                    finalResult= 1;
        //                }
        //                else finalResult= - 66;
        //                break;

        //            }
        //        case DirectionPoint.Central:
        //            {
        //                int count = ColumnAndRow(piece);
        //                if (count == 5)
        //                {
        //                    finalResult= 1;
        //                }
        //                else finalResult= - 77;
        //                break;
        //            }
        //        case DirectionPoint.MiddleColumn:
        //            {
        //                int count = ColumnAndRow(piece);
        //                if (count == 5)
        //                {
        //                    finalResult= 1;
        //                }
        //                else finalResult= - 88;
        //                break;
        //            }
        //        case DirectionPoint.MiddleRow:
        //            {
        //                int count = ColumnAndRow(piece);
        //                if (count == 5)
        //                {
        //                    finalResult= 1;
        //                }
        //                else finalResult= - 2525;
        //                break;
        //            }
        //    }
        //    if (availableMove == 25)
        //    {
        //        finalResult = 0;
        //        return;
        //    }

        //}
        //private int ColumnAndRow(PieceBoard piece)
        //{
        //    //vertical
        //    int count = 0;
        //    for (int y = 0; y < 5; y++)
        //    {
        //        if (arrBoard[piece.X, piece.Y] == arrBoard[piece.X, y])
        //        {
        //            count++;
        //        }
        //        else
        //        {
        //            count = 0;
        //            break;
        //        }
        //    }
        //    if (count == 5)
        //        return count;

        //    //horizontal
        //    for (int x = 0; x < 5; x++)
        //    {
        //        if (arrBoard[piece.X, piece.Y] == arrBoard[x, piece.Y])
        //        {
        //            count++;
        //        }
        //        else
        //        {
        //            count = 0;
        //            break;
        //        }
        //    }
        //    return count;
        //}
        public void RestartGame()
        {
            SceneManager.LoadScene(0);
        }
    }
}
