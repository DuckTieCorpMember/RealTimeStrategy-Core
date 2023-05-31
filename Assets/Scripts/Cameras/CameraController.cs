using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThikness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;

    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        if (!isOwned || !Application.isFocused) { return; }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        if(previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;
            Vector2 cursosPosition = Mouse.current.position.ReadValue();

            if(cursosPosition.y >= Screen.height - screenBorderThikness)
            {
                cursorMovement.z += 1;
            } else if(cursosPosition.y <= screenBorderThikness)
            {
                cursorMovement.z -= 1;
            }
            if (cursosPosition.x >= Screen.width - screenBorderThikness)
            {
                cursorMovement.x += 1;
            }
            else if (cursosPosition.x <= screenBorderThikness)
            {
                cursorMovement.x -= 1;
            }

            pos += cursorMovement.normalized * speed * Time.deltaTime;
        } else
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = pos;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx) 
    {
        previousInput = ctx.ReadValue<Vector2>();
    }
}
