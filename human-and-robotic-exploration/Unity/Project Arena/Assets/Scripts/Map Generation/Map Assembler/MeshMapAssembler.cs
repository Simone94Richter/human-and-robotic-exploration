using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MeshMapAssembler is an implementation of MapAssembler that assembles the maps using meshes.
/// </summary>
public class MeshMapAssembler : MapAssembler {

    [SerializeField]
    private bool isSkyVisibile;

    [Header("Mesh materials")]
    [SerializeField]
    private Material topMaterial;
    [SerializeField]
    private Material wallMaterial;
    [SerializeField]
    private Material floorMaterial;

    private SquareGrid squareGrid;
    private MeshFilter topMeshFilter;
    private MeshFilter wallsMeshFilter;
    private MeshFilter floorMeshFilter;
    private MeshCollider wallsCollider;
    private MeshCollider floorCollider;
    private MeshCollider topCollider;

    private List<Vector3> vertices;
    private List<int> triangles;

    // A dictionary contains key-value pairs. We use the vertex index as a key and as value the list 
    // off all triangles that own that vertex.
    private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    // We can have multiple outlines, each one is a list of vertices.
    List<List<int>> outlines = new List<List<int>>();
    // We use this so that if we have checked a vertex we won't check it again;
    HashSet<int> checkedVertices = new HashSet<int>();

    private void Start() {
        GameObject childObject;

        childObject = new GameObject("Top - Mesh Filter");
        childObject.transform.parent = transform;
        childObject.transform.localPosition = Vector3.zero;
        topMeshFilter = childObject.AddComponent<MeshFilter>();
        childObject.AddComponent<MeshRenderer>();
        childObject.GetComponent<MeshRenderer>().material = topMaterial;

        childObject = new GameObject("Walls - Mesh Filter");
        childObject.transform.parent = transform;
        childObject.transform.localPosition = Vector3.zero;
        wallsMeshFilter = childObject.AddComponent<MeshFilter>();
        childObject.AddComponent<MeshRenderer>();
        childObject.GetComponent<MeshRenderer>().material = wallMaterial;

        childObject = new GameObject("Floor - Mesh Filter");
        childObject.transform.parent = transform;
        childObject.transform.localPosition = Vector3.zero;
        floorMeshFilter = childObject.AddComponent<MeshFilter>();
        childObject.AddComponent<MeshRenderer>();
        childObject.GetComponent<MeshRenderer>().material = floorMaterial;

        childObject = new GameObject("Walls - Collider");
        childObject.transform.parent = transform;
        childObject.transform.localPosition = Vector3.zero;
        wallsCollider = childObject.AddComponent<MeshCollider>();

        childObject = new GameObject("Floor - Collider");
        childObject.transform.parent = transform;
        childObject.transform.localPosition = Vector3.zero;
        floorCollider = childObject.AddComponent<MeshCollider>();

        if (!isSkyVisibile) {
            childObject = new GameObject("Top - Collider");
            childObject.transform.parent = transform;
            childObject.transform.localPosition = Vector3.zero;
            topCollider = childObject.AddComponent<MeshCollider>();
        }

        SetReady(true);
    }

    // Generates the Mesh.
    public override void AssembleMap(char[,] map, char wallChar, char roomChar) {
        outlines.Clear();
        checkedVertices.Clear();
        triangleDictionary.Clear();

        squareGrid = new SquareGrid(map, wallChar, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        CreateTopMesh(map.GetLength(0), map.GetLength(1));

        CreateWallMesh();

        CreateFloorMesh(map.GetLength(0), map.GetLength(1));
    }

    public override void AssembleMap(List<char[,]> maps, char wallChar, char roomChar,
        char voidChar) { }

    // Creates the top mesh.
    private void CreateTopMesh(int sizeX, int sizeY) {
        if (isSkyVisibile) {
            Mesh topMesh = new Mesh {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            topMesh.RecalculateNormals();
            topMeshFilter.mesh = topMesh;
        } else {
            Mesh topMesh = CreateRectangularMesh(sizeX, sizeY, squareSize, 0, isSkyVisibile);
            topMeshFilter.mesh = topMesh;
            topCollider.sharedMesh = topMesh;
        }

    }

    // Creates the wall mesh.
    private void CreateWallMesh() {
        CalculateMeshOutilnes();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();

        Mesh wallsMesh = new Mesh();

        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                // Left vertex of the wall panel.
                wallVertices.Add(vertices[outline[i]]);
                // Rigth vertex of the wall panel.
                wallVertices.Add(vertices[outline[i + 1]]);
                // Bottom left vertex of the wall panel.
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight);
                // Bottom rigth vertex of the wall panel.
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight);

                // The wall will be seen from inside so we wind them anticlockwise.
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallsMesh.vertices = wallVertices.ToArray();
        wallsMesh.triangles = wallTriangles.ToArray();
        wallsMesh.RecalculateNormals();

        wallsMeshFilter.mesh = wallsMesh;

        wallsCollider.sharedMesh = wallsMesh;
    }

    // Creates the floor mesh.
    private void CreateFloorMesh(int sizeX, int sizeY) {
        Mesh floorMesh = CreateRectangularMesh(sizeX, sizeY, squareSize, wallHeight, true);

        floorMeshFilter.mesh = floorMesh;
        floorCollider.sharedMesh = floorMesh;
    }

