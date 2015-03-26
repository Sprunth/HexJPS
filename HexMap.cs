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
	        return  new Tuple<int, int, int>(axialX, axialZ, -1*axialX - axialZ);
	    }

        public static Tuple<int, int> CubicToAxial(Tuple<int, int, int> cubic)
        {
            return CubicToAxial(cubic.First, cubic.Second, cubic.Third);
        }
	    public static Tuple<int, int> CubicToAxial(int cubeX, int cubeY, int cubeZ)
	    {
	        return new Tuple<int, int>(cubeX, cubeZ);
	    }

		private readonly Dictionary<Tuple<int, int>, Hexagon<T>> _hexes;
	    private readonly HashSet<Hexagon<T>> _obstacles;

		public HexMap()
		{
			_hexes = new Dictionary<Tuple<int, int>, Hexagon<T>>();
            _obstacles = new HashSet<Hexagon<T>>();

            Debug.Log(new Tuple<int, int>(0, 0).GetHashCode());
            Debug.Log(new Tuple<int, int>(0, 0).GetHashCode());
		}

		public Hexagon<T> this[int x, int y]
		{
			get { return _hexes[new Tuple<int, int>(x,y)]; }
		    set
		    {
                Debug.Log("Adding " + x + " " + y + " to hexes list");
		        var tup = new Tuple<int, int>(x, y);
                _hexes.Add(tup, value);
                Debug.Log(_hexes[tup]);
		    }
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
            return new List<Hexagon<T>>(_obstacles);
            /*
	        if (Start == null || End == null)
	            return null;
	        return JPS<T>.JPSPath(this);
            */
	    }

        /// <summary>
        /// Fill the "Neighbors" variable in every Hexagon with the corresponding Hexagon, if any
        /// </summary>
	    private void SetupNeighbors()
	    {
            // TODO: Optimize this?
            //foreach (var hexagon in _hexes.Values)
	        {
	            var hexagon = _hexes[new Tuple<int, int>(0, 0)];

                var currX = hexagon.CubeX;
                var currY = hexagon.CubeY;
                var currZ = hexagon.CubeZ;

                var plusXKey = CubicToAxial(currX + 0, currY + 1, currZ - 1);
                var plusYKey = CubicToAxial(currX + 1, currY + 0, currZ - 1);
                var plusZKey = CubicToAxial(currX + 1, currY - 1, currZ + 0);
                var minusXKey = CubicToAxial(currX + 0, currY - 1, currZ + 1);
                var minusYKey = CubicToAxial(currX - 1, currY + 0, currZ + 1);
                var minusZKey = CubicToAxial(currX - 1, currY + 1, currZ + 0);

                Debug.Log(plusXKey + " " + plusYKey + " " + plusZKey);

                hexagon.Neighbors[HexDirection.PlusX]   =_hexes.ContainsKey(plusXKey)  ? _hexes[plusXKey]   : null;
                hexagon.Neighbors[HexDirection.PlusY]   =_hexes.ContainsKey(plusYKey)  ? _hexes[plusYKey]   : null;
                hexagon.Neighbors[HexDirection.PlusZ]   =_hexes.ContainsKey(plusZKey)  ? _hexes[plusZKey]   : null;
                hexagon.Neighbors[HexDirection.MinusX]  =_hexes.ContainsKey(minusXKey) ? _hexes[minusXKey]  : null;
                hexagon.Neighbors[HexDirection.MinusY]  =_hexes.ContainsKey(minusYKey) ? _hexes[minusYKey]  : null;
                hexagon.Neighbors[HexDirection.MinusZ]  =_hexes.ContainsKey(minusZKey) ? _hexes[minusZKey]  : null;
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
