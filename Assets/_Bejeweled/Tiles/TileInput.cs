﻿using System.Collections;
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
                Table.TileDirection dragDirection;

                if(Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                {
                    dragDirection = Mathf.Sign(dragDelta.x) > 0 ? Table.TileDirection.Right : Table.TileDirection.Left;
                }
                else
                {
                    dragDirection = Mathf.Sign(dragDelta.y) > 0 ? Table.TileDirection.Up : Table.TileDirection.Down;
                }

                Tile otherTile = table.FindTileInDirection(tile.Position, dragDirection);

                if (otherTile == null)
                {
                    tile.ToggleSelect(false);
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
            }
            else
            {
                tile.ToggleSelect(!tile.IsSelected);
            }
        }

        public void Initialize(Table table)
        {
            this.table = table;
        }
    }
}