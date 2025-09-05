namespace GluttonousSnake
{
    /// <summary>
    /// 蛇类, 用来实现贪吃蛇的逻辑
    /// </summary>
    public class Snake
    {
        private readonly Queue<Grid> _body = [] ;
        private int _length = Constants.InitialSnakeLength ;
        private Direction _direction = Direction.Right ;

        /// <summary>
        /// 蛇的身体, 用队列存储, 队列头为蛇尾, 队列尾为蛇头
        /// </summary>
        public Queue<Grid> Body => _body ;

        /// <summary>
        /// 蛇的长度
        /// </summary>
        public int Length 
        {
            get => _length ;
            set => _length = value ;
        }

        /// <summary>
        /// 蛇的移动方向
        /// </summary>
        public Direction Direction
        {
            get => _direction ;
            set => _direction = value ;
        }

        /// <summary>
        /// 构造函数, 初始化蛇
        /// </summary>
        /// <param name="board">场地</param>
        public Snake( Board board )
        {
            for ( int i = 0 ; i < Constants.InitialSnakeLength ; i++ )
            {
                _body.Enqueue( new Grid( Constants.InitialSnakeX + i , Constants.InitialSnakeY , GridType.SnakeBody ) ) ;
            }
            _body.Last().Type = GridType.SnakeHead ;
            SaveSnakeToBoard( board ) ;
        }

        /// <summary>
        /// 保存蛇到场地中
        /// </summary>
        /// <param name="board">场地</param>
        private void SaveSnakeToBoard( Board board )
        {
            foreach ( var grid in _body )
            {
                board[ grid.X , grid.Y ].Type = grid.Type ;
            }
        }

        /// <summary>
        /// 蛇沿当前方向前进一格, 并更新场地
        /// </summary>
        /// <param name="board">场地</param>
        /// <param name="score">分数</param>
        /// <param name="foodCount">食物数量</param>
        /// <returns>-1表示游戏结束, 0表示正常前进</returns>
        public int GoAhead( Board board , ref int score , ref int foodCount ) 
        {
            // 计算蛇头的下一个位置
            Grid head = _body.Last() ;
            int x = head.X , y = head.Y ;
            switch ( _direction )
            {
                case Direction.Up:
                    y-- ;
                    break ;
                case Direction.Down:
                    y++ ;
                    break ;
                case Direction.Left:
                    x-- ;
                    break ;
                case Direction.Right:
                    x++ ;
                    break ;
            }

            // 判断是否撞墙或撞自己
            if ( x < 0 || x >= Constants.GridWidth || y < 0 || y >= Constants.GridHeight )
            {
                return -1 ;
            }
            if ( _body.Any( grid => grid.X == x && grid.Y == y ) )
            {
                return -1 ;
            }

            // 更新蛇头的位置
            _body.Last().Type = GridType.SnakeBody ;
            Board.ChangeGrid( _body.Last() );
            _body.Enqueue( new Grid( x , y , GridType.SnakeHead ) ) ;
            Board.ChangeGrid( _body.Last() , _direction );

            // 判断是否吃到食物, 并保存蛇到场地
            if ( board[ x , y ].Type == GridType.Food )
            {
                ++_length ;
                --foodCount ;
                ++score ;
            }
            else
            {
                Grid tail = _body.Dequeue() ;
                board[ tail.X , tail.Y ].Type = GridType.Empty ;
                Board.ChangeGrid( board[ tail.X , tail.Y ] );
            }
            board[ x , y ].Type = GridType.SnakeHead ;
            board[ head.X , head.Y ].Type = GridType.SnakeBody ;

            return 0;
        }
    }
}