    // Creates a rectangular mesh.
    private Mesh CreateRectangularMesh(int sizeX, int sizeY, float squareSize, float height,
        bool facingUpwards) {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(-squareSize / 2, -height, -squareSize / 2);
        vertices[1] = new Vector3(-squareSize / 2, -height,
            sizeY * squareSize + squareSize / 2);
        vertices[2] = new Vector3(sizeX * squareSize + squareSize / 2, -height,
            -squareSize / 2);
        vertices[3] = new Vector3(sizeX * squareSize + squareSize / 2, -height,
            sizeY * squareSize + squareSize / 2);

        int[] triangles;
        if (facingUpwards) {
            triangles = new int[] { 0, 1, 2, 2, 1, 3 };
        } else {
            triangles = new int[] { 3, 1, 2, 2, 1, 0 };
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // Depending on the configuration of a Square I create the rigth mesh.
    private void TriangulateSquare(Square square) {
        switch (square.configuration) {
            case 0:
                break;
            // 1 point cases.
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;
            // 2 points cases.
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft,
                    square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight,
                    square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom,
                    square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight,
                    square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight,
                    square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight,
                    square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            // 3 points cases.
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight,
                    square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight,
                    square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight,
                    square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight,
                    square.centreBottom, square.centreLeft);
                break;
            // 4 point case.
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight,
                    square.bottomLeft);
                // All sorrounding Nodes are walls, so none of this vertices can belong to an 
                // outline.
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.centreBottom.vertexIndex);
                break;
        }
    }

    // Create a mesh from the Nodes passed as parameters.
    private void MeshFromPoints(params Node[] points) {
        AssignVertices(points);

        if (points.Length >= 3) {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4) {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5) {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6) {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    // I add the Nodes to the vertices list after assigning them an incremental ID.
    private void AssignVertices(Node[] points) {
        for (int i = 0; i < points.Length; i++) {
            // vertexIndex default value is -1, if the value is still the same the vertix has not
            // been assigned.
            if (points[i].vertexIndex == -1) {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    // Create a new triangle by adding its vertices to the triangle list.
    private void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    // Goes trough every single vertex in the map, it checks if it is an outline and if it follows
    // it until it meets up with itself. Then it adds it to the outline list.
    private void CalculateMeshOutilnes() {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if (!checkedVertices.Contains(vertexIndex)) {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);

                if (newOutlineVertex != -1) {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    // Starting from a vertex it scans the outline the vertex belongs to.
    private void FollowOutline(int vertexIndex, int outlineIndex) {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1) {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    // Returns a connected vertex, if any, which forms an outline edge with the one passed as 
    // parameter.
    private int GetConnectedOutlineVertex(int vertexIndex) {
        // List of all the triangles containing the vertex index.
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++) {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++) {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
                    if (IsOutlineEdge(vertexIndex, vertexB)) {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    // Given two vertex indeces tells if they define an edge, this happens if the share only a 
    // triangle.
    private bool IsOutlineEdge(int vertexA, int vertexB) {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++) {
            if (trianglesContainingVertexA[i].Contains(vertexB)) {
                sharedTriangleCount++;

                if (sharedTriangleCount > 1) {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }

    // Adds a triangle to the dictionary. 
    private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
        if (triangleDictionary.ContainsKey(vertexIndexKey)) {
            triangleDictionary[vertexIndexKey].Add(triangle);
        } else {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    public override void AssembleMap(float[,] map, float wallChar, float roomChar)
    {
        throw new System.NotImplementedException();
    }

    // A triangle is generated by three vertices.
    public struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c) {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        // An indexer allows to get elements of a struct as an array.
        public int this[int i] {
            get {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex) {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB ||
                vertexIndex == vertexIndexC;
        }
    }

    // Generates the grid of Squares used to generate the Mesh.
    public class SquareGrid {
        public Square[,] squares;

        public SquareGrid(char[,] map, char charWall, float squareSize) {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);

            // We create a grid of Control Nodes.
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    Vector3 pos = new Vector3(x * squareSize, 0, y * squareSize);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == charWall, squareSize);
                }
            }

            // We create a grid of Squares out of the Control Nodes.
            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX - 1; x++) {
                for (int y = 0; y < nodeCountY - 1; y++) {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1],
                        controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    // a   1   b    a, b, c, d are Control Nodes, each of them owns two Nodes (1, 2, 3, 4). 
    // 2       3    e.g.: c owns 2 and 4. A square contains 4 Control Nodes, 4 Nodes and a 
    // c   4   d    configuration,which depends on which Control Nodes are active.
    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode tl, ControlNode tr, ControlNode br, ControlNode bl) {
            topLeft = tl;
            topRight = tr;
            bottomRight = br;
            bottomLeft = bl;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active) {
                configuration += 8;
            }
            if (topRight.active) {
                configuration += 4;
            }
            if (bottomRight.active) {
                configuration += 2;
            }
            if (bottomLeft.active) {
                configuration += 1;
            }
        }
    }

    // A Node is placed between two Control Nodes and belongs to a single Control Node.
    public class Node {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 p) {
            position = p;
        }
    }

    // An active Control Node is a wall.
    public class ControlNode : Node {
        public bool active;
        public Node above, right;

        // "base" means that what follows will be set by the dafault constructor.
        public ControlNode(Vector3 p, bool a, float squareSize) : base(p) {
            active = a;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }

    // Draws the map.
    /* void NotOnDrawGizmos() {
        if (squareGrid != null) {
            for (int x = 0; x < squareGrid.squares.GetLength(0); x ++) {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y ++) {
                    Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : 
                        Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : 
                        Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : 
                        Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, 
                        Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : 
                        Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, 
                        Vector3.one * .4f);

                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, 
                        Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, 
                        Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, 
                        Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, 
                        Vector3.one * .15f);
				}
            }
        }
    } */

}