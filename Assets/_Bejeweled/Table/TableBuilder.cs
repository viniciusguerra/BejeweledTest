using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class TableBuilder : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private Table table;
        [SerializeField]
        private TableNavigator tableNavigator;
        [SerializeField]
        private TableMatcher tableMatcher;
        [SerializeField]
        private Tile.TileFactory tileFactory;

        [Header("Settings")]        
        [SerializeField]
        private Pool[] gemTypeSourceArray;        

        public Tile[,] BuildTiles()
        {
            var tileMatrix = new Tile[table.Rows, table.Rows];

            string[] gemTypeNames = Array.ConvertAll(gemTypeSourceArray, (x) => { return x.Id; });

            tileFactory.BuildTileTypeArray(gemTypeNames);

            List<TileType> _tileTypeList = new List<TileType>();
            TileType _type;
            Tile _tile;

            for (int i = 0; i < table.Rows; i++)
            {
                for (int j = 0; j < table.Rows; j++)
                {
                    // find matching types to the left
                    _type = tableMatcher.FindMatchingTypeInDirection(new Vector2Int(i, j), TileDirection.Left);

                    if (_type != null && !_tileTypeList.Contains(_type))
                        _tileTypeList.Add(_type);

                    // find matching types up
                    _type = tableMatcher.FindMatchingTypeInDirection(new Vector2Int(i, j), TileDirection.Up);

                    if (_type != null && !_tileTypeList.Contains(_type))
                        _tileTypeList.Add(_type);

                    _tileTypeList = tileFactory.FindTypesDifferentThan(_tileTypeList);

                    _tile = tileFactory.CreateTile(table, tableNavigator, new Vector2Int(i, j), _tileTypeList);

                    tileMatrix[i, j] = _tile;
                }
            }

            return tileMatrix;
        }

        public List<TileType> FindDifferentTypesThan(List<TileType> unwantedTileTypeList)
        {
            return tileFactory.FindTypesDifferentThan(unwantedTileTypeList);
        }
    }
}