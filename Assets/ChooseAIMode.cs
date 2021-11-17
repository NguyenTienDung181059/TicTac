using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script {
    public class ChooseAIMode : MonoBehaviour
    {
        private Button button;
        [SerializeField]
        private int selectedDepth;
        [SerializeField]
        private diff difficult;
        Controller controller;

        public enum diff{ Minimax, AlphaBeta}
        // Start is called before the first frame update
        void Start()
        {
            controller = Controller.controller;
            button = GetComponent<Button>();
            button.onClick.AddListener(SetUpDepth);
        }
        private void SetUpDepth()
        {
            controller.maxDepth = selectedDepth;
            controller.curDifficult = difficult;
            controller.gameOver = false;
            transform.parent.parent.gameObject.SetActive(false);
        }
        private void Restart()
        {
            gameObject.SetActive(true);
        }
    }
}
