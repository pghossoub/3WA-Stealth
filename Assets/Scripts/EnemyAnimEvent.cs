using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimEvent : MonoBehaviour
{
    [SerializeField]
    private GameObject _colliderKick;

    private Enemy _enemy;
    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
    }
    public void StartAttack()
    {
        _colliderKick.SetActive(true);
    }

    public void StopAttack()
    {
        _colliderKick.SetActive(false);
    }
}
