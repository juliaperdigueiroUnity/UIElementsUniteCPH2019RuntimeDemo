using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        // Public Properties
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public int m_ShellRandomRange = 20;
        public int m_ShellForce = 25;
        public int m_ShellWaveCount = 10;
        public float m_ShellDelay = 0.1f;
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks.

        public MainMenuScreen mainMenuScreenPrefab;
        public GameScreen gameScreenPrefab;
        public EndScreen endScreenPrefab;

        // Game-related variables.
        private TankMovement m_Player1Movement;
        private TankShooting m_Player1Shooting;
        private TankHealth m_Player1Health;

        private GameScreen m_GameScreen;
        private UIContent m_currentScreen;

        // Internal variables.
        WaitForSeconds m_ShellTime;
        int m_MaxFirestormCount = 5;

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // MonoBehaviour States

        // OnEnable
        // Register our postUxmlReload callbacks to be notified if and when
        // the UXML or USS assets being user are changed (by the UI Builder).
        // In these callbacks, we just rebind UI VisualElements to data or
        // to click events.
        private void OnEnable()
        {
            m_ShellTime = new WaitForSeconds(m_ShellDelay);
        }

        // Start
        // Just go to main menu.
        private void Start()
        {
#if !UNITY_EDITOR
            if (Screen.fullScreen)
                Screen.fullScreen = false;
#endif

            //GoToMainMenu();
            StartRound();
        }

        // Update
        // Update UI Labels with data from the game. (also run some minimal game logic)
        private void Update()
        {
            // If UI wasn't initialized, do nothing.
            if (m_GameScreen == null || m_Tanks.Length == 0 || m_Player1Movement == null || m_Player1Health == null)
                return;

            // Player is dead..
            if (m_Player1Health.m_Dead)
                EndRound();

            // Gather all values to update the UI.
            var kills = m_Tanks.Length;
            foreach (var tank in m_Tanks)
                if (tank.m_Instance.activeSelf)
                    kills--;

            var fireCount = m_Player1Shooting.m_FireCount;

            var hitCount = m_Player1Shooting.m_HitCount;
            if (fireCount == 0)
                fireCount = 1; // Avoid div by 0.
            var percent = (int)(((float)hitCount / (float)fireCount) * 100);
            
            // Update UI.
            m_GameScreen.UpdateValues(m_Player1Movement.m_Speed, kills, fireCount, percent);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // Game Logic (unrelated to UI)

        private IEnumerator Firestorm()
        {
            var shellsLeft = m_ShellWaveCount;

            while (shellsLeft > 0)
            {
                var x = Random.Range(-m_ShellRandomRange, m_ShellRandomRange);
                var z = Random.Range(-m_ShellRandomRange, m_ShellRandomRange);
                var position = new Vector3(x, 20, z);
                var rotation = Quaternion.FromToRotation(position, new Vector3(x, 0f, z));

                Rigidbody shellInstance =
                    Instantiate(m_Shell, position, rotation) as Rigidbody;

                shellInstance.gameObject.GetComponent<ShellExplosion>().m_TankMask = -1;

                // Set the shell's velocity to the launch force in the fire position's forward direction.
                shellInstance.velocity = 30.0f * Vector3.down;

                shellsLeft--;

                yield return m_ShellTime;
            }
        }

        private void SpawnAllTanks()
        {
            // For all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                var ran = Random.Range(0, 180);
                var rot = Quaternion.Euler(0, ran, 0);

                // ... create them, set their player number and references needed for control.
                m_Tanks[i].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, rot) as GameObject;
                m_Tanks[i].m_Instance.transform.localRotation = rot;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }

            var instance = m_Tanks[0].m_Instance;
            m_Player1Movement = instance.GetComponent<TankMovement>();
            m_Player1Shooting = instance.GetComponent<TankShooting>();
            m_Player1Health = instance.GetComponent<TankHealth>();
        }

        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            Transform[] targets = new Transform[1];

            // Just add the first tank to the transform.
            targets[0] = m_Tanks[0].m_Instance.transform;

            // These are the targets the camera should follow.
            m_CameraControl.m_Targets = targets;
        }

        private void GoToMainMenu()
        {
            var mainMenuScreen = Instantiate(mainMenuScreenPrefab, transform);
            mainMenuScreen.gameManager = this;

            m_currentScreen = mainMenuScreen;
        }

        public void StartRound()
        {
            SpawnAllTanks();
            SetCameraTargets();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize();

            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl();

            Destroy(m_currentScreen);
            
            m_GameScreen = Instantiate(gameScreenPrefab, transform);
            m_GameScreen.gameManager = this;
            
            m_currentScreen = m_GameScreen;
        }

        private void EndRound()
        {
            // Stop tanks from moving.
            DisableTankControl();

            Destroy(m_currentScreen);
            
            m_currentScreen = Instantiate(endScreenPrefab, transform);
        }

        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }

        public void IncreasePlayer1Speed()
        {
            m_Player1Movement.m_Speed += 1;
        }

        public void StartRandomExplosions()
        {
            m_MaxFirestormCount--;
            if (m_MaxFirestormCount < 0)
                EndRound();
            StartCoroutine(Firestorm());
        }
    }
}