using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace HexJPS
{
	public class HexMap<T>
	{
        public static Tuple<int, int, int> AxialToCubic(Tuple<int, int> axial)
        {
            return AxialToCubic(axial.First, axial.Second);
        }
	    public static Tuple<int, int, int> AxialToCubic(int axialX, int axialZ)
	    {
	        return  new Tuple<int, int, int>(axialX, -1*axialX - axialZ, axialZ);
	    }

        public static Tuple<int, int> CubicToAxial(Tuple<int, int, int> cubic)
        {
            return CubicToAxial(cubic.First, cubic.Second, cubic.Third);
        }
	    public static Tuple<int, int> CubicToAxial(int cubeX, int cubeY, int cubeZ)
	    {
	        return new Tuple<int, int>(cubeX, cubeZ);
	    }

	    private readonly int mapX, mapY;
	    private readonly Hexagon<T>[,] _hexes; 
	    private readonly HashSet<Hexagon<T>> _obstacles;

		public HexMap(int maxMapX, int maxMapY)
		{
		    mapX = maxMapX;
		    mapY = maxMapY;
			_hexes = new Hexagon<T>[maxMapX,maxMapY];
            _obstacles = new HashSet<Hexagon<T>>();
		}

		public Hexagon<T> this[int x, int y]
		{
			get { return _hexes[x, y]; }
		    set { _hexes[x, y] = value; }
		}

	    public bool AddObstacle(Hexagon<T> hex)
	    {
	        if (_obstacles.Add(hex))
	        {
	            hex.IsObstacle = true;
	            return true;
	        }
	        return false;
	    }

	    public bool RemoveObstacle(Hexagon<T> hex)
	    {
            if (_obstacles.Remove(hex))
            {
                hex.IsObstacle = false;
                return true;
            }
            return false;
	    }

	    public bool IsObstacle(Hexagon<T> hex)
	    {
	        return hex.IsObstacle;
	    }

	    public bool IsStartEnd(Hexagon<T> hex)
	    {
	        return (hex == Start || hex == End);
	    }

	    public IEnumerable<Hexagon<T>> GetObstacles()
	    {
	        return _obstacles;
	    }

	    public List<Hexagon<T>> GetPath()
	    {
            // TODO: Could use some sort of dirty flag here to skip recalculation if no new hexagons added
	        SetupNeighbors();

            // for testing
            //return new List<Hexagon<T>>(_obstacles);
            
	        if (Start == null || End == null)
	            return null;
	        return JPS<T>.JPSPath(this);
            
	    }

        /// <summary>
        /// Fill the "Neighbors" variable in every Hexagon with the corresponding Hexagon, if any
        /// </summary>
	    private void SetupNeighbors()
	    {
            // TODO: Optimize this?
            for (var x = 0; x < mapX; x++)
            {
                for (var y = 0; y < mapY; y++)
                {

                    var hexagon = _hexes[x, y];

                    var currX = hexagon.CubeX;
                    var currY = hexagon.CubeY;
                    var currZ = hexagon.CubeZ;

                    var plusXKey = CubicToAxial(currX + 0, currY + 1, currZ - 1);
                    var plusYKey = CubicToAxial(currX + 1, currY + 0, currZ - 1);
                    var plusZKey = CubicToAxial(currX + 1, currY - 1, currZ + 0);
                    var minusXKey = CubicToAxial(currX + 0, currY - 1, currZ + 1);
                    var minusYKey = CubicToAxial(currX - 1, currY + 0, currZ + 1);
                    var minusZKey = CubicToAxial(currX - 1, currY + 1, currZ + 0);

                    /*
                    if (hexagon.Equals(Start))
                    {
                        Debug.Log("plusx" + plusXKey);
                        Debug.Log("plusy" + plusYKey);
                        Debug.Log("plusz" + plusZKey);
                        Debug.Log("minusx" + minusXKey);
                        Debug.Log("minusy" + minusYKey);
                        Debug.Log("minusz" + minusZKey);
                    
                    }*/
                    

                    SetupNeighborHelper(hexagon, HexDirection.PlusX, plusXKey.First, plusXKey.Second);
                    SetupNeighborHelper(hexagon, HexDirection.PlusY, plusYKey.First, plusYKey.Second);
                    SetupNeighborHelper(hexagon, HexDirection.PlusZ, plusZKey.First, plusZKey.Second);
                    SetupNeighborHelper(hexagon, HexDirection.MinusX, minusXKey.First, minusXKey.Second);
                    SetupNeighborHelper(hexagon, HexDirection.MinusY, minusYKey.First, minusYKey.Second);
                    SetupNeighborHelper(hexagon, HexDirection.MinusZ, minusZKey.First, minusZKey.Second);
                }
            }
	        
	    }

	    public void Test()
	    {
            SetupNeighbors();
	        var fn = JPS<T>.HasForcedNeighbor(Start, HexDirection.PlusZ);
	        Debug.Log(fn);
	    }

        /// <summary>
        /// Handle assignment of the hexagon neighbors. Does bounds checking
        /// </summary>
        /// <param name="hexagon"></param>
        /// <param name="dir"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
	    private void SetupNeighborHelper(Hexagon<T> hexagon, HexDirection dir, int x, int y)
	    {
            if ((x < 0 || x >= mapX) || (y < 0 || y >= mapY))
            {
                hexagon.Neighbors[dir] = null;
            }
            else
            {
                hexagon.Neighbors[dir] = _hexes[x, y];
            }
	    }

		private Hexagon<T> _start;
		public Hexagon<T> Start
		{
			get { return _start; }
			set
			{
				if (value == End)
					throw new Exception("Start cannot be end");
                if (_obstacles.Contains(value))
                    throw new Exception("Obstacle cannot be start");
			    _start = value;
			}
		}
		private Hexagon<T> _end;
		public Hexagon<T> End
		{
			get { return _end; }
			set
			{
				if (value == Start)
					throw new Exception("End cannot be start");
                if (_obstacles.Contains(value))
                    throw new Exception("Obstacle cannot be end");
			    _end = value;
			}
		} 
	}

	public class Tuple<T1, T2>
	{
		public T1 First { get; private set; }
		public T2 Second { get; private set; }
		internal Tuple(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            int hash = 17;
	            hash = hash*29 + First.GetHashCode();
	            hash = hash*29 + Second.GetHashCode();
	            return hash;
	        }
	    }

	    public override string ToString()
	    {
	        return "Tuple2 " + First + " " + Second;
	    }
	}
    public class Tuple<T1, T2, T3>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        public T3 Third { get; private set; }
        internal Tuple(T1 first, T2 second, T3 third)
        {
            First = first;
            Second = second;
            Third = third;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash*29 + First.GetHashCode();
                hash = hash*29 + Second.GetHashCode();
                hash = hash*29 + Third.GetHashCode();
                return hash;
            }
        }
    }
}
