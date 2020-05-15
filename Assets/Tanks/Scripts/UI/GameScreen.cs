using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Complete
{
    public class GameScreen : UIContent
    {
        public GameManager gameManager;
        
        // Pre-loaded UI assets (ie. UXML/USS).
        public VisualTreeAsset m_PlayerListItem;

        // We need to update the values of some UI elements so here are
        // their remembered references after being queried from the cloned
        // UXML.
        private Label m_SpeedLabel;
        private Label m_KillsLabel;
        private Label m_ShotsLabel;
        private Label m_AccuracyLabel;
        
        protected override void InitializeVisualTree()
        {
            // Stats
            m_SpeedLabel = rootVisualElement.Q<Label>("_speed");
            m_KillsLabel = rootVisualElement.Q<Label>("_kills");
            m_ShotsLabel = rootVisualElement.Q<Label>("_shots");
            m_AccuracyLabel = rootVisualElement.Q<Label>("_accuracy");

            // Buttons
            var increaseSpeedButton = rootVisualElement.Q<Button>("increase-speed");
            if (increaseSpeedButton != null)
            {
                increaseSpeedButton.clickable.clicked += () =>
                {
                    gameManager.IncreasePlayer1Speed();
                };
            }
            var backToMenuButton = rootVisualElement.Q<Button>("back-to-menu");
            if (backToMenuButton != null)
            {
                backToMenuButton.clickable.clicked += () =>
                {
                    SceneManager.LoadScene(0);
                };
            }
            var randomExplosionButton = rootVisualElement.Q<Button>("random-explosion");
            if (randomExplosionButton != null)
            {
                randomExplosionButton.clickable.clicked += () =>
                {
                    gameManager.StartRandomExplosions();
                };
            }

            var listView = rootVisualElement.Q<ListView>("player-list");
            if (listView != null)
            {
                listView.selectionType = SelectionType.None;

                if (listView.makeItem == null)
                    listView.makeItem = MakeItem;
                if (listView.bindItem == null)
                    listView.bindItem = BindItem;

                listView.itemsSource = gameManager.m_Tanks;
                listView.Refresh();
            }
        }
        
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // In-Game Virtualized ListView Implementation

        private VisualElement MakeItem()
        {
            var element = m_PlayerListItem.CloneTree();

            element.schedule.Execute(() => UpdateHealthBar(element)).Every(200);

            return element;
        }

        private void BindItem(VisualElement element, int index)
        {
            element.Q<Label>("player-name").text = "Player " + gameManager.m_Tanks[index].m_PlayerNumber;

            var playerColor = gameManager.m_Tanks[index].color;
            playerColor.a = 0.9f;
            element.Q("icon").style.unityBackgroundImageTintColor = playerColor;

            element.userData = gameManager.m_Tanks[index];

            UpdateHealthBar(element);
        }

        private void UpdateHealthBar(VisualElement element)
        {
            var tank = element.userData as TankManager;
            if (tank == null)
                return;

            var healthBar = element.Q("health-bar");
            var healthBarFill = element.Q("health-bar-fill");

            var totalWidth = healthBar.resolvedStyle.width;

            var healthComponent = tank.m_Instance.GetComponent<TankHealth>();
            var currentHealth = healthComponent.m_CurrentHealth;
            var startingHealth = healthComponent.m_StartingHealth;
            var percentHealth = currentHealth / startingHealth;

            healthBarFill.style.width = totalWidth * percentHealth;
        }

        public void UpdateValues(float speed, int kills, int fireCount, int percentAccuracy)
        {
            m_SpeedLabel.text = speed.ToString();
            m_KillsLabel.text = kills.ToString();
            m_ShotsLabel.text = fireCount.ToString();
            m_AccuracyLabel.text = percentAccuracy.ToString();
        }
    }
}