namespace GluttonousSnake
{
    /// <summary>
    /// 游戏类, 用于实现游戏的进行
    /// </summary>
    public class Game
    {
        /// <summary>
        /// 游戏场地
        /// </summary>
        private readonly Board _board ;

        /// <summary>
        /// 贪吃蛇
        /// </summary>
        private readonly Snake _snake ;

        /// <summary>
        /// 当前得分
        /// </summary>
        private int _score;

        /// <summary>
        /// 当前场地上食物数量
        /// </summary>
        private int _foodCount;

        /// <summary>
        /// 游戏开始时间
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// 暂停开始时间
        /// </summary>
        private DateTime _startPauseTime;

        /// <summary>
        /// 总暂停时间
        /// </summary>
        private TimeSpan _totalPauseTime;

        /// <summary>
        /// 存活时间
        /// </summary>
        private TimeSpan _survivalTime;
        
        /// <summary>
        /// 线程锁, 保证对场地操作的线程安全
        private readonly object _lock;

        /// <summary>
        /// 构造函数, 初始化游戏
        /// </summary>
        public Game()
        {
            _board = new Board() ;
            _snake = new Snake( _board ) ;
            _score = 0 ;
            _foodCount = 0 ;
            _startTime = new() ;
            _startPauseTime = new() ;
            _totalPauseTime = new( 0 ) ;
            _survivalTime = new() ;
            _lock = new() ;
        }

        /// <summary>
        /// 进行游戏
        /// </summary>
        /// <param name="score">返回最终得分</param>
        /// <param name="endTime">返回游戏结束时间</param>
        /// <param name="survivalTime">返回存活时间</param>
        public void Run( out int score , out DateTime endTime , out TimeSpan survivalTime )
        {
            ThreadVariables.IsGameOver = false ;
            ThreadVariables.IsPaused = false ;
            ThreadVariables.IsAccelarate = false ;
            ThreadVariables.NextDirection = Direction.Right ;

            _board.Show( _snake );
            _startTime = DateTime.Now ;

            Thread checkKeyThread = new( CheckKey ) ;
            Thread generateFoodThread = new( GenerateFood ) ;
            Thread calculateSurvivalTimeThread = new( CalculateSurvivalTime ) ;

            checkKeyThread.Start();
            generateFoodThread.Start();
            calculateSurvivalTimeThread.Start();
            Update();

            checkKeyThread.Join();
            generateFoodThread.Join();
            calculateSurvivalTimeThread.Join();

            score = _score ;
            endTime = DateTime.Now ;
            survivalTime = _survivalTime ;
        }

        /// <summary>
        /// 贪吃蛇持续前进并更新游戏状态
        /// </summary>
        private void Update()
        {
            while ( true )
            {
                Board.ChangeStatusBar( _score , _survivalTime );

                // 暂停
                if ( ThreadVariables.IsPaused )
                {
                    Thread.Sleep( 10 );
                    continue;
                }

                // 更新前进方向
                if ( !IsOpposite( _snake.Direction , ThreadVariables.NextDirection ) )
                {
                    _snake.Direction = ThreadVariables.NextDirection ;
                }

                // 蛇前进一格
                lock ( _lock )
                {
                    int result = _snake.GoAhead( _board , ref _score , ref _foodCount ) ;
                    if ( result == -1 )
                    {
                        ThreadVariables.IsGameOver = true ;
                        return;
                    }
                }

                Thread.Sleep( ThreadVariables.IsAccelarate ? Constants.FastSpeed : Constants.NormalSpeed );
            }
        }

        /// <summary>
        /// 判断两个方向是否相反 
        /// </summary>
        /// <returns>返回是否相反</returns>
        private static bool IsOpposite( Direction direction1 , Direction direction2 )
        {
            return ( direction1 == Direction.Up && direction2 == Direction.Down ) ||
                   ( direction1 == Direction.Down && direction2 == Direction.Up ) ||
                   ( direction1 == Direction.Left && direction2 == Direction.Right ) ||
                   ( direction1 == Direction.Right && direction2 == Direction.Left ) ;
        }

        /// <summary>
        /// 检测键盘按键, 并更新下一步方向
        /// </summary>
        private void CheckKey()
        {
            while ( !ThreadVariables.IsGameOver )
            {
                if ( Console.KeyAvailable )
                {
                    ConsoleKey key = Console.ReadKey( true ).Key ;
                    switch ( key )
                    {
                        case ConsoleKey.W:
                            ThreadVariables.NextDirection = Direction.Up ;
                            break;
                        case ConsoleKey.S:
                            ThreadVariables.NextDirection = Direction.Down ;
                            break;
                        case ConsoleKey.A:
                            ThreadVariables.NextDirection = Direction.Left ;
                            break;
                        case ConsoleKey.D:
                            ThreadVariables.NextDirection = Direction.Right ;
                            break;
                        case ConsoleKey.Spacebar:
                            ThreadVariables.IsPaused = !ThreadVariables.IsPaused ;
                            break;
                        case ConsoleKey.Enter:
                            ThreadVariables.IsAccelarate = !ThreadVariables.IsAccelarate ;
                            break;
                        default:
                            break;
                    }
                }
                Thread.Sleep( 10 );
            }
        }

        /// <summary>
        /// 随机在场地生成食物
        /// </summary>
        private void GenerateFood()
        {
            Random random = new() ;
            int i , x , y ;

            while ( !ThreadVariables.IsGameOver )
            {
                if ( _foodCount >= Constants.MaxFoodCount || ThreadVariables.IsPaused )
                {
                    Thread.Sleep( 10 );
                    continue;
                }

                for ( i = 0 ; i < random.Next( Constants.GenerateFoodCountOnce ) ; i++ )
                {
                    do
                    {
                        x = random.Next( Constants.GridWidth ) ;
                        y = random.Next( Constants.GridHeight ) ;

                        lock ( _lock )
                        {
                            if ( !_snake.Body.Any( grid => grid.X == x && grid.Y == y ) )
                            {
                                _board[x, y].Type = GridType.Food ;
                                Board.ChangeGrid( _board[x, y] );
                                ++_foodCount ;
                                break;
                            }
                        }
                    } while ( true ) ;

                    lock ( _lock )
                    {
                        _board[ x , y ].Type = GridType.Food ;
                        Board.ChangeGrid( _board[ x , y ] ) ;
                        ++_foodCount ;
                    }
                }

                Thread.Sleep( Constants.GenerateFoodSpeed );
            }
        }

        /// <summary>
        /// 统计存活时间并更新
        /// </summary>
        private void CalculateSurvivalTime()
        {
            while ( !ThreadVariables.IsGameOver )
            {
                if ( ThreadVariables.IsPaused )
                {
                    _startPauseTime = DateTime.Now ;
                    while ( ThreadVariables.IsPaused ) ;
                    _totalPauseTime += DateTime.Now - _startPauseTime ;
                }
                
                _survivalTime = DateTime.Now - _startTime - _totalPauseTime ;
                Thread.Sleep( 10 );
            }
        }
    }
}