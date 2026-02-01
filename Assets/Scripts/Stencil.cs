using UnityEngine;
using UnityEngine.InputSystem;

public class Stencil : MonoBehaviour
{
    public InputAction clickAction;
    public InputAction moveAction;

    private InputSystem_Actions input;

    void Awake()
    {
        // Not sure why it's necessary to call enable, but without it the input doesn't work.
        input = new InputSystem_Actions();
        input.Player.Enable();
        input.UI.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (input.Player.Attack.WasPressedThisFrame())
        {
            Move();
        }
        
    }

    // Move the stencil like it's on a grid.
    private void Move() 
    {
        // Switch from (0,0) bottom-left to (0,0) center
        var screenPosition = input.UI.Point.ReadValue<Vector2>();
        var worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        print("Move: " + input.UI.Point.ReadValue<Vector2>());
        GetComponent<Transform>().position = new Vector3(
            worldPosition.x,
            worldPosition.y,
            GetComponent<Transform>().position.z
        );
    }
}
