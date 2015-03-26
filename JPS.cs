using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexJPS
{
    internal class JPS<T>
    {
        private static readonly List<Hexagon<T>> HexagonsToSearch = new List<Hexagon<T>>();

        /// <summary>
        /// Do a pathfinding search from Start to End with respect to _hexes, avoiding _obstacles
        /// This algorithm is WIP. It should be optimized A*, but unproven.
        /// </summary>
        /// <param name="map">The HexWorld map that contains the start/end, tiles, and obstacles</param>
        /// <returns>A list of hexagons representing a path from start to end</returns>
        public static List<Hexagon<T>> JPSPath(HexMap<T> map)
        {
            // Clear hexagon to consider list before adding start.
            HexagonsToSearch.Clear();
            HexagonsToSearch.Add(map.Start);


            var jumpPoints = JPSSearch(map);

            return JumpPointsToPath(jumpPoints);
        }

        /// <summary>
        /// The meat of the JPS Search
        /// </summary>
        /// <param name="map">The HexMap</param>
        /// <returns>A list of jump points to get from start to end.</returns>
        private static List<Hexagon<T>> JPSSearch(HexMap<T> map)
        {
            var jumpPoints = new List<Hexagon<T>>();



            return jumpPoints;
        }

        /// <summary>
        /// Given a list of jump points, construct the hex-to-hex path
        /// </summary>
        /// <param name="jumpPoints">A List of hexagons representing the jump points</param>
        /// <returns>The list of hexagons representing the final path</returns>
        private static List<Hexagon<T>> JumpPointsToPath(List<Hexagon<T>> jumpPoints)
        {
            throw new NotImplementedException();
        }
    }

    public enum HexDirection { PlusX, PlusY, PlusZ, MinusX, MinusY, MinusZ }
}
