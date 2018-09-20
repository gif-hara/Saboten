using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Saboten
{
    /// <summary>
    /// サボテンを制御するクラス
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SabotenController : MonoBehaviour
    {
        [SerializeField][Range(3, 99999)]
        private int splitNumber;
        public int SplitNumber { get { return this.splitNumber; } }

        [SerializeField][Range(2, 100)]
        private int frameNumber;

        [SerializeField]
        private RandomRange length;

        [SerializeField]
        private RandomRange radius;

        [SerializeField][Range(0.0f, 1.0f)]
        private float initialGrowth;

        [SerializeField]
        private float growthVelocity;

        [SerializeField]
        private Material material;

        private MeshFilter meshFilter;

        private MeshRenderer meshRenderer;

        public Mesh Mesh { get; private set; }

        private Node rootNode;

        public List<Vector3> Vertices { get; private set; }

        private List<int> triangles;

        void Start()
        {
            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshRenderer = this.GetComponent<MeshRenderer>();
            this.Mesh = new Mesh();

            this.Vertices = this.GetVertices();
            this.triangles = this.GetAllTriangles(this.Vertices, this.splitNumber, this.frameNumber);
            this.Mesh.SetVertices(Vertices);
            this.Mesh.SetTriangles(this.triangles, 0);
            this.meshFilter.mesh = this.Mesh;
            this.Mesh.RecalculateNormals();
            this.meshRenderer.sharedMaterial = this.material;

            Node node = null;
            for (var i = 0; i < this.frameNumber; i++)
            {
                node = new Node(
                    node,
                    this,
                    Vector3.up,
                    i * this.splitNumber,
                    i,
                    this.length.Evalute,
                    this.radius.Evalute,
                    this.initialGrowth
                    );
                if(i == 0)
                {
                    this.rootNode = node;
                }
            }
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                var lastChildren = this.rootNode.Ends;
                foreach(var c in lastChildren)
                {
                    Debug.Log(string.Format("[{0}]", c.Generation));
                    this.NewGeneration(c);
                }
            }
            if(Input.GetKeyDown(KeyCode.W))
            {
                this.rootNode.PrintRecursive();
            }
            if(Input.GetKey(KeyCode.Space))
            {
                this.rootNode.AddGrowthRecursive(this.growthVelocity * Time.deltaTime);
                this.rootNode.UpdateVerticesRecursive();
                this.Mesh.SetVertices(this.Vertices);
                this.Mesh.RecalculateNormals();
            }
        }

        void OnDrawGizmos()
        {
            if(this.Mesh == null)
            {
                return;
            }
            Gizmos.color = Color.magenta;
            foreach(var v in this.Mesh.vertices)
            {
                Gizmos.DrawSphere(v, 0.05f);
            }
        }

        public void NewGeneration(Node parent)
        {
            this.frameNumber++;
            this.Vertices = this.GetVertices();
            this.triangles = this.GetAllTriangles(this.Vertices, this.splitNumber, this.frameNumber);
            this.Mesh.SetVertices(Vertices);
            this.Mesh.SetTriangles(this.triangles, 0);
            this.Mesh.RecalculateNormals();
            var generation = parent.Generation + 1;
            new Node(
                parent,
                this,
                Vector3.up,
                generation * this.splitNumber,
                generation,
                this.length.Evalute,
                this.radius.Evalute,
                0.0f
                );
        }

        private List<Vector3> GetVertices()
        {
            var capacity = (this.splitNumber * this.frameNumber) + 1;
            var result = new List<Vector3>(capacity);
            for (var i = 0; i < (this.splitNumber * this.frameNumber) + 1; i++)
            {
                result.Add(new Vector3());
            }
            return result;
        }

        private Vector3 GetVertice(float angle, float radius, float y)
        {
            angle = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), y, Mathf.Sin(angle)) * radius;
        }

        private List<int> GetAllTriangles(List<Vector3> vertices, int splitNumber, int frameNumber)
        {
            return this.GetCoverTriangles(vertices, splitNumber).Concat(this.GetEdgeTriangles(splitNumber, frameNumber)).ToList();
        }

        /// <summary>
        /// フタ部分の三角形情報を返す
        /// </summary>
        private List<int> GetCoverTriangles(List<Vector3> vertices, int splitNumber)
        {
            var result = new int[splitNumber * 3];
            var length = vertices.Count - 1;
            for (var i = 0; i < result.Length / 3; i++)
            {
                var x = i;
                var y = (i + 1) % splitNumber;
                var z = splitNumber;
                result[(i * 3) + 0] = length - x;
                result[(i * 3) + 1] = length - y;
                result[(i * 3) + 2] = length - z;
            }

            return result.ToList();
        }

        /// <summary>
        /// 側面の三角形情報を返す
        /// </summary>
        private List<int> GetEdgeTriangles(int splitNumber, int frameNumber)
        {
            if(frameNumber <= 1)
            {
                Debug.LogError("Edgeが作れません");
                return null;
            }
            var result = new int[((splitNumber) * (frameNumber - 1)) * 6];
            for (var i = 0; i < result.Length / 6; i++)
            {
                if((i * 6) > result.Length)
                {
                    break;
                }
                var x = i;
                var y = ((i + 1) % this.splitNumber) + ((i / this.splitNumber) * this.splitNumber);
                var z = y + this.splitNumber;
                var a = x + this.splitNumber;
                result[(i * 6) + 0] = x;
                result[(i * 6) + 2] = y;
                result[(i * 6) + 1] = z;
                result[(i * 6) + 3] = x;
                result[(i * 6) + 5] = z;
                result[(i * 6) + 4] = a;
            }

            return result.ToList();
        }
    }
}
