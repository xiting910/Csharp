using System.Text.Json;

namespace GluttonousSnake
{
    /// <summary>
    /// 用户系统类( 静态类 ), 用于管理用户
    /// </summary>
    public static class UserSystem
    {
        /// <summary>
        /// 用于记录所有已经存在的用户
        /// </summary>
        private static List<User> users = [] ;
  
        /// <summary>
        /// 用于记录当前登录的用户
        /// </summary>
        private static User? currUser = null ;

        /// <summary>
        /// 静态构造函数, 用于初始化用户系统
        /// </summary>
        static UserSystem()
        {
            if ( !Directory.Exists( Constants.DataPath ) )
            {
                try
                {
                    Directory.CreateDirectory( Constants.DataPath );
                }
                catch ( Exception e )
                {
                    Error.Exception = e ;
                    ThreadVariables.IsLoadUserSystem = false ;
                    return;
                }
            }
            else
            {
                UserData.RemoveHiddenAttributeForDirectory( Constants.DataPath );
            }

            if ( !File.Exists( Constants.UserFilePath ) )
            {
                try
                {
                    string directoryPath = Path.GetDirectoryName( Constants.UserFilePath ) ?? "" ;
                    if ( !Directory.Exists( directoryPath ) )
                    {
                        Directory.CreateDirectory( directoryPath );
                    }
                    File.Create( Constants.UserFilePath ).Close();
                }
                catch ( Exception e )
                {
                    Error.Exception = e ;
                    ThreadVariables.IsLoadUserSystem = false ;
                    return;
                }
            }

            string json = File.ReadAllText( Constants.UserFilePath ) ;
            if ( !string.IsNullOrEmpty( json ) )
            {
                users = JsonSerializer.Deserialize< List<User> >( json ) ?? [] ;
            }

            UserData.SetHiddenAttribute( Constants.UserFilePath );
        }

        /// <summary>
        /// 保存用户信息到json文件
        /// </summary>
        private static void SaveUsers()
        {
            UserData.RemoveHiddenAttribute( Constants.UserFilePath );

            File.WriteAllText( Constants.UserFilePath , "" ) ;

            if ( users.Count != 0 )
            {
                string json = JsonSerializer.Serialize( users ) ;
                File.WriteAllText( Constants.UserFilePath , json ) ;  
            }

            UserData.SetHiddenAttribute( Constants.UserFilePath );
        }

        /// <summary>
        /// 通过用户名查找已有用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>找到的用户, 找不到则返回null</returns>
        private static User? FindUser( string username )
        {
            return users.Find( user => user.Username == username );
        }

        /// <summary>
        /// 通过id查找已有用户
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns>找到的用户, 找不到则返回null</returns>
        private static User? FindUser( int id )
        {
            return users.Find( user => user.Id == id );
        }

