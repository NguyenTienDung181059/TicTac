using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script {
    public class GameOverPanel : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        private Text text;
        void Start()
        {
            Controller.controller.onGameOver += ShowPanel;
            gameObject.SetActive(false);
        }
        
        protected void ShowPanel(string winner)
        {
            text.text = winner;
            gameObject.SetActive(true);
        }
    }
}
