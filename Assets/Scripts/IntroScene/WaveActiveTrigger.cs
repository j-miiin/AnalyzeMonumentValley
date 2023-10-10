using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveActiveTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _waveSprite;

    private void OnCollisionEnter(Collision collision)
    {
        _waveSprite.SetActive(true);
    }
}