        /// <summary>
        /// 展示主界面窗口
        /// </summary>
        private static void ShowInterface()
        {
            Console.Clear();
            Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
            Methods.WriteColorMessage( "贪吃蛇游戏1.0" , ConsoleColor.Yellow );
            Methods.WriteColorMessage( "*****************\n" , ConsoleColor.Green );
            Methods.WriteColorMessage( "\n1. 创建新账号\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "2. 登录账号\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "3. 显示已有账号\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "4. 显示排行榜\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "5. 登录管理员\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "Esc. 退出\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage( "\n***********************************************\n" , ConsoleColor.Green );
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="loadThread">加载线程, 用于等待加载完成</param>
        public static void Work( Thread loadThread )
        {
            loadThread.Join(); 

            while ( true )
            {
                ShowInterface();

                ConsoleKeyInfo keyInfo = Console.ReadKey( true ) ;
                switch ( keyInfo.Key )
                {
                    case ConsoleKey.D1:
                        CreateUser(); 
                        break;
                    case ConsoleKey.D2:
                        LoginUser();
                        if ( currUser != null )
                        {
                            int index = users.FindIndex( user => user.Id == currUser.Id ) ;
                            currUser = currUser.Work() ;
                            users.RemoveAt( index ) ;
                            if ( currUser != null )
                            {
                                users.Add( currUser ) ;
                            }
                            SaveUsers();
                            currUser = null ;
                            Methods.ReturnByKey();
                        }
                        break;
                    case ConsoleKey.D3:
                        ShowUsers();
                        break;
                    case ConsoleKey.D4:
                        ShowRankingList();
                        break;
                    case ConsoleKey.D5:
                        LoginAdministrator();
                        break;
                    case ConsoleKey.Escape:
                        UserData.SetHiddenAttributeForDirectory( Constants.DataPath );
                        return;
                    default:
                        break;
                }
            }
        }
        
        /// <summary>
        /// 创建新用户
        /// </summary>
        private static void CreateUser()
        {
            string? username;
            string? password1;
            string? password2;
            int id;

            do
            {
                Console.Clear();
                Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
                Methods.WriteColorMessage( "创建新账号" , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "*****************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "用户名长度2-10,不能包含空格,其余字符不限(可以包含中文)\n" , ConsoleColor.DarkMagenta );
                Methods.WriteColorMessage( "\n请输入用户名:" , ConsoleColor.DarkBlue );

                username = Methods.ReadInputWithEscCheck() ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return;
                }

                if ( string.IsNullOrEmpty( username ) )
                {
                    Methods.WriteColorMessage( "\n用户名不能为空!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }
                if ( !User.IsUsername( username ) )
                {
                    Methods.WriteColorMessage( "\n用户名不合法!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }
                if ( FindUser( username ) != null )
                {
                    Methods.WriteColorMessage( "\n用户名已存在!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                break;
            } while ( true );

            do
            {
                Console.Clear();
                Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
                Methods.WriteColorMessage( "创建新账号" , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "*****************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "密码长度6-15,只能包含大小写字母和数字,并且至少包含其中两种\n" , ConsoleColor.DarkMagenta );
                Methods.WriteColorMessage( "\n请输入密码:" , ConsoleColor.DarkBlue );

                password1 = Methods.ReadInputWithEscCheck( true ) ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return;
                }

                if ( string.IsNullOrEmpty( password1 ) )
                {
                    Methods.WriteColorMessage( "\n密码不能为空!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }
                if ( !User.IsPassword( password1 ) )
                {
                    Methods.WriteColorMessage( "\n密码不合法!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                Console.Clear();
                Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
                Methods.WriteColorMessage( "创建新账号" , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "*****************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "\n请再次确认密码:" , ConsoleColor.DarkBlue );

                password2 = Methods.ReadInputWithEscCheck( true ) ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return;
                }

                if ( password2 != password1 )
                {
                    Methods.WriteColorMessage( "\n两次输入的密码不一致!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                break;
            } while ( true );

            do
            {
                Random random = new() ;
                id = random.Next( Constants.MinId , Constants.MaxId + 1 ) ;

                if ( FindUser( id ) == null )
                {
                    break;
                }
            } while ( true ) ;

            User newUser = new( username , UserData.EncryptPassword( password1 ) , id ) ;
            users.Add( newUser );
            SaveUsers();
            UserData.CreateUserDataFile( id );
            Console.Clear();
            Methods.WriteColorMessage( "创建成功!\n" , ConsoleColor.DarkGreen );
            Methods.ReturnByKey();
        }

        /// <summary>
        /// 登录用户
        /// </summary>
        private static void LoginUser()
        {
            if ( users.Count == 0 )
            {
                Console.Clear();
                Methods.WriteColorMessage( "当前没有任何用户账号!\n" , ConsoleColor.DarkYellow );
                Methods.ReturnByKey();
                return;
            }

            string? username;
            string? password;

            do 
            {
                Console.Clear();
                Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
                Methods.WriteColorMessage( "登录账号" , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "*****************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "\n请输入用户名:" , ConsoleColor.DarkBlue );

                username = Methods.ReadInputWithEscCheck() ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return;
                }

                if ( string.IsNullOrEmpty( username ) )
                {
                    Methods.WriteColorMessage( "\n用户名不能为空!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                currUser = FindUser( username ) ;
                if ( currUser == null )
                {
                    Methods.WriteColorMessage( "\n用户名不存在!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                break;
            } while ( true );

            do
            {
                Console.Clear();
                Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
                Methods.WriteColorMessage( "登录账号" , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "*****************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "\n请输入密码:" , ConsoleColor.DarkBlue );

                password = Methods.ReadInputWithEscCheck( true ) ;
                if ( Methods.IsEscPressed )
                {
                    currUser = null ;
                    Methods.IsEscPressed = false ;
                    return;
                }

                if ( password == null || UserData.EncryptPassword( password ) != currUser.Password )
                {
                    Methods.WriteColorMessage( "\n密码错误!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                break;
            } while ( true );
        }

        /// <summary>
        /// 显示已有用户
        /// </summary>
        private static void ShowUsers()
        {
            if ( users.Count == 0 )
            {
                Console.Clear();
                Methods.WriteColorMessage( "当前没有任何用户账号!\n" , ConsoleColor.DarkYellow );
                Methods.ReturnByKey();
                return;
            }

            Console.Clear();
            Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
            Methods.WriteColorMessage( "已有用户" , ConsoleColor.Yellow );
            Methods.WriteColorMessage( "*****************\n\n" , ConsoleColor.Green );
            foreach ( var user in users )
            {
                Methods.WriteColorMessage( "用户名: " , ConsoleColor.Blue );
                Console.Write( user.Username );
                Methods.WriteColorMessage( " ( id: " , ConsoleColor.Blue );
                Console.Write( user.Id );
                Methods.WriteColorMessage( " )\n" , ConsoleColor.Blue );
            }
            Methods.WriteColorMessage( "\n*********************************************\n" , ConsoleColor.Green );
            Methods.ReturnByKey();
        }

        /// <summary>
        /// 显示排行榜
        /// </summary>
        private static void ShowRankingList()
        {
            Console.Clear();

            if ( UserData.AllGameResults.Count == 0 )
            {
                Methods.WriteColorMessage( "当前没有任何游戏记录!\n" , ConsoleColor.DarkYellow );
                Methods.ReturnByKey();
                return;
            }

            Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
            Methods.WriteColorMessage( "排行榜" , ConsoleColor.Yellow );
            Methods.WriteColorMessage( "*****************\n\n" , ConsoleColor.Green );
            string title = "排名".PadRight(4) + " " + "用户名".PadRight( Constants.MaxUsernameLength ) + "分数".PadRight( Constants.MaxScoreLength ) + "   " + "时间" ;
            Methods.WriteColorMessage( title + "\n" , ConsoleColor.Blue );
            Methods.WriteColorMessage( "-----------------------------------------------------------------\n" , ConsoleColor.DarkBlue);

            List<GameResult> allGameResults = UserData.AllGameResults ;
            UserData.SortGameResults( ref allGameResults );
            int count = 1 ;

            foreach ( var gameResult in allGameResults )
            {
                string rank = count.ToString().PadRight(4) ;
                string username = gameResult.Username.PadRight( Constants.MaxUsernameLength ) ;
                string score = gameResult.Score.ToString().PadRight( Constants.MaxScoreLength ) ;
                string endTime = gameResult.EndTime.ToString() ;
                Console.WriteLine("{0}   {1}   {2}   {3}" , rank , username , score , endTime );
                ++count;

                if ( count > Constants.MaxRankListLength )
                {
                    break;
                }
            }

            Methods.ReturnByKey();
        }

        /// <summary>
        /// 登录管理员
        /// </summary>
        private static void LoginAdministrator()
        {
            do
            {
                Console.Clear();
                Methods.WriteColorMessage( "*****************" , ConsoleColor.Green );
                Methods.WriteColorMessage( "登录管理员" , ConsoleColor.Yellow );
                Methods.WriteColorMessage( "*****************\n" , ConsoleColor.Green );
                Methods.WriteColorMessage( "\n请输入管理员密码:" , ConsoleColor.DarkBlue );

                string? password = Methods.ReadInputWithEscCheck( true ) ;
                if ( Methods.IsEscPressed )
                {
                    Methods.IsEscPressed = false ;
                    return;
                }

                if ( password != Constants.AdministratorPassword )
                {
                    Methods.WriteColorMessage( "\n密码错误!\n" , ConsoleColor.Red );
                    Methods.ReturnByKey();
                    continue;
                }

                break;
            } while ( true );

            users = Administrator.Work( users ) ;
            SaveUsers();
            Methods.ReturnByKey();
        }
    }
}