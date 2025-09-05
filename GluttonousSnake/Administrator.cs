namespace GluttonousSnake
{
    /// <summary>
    /// 管理员类, 使用静态类实现管理员功能( 有且只有一个管理员 )
    /// </summary>
    public static class Administrator
    {
        /// <summary>
        /// 展示管理员界面
        /// </summary>
        public static void ShowInterface()
        {
            Console.Clear();
            Methods.WriteColorMessage("***************" , ConsoleColor.Green );
            Methods.WriteColorMessage(" 管理员系统" , ConsoleColor.Yellow );
            Methods.WriteColorMessage(" ***************\n\n" , ConsoleColor.Green );
            Methods.WriteColorMessage("1. 查看所有用户和密码\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage("2. 删除所有用户数据\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage("3. 系统初始化(删除所有用户和数据)\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage("Esc. 退出\n\n" , ConsoleColor.DarkMagenta );
            Methods.WriteColorMessage("***************************************\n" , ConsoleColor.Green );
        }

        /// <summary>
        /// 进行管理员操作
        /// </summary>
        /// <param name="users">用户列表</param>
        /// <returns>操作后的用户列表</returns>
        public static List<User> Work( List<User> users )
        {
            do
            {
                ShowInterface();

                ConsoleKeyInfo keyInfo = Console.ReadKey( true ) ;
                switch ( keyInfo.Key )
                {
                    case ConsoleKey.D1:
                        ShowAllUsers( users );
                        Methods.ReturnByKey();
                        break;
                    case ConsoleKey.D2:
                        DeleteAllUserData();
                        Methods.ReturnByKey();
                        break;
                    case ConsoleKey.D3:
                        SystemInitialization();
                        Methods.ReturnByKey();
                        break;
                    case ConsoleKey.Escape:
                        Methods.WriteColorMessage("\n退出成功!\n" , ConsoleColor.DarkGreen );
                        return users;
                    default:
                        break;
                }
            } while ( true );
        }

        /// <summary>
        /// 展示所有用户和密码
        /// </summary>
        /// <param name="users">用户列表</param>
        private static void ShowAllUsers( List<User> users )
        {
            Console.Clear();
            if ( users.Count == 0 )
            {
                Methods.WriteColorMessage("没有用户数据!\n" , ConsoleColor.DarkYellow );
                return;
            }

            Methods.WriteColorMessage("************************* " , ConsoleColor.Green );
            Methods.WriteColorMessage("所有用户和密码" , ConsoleColor.Yellow );
            Methods.WriteColorMessage(" ************************\n\n" , ConsoleColor.Green );
            string title = "用户名".PadRight( Constants.MaxUsernameLength ) + "   " + "id".PadRight( Constants.IdLength ) + "   " + "密码".PadRight( Constants.MaxPasswordLength ) ;
            Methods.WriteColorMessage( title + "\n" , ConsoleColor.Blue );
            Methods.WriteColorMessage( "-----------------------------------------------------------------\n" , ConsoleColor.DarkBlue);

            foreach ( var user in users )
            {
                string username = user.Username.PadRight( Constants.MaxUsernameLength ) ; 
                string id = user.Id.ToString().PadRight( Constants.IdLength ) ; 
                string password = UserData.DecryptPassword( user.Password ).PadRight( Constants.MaxPasswordLength ) ; 
                Console.WriteLine("{0}   {1}   {2}" , username , id , password );
            }
        }

        /// <summary>
        /// 删除所有用户数据, 如果确认删除则还会重启程序
        /// </summary>
        private static void DeleteAllUserData()
        {
            Console.Clear();
            Methods.WriteColorMessage("注意: 此操作将删除所有用户数据,请谨慎操作!\n" , ConsoleColor.DarkRed );
            if ( Methods.ConfirmOperation() )
            {
                UserData.DeleteAllUserDataFiles();
                Methods.WriteColorMessage("\n删除所有用户数据成功!\n" , ConsoleColor.DarkGreen );
                Methods.ReturnByKey();
                Methods.Restart();
            }
            else
            {
                Methods.WriteColorMessage("\n取消删除所有用户数据!\n" , ConsoleColor.DarkGreen );
            }
        }
        
        /// <summary>
        /// 系统初始化(删除所有用户和数据), 如果确认删除则还会重启程序
        /// </summary>
        private static void SystemInitialization()
        {
            Console.Clear();
            Methods.WriteColorMessage("注意: 此操作将删除所有用户数据,请谨慎操作!\n" , ConsoleColor.DarkRed );
            if ( Methods.ConfirmOperation() )
            {
                UserData.DeleteAllUserDataFiles();
                if ( File.Exists( Constants.UserFilePath ) )
                {
                    File.Delete( Constants.UserFilePath );
                }

                Methods.WriteColorMessage("\n系统初始化成功!\n" , ConsoleColor.DarkGreen );
                Methods.ReturnByKey();
                Methods.Restart();
            }
            else
            {
                Methods.WriteColorMessage("\n取消系统初始化!\n" , ConsoleColor.DarkGreen );
            }
        }   
    }
}