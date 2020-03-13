using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKick : MonoBehaviour
{
    private bool _playerIsHit = false;
    private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
    }
    private void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("Player") && !_playerIsHit)
        {
            _playerIsHit = true;
            _enemy.m_playerIsDead = true;
            other.GetComponent<Player>().IsHit();

        }
    }
}
