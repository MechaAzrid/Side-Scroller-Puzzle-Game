using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    public float cameraZPos = -10f;
    public float cameraXOffset = 1f;
    public float cameraYOffset = 1f;

    public float horizontalSpeed = 1f;
    public float verticalSpeed = 3f;

    private Transform _camera;
    public PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();

        _camera = Camera.main.transform;

        _camera.position = new Vector3(
            player.transform.position.x + cameraXOffset,
            player.transform.position.x + cameraYOffset,
            player.transform.position.x + cameraZPos);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.isFacingRight)
        {
            _camera.position = new Vector3(
                Mathf.Lerp(_camera.position.x, player.transform.position.x + cameraXOffset, horizontalSpeed * Time.deltaTime),
                Mathf.Lerp(_camera.position.y, player.transform.position.y + cameraYOffset, verticalSpeed * Time.deltaTime),
                cameraZPos);
        }
        else
        {
            _camera.position = new Vector3(
                Mathf.Lerp(_camera.position.x, player.transform.position.x - cameraXOffset, horizontalSpeed * Time.deltaTime),
                Mathf.Lerp(_camera.position.y, player.transform.position.y + cameraYOffset, verticalSpeed * Time.deltaTime),
                cameraZPos);
        }
    }
}
