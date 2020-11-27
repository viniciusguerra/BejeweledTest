using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class Table : MonoBehaviour
    {
        private const string INVALID_DIRECTION_MESSAGE = "Trying to find Tile in invalid direction";

        [Header("Dependencies")]
        [SerializeField]
        private Tile.TileFactory tileFactory;
        [SerializeField]
        private PoolController poolController;

        [Header("Settings")]
        [SerializeField]
        private Pool[] gemPoolArray;
        [SerializeField]
        private int rows;
        [SerializeField]
        private float tileDistance;

        [Header("Animations")]
        [SerializeField]
        private float switchAnimationTime;

        private Tile[,] tileMatrix;

        private Tile _tile;
        private TileType _type;
        private List<TileType> _tileTypeList = new List<TileType>();

        public event EventHandler<TileSelectedArgs> OnTileSelected;

        public void SelectedTile(Tile tile)
        {
            OnTileSelected?.Invoke(this, new TileSelectedArgs(tile));
        }

        public void SwitchTiles(Tile tileA, Tile tileB)
        {
            StartCoroutine(SwitchTilesCoroutine(tileA, tileB));
        }

        private IEnumerator SwitchTilesCoroutine(Tile tileA, Tile tileB)
        {
            // lock input to avoid interacting while the animation is running
            tileA.LockedInput = true;
            tileB.LockedInput = true;

            // find transform positions for tile animation
            Vector2Int selectedPosition = tileA.Position, otherTilePosition = tileB.Position;

            // animate tiles at the same time
            yield return AnimateTiles(
                new Tile[] { tileA, tileB },
                new Vector2Int[] { otherTilePosition, selectedPosition },
                switchAnimationTime
            );

            Tile[] matchingTiles;

            TileDirection switchDirection = tileA.Position.x == tileB.Position.x ? TileDirection.Horizontal : TileDirection.Vertical;

            bool matchedHorizontal = FindMatchInRow(tileA, TileDirection.Horizontal, out matchingTiles);

            if (matchedHorizontal)
            {
                yield return ClearTiles(matchingTiles, TileDirection.Horizontal);
            }

            if (switchDirection == TileDirection.Vertical)
            {
                matchedHorizontal |= FindMatchInRow(tileB, TileDirection.Horizontal, out matchingTiles);

                if (matchedHorizontal)
                {
                    yield return ClearTiles(matchingTiles, TileDirection.Horizontal);
                }
            }

            bool matchedVertical = FindMatchInRow(tileA, TileDirection.Vertical, out matchingTiles);

            if (matchedVertical)
            {
                yield return ClearTiles(matchingTiles, TileDirection.Vertical);
            }

            if (switchDirection == TileDirection.Vertical)
            {
                matchedVertical |= FindMatchInRow(tileB, TileDirection.Vertical, out matchingTiles);

                if (matchedVertical)
                {
                    yield return ClearTiles(matchingTiles, TileDirection.Vertical);
                }
            }

            if (!(matchedHorizontal || matchedVertical))
            {
                // return tiles to previous positions if there is no match
                yield return AnimateTiles(
                    new Tile[] { tileA, tileB },
                    new Vector2Int[] { selectedPosition, otherTilePosition },
                    switchAnimationTime
                );

                tileA.ToggleSelect(false);
                tileB.ToggleSelect(false);
            }

            // unlock input to allow interaction
            tileA.LockedInput = false;
            tileB.LockedInput = false;
        }

        float _counter;

        private IEnumerator AnimateTiles(Tile[] tileArray, Vector2Int[] targetPositionsArray, float duration)
        {
            List<Vector3> originPositionList;
            List<Tile> clearedTileList;

            GatherOriginPositionsAndClearedTiles(tileArray, out originPositionList, out clearedTileList);

            yield return HideClearedTiles(clearedTileList, duration);

            yield return LerpPositions(tileArray, originPositionList, targetPositionsArray, duration);

            RandomizeClearedTiles(clearedTileList);

            yield return ShowResetTiles(clearedTileList, duration);
        }

        private static void GatherOriginPositionsAndClearedTiles(Tile[] tileArray, out List<Vector3> originPositionList, out List<Tile> clearedTileList)
        {
            originPositionList = new List<Vector3>();
            clearedTileList = new List<Tile>();
            foreach (var tile in tileArray)
            {
                originPositionList.Add(tile.transform.position);

                if (tile.Cleared)
                    clearedTileList.Add(tile);
            }
        }

        private IEnumerator LerpPositions(Tile[] tileArray, List<Vector3> originPositionList, Vector2Int[] targetPositionsArray, float duration)
        {
            _counter = 0;

            do
            {
                for (int i = 0; i < tileArray.Length; i++)
                {
                    tileArray[i].transform.position = Vector3.Lerp(
                        originPositionList[i],                                  // origin Lerp position
                        TableSpaceToWorldSpace(targetPositionsArray[i]),        // target Lerp position
                        _counter / duration                                      // interpolation step
                    );
                }

                _counter += Time.deltaTime;

                yield return null;

            } while (_counter < duration);

            for (int i = 0; i < tileArray.Length; i++)
            {
                // set tile to matrix and matrix position in tile
                tileArray[i].Position = targetPositionsArray[i];
                tileMatrix[targetPositionsArray[i].x, targetPositionsArray[i].y] = tileArray[i];
            }
        }

        private void RandomizeClearedTiles(List<Tile> clearedTileList)
        {
            foreach (var clearedTile in clearedTileList)
            {
                _tileTypeList = FindMatchingTypesForPosition(clearedTile.Position);
                clearedTile.SelectRandomTypeFromPossibleTypes(tileFactory.FindTypesDifferentThan(_tileTypeList));

                clearedTile.Cleared = false;
            }
        }

        private IEnumerator HideClearedTiles(List<Tile> clearedTileList, float duration)
        {
            _counter = 0;

            do
            {
                foreach (var clearedTile in clearedTileList)
                {
                    clearedTile.transform.localScale = Vector3.Lerp(
                        Vector3.one,
                        Vector3.zero,
                        _counter / (duration / 2)
                    );
                }

                _counter += Time.deltaTime;

                yield return null;

            } while (_counter < (duration / 2));

            foreach (var clearedTile in clearedTileList)
            {
                clearedTile.transform.localScale = Vector3.zero;
            }
        }

        private IEnumerator ShowResetTiles(List<Tile> clearedTileList, float duration)
        {
            _counter = 0;

            do
            {
                foreach (var clearedTile in clearedTileList)
                {
                    clearedTile.transform.localScale = Vector3.Lerp(
                        Vector3.zero,
                        Vector3.one,
                        _counter / (duration / 2)
                    );
                }

                _counter += Time.deltaTime;

                yield return null;

            } while (_counter < (duration / 2));

            foreach (var clearedTile in clearedTileList)
            {
                clearedTile.transform.localScale = Vector3.one;
            }
        }

        List<Tile> _tileList = new List<Tile>();

        private bool FindMatchInRow(Tile rowTile, TileDirection direction, out Tile[] matchingTiles)
        {
            int consecutive = 0;
            bool match = false;

            _tileList.Clear();

            for (int i = 1; i < rows; i++)
            {
                _tileA = direction == TileDirection.Horizontal ? tileMatrix[i - 1, rowTile.Position.y] : tileMatrix[rowTile.Position.x, i - 1];
                _tileB = direction == TileDirection.Horizontal ? tileMatrix[i, rowTile.Position.y] : tileMatrix[rowTile.Position.x, i];                

                if(_tileA.Equals(_tileB))
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
                    if(match)
                    {
                        break;
                    }
                    else
                    {
                        consecutive = 0;
                        _tileList.Clear();
                    }
                }

                if(consecutive >= 3 || match == true)
                {
                    match = true;
                }
            }

            matchingTiles = _tileList.ToArray();

            return match;
        }

        private void BuildTiles()
        {
            tileMatrix = new Tile[rows, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    _tileTypeList.Clear();

                    // find matching types to the left
                    _type = FindMatchingTypeInDirection(new Vector2Int(i, j), TileDirection.Left);

                    if (_type != null && _tileTypeList.IndexOf(_type) == -1)
                        _tileTypeList.Add(_type);

                    // find matching types up
                    _type = FindMatchingTypeInDirection(new Vector2Int(i, j), TileDirection.Up);

                    if (_type != null && _tileTypeList.IndexOf(_type) == -1)
                        _tileTypeList.Add(_type);

                    _tileTypeList = tileFactory.FindTypesDifferentThan(_tileTypeList);

                    _tile = tileFactory.CreateTile(this, new Vector2Int(i, j), _tileTypeList);

                    tileMatrix[i, j] = _tile;
                }
            }
        }

        #region Tile Matching
        private TileType _matchingType;
        private int _matchingTypeIndex;

        public void SwitchTiles(Vector2Int positionA, Vector2Int positionB)
        {
            _tileA = tileMatrix[positionA.x, positionA.y];
            _tileB = tileMatrix[positionB.x, positionB.y];

            tileMatrix[positionA.x, positionA.y] = _tileB;
            tileMatrix[positionB.x, positionB.y] = _tileA;
        }

        public IEnumerator ClearTiles(Tile[] tileArray, TileDirection direction)
        {
            List<Vector2Int> positionList = new List<Vector2Int>();

            foreach (var tile in tileArray)
            {
                positionList.Add(tile.Position);
            }

            List<Tile> tileToDropList = new List<Tile>();
            List<Vector2Int> targetPositionList = new List<Vector2Int>();

            // drop tiles in all x columns
            if (direction == TileDirection.Horizontal)
            {
                DropTilesInRow(positionList, tileToDropList, targetPositionList);
            }
            // find lowest tile and drop in one column to its position
            else
            {
                DropTilesInColumn(tileArray, tileToDropList, targetPositionList);
            }

            yield return AnimateTiles(tileToDropList.ToArray(), targetPositionList.ToArray(), switchAnimationTime);
        }

        private void DropTilesInColumn(Tile[] clearedTileArray, List<Tile> tilesToAnimateList, List<Vector2Int> targetPositionList)
        {
            Tile highestTile = null;
            Tile lowestTile = null;
            int highest = int.MinValue;
            int lowest = int.MaxValue;

            foreach (var tile in clearedTileArray)
            {
                if (tile.Position.y > highest)
                {
                    highest = tile.Position.y;
                    lowestTile = tile;
                }

                if(tile.Position.y < lowest)
                {
                    lowest = tile.Position.y;
                    highestTile = tile;
                }
            }

            FindTileColumnOverPosition(highestTile.Position, lowestTile.Position, tilesToAnimateList, targetPositionList);

            SendClearedTilesToTop(clearedTileArray, tilesToAnimateList, targetPositionList);
        }

        private void SendClearedTileToTop(Tile clearedTile, List<Tile> tilesToAnimateList, List<Vector2Int> targetPositionsList, int height = 0)
        {
            _upperTilePosition.x = clearedTile.Position.x;
            _upperTilePosition.y = height;

            clearedTile.Cleared = true;

            tilesToAnimateList.Add(clearedTile);
            targetPositionsList.Add(_upperTilePosition);
        }

        private void SendClearedTilesToTop(Tile[] clearedColumnTileArray, List<Tile> tilesToAnimateList, List<Vector2Int> targetPositionsList)
        {
            for (int i = 0; i < clearedColumnTileArray.Length; i++)
            {
                SendClearedTileToTop(clearedColumnTileArray[i], tilesToAnimateList, targetPositionsList, i);
            }
        }

        private void DropTilesInRow(List<Vector2Int> positionList, List<Tile> tileToDropList, List<Vector2Int> targetPositionList)
        {
            foreach (var position in positionList)
            {
                _tile = tileMatrix[position.x, position.y];

                FindTileColumnOverPosition(position, position, tileToDropList, targetPositionList);

                SendClearedTileToTop(_tile, tileToDropList, targetPositionList);
            }
        }

        Vector2Int _upperTilePosition;

        private void FindTileColumnOverPosition(Vector2Int highestClearedPosition, Vector2Int lowestClearedPosition, List<Tile> tileToDropList, List<Vector2Int> targetPositionList)
        {
            if (TileExistsInDirection(highestClearedPosition, TileDirection.Up))
            {
                // scan up from highest cleared position
                highestClearedPosition.y--;

                // stage upper tile
                tileToDropList.Add(tileMatrix[highestClearedPosition.x, highestClearedPosition.y]);

                // to drop to current low position
                targetPositionList.Add(lowestClearedPosition);

                // set next low position to be above current one
                lowestClearedPosition.y--;

                // and do that until there are no more upper tiles
                FindTileColumnOverPosition(highestClearedPosition, lowestClearedPosition, tileToDropList, targetPositionList);
            }
        }

        public Tile[] NeighbourTiles(Tile tile)
        {
            List<Tile> neighbourTileList = new List<Tile>();

            // use try/catch so that if you try to get tiles outside matrix bounds, it just keeps going for the other ones
            try
            {
                neighbourTileList.Add(tileMatrix[tile.Position.x - 1, tile.Position.y]); // left tile
            } catch(Exception) { }
            try
            {
                neighbourTileList.Add(tileMatrix[tile.Position.x + 1, tile.Position.y]); // right tile
            }
            catch (Exception) { }
            try
            {
                neighbourTileList.Add(tileMatrix[tile.Position.x, tile.Position.y - 1]); // up tile
            }
            catch (Exception) { }
            try
            {
                neighbourTileList.Add(tileMatrix[tile.Position.x, tile.Position.y + 1]); // down tile
            }
            catch (Exception) { }

            return neighbourTileList.ToArray();
        }

        private List<TileType> FindMatchingTypesForPosition(Vector2Int tablePosition)
        {
            _tileTypeList.Clear();

            foreach (TileDirection direction in Enum.GetValues(typeof(TileDirection)))
            {
                if (direction == TileDirection.Horizontal || direction == TileDirection.Vertical)
                    continue;

                _matchingType = FindMatchingTypeInDirection(tablePosition, direction);

                // if there is a matching type to that direction
                // and it is not in the matching type list
                if (_matchingType != null && _tileTypeList.IndexOf(_matchingType) != -1)
                {
                    // add it to the list
                    _tileTypeList.Add(_matchingType);
                }
            }

            return _tileTypeList;
        }

        Tile _tileA, _tileB;

        private TileType FindMatchingTypeInDirection(Vector2Int tablePosition, TileDirection direction)
        {
            switch (direction)
            {
                case TileDirection.Left:
                {
                    // can't match to the left if not two away from left
                    if (!TileExistsInDirection(new Vector2Int(tablePosition.x - 1, tablePosition.y), direction))
                        return null;

                    _tileA = FindTileInDirection(tablePosition, direction);
                    _tileB = FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Right:
                {
                    // can't match to the right if not two away from right
                    if (!TileExistsInDirection(new Vector2Int(tablePosition.x + 1, tablePosition.y), direction))
                        return null;

                    _tileA = FindTileInDirection(tablePosition, direction);
                    _tileB = FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Up:
                {
                    // can't match up if not 2 away from top
                    if (!TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y - 1), direction))
                        return null;

                    _tileA = FindTileInDirection(tablePosition, direction);
                    _tileB = FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Down:
                {
                    // can't match up if not 2 away from bottom
                    if (!TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y + 1), direction))
                        return null;

                    _tileA = FindTileInDirection(tablePosition, direction);
                    _tileB = FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Horizontal:
                {
                    // can't match horizontally if position is in left or right corners
                    if (!TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y), direction))
                        return null;

                    _tileA = FindTileInDirection(tablePosition, direction);
                    _tileB = FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                case TileDirection.Vertical:
                {
                    // can't match vertically if position is in top or bottom corners
                    if (!TileExistsInDirection(new Vector2Int(tablePosition.x, tablePosition.y), direction))
                        return null;

                    _tileA = FindTileInDirection(tablePosition, direction);
                    _tileB = FindTileInDirection(_tileA.Position, direction);

                    break;
                }
                default:
                    throw new UnityException(INVALID_DIRECTION_MESSAGE);
            }

            if (_tileA.Equals(_tileB))
                return _tileA.Type;
            else
                return null;
        }
        #endregion

        #region Table Navigation
        public Vector3 TableSpaceToWorldSpace(Vector2Int tableSpacePosition)
        {
            return new Vector3(tableSpacePosition.x * tileDistance, -tableSpacePosition.y * tileDistance, 0);
        }

        private bool TileExistsInDirection(Vector2Int position, TileDirection direction)
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
                    return position.x < rows - 1;
                }
                case TileDirection.Up:
                {
                    // if position.y is 0, Tile is at uppermost corner and there is no Tile upwards
                    return position.y > 0;
                }
                case TileDirection.Down:
                {
                    // if position.y is rows - 1, Tile is at lowermost corner and there is no Tile downwards
                    return position.y < rows - 1;
                }
                case TileDirection.Horizontal:
                {
                    // can't match horizontally if in left or right corner
                    return position.x > 0 && position.x < rows - 1;
                }
                case TileDirection.Vertical:
                {
                    // can't match vertically if in top or bottom corner
                    return position.y > 0 && position.y < rows - 1;
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
                    return tileMatrix[position.x - 1, position.y];
                }
                case TileDirection.Right:
                {
                    return tileMatrix[position.x + 1, position.y];
                }
                case TileDirection.Up:
                {
                    return tileMatrix[position.x, position.y - 1];
                }
                case TileDirection.Down:
                {
                    return tileMatrix[position.x, position.y + 1];
                }
                default:
                {
                    throw new UnityException(INVALID_DIRECTION_MESSAGE);
                }
            }
        }

        public enum TileDirection
        {
            Left,
            Right,
            Up,
            Down,
            Horizontal,
            Vertical
        }
        #endregion

        #region MonoBehavior
        private void Start()
        {
            string[] gemTypeNames = Array.ConvertAll(gemPoolArray, (x) => { return x.Id; });

            tileFactory.BuildTileTypeArray(gemTypeNames);

            BuildTiles();
        }
        #endregion

        public class TileSelectedArgs : EventArgs
        {
            public readonly Tile Tile;

            public TileSelectedArgs(Tile tile)
            {
                this.Tile = tile;
            }
        }
    }
}