using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class TableAnimator : MonoBehaviour
    {
        [SerializeField]
        private Table table;
        [SerializeField]
        private TableNavigator tableNavigator;

        [SerializeField]
        private float switchAnimationTime;

        float _counter;

        public IEnumerator AnimateTiles(Tile[] tileArray, Vector2Int[] targetPositionsArray)
        {
            List<Vector3> originPositionList;
            List<Tile> clearedTileList;

            table.GatherOriginPositionsAndClearedTiles(tileArray, out originPositionList, out clearedTileList);

            yield return HideClearedTiles(clearedTileList, switchAnimationTime);

            yield return LerpPositions(tileArray, originPositionList, targetPositionsArray, switchAnimationTime);

            table.RandomizeTileTypes(clearedTileList);

            yield return ShowResetTiles(clearedTileList, switchAnimationTime);
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
                        tableNavigator.TableSpaceToWorldSpace(targetPositionsArray[i]),        // target Lerp position
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
                table.TileMatrix[targetPositionsArray[i].x, targetPositionsArray[i].y] = tileArray[i];
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
    }
}