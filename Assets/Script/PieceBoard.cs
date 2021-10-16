using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{
    public class PieceBoard : MonoBehaviour
    {
        public int X;

        public int Y;

        public bool hasChar;
        // Start is called before the first frame update
        void Start()
        {
            hasChar = false;
        }
        public PieceBoard(int horizontal,int vertical)
        {
            X = horizontal;
            Y = vertical;
        }

        public void SetUpNewChar(bool isPlayerTurn)
        {
            Controller tmp = Controller.controller;
            if (isPlayerTurn)
                Instantiate(tmp.x_Obj, transform);
            else Instantiate(tmp.y_Obj, transform);

            hasChar = true;

        }

    }
}
