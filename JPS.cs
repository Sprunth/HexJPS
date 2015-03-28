using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using C5;

namespace HexJPS
{
    internal static class JPS<T>
    {
        /// <summary>
        /// HexagonsToSearch acts as a priority queue for A* + JPS. HexRefList is a lookup table for C5's references to the hexagons
        /// </summary>
        private static readonly IntervalHeap<Hexagon<T>> HexagonsToSearch = new IntervalHeap<Hexagon<T>>();
        private static readonly Dictionary<IPriorityQueueHandle<Hexagon<T>>,Hexagon<T>> HexRefList = new Dictionary<IPriorityQueueHandle<Hexagon<T>>, Hexagon<T>>();

        /// <summary>
        /// Do a pathfinding search from Start to End with respect to _hexes, avoiding _obstacles
        /// This algorithm is WIP. It should be optimized A*, but unproven.
        /// </summary>
        /// <param name="map">The HexWorld map that contains the start/end, tiles, and obstacles</param>
        /// <returns>A list of hexagons representing a path from start to end</returns>
        public static List<Hexagon<T>> JPSPath(HexMap<T> map)
        {
            // Clear hexagon to consider list before adding start.
            while (HexagonsToSearch.Count > 0)
            {
                IPriorityQueueHandle<Hexagon<T>> tmp;
                HexagonsToSearch.DeleteMin(out tmp);
            }
            HexRefList.Clear();


            // Add the start to the search list to start it off
            map.Start.DistanceFromStart = 0;
            AddHexToSearchList(map.Start);

            var jumpPoints = JPSSearch(map);

            //return JumpPointsToPath(jumpPoints);
            return jumpPoints;
        }

        /// <summary>
        /// The meat of the JPS Search
        /// </summary>
        /// <param name="map">The HexMap</param>
        /// <returns>A list of jump points to get from start to end.</returns>
        private static List<Hexagon<T>> JPSSearch(HexMap<T> map)
        {
            var directions = (HexDirection[])Enum.GetValues(typeof(HexDirection));

            var jumpPoints = new List<Hexagon<T>>();

            // HexagonsToSearch should contain only the start hexagon right now.
            while (HexagonsToSearch.Count > 0)
            {
                // get next item
                var hexToExamine = GetMinFromSearchList();

                // do stuff with hexToExamine
                // iterate through the neighbors of hexToExamine's neighbors
                foreach (var direction in directions)
                {
                    var dist = hexToExamine.DistanceFromStart;

                    var neighbor = hexToExamine.Neighbors[direction];
                    // go in the direction of this neighbor until
                    // obstacle, undefined hex (out of map/hole), or forced neighbor
                    while (neighbor != null && FreeHex(neighbor) && !HasForcedNeighbor(neighbor, direction))
                    {
                        // everytime we hop away from hexToExamine, increment the distance by 1
                        // Since the algorithm is essentially a BFS with some other stuff,
                        // even if we take 3 right turns (for example) the distance metric should still be okay.
                        dist++;
                        neighbor = neighbor.Neighbors[direction];
                    }
                    // now neighbor is either invalid/obstacle, in which case we can just stop
                    if (neighbor == null || !FreeHex(neighbor))
                        continue;
                    else
                    {
                        // we must have a forced neighbor
                        // we don't actually need to know where the forced neighbor is
                        // just that the current neighbor variable is a jump point we need to consider
                        neighbor.DistanceFromStart = dist;
                        AddHexToSearchList(neighbor);
                    }
                }
            }

            // now that End is found, go backwards by following Hexagon.prevJumpPoint to Start
            var goBack = map.End;
            while (goBack != map.Start)
            {
                jumpPoints.Add(goBack);
                goBack = goBack.prevJumpPoint;
            }
            jumpPoints.Add(goBack); // goBack is map.Start now...
            return jumpPoints;
        }

        private static void AddHexToSearchList(Hexagon<T> hex)
        {
            IPriorityQueueHandle<Hexagon<T>> startRef = null;
            HexagonsToSearch.Add(ref startRef, hex);
            HexRefList.Add(startRef, hex);
        }

