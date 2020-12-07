using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class Table : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private TableBuilder tableBuilder;
        [SerializeField]
        private TableAnimator tableAnimator;
        [SerializeField]
        private TableMatcher tableMatcher;
        [SerializeField]
        private TableNavigator tableNavigator;

        [Header("Settings")]
        [SerializeField]
        private int rows;
        public int Rows
        {
            get => rows;
        }

        [SerializeField]
        private float tileDistance;
        public float TileDistance
        {
            get => tileDistance;
        }

        private Tile[,] tileMatrix;
        public Tile[,] TileMatrix
        {
            get => tileMatrix;
        }       

        public event EventHandler<TileSelectedArgs> OnTileSelected;

        Tile _tile, _tileA, _tileB;
        List<TileType> _tileTypeList = new List<TileType>();

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
            yield return tableAnimator.AnimateTiles(
                new Tile[] { tileA, tileB },
                new Vector2Int[] { otherTilePosition, selectedPosition }
            );

            Tile[] matchingTiles;

            TileDirection switchDirection = tileA.Position.x != tileB.Position.x ? TileDirection.Horizontal : TileDirection.Vertical;

            bool matchedHorizontal = tableMatcher.FindMatchInRow(tileA, TileDirection.Horizontal, out matchingTiles);

            if (matchedHorizontal)
            {
                yield return ClearTiles(matchingTiles, TileDirection.Horizontal);
            }

            if (switchDirection == TileDirection.Vertical)
            {
                if (tableMatcher.FindMatchInRow(tileB, TileDirection.Horizontal, out matchingTiles))
                {
                    matchedHorizontal = true;

                    yield return ClearTiles(matchingTiles, TileDirection.Horizontal);
                }
            }

            bool matchedVertical = tableMatcher.FindMatchInRow(tileA, TileDirection.Vertical, out matchingTiles);

            if (matchedVertical)
            {
                yield return ClearTiles(matchingTiles, TileDirection.Vertical);
            }

            if (switchDirection == TileDirection.Vertical)
            {
                if (tableMatcher.FindMatchInRow(tileB, TileDirection.Vertical, out matchingTiles))
                {
                    matchedVertical = true;

                    yield return ClearTiles(matchingTiles, TileDirection.Vertical);
                }
            }

            if (!(matchedHorizontal || matchedVertical))
            {
                // return tiles to previous positions if there is no match
                yield return tableAnimator.AnimateTiles(
                    new Tile[] { tileA, tileB },
                    new Vector2Int[] { selectedPosition, otherTilePosition }
                );
            }

            // unlock input to allow interaction
            tileA.LockedInput = false;
            tileB.LockedInput = false;

            tileA.ToggleSelect(false);
            tileB.ToggleSelect(false);
        }

        public void GatherOriginPositionsAndClearedTiles(Tile[] tileArray, out List<Vector3> originPositionList, out List<Tile> clearedTileList)
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

        public void RandomizeTileTypes(List<Tile> clearedTileList)
        {
            foreach (var clearedTile in clearedTileList)
            {
                _tileTypeList = tableMatcher.FindMatchingTypesForPosition(clearedTile.Position);
                
                clearedTile.SelectRandomTypeFromPossibleTypes(tableBuilder.FindDifferentTypesThan(_tileTypeList));

                clearedTile.Cleared = false;
            }
        }

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

            yield return tableAnimator.AnimateTiles(tileToDropList.ToArray(), targetPositionList.ToArray());
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

            tableNavigator.FindTileColumnOverPosition(highestTile.Position, lowestTile.Position, tilesToAnimateList, targetPositionList);

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

                tableNavigator.FindTileColumnOverPosition(position, position, tileToDropList, targetPositionList);

                SendClearedTileToTop(_tile, tileToDropList, targetPositionList);
            }
        }

        Vector2Int _upperTilePosition;

        #region MonoBehavior
        private void Start()
        {
            tableBuilder.BuildTiles();
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