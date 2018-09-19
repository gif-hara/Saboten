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

        public Vector3 WorldPosition { get; private set; }

        public int VerticesStartIndex { get; private set; }

        public int Generation { get; private set; }

        public float LengthMax { get; private set; }

        public float Growth { get; private set; }

        public Node(
            Node parent,
            SabotenController sabotenController,
            Vector3 up,
            int verticesStartIndex,
            int generation,
            float lengthMax)
        {
            this.Parent = parent;
            this.Children = new List<Node>();
            this.SabotenController = sabotenController;
            this.Up = up;
            this.VerticesStartIndex = verticesStartIndex;
            this.Generation = generation;
            this.LengthMax = lengthMax;
            if (this.Parent != null)
            {
                this.Parent.Children.Add(this);
                this.LocalPosition = this.Parent.LocalPosition;
                this.WorldPosition = this.Parent.LocalPosition + this.Parent.ToPosition;
            }
            else
            {
                this.LocalPosition = Vector3.zero;
                this.WorldPosition = Vector3.zero;
            }
        }

        public Vector3 ToPosition
        {
            get
            {
                return this.LocalPosition + this.Up * (this.LengthMax * this.Growth);
            }
        }

        /// <summary>
        /// 親がいないか返す
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return this.Parent == null;
            }
        }

        /// <summary>
        /// 子供がいないか返す
        /// </summary>
        public bool IsEnd
        {
            get
            {
                return this.Children.Count <= 0;
            }
        }

        public void SetGrowthRecursive(float value)
        {
            this.Growth = value;
            this.UpdateVertices();
            foreach(var c in this.Children)
            {
                c.SetGrowthChild(value, this.WorldPosition);
            }
        }

        private void SetGrowthChild(float value, Vector3 parentWorldPosition)
        {
            this.Growth = value;
            this.WorldPosition = parentWorldPosition + this.ToPosition;
            this.UpdateVertices();
            foreach(var c in this.Children)
            {
                c.SetGrowthChild(value, this.WorldPosition);
            }
        }

        private void UpdateVertices()
        {
            for (var i = 0; i < this.SabotenController.SplitNumber; i++)
            {
                var v = this.SabotenController.Vertices[this.VerticesStartIndex + i];
                this.SabotenController.Vertices[this.VerticesStartIndex + i] = new Vector3(v.x, this.WorldPosition.y, v.z);
            }
            if(this.IsEnd)
            {
                var index = this.VerticesStartIndex + this.SabotenController.SplitNumber;
                var v = this.SabotenController.Vertices[index];
                this.SabotenController.Vertices[index] = new Vector3(v.x, this.WorldPosition.y, v.z);
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
