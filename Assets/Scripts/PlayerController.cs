using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float walkSpeed;

    [SerializeField]
    private float lookSensitivity;

    [SerializeField]
    private float camRotationLimit;
    private float currentCamRotationX = 0.0f;

    [SerializeField]
    private Camera m_cam;
    private Rigidbody m_rigid;


    // Start is called before the first frame update
    void Start()
    {
        m_rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        CameraRotation();
        CharacterRotation();

    }

    private void Move()
    {
        float _movedirX = Input.GetAxisRaw("Horizontal");
        float _movedirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _movedirX;
        Vector3 _moveVertical = transform.forward * _movedirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * walkSpeed;
        

        m_rigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCamRotationX -= _cameraRotationX;
        currentCamRotationX = Mathf.Clamp(currentCamRotationX, -camRotationLimit, camRotationLimit);

        m_cam.transform.localEulerAngles = new Vector3(currentCamRotationX, 0f, 0f); 

    }

    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        m_rigid.MoveRotation(m_rigid.rotation * Quaternion.Euler(_characterRotationY));
    }
}
