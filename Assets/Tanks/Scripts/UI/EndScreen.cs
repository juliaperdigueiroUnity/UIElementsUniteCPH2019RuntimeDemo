using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Complete
{
    public class EndScreen : UIContent
    {
        // UIElements has minimal support for animation at this time so we
        // implement a custom solution here to animate the game logo. We
        // just use the built-in UIElements scheduler to repeatedly change
        // the background image of an element with the next frame in the
        // animation.
        int m_CurrentEndScreenFrame = 0;
        public List<Texture2D> m_EndScreenFrames = new List<Texture2D>();
        
        protected override void InitializeVisualTree()
        {
            rootVisualElement.Q<Button>("back-to-menu-button").clickable.clicked += () =>
            {
                SceneManager.LoadScene(0);
            };

            // Animate end skull.
            var titleLogo = rootVisualElement.Q("menu-title-image");
            titleLogo?.schedule.Execute(() =>
            {
                if (m_EndScreenFrames.Count == 0)
                    return;

                m_CurrentEndScreenFrame = (m_CurrentEndScreenFrame + 1) % m_EndScreenFrames.Count;
                var frame = m_EndScreenFrames[m_CurrentEndScreenFrame];
                titleLogo.style.backgroundImage = frame;
            }).Every(100);
        }
    }
}