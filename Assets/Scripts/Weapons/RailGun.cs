using System.Collections;
using UnityEngine;
using TerrorCube.Core;

namespace TerrorCube.Weapons
{
    /// <summary>
    /// Implementation of the Hitscan Rail Gun.
    /// Fires a high-velocity beam that deals immediate damage.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RailGun : WeaponBase
    {
        [Header("Rail Gun Visuals")]
        [Tooltip("How long the trail remains visible after firing.")]
        [SerializeField] private float beamDuration = 0.2f;

        [Tooltip("Width of the laser beam.")]
        [SerializeField] private float beamWidth = 0.05f;
        
        [Tooltip("The start point of the beam. If null, uses this transform.")]
        [SerializeField] private Transform muzzlePoint;

        [Header("Collision")]
        [Tooltip("Layers to hit. Uncheck 'Player' in Inspector to avoid shooting yourself.")]
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("References")]
        [Tooltip("The camera used for aiming. If null, attempts to find Camera.main.")]
        [SerializeField] private Camera aimCamera;

        private LineRenderer _lineRenderer;

        /// <summary>
        /// Initializes references and forces correct LineRenderer settings.
        /// </summary>
        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            
            // Fallback to Main Camera if not assigned
            if (aimCamera == null) aimCamera = Camera.main;

            _lineRenderer.enabled = false;
            _lineRenderer.startWidth = beamWidth;
            _lineRenderer.endWidth = beamWidth;
            _lineRenderer.useWorldSpace = true;
        }

        private void OnValidate()
        {
            // Allow tuning width in Editor
            if (_lineRenderer != null)
            {
                _lineRenderer.startWidth = beamWidth;
                _lineRenderer.endWidth = beamWidth;
            }
        }

        /// <summary>
        /// Executes the raycast and handles damage/visuals.
        /// </summary>
        protected override void Fire()
        {
            // Final safety check for camera
            if (aimCamera == null) aimCamera = Camera.main;
            if (aimCamera == null)
            {
                Debug.LogError("RailGun: No Camera found to aim with!");
                return;
            }

            // Determine ray origin (center of screen)
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            // DEBUG: Visualize the ray in Scene view (Yellow line appearing for 1 second)
            // This helps confirm WHICH camera is shooting and where it thinks it is looking
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.yellow, 1.0f);

            RaycastHit hit;
            Vector3 endPoint;

            // Perform Raycast with LayerMask and ignoring Triggers
            if (Physics.Raycast(ray, out hit, range, hitLayers, QueryTriggerInteraction.Ignore))
            {
                endPoint = hit.point;
                
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
            else
            {
                endPoint = ray.GetPoint(range);
            }

            StartCoroutine(ShowBeamRoutine(endPoint));
        }

        /// <summary>
        /// Coroutine to display the cosmetic laser beam.
        /// </summary>
        /// <param name="targetPoint">The world position where the beam ends.</param>
        private IEnumerator ShowBeamRoutine(Vector3 targetPoint)
        {
            _lineRenderer.enabled = true;
            
            // Use muzzle point if defined, otherwise current transform
            Vector3 startPos = muzzlePoint != null ? muzzlePoint.position : transform.position;
            
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, targetPoint);

            yield return new WaitForSeconds(beamDuration);

            _lineRenderer.enabled = false;
        }
    }
}
