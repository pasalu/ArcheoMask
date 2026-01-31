using UnityEngine;
using UnityEngine.InputSystem;

public class Stencil : MonoBehaviour
{
    public InputAction clickAction;
    public InputAction moveAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (clickAction.triggered)
        {
            Move();
            print("Moved Stencil to: " + GetComponent<Transform>().position);
        }
        
    }

    // Move the stencil like it's on a grid.
    private void Move() 
    {
        GetComponent<Transform>().position = new Vector3(
            Mathf.Round(GetComponent<Transform>().position.x),
            Mathf.Round(GetComponent<Transform>().position.y),
            Mathf.Round(GetComponent<Transform>().position.z)
        );
    }
}
