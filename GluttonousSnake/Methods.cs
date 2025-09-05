using System.Diagnostics;

namespace GluttonousSnake
{
    /// <summary>
    /// 方法类, 用于存放一些方法
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// 是否按下了Esc键
        /// </summary>
        private static bool _isEscPressed = false ;

        /// <summary>
        /// 是否按下了Esc键
        /// </summary>
        public static bool IsEscPressed
        {
            get => _isEscPressed ;
            set => _isEscPressed = value ;
        }

        /// <summary>
        /// 用于输入, 同时检测是否按下了Esc键, 如果是密码输入, 则显示为*
        /// </summary>
        /// <param name="isPassword">是否是密码输入, 默认为false</param>
        /// <returns>输入的字符串, 如果按下了Esc键, 则返回null</returns>
        public static string? ReadInputWithEscCheck( bool isPassword = false )
        {
            Console.CursorVisible = true ;

            string input = "" ;
            int originalCursorLeft = Console.CursorLeft ;
            int currIndex = -1 ;

            while ( true )
            {
                if ( Console.KeyAvailable )
                {
                    var key = Console.ReadKey( intercept: true ) ;
                    if ( key.Key == ConsoleKey.Escape )
                    {
                        _isEscPressed = true ;
                        Console.CursorVisible = false ;
                        return null;
                    }
                    if ( key.Key == ConsoleKey.Enter )
                    {
                        Console.WriteLine();
                        break;
                    }
                    if ( key.Key == ConsoleKey.Backspace && Console.CursorLeft > originalCursorLeft )
                    {
                        input = input.Remove( currIndex , 1 ) ;
                        Console.SetCursorPosition( Console.CursorLeft - 1 , Console.CursorTop );
                        if ( isPassword )
                        {
                            string temp = new( '*' , input.Length - currIndex ) ;
                            Console.Write( temp + " \b" );
                        }
                        else
                        {
                            Console.Write( input[currIndex..] + " \b" );
                        }
                        Console.SetCursorPosition( Console.CursorLeft - input.Length + currIndex , Console.CursorTop );
                        -- currIndex ;
                    }
                    if ( key.Key == ConsoleKey.LeftArrow )
                    {
                        if ( Console.CursorLeft > originalCursorLeft )
                        {
                            Console.SetCursorPosition( Console.CursorLeft - 1 , Console.CursorTop );
                            -- currIndex ;
                        }
                    }
                    if ( key.Key == ConsoleKey.RightArrow )
                    {
                        if ( Console.CursorLeft < originalCursorLeft + input.Length )
                        {
                            Console.SetCursorPosition( Console.CursorLeft + 1 , Console.CursorTop );
                            ++ currIndex ;
                        }
                    }
                    if ( !char.IsControl( key.KeyChar ) )
                    {
                        Console.Write( isPassword ? '*' : key.KeyChar );
                        ++ currIndex ;
                        input = input.Insert( currIndex , key.KeyChar.ToString() ) ;
                        if ( isPassword )
                        {
                            string temp = new( '*' , input.Length - 1 - currIndex ) ;
                            Console.Write( temp );
                        }
                        else
                        {
                            Console.Write( input[( currIndex + 1 )..] );
                        }
                        Console.SetCursorPosition( Console.CursorLeft - input.Length + currIndex + 1 , Console.CursorTop );
                    }
                }
            }

            Console.CursorVisible = false ;
            return input;
        }

        /// <summary>
        /// 用于输出指定颜色的信息, 输出后会重置颜色
        /// </summary>
        /// <param name="message">要输出的信息</param>
        /// <param name="color">颜色, 默认为白色</param>
        public static void WriteColorMessage( string message , ConsoleColor color = ConsoleColor.White )
        {
            Console.ForegroundColor = color ;
            Console.Write( message );
            Console.ResetColor();
        }

        /// <summary>
        /// 用于暂停程序, 并输出提示信息, 要按任意键继续
        /// </summary>
        /// <param name="message">提示信息, 默认为"按任意键继续..."</param>
        public static void ReturnByKey( string message = "按任意键继续..." )
        {
            Console.WriteLine();
            Console.Write( message );
            Console.ReadKey();
        }