        private static Hexagon<T> GetMinFromSearchList()
        {
            IPriorityQueueHandle<Hexagon<T>> outref;
            HexagonsToSearch.DeleteMin(out outref);
            return HexRefList[outref];
        }

        /// <summary>
        /// Given a hexagon and the forward direction, check if there's a forced neighbor
        /// Yes if the 120 degree from front neighbors are obstacles (or not valid)
        ///     but the 60 degree corresponding neighbors are open
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="forwardDirection"></param>
        /// <returns></returns>
        public static bool HasForcedNeighbor(Hexagon<T> hex, HexDirection forwardDirection)
        {
            // hardcode nessesary unless angle relations of the hexdirection enums are set...
            switch (forwardDirection)
            {
                case HexDirection.PlusX:
                    if (
                        (!FreeHex(hex.Neighbors[HexDirection.PlusZ]) && FreeHex(hex.Neighbors[HexDirection.PlusY])) ||
                        (!FreeHex(hex.Neighbors[HexDirection.MinusY]) && FreeHex(hex.Neighbors[HexDirection.MinusZ]))
                        )
                        return true;
                    break;
                case HexDirection.PlusY:
                    if (
                        (!FreeHex(hex.Neighbors[HexDirection.MinusX]) && FreeHex(hex.Neighbors[HexDirection.PlusZ])) ||
                        (!FreeHex(hex.Neighbors[HexDirection.MinusZ]) && FreeHex(hex.Neighbors[HexDirection.PlusX]))
                        )
                        return true;
                    break;
                case HexDirection.PlusZ:
                    foreach (var neighbor in hex.Neighbors)
                    {
                        //Debug.Log(neighbor.Key + ": " + FreeHex(neighbor.Value) + " " + neighbor.Value.IsObstacle);
                    }
                    if (
                        (!FreeHex(hex.Neighbors[HexDirection.PlusX]) && FreeHex(hex.Neighbors[HexDirection.PlusY])) ||
                        (!FreeHex(hex.Neighbors[HexDirection.MinusY]) && FreeHex(hex.Neighbors[HexDirection.MinusX]))
                        )
                        return true;
                    break;
                case HexDirection.MinusX:
                    if (
                        (!FreeHex(hex.Neighbors[HexDirection.PlusY]) && FreeHex(hex.Neighbors[HexDirection.PlusZ])) ||
                        (!FreeHex(hex.Neighbors[HexDirection.MinusZ]) && FreeHex(hex.Neighbors[HexDirection.MinusY]))
                        )
                        return true;
                    break;
                case HexDirection.MinusY:
                    if (
                        (!FreeHex(hex.Neighbors[HexDirection.PlusX]) && FreeHex(hex.Neighbors[HexDirection.MinusZ])) ||
                        (!FreeHex(hex.Neighbors[HexDirection.PlusZ]) && FreeHex(hex.Neighbors[HexDirection.MinusX]))
                        )
                        return true;
                    break;
                case HexDirection.MinusZ:
                    if (
                        (!FreeHex(hex.Neighbors[HexDirection.PlusY]) && FreeHex(hex.Neighbors[HexDirection.PlusX])) ||
                        (!FreeHex(hex.Neighbors[HexDirection.MinusX]) && FreeHex(hex.Neighbors[HexDirection.MinusY]))
                        )
                        return true;
                    break;
                default:
                    throw new Exception("Unknown Hexdirection: " + forwardDirection);
            }
            return false;
        }

        /// <summary>
        /// Helper to figure out whether a hex is a free space from JPS' viewpoint
        /// </summary>
        /// <param name="hex"></param>
        /// <returns>True if hex is free</returns>
        private static bool FreeHex(Hexagon<T> hex)
        {
            return !(hex.IsObstacle);
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

    /// <summary>
    /// Enumeration of directions for a hexagonal grid (in cubic coordinates)
    /// </summary>
    public enum HexDirection { PlusX, PlusY, PlusZ, MinusX, MinusY, MinusZ }
    // Although which axis the directions are aligned with are not important, the relativity of each direction to each other is.
    // Use this as a reference:
    //----------minusy---minusx---------
    //--------/----------------\--------
    //-minusz-|-----------------|-plusz-
    //--------\-----------------/-------
    //---------plusx-------plusy--------
}
