namespace GluttonousSnake
{
    /// <summary>
    /// 用户类, 用来处理用户的一些操作
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="id">用户id</param>
    public class User( string username , string password , int id )
    {
        private string _username = username ;
        private string _password = UserData.DecryptPassword( password ) ;
        private int id = id ;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get => _username ;
            set => _username = value ;
        }

        /// <summary>
        /// 密码, 自动加密解密
        /// </summary>
        public string Password
        {
            get => UserData.EncryptPassword( _password ) ;
            set => _password = UserData.DecryptPassword( value ) ;
        }

        /// <summary>
        /// 用户id
        /// </summary>
        public int Id
        {
            get => id ;
            set => id = value ;
        }

        /// <summary>
        /// 判断一个密码是否合法
        /// </summary>
        /// <remarks>
        /// 长度6-15,只能包含大小写字母和数字,并且至少包含其中两种
        /// </remarks>
        /// <param name="password">密码</param>
        /// <returns>是否合法</returns>
        public static bool IsPassword( string password )
        {
            if ( password.Length < Constants.MinPasswordLength || password.Length > Constants.MaxPasswordLength )
            {
                return false;
            }

            bool hasUpper = false ;
            bool hasLower = false ;
            bool hasDigit = false ;

            foreach ( char c in password )
            {       
                if ( char.IsUpper(c) )
                {
                    hasUpper = true ;
                }
                else if ( char.IsLower(c) )
                {
                    hasLower = true ;
                }
                else if ( char.IsDigit(c) )
                {
                    hasDigit = true ;
                }
                else
                {
                    return false; // 包含非法字符
                }
            }

            var typesCount = ( hasUpper ? 1 : 0 ) + ( hasLower ? 1 : 0 ) + ( hasDigit ? 1 : 0 ) ;
            return typesCount >= 2 ;
        }

