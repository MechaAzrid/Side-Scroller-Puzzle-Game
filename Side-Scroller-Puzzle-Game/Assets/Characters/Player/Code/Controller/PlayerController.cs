using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{
    [Header("Collision flags")]
    public CharacterController2D.CharacterCollisionState2D flags;

    [Header("Physics Variables")]
    [SerializeField]
    private float _walkSpeed = 6.0f;
    [SerializeField]
    private float _jumpSpeed = 8.0f;
    [SerializeField]
    private float _gravity = 20.0f;
    [SerializeField]
    private Vector3 _moveDirection = new Vector3(0, 0, 0);


    [Header("Player States")]
    [SerializeField]
    private bool _isGrounded;
    [SerializeField]
    private bool _isMovingRight;
    [SerializeField]
    private bool _isJumping;


    [Header("Components")]
    private CharacterController2D _characterController;

    // Awake is called after a prefab is instantiated
    private void Awake()
    {
        _characterController = GetComponent<CharacterController2D>();

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection.x = Input.GetAxis("Horizontal");
        _moveDirection.x *= _walkSpeed;

        // Player is on the ground
        if (_isGrounded)
        {
            _moveDirection.y = 0;
            _isJumping = false;

            if (_moveDirection.x > 0)
            {
                _isMovingRight = true;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else if (_moveDirection.x < 0)
            {
                _isMovingRight = false;
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                // IDLE
            }

            if (Input.GetButtonDown("Jump"))
            {
                _moveDirection.y = _jumpSpeed;
                _isJumping = true;
            }
        }
        // Player is in the air
        else
        {
            if (Input.GetButtonUp("Jump"))
            {
                if (_moveDirection.y > 0)
                {
                    _moveDirection.y = _moveDirection.y * 0.5f;
                }
            }
        }

        _moveDirection.y -= _gravity * Time.deltaTime;

        _characterController.move(_moveDirection * Time.deltaTime);

        flags = _characterController.collisionState;

        _isGrounded = flags.below;

        if (flags.above)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }
    }
}
