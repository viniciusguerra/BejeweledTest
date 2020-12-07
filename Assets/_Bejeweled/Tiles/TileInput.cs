using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bejeweled
{
    public class TileInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private const float DRAG_DISTANCE_THRESHOLD = 50;

        [SerializeField]
        Tile tile;

        Table table;
        TableNavigator tableNavigator;

        Vector2 beginDragPosition;
        bool dragged;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (tile.LockedInput)
                return;
            
            tile.ToggleSelect(true);

            beginDragPosition = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (tile.LockedInput)
                return;

            Vector2 dragDelta = eventData.position - beginDragPosition;

            if (dragDelta.magnitude > DRAG_DISTANCE_THRESHOLD)
            {
                TileDirection dragDirection;

                if(Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                {
                    dragDirection = Mathf.Sign(dragDelta.x) > 0 ? TileDirection.Right : TileDirection.Left;
                }
                else
                {
                    dragDirection = Mathf.Sign(dragDelta.y) > 0 ? TileDirection.Up : TileDirection.Down;
                }

                Tile otherTile = tableNavigator.FindTileInDirection(tile.Position, dragDirection);

                if (otherTile == null)
                {
                    tile.ToggleSelect(false);

                    table.DeselectedTile(tile);
                }
                else
                {
                    table.SwitchTiles(tile, otherTile);
                }
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if(!dragged)
            {
                tile.ToggleSelect(false);

                table.DeselectedTile(tile);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (tile.LockedInput)
                return;

            var selectedNeighbour = tile.SelectedNeighbourTile;

            if (selectedNeighbour != null)
            {
                table.SwitchTiles(selectedNeighbour, tile);

                table.DeselectedTile(tile);
            }
            else
            {
                tile.ToggleSelect(!tile.IsSelected);
            }
        }

        public void Initialize(Table table, TableNavigator tableNavigator)
        {
            this.table = table;
            this.tableNavigator = tableNavigator;
        }
    }
}