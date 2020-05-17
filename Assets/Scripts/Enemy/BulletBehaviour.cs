using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Player;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.AddHealth(-GameSettings.Instance.bulletDamage);
            StartCoroutine(ObjectPooler.Instance.CustomDestroy(gameObject));
        }
    }
}