        /// <summary>
        /// 提示用户确认操作
        /// </summary>
        /// <returns>是否继续</returns>
        public static bool ConfirmOperation()
        {
            WriteColorMessage("是否继续? ( y / n )\n" , ConsoleColor.DarkBlue );
            ConsoleKeyInfo keyInfo = Console.ReadKey( true ) ;
            return keyInfo.Key == ConsoleKey.Y ;
        }

        /// <summary>
        /// 重启程序
        /// </summary>
        public static void Restart()
        {
            if ( Environment.ProcessPath == null )
            {
                Console.Clear();
                Console.WriteLine("系统重启失败!");
                Environment.Exit(1);
            }
            Process.Start( Environment.ProcessPath );
            Environment.Exit(0);
        }

        /// <summary>
        /// 加载程序动画
        /// </summary>
        public static void LoadProgram()
        {
            Random random = new() ;
            int loadPercent;
    
            loadPercent = 0 ;
            while ( loadPercent < 100 )
            {
                Console.Clear();
                WriteColorMessage( "tip: 按 Esc 键可以返回上一级窗口\n" , ConsoleColor.DarkMagenta );
                WriteColorMessage( "当提示按任意键继续时, 就不要按 Esc 键了, 否则可能会出现显示BUG\n\n" , ConsoleColor.DarkMagenta );
                Console.Write( $"环境检测中...{loadPercent}%" );
                Thread.Sleep( 150 );
                loadPercent = random.Next( loadPercent + 1 , 101 ) ;
            }
            if ( !ThreadVariables.IsWindowsSystem )
            {
                Console.Clear();
                WriteColorMessage("该程序只支持 Windows 系统!\n" , ConsoleColor.Red );
                ReturnByKey( "按任意键退出..." );
                return;
            }

            loadPercent = 0 ;
            while ( loadPercent < 100 )
            {
                Console.Clear();
                WriteColorMessage( "tip: 按 Esc 键可以返回上一级窗口\n" , ConsoleColor.DarkMagenta );
                WriteColorMessage( "当提示按任意键继续时, 就不要按 Esc 键了, 否则可能会出现显示BUG\n\n" , ConsoleColor.DarkMagenta );
                WriteColorMessage( "环境检测完成\n" , ConsoleColor.Green );
                Console.Write( $"用户账号加载中...{loadPercent}%" );
                Thread.Sleep( 150 );
                loadPercent = random.Next( loadPercent + 1 , 101 ) ;
            }
            if ( !ThreadVariables.IsLoadUserSystem )
            {
                Console.Clear();
                WriteColorMessage("用户账号加载失败!\n\n" , ConsoleColor.Red );
                WriteColorMessage("发生错误: " , ConsoleColor.DarkRed );
                Console.WriteLine( Error.Exception.Message );
                ReturnByKey( "按任意键退出..." );
                Environment.Exit( 1 );
            }

            loadPercent = 0 ;
            while ( loadPercent < 100 )
            {
                Console.Clear();
                WriteColorMessage( "tip: 按 Esc 键可以返回上一级窗口\n" , ConsoleColor.DarkMagenta );
                WriteColorMessage( "当提示按任意键继续时, 就不要按 Esc 键了, 否则可能会出现显示BUG\n\n" , ConsoleColor.DarkMagenta );
                WriteColorMessage( "环境检测完成\n" , ConsoleColor.Green );
                WriteColorMessage( "用户账号加载完成\n" , ConsoleColor.Green );
                Console.Write( $"用户数据加载中...{loadPercent}%" );
                Thread.Sleep( 150 );
                loadPercent = random.Next( loadPercent + 1 , 101 ) ;
            }
            if ( !ThreadVariables.IsLoadUserDatas )
            {
                Console.Clear();
                WriteColorMessage("用户数据加载失败!\n\n" , ConsoleColor.Red );
                WriteColorMessage("发生错误: " , ConsoleColor.DarkRed );
                Console.WriteLine( Error.Exception.Message );
                ReturnByKey( "按任意键退出..." );
                Environment.Exit( 1 );
            }

            Console.Clear();
            WriteColorMessage( "tip: 按 Esc 键可以返回上一级窗口\n" , ConsoleColor.DarkMagenta );
            WriteColorMessage( "当提示按任意键继续时, 就不要按 Esc 键了, 否则可能会出现显示BUG\n\n" , ConsoleColor.DarkMagenta );
            WriteColorMessage( "环境检测完成\n" , ConsoleColor.Green );
            WriteColorMessage( "用户账号加载完成\n" , ConsoleColor.Green );
            WriteColorMessage( "用户数据加载完成\n" , ConsoleColor.Green );
            WriteColorMessage( "\n程序加载完成!\n" , ConsoleColor.DarkBlue );
            ReturnByKey( "按任意键进入程序" );
        }

