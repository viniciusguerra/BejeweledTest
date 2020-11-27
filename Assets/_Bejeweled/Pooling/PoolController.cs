using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class PoolController : MonoBehaviour
    {
        [SerializeField]
        private Pool[] poolArray;

        Pool _pool;

        public GameObject RetrieveInstance(string id)
        {
            _pool = Array.Find(poolArray, p => p.Id == id);

            return _pool.RetrieveInstance();
        }

        public void StoreInstance(GameObject instance, string id)
        {
            _pool = Array.Find(poolArray, p => p.Id == id);

            _pool.StoreInstance(instance);
        }

        private void Awake()
        {
            foreach (var pool in poolArray)
            {
                pool.Initialize();
            }
        }
    }
}