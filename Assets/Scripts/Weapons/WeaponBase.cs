using UnityEngine;

namespace TerrorCube.Weapons
{
    /// <summary>
    /// Abstract base class for all weapons.
    /// Handles ammunition, fire rate cooldowns, and input states.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Stats")]
        [Tooltip("Damage dealt per shot or per hit.")]
        [SerializeField] protected float damage = 10f;

        [Tooltip("Minimum time (in seconds) between shots.")]
        [SerializeField] protected float fireRate = 0.5f;

        [Tooltip("Maximum range of the weapon.")]
        [SerializeField] protected float range = 100f;

        /// <summary>
        /// Time stamp of when the weapon can next fire.
        /// </summary>
        protected float nextFireTime;

        /// <summary>
        /// Attempts to fire the weapon.
        /// Validates cooldowns and ammo before triggering the actual fire logic.
        /// </summary>
        /// <returns>True if the weapon successfully fired, false otherwise.</returns>
        public bool AttemptFire()
        {
            if (Time.time < nextFireTime)
            {
                return false;
            }

            nextFireTime = Time.time + fireRate;
            Fire();
            return true;
        }

        /// <summary>
        /// Core firing logic to be implemented by specific weapon types (Hitscan, Projectile, etc).
        /// </summary>
        protected abstract void Fire();
    }
}
