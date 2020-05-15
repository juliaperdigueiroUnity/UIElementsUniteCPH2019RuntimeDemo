#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Complete
{
    public class MainMenuScreen : UIContent
    {
        public GameManager gameManager;
        
        // UIElements has minimal support for animation at this time so we
        // implement a custom solution here to animate the game logo. We
        // just use the built-in UIElements scheduler to repeatedly change
        // the background image of an element with the next frame in the
        // animation.
        int m_CurrentTitleLogoFrame = 0;
        public List<Texture2D> m_TitleLogoFrames = new List<Texture2D>();
        
        protected override void InitializeVisualTree()
        {
            var startButton = rootVisualElement.Q<Button>("start-button");
            if (startButton != null)
            {
                startButton.clickable.clicked += () =>
                {
                    gameManager.StartRound();
                };
            }

            var exitButton = rootVisualElement.Q<Button>("exit-button");
            if (exitButton != null)
            {
                exitButton.clickable.clicked += () =>
                {
#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                };
            }

            // Animate title logo.
            var titleLogo = rootVisualElement.Q("menu-title-image");
            titleLogo?.schedule.Execute(() =>
            {
                if (m_TitleLogoFrames.Count == 0)
                    return;

                m_CurrentTitleLogoFrame = (m_CurrentTitleLogoFrame + 1) % m_TitleLogoFrames.Count;
                var frame = m_TitleLogoFrames[m_CurrentTitleLogoFrame];
                titleLogo.style.backgroundImage = frame;
            }).Every(200);
        }
    }
}