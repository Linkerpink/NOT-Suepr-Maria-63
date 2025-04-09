using UnityEngine;
using UnityEngine.UI;

public class StarSelect : MonoBehaviour
{
    [SerializeField] private GameObject[] m_menuStars;
    
    private int m_selectedStar = 0;

    private void Start()
    {
        HoverStar();
    }

    private void HoverStar()
    {
        for (int i = 0; i < m_menuStars.Length; i++)
        {
            if (m_selectedStar == i)
            {
                m_menuStars[i].GetComponent<Image>().color = Color.white;
                m_menuStars[i].gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            }
            else
            {
                m_menuStars[i].GetComponent<Image>().color = Color.grey;
                m_menuStars[i].gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
