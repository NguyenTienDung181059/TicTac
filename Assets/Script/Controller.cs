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

        private int aiPoint;
        private int humanPoint;
        [SerializeField]
        private Text aiText;
        [SerializeField]
        private Text humanText;
        public enum DirectionPoint { TopLeft, TopRight, BotLeft, BotRight, MiddleRow, MiddleColumn, Central, None }
        private void Start()
        {
            aiPoint = PlayerPrefs.GetInt("AI");
            humanPoint = PlayerPrefs.GetInt("HUMAN");
            aiText.text = "AI: " + aiPoint.ToString();
            humanText.text = "YOU: " + humanPoint.ToString();
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

                //  CheckWinV2(point, piece);


                LastResult(CheckWin(true));
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
            //   var point = CheckCurrentPoint(selectPiece);
            //CheckWinV2(point, selectPiece);
            LastResult(CheckWin(true));
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
                            int score = Minimax(arrBoard, 0, false);
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

            LastResult(CheckWin(true));
            playerTurn = true;
        }
        private int Minimax(char[,] checkBoard, int depth, bool isMaximum)
        {
            //  PieceBoard piece= new PieceBoard(horizon,vertical);
            // CheckWinV2(CheckCurrentPoint(piece), piece);
            if (depth == maxDepth)
            {
                if (isMaximum)
                    return 1;
                else return -1;
            }

            int tmp = CheckWin(false);
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
                            int curScore = Minimax(checkBoard, depth, false);
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
                            int curScore = Minimax(checkBoard, depth, true);
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
            PlayerPrefs.SetInt("AI", aiPoint);
            PlayerPrefs.SetInt("HUMAN", humanPoint);
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

        private void ChangeWinColor(Direction type, int index,Color winnerColor)
        {
            if(type==Direction.horizontal)
            {
                for (int i = 0; i < 5; i++)
                {
                    arrPiece[index, i].GetComponent<SpriteRenderer>().color = winnerColor;
                }
            }
            else if(type == Direction.vertical)
            {
                for (int i = 0; i < 5; i++)
                {
                    arrPiece[i, index].GetComponent<SpriteRenderer>().color = winnerColor;
                }
            }
            else if(type == Direction.diagonal)
            {
                if(index==44)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        arrPiece[i, i].GetComponent<SpriteRenderer>().color = winnerColor;
                    }
                }
                else if(index==40)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        arrPiece[i, 4-i].GetComponent<SpriteRenderer>().color = winnerColor;
                    }
                }
            }
        }
        enum Direction { horizontal, vertical, diagonal}
        public int CheckWin(bool changeColor)
        {
            // && arrBoard[y, 0]== arrBoard[y, 5] && arrBoard[y, 0]== arrBoard[y, 4]
            for (int y = 0; y < 5; y++)
            {
                if (arrBoard[y, 0] == arrBoard[y, 1] && arrBoard[y, 0] == arrBoard[y, 2]
                   && arrBoard[y, 0] == arrBoard[y, 3] && arrBoard[y, 0] == arrBoard[y, 4])
                {

                    if (arrBoard[y, 0] == playerChar)
                    {
                        if (changeColor)
                        {
                            ChangeWinColor(Direction.horizontal, y,Color.cyan);
                        }

                        return 1;
                    }
                    else if (arrBoard[y, 0] == botChar)
                    {
                        if (changeColor)
                        {
                            ChangeWinColor(Direction.horizontal, y, Color.red);
                        }

                        return -1;
                    }
                }
            }
            // && arrBoard[0, x] == arrBoard[5, x] && arrBoard[0, x] == arrBoard[4, x]
            for (int x = 0; x < 5; x++)
            {
                if (arrBoard[0, x] == arrBoard[1, x] && arrBoard[0, x] == arrBoard[2, x]
                  && arrBoard[0, x] == arrBoard[3, x] && arrBoard[0, x] == arrBoard[4, x])
                {

                    if (arrBoard[0, x] == playerChar)
                    {
                        if (changeColor)
                        {
                            ChangeWinColor(Direction.vertical, x, Color.cyan);
                        }
                        return 1;
                    }
                    else if (arrBoard[0, x] == botChar)
                    {
                        if (changeColor)
                        {
                            ChangeWinColor(Direction.vertical, x, Color.red);
                        }
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
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.vertical, 44, Color.cyan);
                    }
                    return 1;
                }
                else if (arrBoard[1, 1] == botChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.vertical, 44, Color.red);
                    }
                    return -1;
                }
            }

            //arrBoard[0, 2] == arrBoard[1, 1] && arrBoard[0, 2] == arrBoard[2, 0]
            if (arrBoard[0, 4] == arrBoard[1, 3] && arrBoard[0, 4] == arrBoard[2, 2]
                && arrBoard[0, 4] == arrBoard[3, 1] && arrBoard[0, 4] == arrBoard[4, 0])
            {

                if (arrBoard[2, 2] == playerChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.vertical, 40, Color.cyan);
                    }
                    return 1;
                }
                else if (arrBoard[2, 2] == botChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.vertical, 40, Color.red);
                    }
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

        //private DirectionPoint CheckCurrentPoint(int horizontal, int vertical)
        //{
        //    if (horizontal < 2 && vertical < 2)
        //    {
        //        return DirectionPoint.BotLeft;
        //    }
        //    else if (horizontal < 2 && vertical > 2)
        //    {
        //        return DirectionPoint.TopLeft;
        //    }
        //    else if (horizontal > 2 && vertical < 2)
        //    {
        //        return DirectionPoint.BotRight;
        //    }
        //    else if (horizontal > 2 && vertical > 2)
        //    {
        //        return DirectionPoint.TopRight;
        //    }
        //    else if (vertical == 2)
        //    {
        //        if (vertical != 2)
        //            return DirectionPoint.MiddleRow;
        //        else return DirectionPoint.Central;
        //    }
        //    else if (vertical == 2)
        //    {
        //        if (vertical != 2)
        //            return DirectionPoint.MiddleColumn;
        //        else return DirectionPoint.Central;
        //    }
        //    else return DirectionPoint.None;
        //}

        //private int CheckWinV2(DirectionPoint point, int _x, int _y,bool isMaximum)
        //{
        //    int checkWin=-Inf;
        //    switch (point)
        //    {
        //        case DirectionPoint.BotLeft:
        //        case DirectionPoint.TopRight:
        //            {
        //                int count;
        //                count = ColumnAndRow(_x,_y);
        //                if (count == 5)
        //                {
        //                    checkWin = 1;
        //                    break;
        //                }
        //                //right diagonal
        //                for (int d = 0; d < 5; d++)
        //                {
        //                    if (arrBoard[_x, _y] == arrBoard[d, d])
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
        //                    checkWin = 1;
        //                }
        //                else checkWin = -Inf;
        //                break;
        //            }
        //        case DirectionPoint.TopLeft:
        //        case DirectionPoint.BotRight:
        //            {
        //                int count;
        //                count = ColumnAndRow(_x,_y);
        //                if (count == 5)
        //                {
        //                    checkWin = 1;
        //                    break;
        //                }

        //                //left diagonal
        //                int tmp = 4;
        //                for (int d = 0; d < 5; d++)
        //                {
        //                    if (arrBoard[_x, _y] == arrBoard[d, tmp])
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
        //                    checkWin = 1;
        //                }
        //                else checkWin = -Inf;
        //                break;

        //            }
        //        case DirectionPoint.Central:
        //            {
        //                int count = ColumnAndRow(_x,_y);
        //                if (count == 5)
        //                {
        //                    checkWin = 1;
        //                }
        //                else checkWin = -Inf;
        //                break;
        //            }
        //        case DirectionPoint.MiddleColumn:
        //            {
        //                int count = ColumnAndRow(_x, _y);
        //                if (count == 5)
        //                {
        //                    checkWin = 1;
        //                }
        //                else checkWin = -Inf;
        //                break;
        //            }
        //        case DirectionPoint.MiddleRow:
        //            {
        //                int count = ColumnAndRow(_x, _y);
        //                if (count == 5)
        //                {
        //                    checkWin = 1;
        //                }
        //                else checkWin = -Inf;
        //                break;
        //            }
        //    }

        //    if (availableMove == 25 && checkWin==-Inf)
        //    {
        //        checkWin = 0;
        //        return 0;
        //    }
        //    if(checkWin==1)
        //    {
        //        if (isMaximum)
        //            return 1;
        //        else return -1;
        //    }

        //    return Inf;
        //}
        //private int ColumnAndRow(int _x, int _y)
        //{
        //    //vertical
        //    int count = 0;
        //    for (int y = 0; y < 5; y++)
        //    {
        //        if (arrBoard[_x, _y] == arrBoard[_x, y] && y!=_y)
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
        //        if (arrBoard[_x, _y] == arrBoard[x, _y] && x != _x)
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
        //    return Inf;
        //}
        public void RestartGame()
        {
            SceneManager.LoadScene(0);
        }
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
