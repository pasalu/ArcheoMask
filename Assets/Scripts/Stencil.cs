using UnityEngine;
using UnityEngine.InputSystem;

public class Stencil : MonoBehaviour
{
    private InputSystem_Actions input;
    private bool clickedOnShape = false;
    private bool clickedOnStencil = false;

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
            var screenPosition = input.UI.Point.ReadValue<Vector2>();
            var worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            Collider2D hit = Physics2D.OverlapPoint(worldPosition);

            if (hit != null && hit.transform.IsChildOf(transform) && hit.transform != transform)
            {
                ShapeClicked();
                clickedOnShape = true;
                clickedOnStencil = false;
            }
            else if (hit != null && hit.transform == transform)
            {
                clickedOnStencil = true;
                clickedOnShape = false;
            }
            else
            {
                clickedOnShape = false;
            }
        }

        if (input.Player.Attack.IsPressed() && clickedOnStencil && !clickedOnShape)
        {
            Move();
        }
        
        if (input.Player.Attack.WasReleasedThisFrame())
        {
            clickedOnShape = false;
            clickedOnStencil = false;
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

    public void ShapeClicked()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            print("Child " + i + ": " + child.name);
        }
    }
}
