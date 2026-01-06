namespace TerrorCube.Core
{
    /// <summary>
    /// Contract for any entity that can take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage to the entity.
        /// </summary>
        /// <param name="amount">The amount of health to subtract.</param>
        void TakeDamage(float amount);
    }
}
