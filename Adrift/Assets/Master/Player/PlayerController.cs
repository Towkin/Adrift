using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrift.Game
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private float 
            m_MoveSpeed = 10f,
            m_MaxSpeed = 5.0f;

        


        [SerializeField]
        private Camera m_PlayerCamera = null;

        public Rigidbody PlayerBody { get; private set; }
        private void Awake()
        {
            PlayerBody = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            var accelerationVector = m_PlayerCamera.transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            PlayerBody.AddForce(accelerationVector * m_MoveSpeed, ForceMode.Acceleration);
            if (PlayerBody.velocity.sqrMagnitude > m_MaxSpeed * m_MaxSpeed)
                PlayerBody.velocity = PlayerBody.velocity.normalized * m_MaxSpeed;

            m_PlayerCamera.transform.rotation = Quaternion.Euler(m_PlayerCamera.transform.eulerAngles + new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f));
        }
    }
}