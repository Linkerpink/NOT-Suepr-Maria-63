using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StarSelectStar : MonoBehaviour
{
    private GameManager m_gameManager;
    private Button m_button;
    [SerializeField] private Star m_star;
    private Image m_image;
    
    private void Awake()
    {
        m_gameManager = FindAnyObjectByType<GameManager>();
        m_button = GetComponent<Button>();
        m_image = GetComponent<Image>();
    }
    
    private void Start()
    {
        m_button.onClick.AddListener(() => m_gameManager.SelectStar(m_star));
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            m_gameManager.HoverOverStar(m_star);
        }
        
        if (m_gameManager.starsCollected.Contains(m_star))
        {
            m_image.color = Color.white;
        }
        else
        {
            m_image.color = Color.gray;
        }
    }
}
