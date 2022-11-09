using UnityEngine;

public class Blade : MonoBehaviour
{
    public Vector3 direction { get; private set; }

    private Camera mainCamera;

    private Collider sliceCollider;
    private TrailRenderer sliceTrail;

    public float sliceForce = 5f;
    public float minSliceVelocity = 0.01f;

    private bool slicing;

    //Key Inputs
    bool MouseButtonDown0 = false;
    bool MouseButtonUp0 = false;
    bool CheckSliceLength = false;

    // Creating a Box
    public GameObject box;
    Vector3 PointA;
    Vector3 PointB;
    float width = 0;
    float height = 0;

    // Timer Slice
    float timer = 0;
    Vector3 OldPosition;

    //Box Casting
    bool hit = false;
    Vector3 Coordinate = new();
    public LayerMask fruitLayer;
    public Vector3 size = new();


    private void Awake()
    {
        mainCamera = Camera.main;
        sliceCollider = GetComponent<Collider>();
        sliceTrail = GetComponentInChildren<TrailRenderer>();
        box.SetActive(false);
        box = Instantiate(box);
        OldPosition = transform.position;
    }

    private void OnEnable()
    {
        StopSlice();
    }

    private void OnDisable()
    {
        StopSlice();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            box.SetActive(true);
            PointA = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            PointB = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            CheckSliceLength = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            MouseButtonUp0 = true;
        }

        if (Input.GetMouseButton(0)) // while we're holding the mouse down and creating the box
        {
            PointB = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            CreateBox(PointA, PointB);
        }

        if (width + height > 1.5f && CheckSliceLength==true)
        {
            CheckSliceLength = false;
            MouseButtonDown0 = true;
        }


    }
    private void FixedUpdate()
    {

        if (MouseButtonDown0)
        {
            MouseButtonDown0 = false;
            StartSlice();
        }
        else if (MouseButtonUp0)
        {
            MouseButtonUp0 = false;
            StopSlice();
        }

        if (slicing)
        {
            ContinueSlice();
        }
   
    }

    private void StartSlice()
    {
        Vector3 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0f;
        transform.position = position;

        slicing = true;
        sliceCollider.enabled = true;
        sliceTrail.enabled = true;
        sliceTrail.Clear();
    }

    private void StopSlice()
    {
        slicing = false;
        sliceCollider.enabled = false;
        sliceTrail.enabled = false;

        box.SetActive(false);
        width = 0;
        height = 0;
        timer = 0;
    }


    private void ContinueSlice()
    {
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0f;

        direction = newPosition - transform.position;
        float velocity = minSliceVelocity;


        // not sure if we want min speed on slice based on a fixed time disabled it for now. Maybe use it for combos?
       // timer += Time.fixedDeltaTime;

        if (timer > 0.05f)
        {
            direction = newPosition - OldPosition;
            velocity = direction.magnitude * 1f;
            OldPosition = newPosition;
            timer = 0;
        }

        if (velocity < minSliceVelocity)
        {
            StopSlice();
        }
        else
        {
            sliceCollider.enabled = true;
            transform.position = newPosition;
            CheckSlice();
        }
    }

    private bool CheckSlice()
    {
        RaycastHit RH = new();
        hit = false;

        float MaxDistance = Vector3.Distance(Coordinate,sliceCollider.bounds.center);
        hit = Physics.BoxCast(sliceCollider.bounds.center, size * 0.5f, Coordinate - sliceCollider.bounds.center, out RH, transform.rotation, Mathf.Abs(MaxDistance),fruitLayer); //sliceCollider.LayerMask
        Coordinate = new Vector3(sliceCollider.bounds.center.x, sliceCollider.bounds.center.y, sliceCollider.bounds.center.z);

        if (hit == true)
        {
            //Debug.Log("Hit : " + RH.collider.name);
            RH.collider.gameObject.GetComponent<Fruit>().Slice(direction, transform.position, sliceForce);
        }

        return hit;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        if (hit)
        {
            Gizmos.color = Color.green;
        }

        Gizmos.color = Color.black;
        if (sliceCollider != null)
        Gizmos.DrawWireCube(sliceCollider.bounds.center, size);

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Coordinate, size);

    }

    public void CreateBox(Vector3 a, Vector3 b)
    {

        width = a.x - b.x;
        if (width < 0)
        {
            width = width * -1;
        }

        height = a.y - b.y;
        if (height < 0)
        {
            height = height * -1;
        }

        float directionX;
        float directionY;

        if (a.x < b.x)
        {
            directionX = width / 2;
        }
        else
        {
            directionX = -width / 2;
        }

        if (a.y < b.y)
        {
            directionY = height / 2;
        }
        else
        {
            directionY = -height / 2;
        }
        box.transform.position = new Vector3(a.x + directionX, a.y + directionY);
        box.transform.localScale = new Vector3(width, height);
    }

}
