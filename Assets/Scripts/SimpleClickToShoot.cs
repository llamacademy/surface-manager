using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class SimpleClickToShoot : MonoBehaviour
{
    private Camera Camera;
    [SerializeField]
    private Transform Gun;
    [SerializeField]
    private ImpactType ImpactType;
    [SerializeField]
    private float MouseSensitivity = 0.5f;
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private PoolableObject BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.1f;
    [SerializeField]
    private float Speed = 100;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private bool BouncingBullets;
    [SerializeField]
    private float BounceDistance = 10f;

    private float LastShootTime;

    private Vector2 LastMousePosition;
    private bool CaptureMouse = true;
    private Vector3 CameraRotation;

    private void Awake()
    {
        Camera = GetComponent<Camera>();
        CameraRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (Application.isFocused && Mouse.current.leftButton.isPressed)
        {
            CaptureMouse = true;
            Shoot();
        }
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (Application.isFocused && CaptureMouse)
        {
            Vector2 mouseMovementThisFrame = LastMousePosition - mousePosition;

            CameraRotation = new Vector3(
                Mathf.Clamp(CameraRotation.x + MouseSensitivity * mouseMovementThisFrame.y, -35, 45),
                Mathf.Clamp(CameraRotation.y + MouseSensitivity * -mouseMovementThisFrame.x, -75, 75),
                transform.rotation.eulerAngles.z
            );

            transform.rotation = Quaternion.Euler(CameraRotation);
        }
        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            CaptureMouse = false;
        }

        LastMousePosition = mousePosition;
    }

    private void LateUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, float.MaxValue, Mask))
        {
            Gun.LookAt(hit.point);
        }
    }

    public void Shoot()
    {
        if (LastShootTime + ShootDelay < Time.time)
        {
            ShootingSystem.Play();

            Vector3 direction = BulletSpawnPoint.forward;
            ObjectPool trailPool = ObjectPool.CreateInstance(BulletTrail, 25);
            PoolableObject instance = trailPool.GetObject();
            instance.transform.position = BulletSpawnPoint.position;
            TrailRenderer trail = instance.GetComponent<TrailRenderer>();

            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                StartCoroutine(SpawnTrail(trail, hit.collider, hit.point, hit.normal, hit.triangleIndex, BounceDistance, true));
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, null, direction * 100, Vector3.zero, 0, BounceDistance, false));
            }

            LastShootTime = Time.time;
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Collider HitCollider, Vector3 HitPoint, Vector3 HitNormal, int TriangleIndex, float BounceDistance, bool MadeImpact)
    {
        Vector3 startPosition = Trail.transform.position;
        Vector3 direction = (HitPoint - Trail.transform.position).normalized;

        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float startingDistance = distance;

        while (distance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * Speed;

            yield return null;
        }

        Trail.transform.position = HitPoint;

        if (MadeImpact)
        {
            SurfaceManager.Instance.HandleImpact(HitCollider.gameObject, HitPoint, HitNormal, ImpactType, TriangleIndex);

            if (BouncingBullets && BounceDistance > 0)
            {
                Vector3 bounceDirection = Vector3.Reflect(direction, HitNormal);

                if (Physics.Raycast(HitPoint, bounceDirection, out RaycastHit hit, BounceDistance, Mask))
                {
                    yield return StartCoroutine(SpawnTrail(
                        Trail,
                        hit.collider,
                        hit.point,
                        hit.normal,
                        hit.triangleIndex,
                        BounceDistance - Vector3.Distance(hit.point, HitPoint),
                        true
                    ));
                }
                else
                {
                    yield return StartCoroutine(SpawnTrail(
                        Trail,
                        null,
                        bounceDirection * BounceDistance,
                        Vector3.zero,
                        0,
                        0,
                        false
                    ));
                }
            }
        }

        StartCoroutine(DisableTrail(Trail, Time.time));
    }

    private IEnumerator DisableTrail(TrailRenderer Instance, float Time)
    {
        yield return new WaitForSeconds(Time);
        Instance.gameObject.SetActive(false);
    }
}
