using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexJPS
{
    public class Hexagon<T>
    {
        public int AxialX { get; private set; }
        public int AxialZ { get; private set; }
        public int CubeX { get; private set; }
        public int CubeY { get; private set; }
        public int CubeZ { get; private set; }


        public T Obj { get; set; }
        public bool IsObstacle { get; set; } // TODO: not synced with Hexmap's list of obstacles...

        public readonly Dictionary<HexDirection, Hexagon<T>> Neighbors; 

        public Hexagon(int axialX, int axialZ, T obj) : this(obj)
        {
            AxialX = axialX;
            AxialZ = axialZ;

            var cubic = HexMap<T>.AxialToCubic(new Tuple<int, int>(axialX, axialZ));
            CubeX = cubic.First;
            CubeY = cubic.Second;
            CubeZ = cubic.Third;
        }

        public Hexagon(int cubeX, int cubeY, int cubeZ, T obj) : this(obj)
        {
            CubeX = cubeX;
            CubeY = cubeY;
            CubeZ = cubeZ;

            var axial = HexMap<T>.CubicToAxial(new Tuple<int, int, int>(cubeX, cubeY, cubeZ));
            AxialX = axial.First;
            AxialZ = axial.Second;
        }

        // todo: offset coordinates

        private Hexagon(T obj)
        {
            Obj = obj;
            IsObstacle = false;

            Neighbors = new Dictionary<HexDirection, Hexagon<T>>
            {
                {HexDirection.MinusX, null},
                {HexDirection.MinusY, null},
                {HexDirection.MinusZ, null},
                {HexDirection.PlusX, null},
                {HexDirection.PlusY, null},
                {HexDirection.PlusZ, null},
            };
        }

    }
}
