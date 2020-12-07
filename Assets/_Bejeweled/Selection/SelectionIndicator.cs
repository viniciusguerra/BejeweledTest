using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class SelectionIndicator : MonoBehaviour
    {
        [SerializeField]
        private Table table;
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            table.OnTileSelected += OnTileSelected;
            table.OnTileDeselected += OnTileDeselected;
        }

        private void OnTileDeselected(object sender, Table.TileSelectedArgs e)
        {
            transform.SetParent(null, false);
            spriteRenderer.enabled = false;
        }

        private void OnTileSelected(object sender, Table.TileSelectedArgs e)
        {
            transform.SetParent(e.Tile.transform, false);
            transform.localPosition = Vector3.zero;
            spriteRenderer.enabled = true;
        }
    }
}