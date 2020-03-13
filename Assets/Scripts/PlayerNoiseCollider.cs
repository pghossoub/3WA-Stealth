using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNoiseCollider : MonoBehaviour
{
    private Enemy _enemy;
    private Transform _trPlayer;

    private void Awake()
    {
        _trPlayer = GetComponentInParent<Transform>();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().HeardSomething(_trPlayer);
        }
    }
}
