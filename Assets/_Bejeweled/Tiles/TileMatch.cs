using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class TileMatch
    {
        public readonly TileDirection Direction;
        public readonly Tile[] MatchingTileArray;
        public readonly int MatchSize;

        public TileMatch(TileDirection direction, Tile[] matchingTileArray)
        {
            Direction = direction;
            MatchingTileArray = matchingTileArray;
            MatchSize = matchingTileArray.Length;
        }
    }
}
