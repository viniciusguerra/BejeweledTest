using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Disable warning that GetHashCode must be overriden along with Equals
// No HashCode dependent data structure will be used with Tile
#pragma warning disable 0659

namespace Bejeweled
{
    public class Tile : MonoBehaviour
    {
        private const string TILE_NAME = "Gem";
        private const string SELECTED_BOOL_ANIMATOR = "Selected";

        [SerializeField]
        private TileInput tileInput;

        private Table table;

        private Vector2Int previousPosition;
        public Vector2Int PreviousPosition
        {
            get => previousPosition;
        }

        private Vector2Int position;
        public Vector2Int Position
        {
            get => position;
            set
            {
                previousPosition = position;

                this.position = value;

                transform.position = table.TableSpaceToWorldSpace(position);

                gameObject.name = $"{TILE_NAME}{position.ToString()}";
            }
        }

        private TileType type;
        public TileType Type
        {
            get => type;
            private set
            {
                type?.StoreInstance(transform.GetChild(0).gameObject);

                this.type = value;

                var gem = type.RetrieveInstance();
                
                gem.transform.SetParent(transform, false);
                gem.transform.localPosition = Vector3.zero;

                spriteRenderer = gem.GetComponent<SpriteRenderer>();
                animator = gem.GetComponent<Animator>();
            }
        }

        public Tile SelectedNeighbourTile
        {
            get
            {
                foreach (var neighbourTile in table.NeighbourTiles(this))
                {
                    if (neighbourTile.IsSelected)
                        return neighbourTile;
                }

                return null;
            }
        }

        public bool Cleared = false;

        private SpriteRenderer spriteRenderer;
        private Animator animator;

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
        }

        public bool LockedInput = false;

        // overriding Equals for easier Tile comparison based on Tile.Type
        public override bool Equals(object other)
        {
            if (other is Tile)
            {
                return Type.Equals((other as Tile).Type);
            }
            else
            {
                return base.Equals(other);
            }
        }

        private void Create(Table table, Vector2Int position, List<TileType> possibleTypeList)
        {
            this.table = table;
            this.Position = position;

            this.tileInput.Initialize(table);

            this.table.OnTileSelected += OnTileSelected;

            SelectRandomTypeFromPossibleTypes(possibleTypeList);
        }

        private void OnTileSelected(object sender, Table.TileSelectedArgs e)
        {
            if (e.Tile == this || SelectedNeighbourTile != null)
                return;
            else
            {
                ToggleSelect(false);
            }
        }

        public void SelectRandomTypeFromPossibleTypes(List<TileType> possibleTypes)
        {
            Type = possibleTypes[UnityEngine.Random.Range(0, possibleTypes.Count)];
        }

        public void ToggleSelect(bool selected)
        {
            if (selected)
                table.SelectedTile(this);

            spriteRenderer.sortingOrder = selected ? 1 : 0;
            animator.SetBool(SELECTED_BOOL_ANIMATOR, selected);

            isSelected = selected;
        }

        #region TileFactory
        [Serializable]
        public class TileFactory
        {
            [Header("Dependencies")]
            [SerializeField]
            private PoolController poolController;
            [SerializeField]
            private GameObject tilePrefab;

            [Header("Runtime")]
            [SerializeField]
            private List<TileType> gemTypeList;
            public TileType[] TileTypeArray
            {
                get => gemTypeList.ToArray();
            }

            /// <summary>
            /// Creates a random Tile, except for the types defined in the given array. This is done to avoid generating tiles matching 3 from the start.
            /// </summary>
            /// <param name="excludedTypes">Excluded types of tile which would match 3 if set on the Table</param>
            /// <returns>A random Tile</returns>
            public Tile CreateTile(Table table, Vector2Int position, List<TileType> possibleTypes)
            {
                var tileGameObject = Instantiate(tilePrefab, table.transform, false);
                var tile = tileGameObject.GetComponent<Tile>();

                tile.Create(table, position, possibleTypes);

                return tile;
            }

            public List<TileType> FindTypesDifferentThan(List<TileType> unwantedTypesList)
            {
                // find all elements of the type array different from the ones in the tileTypeList
                return this.gemTypeList.FindAll(t => !unwantedTypesList.Contains(t));
            }

            public void BuildTileTypeArray(string[] tileTypeIdArray)
            {
                gemTypeList = new List<TileType>();

                foreach (var id in tileTypeIdArray)
                {
                    gemTypeList.Add(new TileType(id, poolController));
                }
            }
        }
        #endregion
    }
}