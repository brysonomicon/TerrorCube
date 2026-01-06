using UnityEngine;
using UnityEngine.InputSystem;
using TerrorCube.Weapons;

namespace TerrorCube.Player
{
    /// <summary>
    /// Handles player input for weapon systems.
    /// Manages the currently equipped weapon and triggers firing logic.
    /// </summary>
    public class PlayerWeaponController : MonoBehaviour
    {
        [Header("Loadout")]
        [Tooltip("The weapon currently in hand.")]
        [SerializeField] private WeaponBase startingWeapon;

        private WeaponBase _activeWeapon;
        private bool _isFiring;

        /// <summary>
        /// Initializes the weapon controller.
        /// </summary>
        private void Start()
        {
            if (startingWeapon != null)
            {
                EquipWeapon(startingWeapon);
            }
        }

        /// <summary>
        /// Updates weapon state.
        /// Handles 'Hold to Fire' logic by attempting to fire every frame input is held.
        /// The WeaponBase handles the actual fire rate/cooldown.
        /// </summary>
        private void Update()
        {
            if (_isFiring && _activeWeapon != null)
            {
                _activeWeapon.AttemptFire();
            }
        }

        /// <summary>
        /// Input System Callback for "Fire" action.
        /// </summary>
        /// <param name="context">Input context containing button state.</param>
        public void OnFire(InputAction.CallbackContext context)
        {
            _isFiring = context.ReadValueAsButton();
        }

        /// <summary>
        /// Equips a new weapon and sets it as active.
        /// </summary>
        /// <param name="newWeapon">The weapon instance to equip.</param>
        public void EquipWeapon(WeaponBase newWeapon)
        {
            _activeWeapon = newWeapon;
        }

        private void OnGUI()
        {
            float size = 5f;
            GUI.Box(new Rect(Screen.width / 2f - size / 2f, Screen.height / 2f - size / 2f, size, size), "");
        }
    }
}
