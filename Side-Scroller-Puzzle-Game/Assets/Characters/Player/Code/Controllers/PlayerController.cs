using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private CharacterController2D _characterController;

    [Header("Collision flags")]
    [SerializeField]
    public CharacterController2D.CharacterCollisionState2D flags;

    #region Player Abilities
    [Header("Player Abilities")]
    [SerializeField]
    private bool _enableDoubleJump = true;
    [SerializeField]
    private bool _enableWallJump = true;
    [SerializeField]
    private bool _enableWallRun = true;
    [SerializeField]
    private bool _enableWallRunAfterWallJump = true;
    #endregion

    #region Physics Variables
    [Header("Physics Variables")]
    [SerializeField]
    private float _gravity = 20.0f;
    [SerializeField]
    private Vector3 _moveDirection = new Vector3(0, 0, 0);
    // Walking
    [SerializeField]
    private float _walkSpeed = 6.0f;
    [SerializeField]
    // Jumping
    private float _jumpSpeed = 8.0f;
    // Double Jumping
    [SerializeField]
    private float _doubleJumpSpeed = 8.0f;
    // Wall Jumping
    [SerializeField]
    private float _wallJumpXAmount = 1.0f;
    [SerializeField]
    private float _wallJumpYAmount = 1.4f;
    // Wall Running
    [SerializeField]
    private float _wallRunAmount = 2f;
    #endregion

    #region Player States
    [Header("Player States")]
    [SerializeField]
    private bool _isGrounded;
    [SerializeField]
    public bool isFacingRight;
    [SerializeField]
    private bool _isJumping;
    [SerializeField]
    private bool _doubleJumping;
    [SerializeField]
    private bool _wallJumped;
    [SerializeField]
    private bool _isWallRunning;
    #endregion

    #region Variable Checks
    private bool _wallJumpedRight;
    [SerializeField]
    private bool _canWallRun;

    #endregion


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
        if (_wallJumped == false)
        {
            _moveDirection.x = Input.GetAxis("Horizontal");
            _moveDirection.x *= _walkSpeed;
        }

        // Is Player on the ground
        if (_isGrounded)
        {
            // Values to reset once grounded
            _moveDirection.y = 0;
            _isJumping = false;
            _doubleJumping = false;           

            // Change is facing value depending on moving direction
            if (_moveDirection.x > 0)
            {
                RotatePlayer("right");
            }
            else if (_moveDirection.x < 0)
            {
                RotatePlayer("left");
            }
            else
            {
                // IDLE
            }

            // Jump 
            if (Input.GetButtonDown("Jump"))
            {
                _moveDirection.y = _jumpSpeed;
                _isJumping = true;
                // player can wallRun
                _canWallRun = true;
            }
        }

        // Is Player in the air
        else
        {


            // Did the player let go of jump button
            if (Input.GetButtonUp("Jump"))
            {
                if (_moveDirection.y > 0)
                {
                    _moveDirection.y = _moveDirection.y * 0.5f;
                }
            }

            // Did player press jump 
            if (Input.GetButtonDown("Jump"))
            {
                // has the ability to double jump..
                if (_enableDoubleJump)
                {
                    // hasn't double jumped already
                    if (!_doubleJumping)
                    {
                        _moveDirection.y = _doubleJumpSpeed;
                        _doubleJumping = true;
                    }
                }
            }
        }

        if (_isGrounded == false) { _moveDirection.y -= _gravity * Time.deltaTime; }
        

        _characterController.move(_moveDirection * Time.deltaTime);

        // Checking Player flags/Collision states
        flags = _characterController.collisionState;

        _isGrounded = flags.below;

        //Is there anything above the player
        if (flags.above)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }

        // Is there anything to the left or right of the player
        if (flags.left || flags.right)
        {
            if (_enableWallRun)
            {
                if (_canWallRun)
                {
                    if (_moveDirection.x > 0 && _canWallRun)
                    {
                        _moveDirection.y = _jumpSpeed / _wallRunAmount;
                        StartCoroutine("WallRunTimer");
                        RotatePlayer("right");
                    }
                    else if (_moveDirection.x < 0 && _canWallRun)
                    {
                        _moveDirection.y = _jumpSpeed / _wallRunAmount;
                        StartCoroutine("WallRunTimer");
                        RotatePlayer("left");
                    }
                }
            }
            if (_enableWallJump)
            {
                if (Input.GetButtonDown("Jump") && !_wallJumped && !_isGrounded)
                {
                    if (_moveDirection.x > 0)
                    {
                        _moveDirection.x = -_jumpSpeed * _wallJumpXAmount;
                        _moveDirection.y = _jumpSpeed * _wallJumpYAmount;
                        RotatePlayer("left");
                        _wallJumpedRight =true;

                    }
                    else if (_moveDirection.x < 0)
                    {
                        _moveDirection.x = _jumpSpeed * _wallJumpXAmount;
                        _moveDirection.y = _jumpSpeed * _wallJumpYAmount;
                        RotatePlayer("right");
                        _wallJumpedRight = false;

                    }

                    StartCoroutine("WallJumpTimer");
                }
            }
        }
        else
        {
            if (_enableWallRunAfterWallJump)
            {
                StopCoroutine("WallRunTimer");
                _canWallRun = true;
                
            }
        }
    }

    // rotate player based on direction : left or right
    public void RotatePlayer(string direction)
    {
        if (direction == "right")
        {
            isFacingRight = true;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == "left")
        {
            isFacingRight = false;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

    }


    #region Coroutines
    IEnumerator WallJumpTimer()
    {
        _wallJumped = true;
        yield return new WaitForSeconds(0.35f);
        _wallJumped = false;
    }

    IEnumerator WallRunTimer()
    {
        _isWallRunning = true;
        _canWallRun = true;
        yield return new WaitForSeconds(0.5f);
        _isWallRunning = false;
        _canWallRun = false;



    }

    #endregion
}
