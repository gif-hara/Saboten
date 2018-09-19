using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Saboten
{
    /// <summary>
    /// サボテンの節目を担うクラス
    /// </summary>
    public sealed class Node
    {
        /// <summary>
        /// 親
        /// </summary>
        public Node Parent { get; private set; }

        public List<Node> Children { get; private set; }

        public SabotenController SabotenController { get; private set; }

        public Vector3 Up { get; private set; }

        public Vector3 LocalPosition { get; private set; }

        public int VerticesStartIndex { get; private set; }

        public int Generation { get; private set; }

        public float LengthMax { get; private set; }

        public Node(Node parent, SabotenController sabotenController, Vector3 up, int verticesStartIndex, int generation)
        {
            this.Parent = parent;
            this.Children = new List<Node>();
            this.SabotenController = sabotenController;
            this.Up = up;
            this.VerticesStartIndex = verticesStartIndex;
            this.Generation = generation;
            if (this.Parent != null)
            {
                this.Parent.Children.Add(this);
                this.LocalPosition = this.Parent.LocalPosition;
            }
            else
            {
                this.LocalPosition = Vector3.zero;
            }
        }

        public void PrintRecursive()
        {
            Debug.Log(string.Format("[{0}]", this.Generation));
            foreach(var child in this.Children)
            {
                child.PrintRecursive();
            }
        }
    }
}
