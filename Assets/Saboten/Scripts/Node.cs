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
            float lengthMax,
            float growth
            )
        {
            this.Parent = parent;
            this.Children = new List<Node>();
            this.SabotenController = sabotenController;
            this.Up = up;
            this.VerticesStartIndex = verticesStartIndex;
            this.Generation = generation;
            this.LengthMax = lengthMax;
            this.Growth = growth;
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

        /// <summary>
        /// 末端のノードを返す
        /// </summary>
        public List<Node> Ends
        {
            get
            {
                var result = new List<Node>();
                if(!this.IsEnd)
                {
                    foreach(var c in this.Children)
                    {
                        result.AddRange(c.Ends);
                    }
                }
                else
                {
                    result.Add(this);
                }

                return result;
            }
        }

        public void AddGrowthRecursive(float value)
        {
            Assert.IsTrue(this.IsRoot);
            this.Growth = Mathf.Min(this.Growth + value, 1.0f);
            if (this.IsEnd && this.Growth >= 0.5f)
            {
                this.SabotenController.NewGeneration(this);
            }
            this.UpdateVertices();
            foreach (var c in this.Children)
            {
                c.AddGrowthChild(value, this.WorldPosition);
            }
        }

        private void AddGrowthChild(float value, Vector3 parentWorldPosition)
        {
            Assert.IsFalse(this.IsRoot);
            this.Growth = Mathf.Min(this.Growth + value, 1.0f);
            if (this.IsEnd && this.Growth >= 0.5f)
            {
                this.SabotenController.NewGeneration(this);
            }
            this.WorldPosition = parentWorldPosition + this.ToPosition;
            this.UpdateVertices();
            foreach (var c in this.Children)
            {
                c.AddGrowthChild(value, this.WorldPosition);
            }
        }

        /// <summary>
        /// 子ノードを含めた成長値を設定する
        /// </summary>
        public void SetGrowthRecursive(float value)
        {
            Assert.IsTrue(this.IsRoot);
            this.Growth = value;
            this.UpdateVertices();
            foreach(var c in this.Children)
            {
                c.SetGrowthChild(value, this.WorldPosition);
            }
        }

        /// <summary>
        /// 子ノードの成長値を設定する
        /// </summary>
        private void SetGrowthChild(float value, Vector3 parentWorldPosition)
        {
            Assert.IsFalse(this.IsRoot);
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
            Debug.Log(this.ToString());
            foreach(var child in this.Children)
            {
                child.PrintRecursive();
            }
        }

        public override string ToString()
        {
            return new
            {
                Generation = this.Generation,
                ChildrenCount = this.Children.Count,
                Up = this.Up,
                LocalPosition = this.LocalPosition,
                WorldPosition = this.WorldPosition,
                VerticesStartIndex = this.VerticesStartIndex,
                LengthMax = this.LengthMax,
                Growth = this.Growth
            }.ToString();
        }
    }
}
