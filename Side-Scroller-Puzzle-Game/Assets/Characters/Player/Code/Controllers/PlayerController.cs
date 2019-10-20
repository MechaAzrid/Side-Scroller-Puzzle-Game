using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{


    [Header("Components")]
    [SerializeField]
    private CharacterController2D _characterController;

    #region Environment Variables

    private enum groundType
    {
        None, // in air 
        LevelGeometry, // level geometry
        OneWayPlatform // 1way platforms
    }
    [SerializeField]
    private groundType _groundType;
    #endregion

    [Header("Collision flags")]
    private CharacterController2D.CharacterCollisionState2D flags;

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
    private float _gravity = 35.0f;
    private float _gravityAcceleration = 1.9f;
    [SerializeField]
    private Vector3 _moveDirection = new Vector3(0, 0, 0);
    //Up/Down input
    private float _verticalInput = 0;
    // Walking
    [SerializeField]
    private float _walkSpeed = 16.0f;
    // Jumping
    [SerializeField]
    private float _jumpSpeed = 25f;
    // Double Jumping
    [SerializeField]
    private float _doubleJumpSpeed = 20f;
    // Wall Jumping
    [SerializeField]
    private float _wallJumpPower = 18.0f;
    [SerializeField]
    private float _wallJumpXAmount = 1.0f;
    [SerializeField]
    private float _wallJumpYAmount = 1.1f;
    // Wall Running
    [SerializeField]
    private float _wallRunSpeed = 18f;
    [SerializeField]
    private float _wallSlideSpeed = 3f;
    // length of ground type detection ray
    private float GroundCheckRayLength = 3.5f;
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
    [SerializeField]
    private bool _isWallSliding;
    #endregion

    #region Variable Checks
    [SerializeField]
    private bool _canWallRun;
    private int previousWallSideNumber;
    private int currentWallSideNumber;
    [SerializeField]
    private LayerMask layerMask;

    private float jumpPressedRemember = 0f;
    private float jumpPressedRememberTime = 0.2f;

    private float groundedRemember = 0f;
    private float groundedRememberTime = 0.15f;

    #endregion


    #region Platforms
    [SerializeField]
    private GameObject _tempOneWayPlatform;
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
        GetGroundType();

        jumpPressedRemember -= Time.deltaTime;
        groundedRemember -= Time.deltaTime;

        if (_wallJumped == false)
        {
            _moveDirection.x = Input.GetAxis("Horizontal");
            _moveDirection.x *= _walkSpeed;

            _verticalInput = Input.GetAxis("Vertical");
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedRemember = jumpPressedRememberTime;

        }

        // Is Player on the ground
        if (_isGrounded)
        {
            // Values to reset once grounded
            GroundVariableReset();

            groundedRemember = groundedRememberTime;

            // Change is facing value depending on moving direction
            if (_moveDirection.x > 0)
            {
                RotatePlayer("right");
            }
            else if (_moveDirection.x < 0)
            {
                RotatePlayer("left");
            }
            // IDLE
            else
            {

            }

            if (_verticalInput < 0)
            {
                if (_groundType == groundType.OneWayPlatform)
                {
                    print("DO IT");
                    StartCoroutine(DisableOneWayPlatform());
                }
            }

            // Jump 
            if (jumpPressedRemember > 0 && groundedRemember > 0)
            {
                Jump();
            }
        }

        // Is Player in the air
        else
        {
            // Variable Resets
            _isWallSliding = false;

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


            // Did the player let go of jump button
            if (Input.GetButtonUp("Jump"))
            {
                if (_moveDirection.y > 0)
                {
                    _moveDirection.y = _moveDirection.y * 0.4f;
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
                        DoubleJump();
                    }
                }
            }
        }

        _characterController.move(_moveDirection * Time.deltaTime);

        // Checking Player flags/Collision states
        flags = _characterController.collisionState;

        // Hopefully this doesn't bite me in the ass later :'(
        if (_moveDirection.y != 0 || _moveDirection.x != 0)
        {
            _isGrounded = flags.below;
        }

        if (!_isGrounded)
        {
            if (_moveDirection.y > -60)
            {
                _moveDirection.y -= (_gravity * Time.deltaTime) * _gravityAcceleration;
            }
            else
            {
                _moveDirection.y = -60;
            }
           
        }
        //moveDirection.y -= (gravity * Time.deltaTime) * 2.1f;

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
                WallSlide();

                WallRun();
            }
            if (_enableWallJump)
            {
                if (Input.GetButtonDown("Jump") && !_wallJumped && !_isGrounded)
                {
                    WallJump();
                }
            }
        }
        // if there is nothing to the left or right of the player
        else
        {

            // variable reset
            _isWallSliding = false;

            if (_enableWallRunAfterWallJump)
            {
                StopCoroutine("WallRunTimer");
                _canWallRun = true;
            }

            _isWallRunning = false;
            // No longer touching well, therefore set previous wall to equal the wall that was just run/jumped from
            previousWallSideNumber = currentWallSideNumber;
        }
    }


    void GroundVariableReset()
    {
        _moveDirection.y = 0;
        _isJumping = false;
        _doubleJumping = false;
        previousWallSideNumber = 0;
        currentWallSideNumber = 0;
        _isWallSliding = false;
    }

    void Jump()
    {
        jumpPressedRemember = 0f;
        groundedRemember = 0f;

        _moveDirection.y = _jumpSpeed;
        _isJumping = true;
        _canWallRun = true;
    }

    void DoubleJump()
    {
        _moveDirection.y = _jumpSpeed;//_doubleJumpSpeed;
        _doubleJumping = true;
    }

    void WallJump()
    {
        if (_moveDirection.x > 0)
        {
            _moveDirection.x = -_wallJumpPower * _wallJumpXAmount;
            _moveDirection.y = _wallJumpPower * _wallJumpYAmount;
            RotatePlayer("left");
        }
        else if (_moveDirection.x < 0)
        {
            _moveDirection.x = _wallJumpPower * _wallJumpXAmount;
            _moveDirection.y = _wallJumpPower * _wallJumpYAmount;
            RotatePlayer("right");
        }

        StartCoroutine("WallJumpTimer");
    }

    void WallRun()
    {

        if (_canWallRun)
        {
            if (_moveDirection.x > 0 && _canWallRun && previousWallSideNumber != 2)
            {
                _moveDirection.y = _wallRunSpeed;
                StartCoroutine("WallRunTimer");
                RotatePlayer("right");
                currentWallSideNumber = 2;
            }
            else if (_moveDirection.x < 0 && _canWallRun && previousWallSideNumber != 1)
            {
                _moveDirection.y = _wallRunSpeed;
                StartCoroutine("WallRunTimer");
                RotatePlayer("left");
                currentWallSideNumber = 1;
            }
        }
        else
        {
            if (_moveDirection.x > 0)
            {
                RotatePlayer("right");
            }
            else if (_moveDirection.x < 0)
            {
                RotatePlayer("left");
            }
        }
    }

    void WallSlide()
    {
        if (_moveDirection.y < -0.4)
        {
            _isWallSliding = true;
            _moveDirection.y = -(_wallSlideSpeed);

        }
        else
        {
            _isWallSliding = false;
        }
    }

    // rotate player based on direction : left or right
    void RotatePlayer(string direction)
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

    void GetGroundType()
    {
        RaycastHit2D groundhit = Physics2D.Raycast(transform.position, -Vector3.up, GroundCheckRayLength, layerMask);

        if (groundhit)
        {

            string layerName = LayerMask.LayerToName(groundhit.transform.gameObject.layer);

            if (layerName == "OneWayPlatform")
            {
                _groundType = groundType.OneWayPlatform;
                if (!_tempOneWayPlatform)
                {
                    _tempOneWayPlatform = groundhit.transform.gameObject;
                }
            }
            else if (layerName == "LevelGeometry")
            {
                _groundType = groundType.LevelGeometry;
            }
            else
            {
                _groundType = groundType.None;
            }
        }
        else
        {
            //print("NOT HIT");
            _groundType = groundType.None;
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

    IEnumerator DisableOneWayPlatform()
    {
        if (_tempOneWayPlatform)
        {
            _tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false;
        }
        yield return new WaitForSeconds(0.5f);

        if (_tempOneWayPlatform)
        {
            _tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true;
            _tempOneWayPlatform = null;
        }
    }
    #endregion
}