        /// <summary>
        /// 判断窗口大小是否合适游玩
        /// </summary>
        /// <returns>是否合适</returns>
        private static bool IsWindowSuitable()
        {
            if ( Console.WindowWidth < Constants.NeedWindowWidth || Console.WindowHeight < Constants.NeedWindowHeight )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 用于加载游戏, 并检测窗口大小
        /// </summary>
        /// <returns>是否加载成功</returns>
        public static bool LoadGame()
        {
            Random random = new() ;
            int loadPercent;

            loadPercent = 0 ;
            while ( loadPercent < 100 )
            {
                Console.Clear();
                WriteColorMessage("欢迎来到贪吃蛇游戏, 请把窗口最大化以正常游玩!\n" , ConsoleColor.DarkBlue );
                WriteColorMessage("操作说明: W 上, S 下, A 左, D 右, 空格 暂停,Enter 切换速度\n" , ConsoleColor.DarkBlue );
                WriteColorMessage( "当蛇头碰到蛇身或墙壁, 游戏结束\n" , ConsoleColor.DarkBlue );
                WriteColorMessage( "当蛇吃到绿色的食物, 长度+1, 分数+1\n\n" , ConsoleColor.DarkBlue );

                Console.Write( $"环境检测中...{loadPercent}%" );
                Thread.Sleep( 100 );
                loadPercent = random.Next( loadPercent + 1 , 101 ) ;
            }
            if ( !IsWindowSuitable() )
            {
                Console.Clear();
                WriteColorMessage("请放大窗口后游玩!!!\n" , ConsoleColor.Red );
                ReturnByKey();
                return false;
            }

            loadPercent = 0 ;
            while ( loadPercent < 100 )
            {
                Console.Clear();
                WriteColorMessage("欢迎来到贪吃蛇游戏, 请把窗口最大化以正常游玩!\n" , ConsoleColor.DarkBlue );
                WriteColorMessage("操作说明: W 上, S 下, A 左, D 右, 空格 暂停,Enter 切换速度\n" , ConsoleColor.DarkBlue );
                WriteColorMessage( "当蛇头碰到蛇身或墙壁, 游戏结束\n" , ConsoleColor.DarkBlue );
                WriteColorMessage( "当蛇吃到绿色的食物, 长度+1, 分数+1\n\n" , ConsoleColor.DarkBlue );

                WriteColorMessage( "环境检测完成\n" , ConsoleColor.Green );
                Console.Write( $"游戏加载中...{loadPercent}%" );
                Thread.Sleep( 100 );
                loadPercent = random.Next( loadPercent + 1 , 101 ) ;
            }

            Console.Clear();
            WriteColorMessage("欢迎来到贪吃蛇游戏, 请把窗口最大化以正常游玩!\n" , ConsoleColor.DarkBlue );
            WriteColorMessage("操作说明: W 上, S 下, A 左, D 右, 空格 暂停,Enter 切换速度\n" , ConsoleColor.DarkBlue );
            WriteColorMessage( "当蛇头碰到蛇身或墙壁, 游戏结束\n" , ConsoleColor.DarkBlue );
            WriteColorMessage( "当蛇吃到绿色的食物, 长度+1, 分数+1\n\n" , ConsoleColor.DarkBlue );
            WriteColorMessage( "环境检测完成\n" , ConsoleColor.Green );
            WriteColorMessage( "游戏加载完成\n" , ConsoleColor.Green );
            ReturnByKey();

            return true;
        }
    }
}