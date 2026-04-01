using UnityEngine;

public class GeometryGenerator : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;
    
    void Start()
    {
        if (!(_meshFilter = GetComponent<MeshFilter>()))
        {
            Debug.LogError("Cannot find MeshFilter", this);
            return;
        }
        print("Generating...");
        // _meshFilter.mesh = TestCreateTri();
        _meshFilter.mesh = TestCreateQuad();
        print("Done.");
    }

    void Update()
    {
        transform.Rotate(new(1,1,0), 45 * Time.deltaTime);
    }

    private Mesh TestCreateQuad()
    {
        Mesh mesh = new();

        Vector3[] vertices = new[]
        {
            new Vector3(-1, -1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, -1, 0),
            new Vector3(1, 1, 0)
        };
        mesh.vertices = vertices;

        Vector2[] uv = new[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        Vector3[] normal = new[]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.uv = uv;

        int[] triangles = new[]
        {
            0, 1, 2,
            2, 1, 3
        };
        mesh.triangles = triangles;
        
        return mesh;
    }
    
    private Mesh TestCreateTri()
    {
        Mesh mesh = new();
        
        Vector3[] vertices = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, 1, 0)
        };
        mesh.vertices = vertices;

        Vector2[] uv = new[]
        {
            new Vector2(0.5f, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        Vector3[] normals = new[]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        int[] triangles = new[]
        {
            0, 1, 2
        };
        mesh.triangles = triangles;

        return mesh;
    }
}
