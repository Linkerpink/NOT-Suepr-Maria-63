using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool enableDebug = false;
    private bool lockCursor = true;
    public int transitionOutAnimation = -1;

    private GameObject m_canvas;
    private Animator m_backgroundAnimator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            enableDebug = !enableDebug;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = false;
        }

        if (Application.isFocused)
        {
            lockCursor = true;
        }

        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_canvas = GameObject.Find("Canvas");
        
        if (m_canvas != null)
        {
            m_backgroundAnimator = m_canvas.GetComponentInChildren<Animator>();
            if (m_backgroundAnimator != null)
            {
     //           m_backgroundAnimator.SetTrigger("transition", transitionOutAnimation);
            }
        }
    }

    public void StartTransitionAnimation(int _animation)
    {
        if (m_backgroundAnimator != null)
        {
       //     m_backgroundAnimator.SetTrigger("transition", _animation);
        }
    }
}