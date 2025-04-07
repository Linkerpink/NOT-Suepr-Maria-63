using System;
using UnityEngine;

public class ChainManager : MonoBehaviour
{
    [SerializeField] private GameObject[] chains;

    [SerializeField] private GameObject chainChomp;
    [SerializeField] private GameObject pole;

    private void Update()
    {
        for (int i = 0; i < chains.Length; i++)
        {
            float distance = Vector3.Distance(pole.transform.position, chainChomp.transform.position);
            chains[i].transform.position = pole.transform.position + new Vector3(distance, 0F, distance);
        }
    }
}
