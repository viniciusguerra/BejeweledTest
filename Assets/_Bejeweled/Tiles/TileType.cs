using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Disable warning that GetHashCode must be overriden along with Equals
// No HashCode dependent data structure will be used with TileType
#pragma warning disable 0659

namespace Bejeweled
{
    [Serializable]
    public class TileType
    {
        private PoolController poolController;

        private string id;
        private string Id
        {
            get => id;
        }

        public GameObject RetrieveInstance()
        {
            return poolController.RetrieveInstance(Id);
        }

        public void StoreInstance(GameObject instance)
        {
            poolController.StoreInstance(instance, id);
        }

        // overriding Equals for easier TileType comparison based on Name
        public override bool Equals(object obj)
        {
            if (obj is TileType)
            {
                return Id.Equals((obj as TileType).Id);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public TileType(string name, PoolController poolController)
        {
            this.poolController = poolController;
            this.id = name;
        }
    }
}