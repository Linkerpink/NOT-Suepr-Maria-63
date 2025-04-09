using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StarSelectStar : MonoBehaviour
{
    private GameManager m_gameManager;
    private Button m_button;
    [SerializeField] private Star m_star;
    
    private void Awake()
    {
        m_gameManager = FindAnyObjectByType<GameManager>();
        m_button = GetComponent<Button>();
    }
    
    private void Start()
    {
        m_button.onClick.AddListener(() => m_gameManager.SelectStar(m_star));
    }

    private void Update()
    {
        print(EventSystem.current.currentSelectedGameObject);
        
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            print(m_star);
            m_gameManager.HoverOverStar(m_star);
        }
    }
}
