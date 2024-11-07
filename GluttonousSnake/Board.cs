namespace GluttonousSnake
{
    /// <summary>
    /// 场地类, 用于存储和显示游戏场地
    /// </summary>
    public class Board
    {
        /// <summary>
        /// 场地格子数组
        /// </summary>
        private readonly Grid[,]  _grids;

        /// <summary>
        /// 线程锁, 保证多线程下对场地的操作不会冲突
        /// </summary>
        private static readonly object _lock = new() ;

        /// <summary>
        /// 构造函数, 初始化场地
        /// </summary>
        public Board()
        {
            _grids = new Grid[ Constants.GridWidth , Constants.GridHeight ] ;
            for ( int i = 0 ; i < Constants.GridWidth ; i++ )
            {
                for ( int j = 0 ; j < Constants.GridHeight ; j++ )
                {
                    _grids[ i , j ] = new Grid( i , j ) ;
                }
            }
        }

        /// <summary>
        /// 索引器, 通过坐标获取格子
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns>指定坐标的格子</returns>
        public Grid this[ int x , int y ]
        {
            get => _grids[ x , y ] ;
            set => _grids[ x , y ] = value ;
        }

        /// <summary>
        /// 输出场地( 包括蛇和食物 )
        /// </summary>
        /// <param name="snake">蛇对象</param>
        public void Show( Snake snake )
        {
            Console.Clear();

            for ( int i = 0 ; i < Constants.GridWidth * Constants.OneGridSize + 3 ; i++ )
            {
                Methods.WriteColorMessage( "X" , ConsoleColor.Red );
            }
            Console.WriteLine();
            for ( int i = 0 ; i < Constants.GridHeight ; i++ )
            {
                Methods.WriteColorMessage( "X" , ConsoleColor.Red );
                for ( int j = 0 ; j < Constants.GridWidth ; j++ )
                {
                    Console.Write( " " );
                    ShowGrid( _grids[ j , i ].Type , snake.Direction );
                }
                Methods.WriteColorMessage( " X" , ConsoleColor.Red );
                Console.WriteLine();
            }
            for ( int i = 0 ; i < Constants.GridWidth * Constants.OneGridSize + 3 ; i++ )
            {
                Methods.WriteColorMessage( "X" , ConsoleColor.Red );
            }
        }

        /// <summary>
        /// 输出指定类型的格子
        /// </summary>
        /// <param name="type">格子类型</param>
        /// <param name="direction">蛇头方向</param>
        private static void ShowGrid( GridType type , Direction direction = Direction.Right )
        {
            switch( type )
            {
                case GridType.Empty:
                    Console.Write( " " );
                    break;
                case GridType.SnakeHead:
                    ShowSnakeHead( direction );
                    break;
                case GridType.SnakeBody:
                    Console.Write( "■" );
                    break;
                case GridType.Food:
                    Methods.WriteColorMessage( "★" , ConsoleColor.Green );
                    break;
            }
        }

        /// <summary>
        /// 显示有方向的蛇头
        /// </summary>
        /// <param name="direction">蛇头方向</param>
        private static void ShowSnakeHead( Direction direction )
        {
            switch( direction )
            {
                case Direction.Up:
                    Console.Write( "▲" );
                    break;
                case Direction.Down:
                    Console.Write( "▼" );
                    break;
                case Direction.Left:
                    Console.Write( "◀" );
                    break;
                case Direction.Right:
                    Console.Write( "▶" );
                    break;
            }
        }

        /// <summary>
        /// 移动光标到指定位置更改指定格子的显示
        /// </summary>
        /// <param name="grid">要更改的格子</param>
        /// <param name="direction">蛇头方向, 默认为右</param>
        public static void ChangeGrid( Grid grid , Direction direction = Direction.Right )
        {
            lock ( _lock )
            {
                Console.SetCursorPosition( ( grid.X + 1 ) * Constants.OneGridSize , grid.Y + 1 );
                ShowGrid( grid.Type , direction );
            }
        }

        /// <summary>
        /// 更改底部状态栏
        /// </summary>
        /// <param name="score">得分</param>
        /// <param name="time">存活时间</param>
        public static void ChangeStatusBar( int score , TimeSpan time )
        {
            lock ( _lock )
            {
                Console.SetCursorPosition( 0 , Constants.GridHeight + 3 );
                Methods.WriteColorMessage( "当前蛇长: " , ConsoleColor.DarkBlue );
                Console.Write( score + Constants.InitialSnakeLength );  
                Console.Write( "   " );
                Methods.WriteColorMessage( "当前得分: " , ConsoleColor.DarkBlue );
                Console.Write( score );
                Console.Write( "   " );
                Methods.WriteColorMessage( "是否加速状态: " , ConsoleColor.DarkBlue );
                Console.Write( ThreadVariables.IsAccelarate ? "是" : "否" );
                Console.Write( "   " );
                Methods.WriteColorMessage( "是否暂停状态: " , ConsoleColor.DarkBlue );
                Console.Write( ThreadVariables.IsPaused ? "是" : "否" );
                Console.Write( "   " );
                Methods.WriteColorMessage( "存活时间: " , ConsoleColor.DarkBlue );
                Console.Write( time.TotalSeconds.ToString( "F3" ) + "s" );
            }
        }
    }
}