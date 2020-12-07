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

        public Tile[,] TileMatrix;

        public event EventHandler<TileSelectedArgs> OnTileSelected;

        Tile _tile, _tileA, _tileB;
        Tile[] _matchingTiles;
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

            bool matchedAnyTime = false;
            bool matched;
            TileMatch[] tileMatchArray;

            do
            {
                tileMatchArray = tableMatcher.CheckForMatches();

                matched = tileMatchArray.Length > 0;

                matchedAnyTime |= matched;

                if (matched)
                {
                    foreach (var match in tileMatchArray)
                    {
                        yield return ClearTiles(match);
                    }                    
                }

            } while (matched);

            if (!matchedAnyTime)
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
            _tileA = TileMatrix[positionA.x, positionA.y];
            _tileB = TileMatrix[positionB.x, positionB.y];

            TileMatrix[positionA.x, positionA.y] = _tileB;
            TileMatrix[positionB.x, positionB.y] = _tileA;
        }

        public IEnumerator ClearTiles(TileMatch tileMatch)
        {
            List<Vector2Int> positionList = new List<Vector2Int>();

            foreach (var tile in tileMatch.MatchingTileArray)
            {
                positionList.Add(tile.Position);
            }

            List<Tile> tileToDropList = new List<Tile>();
            List<Vector2Int> targetPositionList = new List<Vector2Int>();

            // drop tiles in all x columns
            if (tileMatch.Direction == TileDirection.Horizontal)
            {
                FindTilesToDropInRow(positionList, tileToDropList, targetPositionList);
            }
            // find lowest tile and drop in one column to its position
            else
            {
                FindTilesToDropInColumn(tileMatch.MatchingTileArray, tileToDropList, targetPositionList);
            }

            yield return tableAnimator.AnimateTiles(tileToDropList.ToArray(), targetPositionList.ToArray());
        }

        private void FindTilesToDropInColumn(Tile[] clearedTileArray, List<Tile> tilesToAnimateList, List<Vector2Int> targetPositionList)
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

        private void FindTilesToDropInRow(List<Vector2Int> positionList, List<Tile> tileToDropList, List<Vector2Int> targetPositionList)
        {
            foreach (var position in positionList)
            {
                _tile = TileMatrix[position.x, position.y];

                tableNavigator.FindTileColumnOverPosition(position, position, tileToDropList, targetPositionList);

                SendClearedTileToTop(_tile, tileToDropList, targetPositionList);
            }
        }

        Vector2Int _upperTilePosition;

        #region MonoBehavior
        private void Start()
        {
            tableBuilder.BuildTileMatrix();
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