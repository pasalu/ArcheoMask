using UnityEngine;
using UnityEngine.InputSystem;

public class Stencil : MonoBehaviour
{
    [SerializeField] private Collider2D tabletBounds;

    private GameObject shapes;
    private InputSystem_Actions input;
    private bool clickedOnShape = false;
    private bool clickedOnStencil = false;

    void Awake()
    {
        // Not sure why it's necessary to call enable, but without it the input doesn't work.
        input = new InputSystem_Actions();
        input.Player.Enable();
        input.UI.Enable();

        tabletBounds = GameObject.Find("TabletBounds").GetComponent<Collider2D>();
    }

    void Start()
    {
        shapes = GameObject.Find("Shapes");
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
                ShapeClicked(worldPosition);
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

    private void Move()
    {
        var mouseScreenPosition = input.UI.Point.ReadValue<Vector2>();
        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        if (tabletBounds == null) return;

        Bounds bounds = tabletBounds.bounds;
        Bounds stencilBounds = GetComponent<SpriteRenderer>().bounds;
        float halfWidth = stencilBounds.extents.x;
        float halfHeight = stencilBounds.extents.y;

        // Snap to grid relative to tablet corner
        float gridSize = 1f;
        float originX = bounds.min.x + halfWidth;
        float originY = bounds.min.y + halfHeight;
        float x = originX + Mathf.Round((mouseWorldPosition.x - originX) / gridSize) * gridSize;
        float y = originY + Mathf.Round((mouseWorldPosition.y - originY) / gridSize) * gridSize;

        // Clamp to tablet bounds
        x = Mathf.Clamp(x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        y = Mathf.Clamp(y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

        transform.position = new Vector3(x, y, transform.position.z);
    }

    public void ShapeClicked(Vector2 worldPosition)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            print("Child " + i + ": " + child.name);
            Instantiate(child.gameObject, worldPosition, Quaternion.identity, shapes.transform);
        }
    }
}
