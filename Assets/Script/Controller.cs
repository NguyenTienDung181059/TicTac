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

        private const int Inf = 272727;

        private char playerChar = 'X';

        private char botChar = 'O';

        public int maxDepth;

        public ChooseAIMode.diff curDifficult;

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

        public char[,] arrBoard = new char[7, 7];

        public PieceBoard[,] arrPiece = new PieceBoard[7, 7];

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
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
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
            yield return new WaitForSeconds(2);
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
               // Debug.Log("Save  " + arrBoard[_x, _y] + "  " + _x +" " + _y);
            }
            else
            {
                arrBoard[_x, _y] = botChar;
            }
            availableMove++;
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

                LastResult(CheckWin(true));
                playerTurn = true;
            }
        }
        private void AIMovement()
        {
            int _x = Inf;
            int _y = Inf;

            int bestScore = -Inf;
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    if (arrBoard[x, y] == emptyChar)
                    {
                        arrBoard[x, y] = botChar;
                        availableMove++;
                        int score;

                       if(curDifficult== ChooseAIMode.diff.Minimax)
                        score = Minimax(arrBoard, 1, false,x,y);
                       else
                        score = MinimaxBeta(arrBoard, 1, false,-Inf,Inf,x,y);
                        
                       // Debug.Log(score);
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
            arrBoard[_x, _y] = botChar;
            PieceBoard selectPiece = arrPiece[_x, _y];
            SetUpMove(_x, _y);
            SpawmCaroChar(selectPiece);

            LastResult(CheckWinV2(true,_x,_y));
            playerTurn = true;
        }
        private int MinimaxBeta(char[,] checkBoard, int depth, bool isMaximum, int alpha, int beta, int horizontal, int vertical)
        {
            //7x7
            if (depth > maxDepth)
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
                return -10+tmp;
            }
            else if (tmp == -1)
            {
                return 10-tmp;
            }
            //  Debug.Log("depth " + depth);

            if (isMaximum)
            {
                int bestScore = -Inf;
                for (int x = 0; x < 7; x++)
                {
                    for (int y = 0; y < 7; y++)
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
                for (int x = 0; x < 7; x++)
                {
                    for (int y = 0; y < 7; y++)
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
            //7x7
            if (depth > maxDepth)
            {
                return 0;
            }


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

            if (isMaximum)
            {
                int bestScore = -Inf;
                for (int x = 0; x < 7; x++)
                {
                    for (int y = 0; y < 7; y++)
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
                for (int x = 0; x < 7; x++)
                {
                    for (int y = 0; y < 7; y++)
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
                _x = UnityEngine.Random.Range(0, 6);
                _y = UnityEngine.Random.Range(0, 6);
            }
            arrPiece[_x, _y].hasChar = true;

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
            //    if (index == 66)
            //    {
            //        for (int i = 0; i < 3; i++)
            //        {
            //            arrPiece[i, i].GetComponent<SpriteRenderer>().color = winnerColor;
            //        }
            //    }
            //    else if (index == 60)
            //    {
            //        for (int i = 0; i < 3; i++)
            //        {
            //            arrPiece[i, 6 - i].GetComponent<SpriteRenderer>().color = winnerColor;
            //        }
            //    }
            //}
        }
        enum Direction { horizontal, vertical, diagonal }
        public int CheckWin(bool changeColor)
        {
            // && arrBoard[y, 0]== arrBoard[y, 7] && arrBoard[y, 0]== arrBoard[y, 6]
            for (int y = 0; y < 7; y++)
            {
                if (arrBoard[y, 0] == arrBoard[y, 1] && arrBoard[y, 0] == arrBoard[y, 2]
                   && arrBoard[y, 0] == arrBoard[y, 3] && arrBoard[y, 0] == arrBoard[y, 6])
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
            // && arrBoard[0, x] == arrBoard[7, x] && arrBoard[0, x] == arrBoard[6, x]
            for (int x = 0; x < 7; x++)
            {
                if (arrBoard[0, x] == arrBoard[1, x] && arrBoard[0, x] == arrBoard[2, x]
                  && arrBoard[0, x] == arrBoard[3, x] && arrBoard[0, x] == arrBoard[6, x])
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
            // && arrBoard[0, 0]== arrBoard[7, 7] && arrBoard[0, 0]== arrBoard[6, 6]
            //Cheo
            if (arrBoard[0, 0] == arrBoard[1, 1] && arrBoard[0, 0] == arrBoard[2, 2]
               && arrBoard[0, 0] == arrBoard[3, 3] && arrBoard[0, 0] == arrBoard[6, 6])
            {

                if (arrBoard[1, 1] == playerChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.diagonal, 66, Color.blue);
                    }
                    return 1;
                }
                else if (arrBoard[1, 1] == botChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.diagonal, 66, Color.red);
                    }
                    return -1;
                }
            }

            //arrBoard[0, 2] == arrBoard[1, 1] && arrBoard[0, 2] == arrBoard[2, 0]
            if (arrBoard[0, 6] == arrBoard[1, 3] && arrBoard[2, 2] == arrBoard[0, 6]
                && arrBoard[0, 6] == arrBoard[3, 1] && arrBoard[0, 6] == arrBoard[6, 0])
            {

                if (arrBoard[2, 2] == playerChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.diagonal, 60, Color.blue);
                    }
                    return 1;
                }
                else if (arrBoard[2, 2] == botChar)
                {
                    if (changeColor)
                    {
                        ChangeWinColor(Direction.diagonal, 60, Color.red);
                    }
                    return -1;
                }
            }
            //Hoa
            if (availableMove == 27)
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
            for(int i=0;i<7-2;i++)
            {
                if (arrBoard[horizontal, i] == arrBoard[horizontal, i + 1] && arrBoard[horizontal, i] == arrBoard[horizontal, i + 2])
                    if (i == 0)
                    {
                        if (arrBoard[horizontal, i + 3] != playerChar && arrBoard[horizontal, i] == botChar)
                            return -1;
                        else if (arrBoard[horizontal, i + 3] != botChar && arrBoard[horizontal, i] == playerChar)
                            return 1;
                    }
                    else if (i == 7-3)
                    {
                        if (arrBoard[horizontal, i - 1] != playerChar && arrBoard[horizontal, i] == botChar)
                            return -1;
                        else if (arrBoard[horizontal, i - 1] != botChar && arrBoard[horizontal, i] == playerChar)
                            return 1;
                    }
                    else
                    {
                       
                        if (arrBoard[horizontal, i + 3] != playerChar && arrBoard[horizontal, i - 1] != playerChar && arrBoard[horizontal, i] == botChar)
                            return -1;
                        if (arrBoard[horizontal, i + 3] != botChar && arrBoard[horizontal, i - 1] != botChar && arrBoard[horizontal, i] == playerChar)
                            return 1;
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
            for(int i=0;i<7-2;i++)
            {
                if (arrBoard[i, vertical] == arrBoard[i+1, vertical] && arrBoard[i, vertical] == arrBoard[i+2, vertical])
                    if (i == 0)
                    {
                        if (arrBoard[i+3, vertical] != playerChar && arrBoard[i, vertical] == botChar)
                            return -1;
                        else if (arrBoard[i + 3, vertical] != botChar && arrBoard[i, vertical] == playerChar)
                            return 1;
                    }
                    else if (i == 7-3)
                    {
                         if (arrBoard[i-1, vertical] != playerChar && arrBoard[i, vertical] == botChar)
                            return -1;
                        else if (arrBoard[i-1, vertical] != botChar && arrBoard[i, vertical] == playerChar)
                            return 1;
                    }
                    else
                    {
                        if (arrBoard[i + 3, vertical] != playerChar && arrBoard[i - 1, vertical] != playerChar && arrBoard[i, vertical] == botChar)
                            return -1;
                        if (arrBoard[i + 3, vertical] != botChar && arrBoard[i - 1, vertical] != botChar && arrBoard[i, vertical] == playerChar)
                            return 1;
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
            int start_y = vertical - horizontal;
            //Cheo phai
            if (checkSide == DiagonalSide.Up || checkSide == DiagonalSide.Mid && maxNumber>=3)
            {
                for (int i = 0; i <= maxNumber - 3; i++)
                {
                    if (arrBoard[start_x + i, i] == arrBoard[start_x + i + 1, i + 1] && arrBoard[start_x + i + 2, i + 2] == arrBoard[start_x + i, i] && arrBoard[start_x + i, i]!=emptyChar)
                    {
                        if (start_x + 3 == 6 + 1)
                        {
                            if (arrBoard[start_x + i, i] == botChar)
                                return -1;
                            else if (arrBoard[start_x + i, i] == playerChar)
                                return 1;
                        }
                        else if (start_x + 3 <= 6)
                        {
                            if (i == 0 && arrBoard[start_x + i + 3, i + 3] != playerChar && arrBoard[start_x + i, i] == botChar)
                                return -1;
                            else if (i == maxNumber - 3 && arrBoard[start_x + i - 1, i - 1] != playerChar && arrBoard[start_x + i, i] == botChar)
                                return -1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[start_x + i - 1, i - 1] == playerChar && arrBoard[start_x + i, i] == botChar && arrBoard[start_x + i + 3, i + 3] == botChar)
                                return -1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[start_x + i + 3, i + 3] == playerChar && arrBoard[start_x + i, i] == botChar && arrBoard[start_x + i - 1, i - 1] == botChar)
                                return -1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[start_x + i + 3, i + 3] != playerChar && arrBoard[start_x + i -1, i -1] != playerChar && arrBoard[start_x + i, i] == botChar)
                                return -1;

                            if (i == 0 && arrBoard[start_x + i + 3, i + 3] != botChar && arrBoard[start_x + i, i] == playerChar)
                                return 1;
                            else if (i == maxNumber - 3 && arrBoard[start_x + i - 1, i - 1] != botChar && arrBoard[start_x + i, i] == playerChar)
                                return 1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[start_x + i - 1, i - 1] == botChar && arrBoard[start_x + i, i] == playerChar && arrBoard[start_x + i + 3, i + 3] == playerChar)
                                return 1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[start_x + i + 3, i + 3] == botChar && arrBoard[start_x + i, i] == playerChar && arrBoard[start_x + i - 1, i - 1] == playerChar)
                                return 1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[start_x + i + 3, i + 3] != botChar && arrBoard[start_x + i - 1, i - 1] != botChar && arrBoard[start_x + i, i] == playerChar)
                                return 1;
                        }
                    }
                }
            }
            if (checkSide == DiagonalSide.Down && maxNumber>=3)
            {
                for (int i = 0; i <= maxNumber - 3; i++)
                {
                    if (arrBoard[i, start_y+i] == arrBoard[i + 1, start_y+i+1] && arrBoard[i + 2, start_y + i + 2] == arrBoard[i,start_y+ i] && arrBoard[i,start_y+ i]!=emptyChar)
                    {
                        if (start_y + 3 == 6 + 1)
                        {
                            if (arrBoard[i, start_y+i] == botChar)
                                return -1;
                            else if (arrBoard[i, start_y + i] == playerChar)
                                return 1;
                        }
                        else if (start_y + 3 <= 6)
                        {
                            if (i == 0 && arrBoard[i + 3, start_y + i + 3] != playerChar && arrBoard[i, start_y+i] == botChar)
                                return -1;
                            else if (i!= 0 && i == maxNumber - 3 && arrBoard[i - 1, start_y + i - 1] != playerChar && arrBoard[i, start_y+ i] == botChar)
                                return -1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[ i - 1, start_y+ i - 1] == playerChar && arrBoard[i, start_y+ i] == botChar && arrBoard[ i + 3, start_y+ i + 3] == botChar)
                                return -1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[i + 3, start_y+ i + 3] == playerChar && arrBoard[i, start_y+ i] == botChar && arrBoard[i - 1, start_y+ i - 1] == botChar)
                                return -1;                 
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[i + 3, start_y+ i + 3] != playerChar && arrBoard[i, start_y+ i] != playerChar && arrBoard[i - 1, start_y+ i - 1] == botChar)
                                return -1;

                            if (i == 0 && arrBoard[i + 3, start_y+ i + 3] != botChar && arrBoard[i, start_y+ i] == playerChar)
                                return 1;
                            else if (i != 0 && i == maxNumber - 3 && arrBoard[i - 1, start_y+ i - 1] != botChar && arrBoard[i, start_y+ i] == playerChar)
                                return 1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[i - 1, start_y+ i - 1] == botChar && arrBoard[i, start_y+ i] == playerChar && arrBoard[i + 3, start_y+ i + 3] == playerChar)
                                return 1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[i + 3, start_y+ i + 3] == botChar && arrBoard[i, start_y+ i] == playerChar && arrBoard[i - 1, start_y+ i - 1] == playerChar)
                                return 1;
                            else if (i > 0 && i != maxNumber - 3 && arrBoard[i + 3, start_y + i + 3] != botChar && arrBoard[i, start_y + i] != botChar && arrBoard[i - 1, start_y + i - 1] == playerChar)
                                return 1;
                        }
                    }
                }
            }

            //Cheo trai

            int maxCount = 1;
            int startX=horizontal;
            int startY=vertical;
            while(startX > 0 && startY < 6)
            {
                startX--;
                startY++;
            }
            int _x=startX;
            int _y=startY;
            while (startX < 6 && startY > 0)
            {
                startX++;
                startY--;
                maxCount++;
            }

            for(int i=0; i<=maxCount-3; i++)
            {
                if (arrBoard[_x+i, _y-i] == arrBoard[_x + i + 1, _y - i - 1] && arrBoard[_x + i, _y - i] == arrBoard[_x + i + 2, _y - i - 2] && arrBoard[_x + i, _y - i]!=emptyChar)
                {
                    if (_x+i + 3 == 6 + 1)
                    {
                        if (arrBoard[_x + i, _y - i] == botChar)
                            return -1;
                        else if (arrBoard[_x + i, _y - i] == playerChar)
                            return 1;
                    }
                    else if (_x+i + 3 <= 6)
                    {
                        if (i == 0 && maxCount==3 && arrBoard[_x + i, _y - i] == botChar)
                            return -1;
                        if (i == 0 && maxCount>3 && arrBoard[_x + i + 3, _y - i - 3] != playerChar && arrBoard[_x + i, _y - i] == botChar)
                            return -1;
                        else if (i != 0 && i == maxCount - 3 && arrBoard[_x + i - 1, _y - i + 1] != playerChar && arrBoard[_x + i, _y - i] == botChar)
                            return -1;
                        else if (i > 0 && i != maxCount - 3 && arrBoard[_x + i - 1, _y - i + 1] == playerChar && arrBoard[_x + i, _y - i] == botChar && arrBoard[_x + i + 3, _y - i - 3] == botChar)
                            return -1;
                        else if (i > 0 && i != maxCount - 3 && arrBoard[_x + i + 3, _y - i - 3] == playerChar && arrBoard[_x + i, _y - i] == botChar && arrBoard[_x + i - 1, _y - i + 1] == botChar)
                            return -1;
                        else if (i > 0 && i != maxCount - 3 && arrBoard[_x + i - 1, _y - i + 1] != playerChar && arrBoard[_x + i + 3, _y - i - 3] != playerChar && arrBoard[_x + i, _y - i] == botChar)
                            return -1;

                        if (i == 0 && maxCount == 3 && arrBoard[_x + i, _y - i] == playerChar)
                            return 1;
                        if (i == 0 && maxCount > 3 && arrBoard[_x + i + 3, _y - i - 3] != botChar && arrBoard[_x + i, _y - i] == playerChar)
                            return 1;
                        else if (i != 0 && i == maxCount - 3 && arrBoard[_x + i - 1, _y - i + 1] != botChar && arrBoard[_x + i, _y - i] == playerChar)
                            return 1; 
                        else if (i > 0 && i != maxCount - 3 && arrBoard[_x + i - 1, _y - i + 1] == botChar && arrBoard[_x + i, _y - i] == playerChar && arrBoard[_x + i + 3, _y - i - 3] == playerChar)
                            return 1;
                        else if (i > 0 && i != maxCount - 3 && arrBoard[_x + i + 3, _y - i - 3] == botChar && arrBoard[_x + i, _y - i] == playerChar && arrBoard[_x + i - 1, _y - i + 1] == playerChar)
                            return 1; 
                        else if (i > 0 && i != maxCount - 3 && arrBoard[_x + i - 1, _y - i + 1] != botChar && arrBoard[_x + i + 3, _y - i - 3] != botChar && arrBoard[_x + i, _y - i] == playerChar)
                            return 1;
                    }
                }
            }

            if (availableMove == 49)
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
                return (6 - horizontal + vertical + 1);
            }
            if (side == DiagonalSide.Down)
            {
                return (horizontal + 6 - vertical + 1);
            }
            else return 7;
        }
       
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
