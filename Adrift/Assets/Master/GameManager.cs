using UnityEngine;

namespace Adrift.Game
{
    public class GameManager: MonoBehaviour
    {
        [SerializeField]
        private PlayerController m_Player = null;
        public PlayerController Player => m_Player;

        [SerializeField]
        private Animator
            m_PlayerAnimation = null;

        public event System.Action OnGameBegin, OnGameEnd;
        
        private void Update()
        {
            switch (m_State)
            {
                case GameState.Menu:
                    if (Input.GetButtonUp("Jump"))
                        BeginGame();

                    break;
                default:
                    break;
            }
        }

        private enum GameState
        {
            Menu,
            Playing,
            Ended,
        }

        private GameState m_State = GameState.Menu;

        public void BeginMenu()
        {
            if (m_State == GameState.Menu)
                return;

            m_State = GameState.Menu;
        }

        public void BeginGame()
        {
            if (m_State == GameState.Playing)
                return;

            m_State = GameState.Playing;
            m_PlayerAnimation.SetTrigger("BeginGame");
            OnGameBegin?.Invoke();
        }

        public void EndGame()
        {
            if (m_State == GameState.Ended)
                return;

            m_State = GameState.Ended;
            OnGameEnd();
        }

        public void QuitGame() 
            => Application.Quit();
    }
}