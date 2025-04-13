using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
    public Star currentStar;
    [SerializeField] private GameObject m_starPrefab;
    
    [SerializeField] private TextMeshProUGUI m_starText;
    
    private Mario m_mario;
    
    private PlayerInput m_playerInput;

    public bool cannonsOpened = false;
    
    // UI
    private TextMeshProUGUI m_starsText;
    
    private void Awake()
    {
        /*
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        */
        
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;

        m_playerInput = GetComponent<PlayerInput>();
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
        
        // UI
        if (m_starsText != null)
        {
            m_starsText.SetText("X" + starsCollected.Count);
        }

        // Application
        if (Application.isFocused)
        {
            lockCursor = true;
        }

        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetObjectsWhenReady());
    }

    private void SetObjects()
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

                if (_child.name == "Star Select")
                {
                    SetStarText();
                }
            }
        }
        
        m_starSelect = GameObject.Find("Star Select");
        m_starObjects = GameObject.Find("Star Objects");
        
        m_mario = FindAnyObjectByType<Mario>();

        if (m_mario != null)
        {
            if (m_starSelect != null)
            {
                m_mario.canMove = false;   
            }
            else
            {
                m_mario.canMove = true;    
            }
        }
        
        m_starsText = GameObject.Find("Stars Text").GetComponent<TextMeshProUGUI>();
    }
    
    private IEnumerator SetObjectsWhenReady()
    {
        while (GameObject.Find("Canvas") == null)
        {
            yield return null;
        }
        SetObjects();
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
        currentStar = _star;
        
        /*
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is destroyed!");
            return null;
        }

        if (m_starSelect == null || m_starObjects == null)
        {
            Debug.LogError("SelectStar: Required objects are not initialized!");
            return null;
        }
        */
        
        return null;
    }
    
    public IEnumerator InitializeLevel(Star _star)
    {
        if (m_starSelect == null && m_starObjects == null)
        {
            Debug.Log("zelfm,orod");
            SetObjects();
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            SetObjects();
            m_starSelect.SetActive(false);
            m_starObjects.SetActive(true);
            m_mario.canMove = true;

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

            if (_star.name == "Star 3")
            {
                foreach (Transform _child in m_starObjects.transform)
                {
                    if (_child.name == "Star 3 Objects")
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
    }

    public void HoverOverStar(Star _star)
    {
        m_canvas = FindAnyObjectByType<Canvas>().gameObject;
        SetStarText();
        m_starText.SetText(_star.starName);
    }

    private void SetStarText()
    {
        GameObject _starText = GameObject.Find("Star Name Text");

        if (_starText != null)
        {
            m_starText = GameObject.Find("Star Name Text").GetComponent<TextMeshProUGUI>();    
        }
    }

    public void SpawnStar(Star _star, Vector3 _position)
    {
        GameObject _starObject = Instantiate(m_starPrefab, new Vector3(_position.x, _position.y, _position.z), Quaternion.identity);
    
        _starObject.GetComponentInChildren<StarHolder>().star = _star;
    }
    
    public void GetStar(Star _star)
    {
        if (!starsCollected.Contains(_star))
        {
            starsCollected.Add(_star);
        }
        
        currentStar = null;
        
        print("starsCollected: " + starsCollected);
    }
}