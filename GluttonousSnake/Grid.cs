namespace GluttonousSnake
{
    /// <summary>
    /// 格子类, 用于表示地图上的一个格子
    /// </summary>
    /// <param name="x">x坐标</param>
    /// <param name="y">y坐标</param>
    /// <param name="type">格子类型</param>
    public class Grid( int x , int y , GridType type = GridType.Empty ) 
    {
        private readonly int _x = x ;
        private readonly int _y = y ;
        private GridType _type = type ;

        /// <summary>
        /// x坐标
        /// </summary>
        public int X => _x ;

        /// <summary>
        /// y坐标
        /// </summary>
        public int Y => _y ;

        /// <summary>
        /// 格子类型
        /// </summary>
        public GridType Type
        {
            get => _type ;
            set => _type = value ;
        }

        public override string ToString()
        {
            return $"( {_x} , {_y} )";
        }
        public override bool Equals( object? obj )
        {
            if ( obj is Grid grid )
            {
                return _x == grid._x && _y == grid._y ;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine( _x , _y );
        }
    }
}