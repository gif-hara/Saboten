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

        [SerializeField][Range(2, 100)]
        private int frameNumber;

        [SerializeField]
        private float intervalY;

        [SerializeField]
        private float radius;

        [SerializeField]
        private Material material;

        private MeshFilter meshFilter;

        private MeshRenderer meshRenderer;

        private Transform cachedTransform;

        private Mesh mesh;

        void Start()
        {
            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshRenderer = this.GetComponent<MeshRenderer>();
            this.cachedTransform = this.transform;
            this.mesh = new Mesh();

            var vertices = this.GetVertices();
            var triangles = this.GetEdgeTriangles(this.splitNumber, this.frameNumber);
            this.mesh.vertices = vertices;
            this.mesh.triangles = triangles;
            this.meshFilter.mesh = this.mesh;
            this.mesh.RecalculateNormals();
            this.meshRenderer.sharedMaterial = this.material;
        }

        void OnDrawGizmos()
        {
            if(this.mesh == null)
            {
                return;
            }
            Gizmos.color = Color.magenta;
            foreach(var v in this.mesh.vertices)
            {
                Gizmos.DrawSphere(v, 0.05f);
            }
        }

        private Vector3[] GetVertices()
        {
            var result = new Vector3[(this.splitNumber * this.frameNumber) + 1];
            var r = 360.0f;
            var a = r / this.splitNumber;
            for (var k = 0; k < this.frameNumber; k++)
            {
                for (var i = 0; i < this.splitNumber; i++)
                {
                    result[(k * this.splitNumber) + i] = this.GetVertice(a * i, this.radius, this.intervalY * k);
                }
            }

            result[result.Length - 1] = new Vector3(0.0f, (this.frameNumber - 1) * this.intervalY, 0.0f);
            return result;
        }

        private Vector3 GetVertice(float angle, float radius, float y)
        {
            angle = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), y, Mathf.Sin(angle)) * radius;
        }

        private int[] GetCoverTriangles(int splitNumber)
        {
            var result = new int[(splitNumber - 1) * 3];
            var v = 1;
            var k = 0;
            for (var i = 0; i < result.Length / 3; i++)
            {
                result[(i * 3)] = 0;
                result[(i * 3) + 1] = ((k + 1) % (splitNumber - 1)) + 1;
                result[(i * 3) + 2] = v;
                v++;
                k++;
            }
            return result;
        }

        /// <summary>
        /// 側面の三角形情報を返す
        /// </summary>
        private int[] GetEdgeTriangles(int splitNumber, int frameNumber)
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

            return result;
        }
    }
}
