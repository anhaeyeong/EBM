using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField]
    public Gun currentGun;

    private float currentFireRate;

    private AudioSource audioSource;

    [SerializeField]
    public Camera m_cam;

    [SerializeField]
    public GameObject bullet;

    [SerializeField]
    public Transform firepoint;

    public Vector3 targetVec;
    public Vector3 magneticVec;
    public bool isTargetSet; // targetVec이 설정되었는지 여부를 나타내는 플래그

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ResetTargetVec();
    }

    void Update()
    {
        GunFireRateCalc();
        TryFire();
    }

    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime;
        }
    }

    private void TryFire()
    {
        if (Input.GetButtonDown("Fire1") && currentFireRate <= 0)
        {
            Fire();
        }
    }

    private void Fire()
    {
        PlaySE(currentGun.FireSound);
        currentFireRate = currentGun.firerate;
        Shoot();
    }

    public void Shoot()
    {
        shootBullet(firepoint);
    }

    private void shootBullet(Transform _firePoint)
    {
        var bulletObj = Instantiate(bullet, _firePoint.position, Quaternion.identity) as GameObject;
        var bulletComponent = bulletObj.GetComponent<Bullet>();

        // 화면 중앙을 향한 초기 방향 설정
        Ray ray = m_cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 initialDirection = ray.direction;
        bulletComponent.SetInitialDirection(initialDirection);

        // 타겟이 탐지된 경우 MagneticVec 설정
        if (isTargetSet && magneticVec != Vector3.zero)
        {
            bulletComponent.SetMagneticVec(magneticVec);
        }

        if (isTargetSet)
        {
            //Debug.Log("MagneticVec: " + magneticVec);
        }
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public void ResetTargetVec()
    {
        targetVec = Vector3.zero;
        magneticVec = Vector3.zero;
        isTargetSet = false;
    }
}
