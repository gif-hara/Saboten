using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Saboten
{
    /// <summary>
    /// 指定した値からランダムな値を制御するクラス
    /// </summary>
    [Serializable]
    public sealed class RandomRange
    {
        [SerializeField]
        private float min;

        [SerializeField]
        private float max;

        public float Evalute
        {
            get
            {
                return UnityEngine.Random.Range(this.min, this.max);
            }
        }
    }
}
