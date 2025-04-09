using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool enableDebug = false;
    private bool lockCursor = true;
    [HideInInspector]
    public string transitionOutAnimation = "fadeOutWhite";

    private GameObject m_canvas;
    [SerializeField] private Animator m_backgroundAnimator;
    
    // Stars
    public Star[] stars;
    public List<Star> starsCollected;
    private GameObject m_starSelect;
    private GameObject m_starObjects;
    
    [SerializeField] private TextMeshProUGUI m_starText;
    
    private Mario m_mario;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (m_mario != null)
            {
                m_mario.Die();
            }
            
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
            foreach (Transform _child in m_canvas.transform)
            {
                if (_child.name == "Background")
                {
                    m_backgroundAnimator = _child.GetComponent<Animator>();
                    StartTransitionAnimation("fadeOutWhite");
                }
            }
                
            m_starText = m_canvas.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        m_starSelect = GameObject.Find("Star Select");
        m_starObjects = GameObject.Find("Star Objects");

        if (m_starObjects != null)
        {
            m_starObjects.SetActive(false);
        }
        
        m_mario = FindAnyObjectByType<Mario>();
    }

    public void StartTransitionAnimation(string _animation)
    {
        if (m_backgroundAnimator != null)
        {
            m_backgroundAnimator.SetTrigger(_animation);
        }
    }

    public UnityAction SelectStar(Star _star)
    {
        StartCoroutine(InitializeLevel(_star));
        return null;    
    }

    public IEnumerator<Star> InitializeLevel(Star _star)
    {
        m_starSelect.SetActive(false);
        m_starObjects.SetActive(true);

        foreach (Transform _child in m_starObjects.transform)
        {
            _child.gameObject.SetActive(false);
        }
        
        if (_star.name == "Star 1")
        {
            foreach (Transform _child in m_starObjects.transform)
            {
                if (_child.name == "Star 1 Objects")
                {
                    _child.gameObject.SetActive(true);
                    
                    foreach (Transform _child2 in _child.transform)
                    {
                        _child2.gameObject.SetActive(true);
                    }
                }
                else
                {
                    _child.gameObject.SetActive(false);
                }
            }
        }
        
        if (_star.name == "Star 2")
        {
            foreach (Transform _child in m_starObjects.transform)
            {
                if (_child.name == "Star 2 Objects")
                {
                    _child.gameObject.SetActive(true);
                    
                    foreach (Transform _child2 in _child.transform)
                    {
                        _child2.gameObject.SetActive(true);
                    }
                }
                else
                {
                    _child.gameObject.SetActive(false);
                }
            }
        }
        yield return null;
    }

    public void HoverOverStar(Star _star)
    {
        m_canvas = FindAnyObjectByType<Canvas>().gameObject;
        m_starText = m_canvas.GetComponentInChildren<TextMeshProUGUI>();
        m_starText.SetText(_star.starName);
    }

    public void GetStar()
    {
        //starsCollected.Add();
    }
}