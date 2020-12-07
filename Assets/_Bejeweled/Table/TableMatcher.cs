using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class TableMatcher : MonoBehaviour
    {
        

        [SerializeField]
        private Table table;
        [SerializeField]
        private TableNavigator tableNavigator;

        private Tile _tile;
        private TileType _type;
        private List<TileType> _tileTypeList = new List<TileType>();
        private List<Tile> _tileList = new List<Tile>();
        private TileType _matchingType;
        private int _matchingTypeIndex;
        private Tile _tileA, _tileB;

        public bool CheckForMatches(Tile tileA, Tile tileB, out Tile[] matchingTiles)
        {
            return FindMatchesInPosition(tileA, out matchingTiles) || FindMatchesInPosition(tileB, out matchingTiles);
        }

        public bool FindMatchesInPosition(Tile tile, out Tile[] matchingTiles)
        {
            return FindMatchInRow(tile, TileDirection.Horizontal, out matchingTiles) || FindMatchInRow(tile, TileDirection.Vertical, out matchingTiles);
        }

        private bool FindMatchInRow(Tile rowTile, TileDirection direction, out Tile[] matchingTiles)
        {
            int consecutive = 0;
            bool match = false;

            _tileList.Clear();

            for (int i = 1; i < table.Rows; i++)
            {
                _tileA = direction == TileDirection.Horizontal ? table.TileMatrix[i - 1, rowTile.Position.y] : table.TileMatrix[rowTile.Position.x, i - 1];
                _tileB = direction == TileDirection.Horizontal ? table.TileMatrix[i, rowTile.Position.y] : table.TileMatrix[rowTile.Position.x, i];

                if (_tileA.Equals(_tileB))
                {
                    if (consecutive == 0)
                    {
                        _tileList.Add(_tileA);
                        consecutive += 2;
                    }
                    else
                        consecutive++;

                    _tileList.Add(_tileB);
                }
                else
                {
                    if (match)
                    {
                        break;
                    }
                    else
                    {
                        consecutive = 0;
                        _tileList.Clear();
                    }
                }

                if (consecutive >= 3 || match == true)
                {
                    match = true;
                }
            }

            matchingTiles = _tileList.ToArray();

            return match;
        }

        public List<TileType> FindMatchingTypesForPosition(Vector2Int tablePosition)
        {
            _tileTypeList.Clear();

            foreach (TileDirection direction in Enum.GetValues(typeof(TileDirection)))
            {
                if (direction == TileDirection.Horizontal || direction == TileDirection.Vertical)
                    continue;

                _matchingType = FindMatchingTypeInDirection(tablePosition, direction);

                // if there is a matching type to that direction
                // and it is not in the matching type list
                if (_matchingType != null && !_tileTypeList.Contains(_matchingType))
                {
                    // add it to the list
                    _tileTypeList.Add(_matchingType);
                }
            }

            return _tileTypeList;
        }

        public TileType FindMatchingTypeInDirection(Vector2Int tablePosition, TileDirection direction)
        {
            switch (direction)
            {
                case TileDirection.Left:
                {
                    // can't match to the left if not two away from left
                    if (!tableNavigator.TileExistsInDirection(new Vector2Int(tablePosition.x - 1, tablePosition.y), direction))
                        return null;

                    _tileA = tableNavigator.FindTileInDirection(tablePosition, direction);
                    _tileB = tableNavigator.FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Right:
                {
                    // can't match to the right if not two away from right
                    if (!tableNavigator.TileExistsInDirection(new Vector2Int(tablePosition.x + 1, tablePosition.y), direction))
                        return null;

                    _tileA = tableNavigator.FindTileInDirection(tablePosition, direction);
                    _tileB = tableNavigator.FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Up:
                {
                    // can't match up if not 2 away from top
                    if (!tableNavigator.TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y - 1), direction))
                        return null;

                    _tileA = tableNavigator.FindTileInDirection(tablePosition, direction);
                    _tileB = tableNavigator.FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Down:
                {
                    // can't match up if not 2 away from bottom
                    if (!tableNavigator.TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y + 1), direction))
                        return null;

                    _tileA = tableNavigator.FindTileInDirection(tablePosition, direction);
                    _tileB = tableNavigator.FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Horizontal:
                {
                    // can't match horizontally if position is in left or right corners
                    if (!tableNavigator.TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y), direction))
                        return null;

                    _tileA = tableNavigator.FindTileInDirection(tablePosition, direction);
                    _tileB = tableNavigator.FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Vertical:
                {
                    // can't match vertically if position is in top or bottom corners
                    if (!tableNavigator.TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y), direction))
                        return null;

                    _tileA = tableNavigator.FindTileInDirection(tablePosition, direction);
                    _tileB = tableNavigator.FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                default:
                    throw new UnityException(TableNavigator.INVALID_DIRECTION_MESSAGE);
            }

            if (_tileA.Equals(_tileB))
                return _tileA.Type;
            else
                return null;
        }
    }
}