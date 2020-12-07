using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class TableNavigator : MonoBehaviour
    {
        public const string INVALID_DIRECTION_MESSAGE = "Trying to find Tile in invalid direction";

        [SerializeField]
        private Table table;

        public Tile[] FindNeighbouringTiles(Tile tile)
        {
            List<Tile> neighbourTileList = new List<Tile>();

            // use try/catch so that if you try to get tiles outside matrix bounds, it just keeps going for the other ones
            try
            {
                neighbourTileList.Add(table.TileMatrix[tile.Position.x - 1, tile.Position.y]); // left tile
            }
            catch (Exception) { }
            try
            {
                neighbourTileList.Add(table.TileMatrix[tile.Position.x + 1, tile.Position.y]); // right tile
            }
            catch (Exception) { }
            try
            {
                neighbourTileList.Add(table.TileMatrix[tile.Position.x, tile.Position.y - 1]); // up tile
            }
            catch (Exception) { }
            try
            {
                neighbourTileList.Add(table.TileMatrix[tile.Position.x, tile.Position.y + 1]); // down tile
            }
            catch (Exception) { }

            return neighbourTileList.ToArray();
        }

        public void FindTileColumnOverPosition(Vector2Int highestClearedPosition, Vector2Int lowestClearedPosition, List<Tile> tileToDropList, List<Vector2Int> targetPositionList)
        {
            if (TileExistsInDirection(highestClearedPosition, TileDirection.Up))
            {
                // scan up from highest cleared position
                highestClearedPosition.y--;

                // stage upper tile
                tileToDropList.Add(table.TileMatrix[highestClearedPosition.x, highestClearedPosition.y]);

                // to drop to current low position
                targetPositionList.Add(lowestClearedPosition);

                // set next low position to be above current one
                lowestClearedPosition.y--;

                // and do that until there are no more upper tiles
                FindTileColumnOverPosition(highestClearedPosition, lowestClearedPosition, tileToDropList, targetPositionList);
            }
        }

        public Vector3 TableSpaceToWorldSpace(Vector2Int tableSpacePosition)
        {
            return new Vector3(tableSpacePosition.x * table.TileDistance, -tableSpacePosition.y * table.TileDistance, 0);
        }

        public bool TileExistsInDirection(Vector2Int position, TileDirection direction)
        {
            switch (direction)
            {
                case TileDirection.Left:
                {
                    // if position.x is 0, Tile is at leftmost corner and there is no Tile to the left
                    return position.x > 0;
                }
                case TileDirection.Right:
                {
                    // if position.x is rows - 1, Tile is at rightmost corner and there is no Tile to the right
                    return position.x < table.Rows - 1;
                }
                case TileDirection.Up:
                {
                    // if position.y is 0, Tile is at uppermost corner and there is no Tile upwards
                    return position.y > 0;
                }
                case TileDirection.Down:
                {
                    // if position.y is rows - 1, Tile is at lowermost corner and there is no Tile downwards
                    return position.y < table.Rows - 1;
                }
                case TileDirection.Horizontal:
                {
                    // can't match horizontally if in left or right corner
                    return position.x > 0 && position.x < table.Rows - 1;
                }
                case TileDirection.Vertical:
                {
                    // can't match vertically if in top or bottom corner
                    return position.y > 0 && position.y < table.Rows - 1;
                }
                default:
                    throw new UnityException(INVALID_DIRECTION_MESSAGE);
            }
        }

        public Tile FindTileInDirection(Vector2Int position, TileDirection direction)
        {
            if (!TileExistsInDirection(position, direction))
                return null;

            switch (direction)
            {
                case TileDirection.Left:
                {
                    return table.TileMatrix[position.x - 1, position.y];
                }
                case TileDirection.Right:
                {
                    return table.TileMatrix[position.x + 1, position.y];
                }
                case TileDirection.Up:
                {
                    return table.TileMatrix[position.x, position.y - 1];
                }
                case TileDirection.Down:
                {
                    return table.TileMatrix[position.x, position.y + 1];
                }
                default:
                {
                    throw new UnityException(INVALID_DIRECTION_MESSAGE);
                }
            }
        }
    }
}