        /// <summary>
        /// 判断用户名是否合法
        /// </summary>
        /// <remarks>
        /// 长度2-10,不能包含空格,其余字符不限 (可以包含中文)
        /// </remarks>
        /// <param name="username">用户名</param>
        /// <returns>是否合法</returns>
        public static bool IsUsername( string username )
        {
            if ( username.Length < Constants.MinUsernameLength || username.Length > Constants.MaxUsernameLength )
            {
                return false;
            }

            foreach ( char c in username )
            {
                if ( char.IsWhiteSpace(c) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 显示用户界面
        /// </summary>
        private void ShowInterface()
        {
            Console.Clear();
            Methods.WriteColorMessage( "欢迎 " , ConsoleColor.Cyan );
            Console.Write( _username );
            Methods.WriteColorMessage( " ( id: " , ConsoleColor.DarkCyan );
            Console.Write( id );
            Methods.WriteColorMessage( " )\n" , ConsoleColor.DarkCyan );

            Methods.WriteColorMessage( "*************** " , ConsoleColor.Green );
            Methods.WriteColorMessage( "用户系统" , ConsoleColor.Yellow );
            Methods.WriteColorMessage( " ***************\n\n" , ConsoleColor.Green );

            Methods.WriteColorMessage( "1. 开始游戏\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "2. 查看历史记录\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "3. 更改密码\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "4. 注销账户\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "Esc. 退出登录\n\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "***************************************\n" , ConsoleColor.Green );
        }

        /// <summary>
        /// 当前用户进行操作
        /// </summary>
        /// <returns>最新的用户信息, 注销账户或退出登录返回null</returns>
        public User? Work()
        {
            do
            {
                ShowInterface();
                
                ConsoleKeyInfo keyInfo = Console.ReadKey( true ) ;
                switch ( keyInfo.Key )
                {
                    case ConsoleKey.D1:
                        StartGame();
                        break;
                    case ConsoleKey.D2:
                        ShowHistory();
                        break;
                    case ConsoleKey.D3:
                        if ( ChangePassword() )
                        {
                            return this;
                        }
                        break;
                    case ConsoleKey.D4:
                        UserData.DeleteUserDataFile( id );
                        Methods.WriteColorMessage( "\n注销账户成功!\n" , ConsoleColor.DarkGreen );
                        return null;
                    case ConsoleKey.Escape:
                        Methods.WriteColorMessage( "\n退出登录成功!\n" , ConsoleColor.DarkGreen );
                        return this;
                    default:
                        break;
                }
            } while ( true );
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            Console.Clear();
            Methods.WriteColorMessage( "注意:请将窗口最大化以正常显示游戏界面\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "注意:请将窗口最大化以正常显示游戏界面\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "注意:请将窗口最大化以正常显示游戏界面\n" , ConsoleColor.DarkMagenta );
            Methods.ReturnByKey();

            if ( !Methods.LoadGame() )
            {
                return;
            }

            Game game = new() ;
            game.Run( out int score , out DateTime endTime , out TimeSpan survivalTime );
            UserData.AddGameResult( id , new( _username , score , endTime ) );

            Console.Clear();
            Methods.WriteColorMessage( "游戏结束!\n\n" , ConsoleColor.DarkGreen );
            Methods.WriteColorMessage( "最终分数: " , ConsoleColor.DarkBlue );
            Console.WriteLine( score );
            Methods.WriteColorMessage( "存活时间: " , ConsoleColor.DarkBlue );
            Console.WriteLine( survivalTime.TotalSeconds.ToString( "F3" ) + "s" );
            Console.Write( "\n按 Esc 退出..." );
            while ( Console.ReadKey( true ).Key != ConsoleKey.Escape ) ;
        }

        /// <summary>
        /// 查看历史游戏记录, 可以选择排序方式
        /// </summary>
        private void ShowHistory()
        {
            Console.Clear();
            
            List<GameResult>? gameResults = UserData.ReadGameResults( id ) ;
            if ( gameResults == null || gameResults.Count == 0 )
            {
                Methods.WriteColorMessage( "暂无历史记录!\n" , ConsoleColor.DarkYellow );
                Methods.ReturnByKey();
                return;
            }

            do
            {
                ShowGameResultsInterface( gameResults );

                ConsoleKeyInfo keyInfo = Console.ReadKey( true ) ; // 捕获键盘输入
                switch ( keyInfo.Key )
                {
                    case ConsoleKey.S:
                        UserData.SortGameResults( ref gameResults );
                        break;
                    case ConsoleKey.D:
                        UserData.SortGameResults( ref gameResults , false );
                        break;
                    case ConsoleKey.Escape:
                        return;
                    default:
                        break;
                }
            } while ( true );
        }

        /// <summary>
        /// 显示gameResult
        /// </summary>
        /// <param name="gameResults"></param>
        private static void ShowGameResultsInterface( List<GameResult> gameResults )
        {
            Console.Clear();
            Methods.WriteColorMessage( "***************" , ConsoleColor.Green );
            Methods.WriteColorMessage( " 历史记录 " , ConsoleColor.Yellow );
            Methods.WriteColorMessage( "***************\n\n" , ConsoleColor.Green );
            string title = "分数".PadRight( Constants.MaxScoreLength ) + "   " + "时间" ;
            Methods.WriteColorMessage( title + "\n" , ConsoleColor.Blue );
            Methods.WriteColorMessage( "--------------------------------\n" , ConsoleColor.DarkBlue );
            foreach ( var gameResult in gameResults )
            {
                string score = gameResult.Score.ToString().PadRight( Constants.MaxScoreLength ) ;
                string time = gameResult.EndTime.ToString() ;
                Console.WriteLine( score + "   " + time );
            }
            Methods.WriteColorMessage( "\n--------------------------------\n" , ConsoleColor.DarkBlue );

            Methods.WriteColorMessage( "S. 按分数排序\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "D. 按时间排序\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "Esc. 返回\n\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "***************************************\n" , ConsoleColor.Green );
        }

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <returns>是否更改成功</returns>
        private bool ChangePassword()
        {
            do
            {
                Console.Clear();
                Methods.WriteColorMessage( "***************" , ConsoleColor.Green );
                Methods.WriteColorMessage( " 更改密码 " , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "***************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "\n请输入原密码:" , ConsoleColor.DarkBlue );

                string? oldPassword = Methods.ReadInputWithEscCheck( true ) ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return false;
                }

                if ( oldPassword != _password )
                {
                    Methods.WriteColorMessage( "\n原密码错误!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                Console.Clear();
                Methods.WriteColorMessage( "***************" , ConsoleColor.Green );
                Methods.WriteColorMessage( " 更改密码 " , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "***************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "提示: 密码长度6-15,只能包含大小写字母和数字,并且至少包含其中两种\n" , ConsoleColor.DarkMagenta );
                Methods.WriteColorMessage( "\n请输入新密码:" , ConsoleColor.DarkBlue );
                string? newPassword = Methods.ReadInputWithEscCheck( true ) ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return false;
                }

                if ( newPassword == null || !IsPassword( newPassword ) )
                {
                    Methods.WriteColorMessage( "\n密码不合法!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                Console.Clear();
                Methods.WriteColorMessage( "***************" , ConsoleColor.Green );
                Methods.WriteColorMessage( " 更改密码 " , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "***************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "\n请再次输入新密码:" , ConsoleColor.DarkBlue );
                string? newPasswordAgain = Methods.ReadInputWithEscCheck( true ) ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return false;
                }
                
                if ( newPassword != newPasswordAgain )
                {
                    Methods.WriteColorMessage( "\n两次输入的密码不一致!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                _password = newPassword ;
                Methods.WriteColorMessage( "\n密码更改成功, 将自动退出登录保存更改\n" , ConsoleColor.DarkGreen );
                break;
            } while ( true );

            return true;
        }
    }
}