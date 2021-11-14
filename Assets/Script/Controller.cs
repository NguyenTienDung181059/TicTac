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
        private bool playerTurn;

        private const int Inf = 252525;

        private char playerChar = 'X';

        private char botChar = 'O';

        public int maxDepth;

        private char emptyChar = ' ';

        public static Controller controller;

        public event Action<string> onGameOver;

        public event Action onGameRestart;
        [HideInInspector]
        public bool gameOver;

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
        [SerializeField]
        private LayerMask layerMask;
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


               // LastResult(CheckWin(true));
                LastResult(CheckWinV2(true,piece.X, piece.Y));
                playerTurn = false;

                StartCoroutine(IWaitSecond());
               // BotMove();

            }
        }

        IEnumerator IWaitSecond()
        {
            yield return new WaitForSeconds(1);
            if (!gameOver)
                AIMovement();
        }
        private void SpawmCaroChar(PieceBoard piece)
        {
            piece.SetUpNewChar(playerTurn);
        }
        private PieceBoard CheckRayscat()
        {
            RaycastHit2D raycastHit2D;
            raycastHit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

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

            int bestScore = -Inf;
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if (arrBoard[x, y] == emptyChar)
                    {
                        arrBoard[x, y] = botChar;
                        availableMove++;
                       // int score = MinimaxBeta(arrBoard, 0, false,-Inf,Inf,x,y);
                        int score = Minimax(arrBoard, 0, false,x,y);
                        Debug.Log(score);
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
            //}
            arrBoard[_x, _y] = botChar;
            PieceBoard selectPiece = arrPiece[_x, _y];
            SetUpMove(_x, _y);
            SpawmCaroChar(selectPiece);

           // LastResult(CheckWin(true));
            LastResult(CheckWinV2(true,_x,_y));
            playerTurn = true;
        }
        private int MinimaxBeta(char[,] checkBoard, int depth, bool isMaximum, int alpha, int beta, int horizontal, int vertical)
        {
            //5x5
            if (depth == maxDepth)
            {
                return 0;
            }

            //   int tmp = CheckWin(false);
            int tmp = CheckWinV2(false,horizontal,vertical);
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
                return 10;
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
                            int curScore = MinimaxBeta(checkBoard, depth+1, false,alpha,beta,x,y);
                            checkBoard[x, y] = emptyChar;
                            availableMove--;
                            bestScore = Math.Max(bestScore, curScore);
                            alpha = Mathf.Max(alpha, curScore);
                            if (beta <= alpha)
                                break;
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
                            int curScore = MinimaxBeta(checkBoard, depth+1, true,alpha,beta,x,y);
                            checkBoard[x, y] = emptyChar;
                            availableMove--;
                            bestScore = Math.Min(bestScore, curScore);
                            beta = Mathf.Min(beta, curScore);
                            if (beta <= alpha)
                                break;
                        }
                    }
                }
                return bestScore;
            }

        }
        private int Minimax(char[,] checkBoard, int depth, bool isMaximum, int horizontal, int vertical)
        {
            //5x5
            if (depth == maxDepth)
            {
                return -1;
            }

            //   int tmp = CheckWin(false);
            int tmp = CheckWinV2(false,horizontal,vertical);
            if (tmp == 0)
            {
                return tmp;
            }
            else if (tmp == 1)
            {
                return -10 + depth;
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
                            int curScore = Minimax(checkBoard, depth+1, false,x,y);
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
                            int curScore = Minimax(checkBoard, depth+1, true,x,y);
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
                _x = UnityEngine.Random.Range(0, 4);
                _y = UnityEngine.Random.Range(0, 4);
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

        private void ChangeWinColor(Direction type, int index, Color winnerColor)
        {
            //if (type == Direction.horizontal)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        arrPiece[index, i].GetComponent<SpriteRenderer>().color = winnerColor;
            //    }
            //}
            //else if (type == Direction.vertical)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        arrPiece[i, index].GetComponent<SpriteRenderer>().color = winnerColor;
            //    }
            //}
            //else if (type == Direction.diagonal)
            //{
            //    if (index == 44)
            //    {
            //        for (int i = 0; i < 3; i++)
            //        {
            //            arrPiece[i, i].GetComponent<SpriteRenderer>().color = winnerColor;
            //        }
            //    }
            //    else if (index == 40)
            //    {
            //        for (int i = 0; i < 3; i++)
            //        {
            //            arrPiece[i, 4 - i].GetComponent<SpriteRenderer>().color = winnerColor;
            //        }
            //    }
            //}
        }
        enum Direction { horizontal, vertical, diagonal }
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
                            ChangeWinColor(Direction.horizontal, y, Color.blue);
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
                            ChangeWinColor(Direction.vertical, x, Color.blue);
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
                        ChangeWinColor(Direction.diagonal, 44, Color.blue);
                    }
                    return 1;
                }
                else if (arrBoard[1, 1] == botChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.diagonal, 44, Color.red);
                    }
                    return -1;
                }
            }

            //arrBoard[0, 2] == arrBoard[1, 1] && arrBoard[0, 2] == arrBoard[2, 0]
            if (arrBoard[0, 4] == arrBoard[1, 3] && arrBoard[2, 2] == arrBoard[0, 4]
                && arrBoard[0, 4] == arrBoard[3, 1] && arrBoard[0, 4] == arrBoard[4, 0])
            {

                if (arrBoard[2, 2] == playerChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.diagonal, 40, Color.blue);
                    }
                    return 1;
                }
                else if (arrBoard[2, 2] == botChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.diagonal, 40, Color.red);
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
        
        public int CheckWinV2(bool changeColor,int horizontal,int vertical)
        {
            // Ngang
            for(int i=0;i<3;i++)
            {
                if (arrBoard[horizontal, i] == arrBoard[horizontal, i + 1] && arrBoard[horizontal, i] == arrBoard[horizontal, i + 2])
                    if (i == 0)
                    {
                        if (arrBoard[horizontal, i + 3] != playerChar && arrBoard[horizontal, i] == botChar)
                            return -1;
                        else if (arrBoard[horizontal, i + 3] != botChar && arrBoard[horizontal, i] == playerChar)
                            return 1;
                    }
                    else if (i == 2)
                    {
                        if (arrBoard[horizontal, i - 1] != playerChar && arrBoard[horizontal, i] == botChar)
                            return -1;
                        else if (arrBoard[horizontal, i - 1] != botChar && arrBoard[horizontal, i] == playerChar)
                            return 1;
                    }
                    else
                    {
                        if (arrBoard[horizontal, i + 3] == playerChar && arrBoard[horizontal, i] == botChar && arrBoard[horizontal, i - 1] == botChar)
                            return -1;
                        else if (arrBoard[horizontal, i -1] == playerChar && arrBoard[horizontal, i] == botChar && arrBoard[horizontal, i+3] == botChar)
                            return -1; 
                        if (arrBoard[horizontal, i + 3] == botChar && arrBoard[horizontal, i] == playerChar && arrBoard[horizontal, i - 1] == playerChar)
                            return 1;
                        else if (arrBoard[horizontal, i -1] == botChar && arrBoard[horizontal, i] == playerChar && arrBoard[horizontal, i+3] == playerChar)
                            return 1;

                    }
            }
            //Doc
            for(int i=0;i<3;i++)
            {
                if (arrBoard[i, vertical] == arrBoard[i+1, vertical] && arrBoard[i, vertical] == arrBoard[i+2, vertical])
                    if (i == 0)
                    {
                        if (arrBoard[i+3, vertical] != playerChar && arrBoard[i, vertical] == botChar)
                            return -1;
                        else if (arrBoard[i + 3, vertical] != botChar && arrBoard[i, vertical] == playerChar)
                            return 1;
                    }
                    else if (i == 2)
                    {
                         if (arrBoard[i-1, vertical] != playerChar && arrBoard[i, vertical] == botChar)
                            return -1;
                        else if (arrBoard[i-1, vertical] != botChar && arrBoard[i, vertical] == playerChar)
                            return 1;
                    }
                    else
                    {
                        if (arrBoard[i + 3, vertical] == playerChar && arrBoard[i, vertical] == botChar && arrBoard[i-1, vertical] == botChar)
                            return -1;
                        else if (arrBoard[i-1, vertical] == playerChar && arrBoard[i, vertical] == botChar && arrBoard[i+3, vertical] == botChar)
                            return -1;
                        if (arrBoard[i+3, vertical] == botChar && arrBoard[i, vertical] == playerChar && arrBoard[i-1, vertical] == playerChar)
                            return 1;
                        else if (arrBoard[i-1, vertical] == botChar && arrBoard[i, vertical] == playerChar && arrBoard[i+3, vertical] == playerChar)
                            return 1;
                    }
            }

            DiagonalSide checkSide = GetPostionDiagonal(horizontal, vertical);
            int maxNumber = GetNumber(checkSide, horizontal, vertical);
            int start_x = horizontal - vertical;
            int start_y = vertical - vertical;
            //Cheo phai
            for (int i = 0; i <= maxNumber-3; i++) 
            {
                if (arrBoard[start_x, i] == arrBoard[start_x+1, i+1] && arrBoard[start_x+2, i+2] == arrBoard[start_x, i])
                    if (i == 2)
                    {
                        if (arrBoard[i - 1, vertical] != playerChar && arrBoard[i, vertical] == botChar)
                            return -1;
                        else if (arrBoard[i - 1, vertical] != botChar && arrBoard[i, vertical] == playerChar)
                            return 1;
                    }
                    if (start_x + 3 <=4)
                    {
                        if (arrBoard[start_x, i] != playerChar && arrBoard[start_x, i+3] == botChar )
                            return -1;
                        else if (arrBoard[i + 3, vertical] != botChar && arrBoard[i, vertical] == playerChar)
                            return 1;
                    }
                    else
                    {
                        if (arrBoard[i + 3, vertical] == playerChar && arrBoard[i, vertical] == botChar && arrBoard[i - 1, vertical] == botChar)
                            return -1;
                        else if (arrBoard[i - 1, vertical] == playerChar && arrBoard[i, vertical] == botChar && arrBoard[i + 3, vertical] == botChar)
                            return -1;
                        if (arrBoard[i + 3, vertical] == botChar && arrBoard[i, vertical] == playerChar && arrBoard[i - 1, vertical] == playerChar)
                            return 1;
                        else if (arrBoard[i - 1, vertical] == botChar && arrBoard[i, vertical] == playerChar && arrBoard[i + 3, vertical] == playerChar)
                            return 1;
                    }
            }

            //    if (vertical <= 2 && arrBoard[horizontal, vertical] == arrBoard[horizontal, vertical+1] && arrBoard[horizontal, vertical] == arrBoard[horizontal, vertical+2]
            //       )
            //    {

            //        if (arrBoard[horizontal, vertical] == playerChar)
            //        {
            //            if (changeColor)
            //            {
            //                ChangeWinColor(Direction.horizontal, vertical, Color.blue);
            //            }

            //            return 1;
            //        }
            //        else if (arrBoard[horizontal, vertical] == botChar)
            //        {
            //            if (changeColor)
            //            {
            //                ChangeWinColor(Direction.horizontal, vertical, Color.red);
            //            }

            //            return -1;
            //        }

            //     }

            //    if (vertical >= 1 && vertical <= 3 && arrBoard[horizontal, vertical] == arrBoard[horizontal, vertical - 1] && arrBoard[horizontal, vertical] == arrBoard[horizontal, vertical + 1]
            //       )
            //     {
            //        if (arrBoard[horizontal, vertical] == playerChar)
            //        {
            //            if (changeColor)
            //            {
            //            ChangeWinColor(Direction.horizontal, vertical, Color.blue);
            //            }
            //            return 1;
            //        }
            //        else if (arrBoard[horizontal, vertical] == botChar)
            //        {
            //            if (changeColor)
            //            {
            //            ChangeWinColor(Direction.horizontal, vertical, Color.red);
            //            }
            //            return -1;
            //        }

            //     }

            //    if (vertical >= 2 && arrBoard[horizontal, vertical] == arrBoard[horizontal, vertical -1] && arrBoard[horizontal, vertical] == arrBoard[horizontal, vertical -2]
            //       )
            //     {
            //        if (arrBoard[horizontal, vertical] == playerChar)
            //        {
            //            if (changeColor)
            //            {
            //            ChangeWinColor(Direction.horizontal, vertical, Color.blue);
            //            }
            //            return 1;
            //        }
            //        else if (arrBoard[horizontal, vertical] == botChar)
            //        {
            //            if (changeColor)
            //            {
            //            ChangeWinColor(Direction.horizontal, vertical, Color.red);
            //            }
            //            return -1;
            //        }               
            //     }
            ////Doc
            //if (horizontal <= 2 && arrBoard[horizontal, vertical] == arrBoard[horizontal+1, vertical] && arrBoard[horizontal, vertical] == arrBoard[horizontal+2, vertical]
            //       )
            //     {
            //        if (arrBoard[horizontal, vertical] == playerChar)
            //        {
            //            if (changeColor)
            //            {
            //            ChangeWinColor(Direction.vertical, horizontal, Color.blue);
            //            }
            //            return 1;
            //        }
            //        else if (arrBoard[horizontal, vertical] == botChar)
            //        {
            //            if (changeColor)
            //            {
            //            ChangeWinColor(Direction.vertical, horizontal, Color.red);
            //            }
            //            return -1;
            //        }               
            //     }

            //if (horizontal <= 3 && horizontal >= 1 && arrBoard[horizontal, vertical] == arrBoard[horizontal + 1, vertical] && arrBoard[horizontal, vertical] == arrBoard[horizontal - 1, vertical]
            //       )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.vertical, 44, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.vertical, 44, Color.red);
            //        }
            //        return -1;
            //    }
            //}

            //if (horizontal >= 2 && arrBoard[horizontal, vertical] == arrBoard[horizontal - 2, vertical] && arrBoard[horizontal, vertical] == arrBoard[horizontal - 1, vertical]
            //       )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.vertical, 40, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.vertical, 40, Color.red);
            //        }
            //        return -1;
            //    }
            //}

            ////Cheo phai
            //if (horizontal <= 2 && vertical <= 2  && arrBoard[horizontal, vertical] == arrBoard[horizontal + 1, vertical+1] && arrBoard[horizontal, vertical] == arrBoard[horizontal + 2, vertical+2]
            //      )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.red);
            //        }
            //        return -1;
            //    }
            //}

            //if (horizontal <= 3 && vertical <= 3 && horizontal >= 1 && vertical >= 1 && arrBoard[horizontal, vertical] == arrBoard[horizontal + 1, vertical+1] && arrBoard[horizontal, vertical] == arrBoard[horizontal - 1, vertical - 1]
            //     )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.red);
            //        }
            //        return -1;
            //    }
            //} 

            //if (horizontal >= 2 && vertical >= 2 && arrBoard[horizontal, vertical] == arrBoard[horizontal - 1, vertical - 1] && arrBoard[horizontal, vertical] == arrBoard[horizontal - 2, vertical - 2]
            //      )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.red);
            //        }
            //        return -1;
            //    }
            //}
            ////Cheo trai
            //if (horizontal <= 2 && vertical >= 2 && arrBoard[horizontal, vertical] == arrBoard[horizontal +1, vertical - 1] && arrBoard[horizontal, vertical] == arrBoard[horizontal +2, vertical - 2]
            //     )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.red);
            //        }
            //        return -1;
            //    }
            //}

            //if (horizontal <= 3 && vertical >= 1 && horizontal >= 1 && vertical <= 3 && arrBoard[horizontal, vertical] == arrBoard[horizontal + 1, vertical - 1] && arrBoard[horizontal, vertical] == arrBoard[horizontal -1, vertical +1]
            //      )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.red);
            //        }
            //        return -1;
            //    }
            //}
            //if (horizontal >= 2 && vertical <= 2 && arrBoard[horizontal, vertical] == arrBoard[horizontal -1, vertical + 1] && arrBoard[horizontal, vertical] == arrBoard[horizontal - 2, vertical + 2]
            //     )
            //{

            //    if (arrBoard[horizontal, vertical] == playerChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.blue);
            //        }
            //        return 1;
            //    }
            //    else if (arrBoard[horizontal, vertical] == botChar)
            //    {
            //        if (changeColor)
            //        {
            //            ChangeWinColor(Direction.diagonal, 40, Color.red);
            //        }
            //        return -1;
            //    }
            //}
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
        enum DiagonalSide { Up, Down, Mid }
        private DiagonalSide GetPostionDiagonal(int horizontal,int vertical)
        {
            if (horizontal > vertical)
                return DiagonalSide.Up;
            else if (horizontal < vertical)
                return DiagonalSide.Down;
            else return DiagonalSide.Mid;
        }

        private int GetNumber(DiagonalSide side, int horizontal, int vertical)
        {
            if(side == DiagonalSide.Up)
            {
                return (4 - horizontal + vertical + 1);
            }
            if (side == DiagonalSide.Down)
            {
                return (horizontal + 4 - vertical + 1);
            }
            else return 5;
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
