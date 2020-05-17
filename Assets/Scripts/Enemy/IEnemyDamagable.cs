using UnityEngine;

namespace Enemy
{
    public interface IEnemyDamagable
    {
        Transform transform { get; }
        void TakeDamage(Vector3 hitpoint, bool isCrit, bool isBlue);
        void Die(Vector3 hitpoint, bool isCrit);
    }
}