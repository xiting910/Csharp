using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace GluttonousSnake
{
    /// <summary>
    /// 用户数据类, 用于保存用户数据和游戏结果
    /// </summary>
    public static class UserData
    {
        /// <summary>
        /// 32 字节密钥
        /// </summary>
        private static readonly byte[] Key = Encoding.UTF8.GetBytes( "12345678901234567890123456789012" );

        /// <summary>
        /// 16 字节 IV
        /// </summary>
        private static readonly byte[] IV = Encoding.UTF8.GetBytes( "1234567890123456" );

        /// <summary>
        /// 用户数据文件名列表
        /// </summary>
        private static readonly List<string> userDatas = [] ;

        /// <summary>
        /// 所有游戏结果列表
        /// </summary>
        private static readonly List<GameResult> allGameResults = [] ;

        /// <summary>
        /// 所有游戏结果列表
        /// </summary>
        public static List<GameResult> AllGameResults => allGameResults ;

        /// <summary>
        /// 读取用户数据文件名列表和所有游戏结果
        /// </summary>
        static UserData()
        {
            if ( !Directory.Exists( Constants.LogFileCatalogue ) )
            {
                try
                {
                    Directory.CreateDirectory( Constants.LogFileCatalogue );
                }
                catch ( Exception e )
                {
                    Error.Exception = e ;
                    ThreadVariables.IsLoadUserDatas = false ;
                    return;
                }
            }
            
            // 遍历数据文件夹下的所有文件(包括隐藏文件)
            DirectoryInfo directoryInfo = new( Constants.LogFileCatalogue ) ;
            foreach ( var file in directoryInfo.GetFiles() )
            {
                // 保存文件名字
                userDatas.Add( file.Name );

                // 读取所有游戏结果
                int currUserId = int.Parse( file.Name.Split( '.' )[0] ) ;
                List<GameResult>? gameResults = ReadGameResults( currUserId ) ;
                if ( gameResults != null )
                {
                    allGameResults.AddRange( gameResults );
                }
            }
        }

        /// <summary>
        /// 创建新用户数据文件, 注册用户时使用
        /// </summary>
        /// <param name="id">用户 ID</param>
        public static void CreateUserDataFile( int id )
        {
            string filePath = Path.Combine( Constants.LogFileCatalogue , id + ".json" ) ;
            if ( !File.Exists( filePath ) )
            {
                File.Create( filePath ).Close();
                SetHiddenAttribute( filePath );
                userDatas.Add( id + ".json" );
            }
        }

        /// <summary>
        /// 删除用户数据文件, 注销用户时使用
        /// </summary>
        /// <param name="id">用户 ID</param>
        public static void DeleteUserDataFile( int id )
        {
            string filePath = Path.Combine( Constants.LogFileCatalogue , id + ".json" ) ;
            if ( File.Exists( filePath ) )
            {
                List<GameResult>? gameResults = ReadGameResults( id ) ;
                if ( gameResults != null )
                {
                    allGameResults.RemoveAll( gameResults.Contains );
                }

                File.Delete( filePath );
                userDatas.Remove( id + ".json" );
            }
        }

        /// <summary>
        /// 删除所有用户数据文件, 管理员使用
        /// </summary>
        public static void DeleteAllUserDataFiles()
        {
            foreach ( var file in userDatas )
            {
                string filePath = Path.Combine( Constants.LogFileCatalogue , file ) ;
                if ( File.Exists( filePath ) )
                {
                    File.Delete( filePath );
                }
            }
            userDatas.Clear();
            allGameResults.Clear();
        }

        /// <summary>
        /// 通过ID读取用户的所有游戏结果
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <returns>游戏结果列表</returns>
        public static List<GameResult>? ReadGameResults( int id )
        {
            string filePath = Path.Combine( Constants.LogFileCatalogue , id + ".json" ) ;
            if ( File.Exists( filePath ) )
            {
                string json = File.ReadAllText( filePath ) ;
                if ( json == "" )
                {
                    return [];
                }
                return JsonSerializer.Deserialize<List<GameResult>>( json );
            }
            return null;
        }
 
        /// <summary>
        /// 向用户文件中添加游戏结果
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="gameResult">要添加的游戏结果</param>
        public static void AddGameResult( int id , GameResult gameResult )
        {
            // 读取用户文件中的所有游戏结果, 没有则创建一个空列表, 然后添加游戏结果
            List<GameResult>? gameResults = ReadGameResults( id ) ?? [] ;
            gameResults.Add( gameResult );
            string json = JsonSerializer.Serialize( gameResults ) ;

            // 保存到用户文件中
            CreateUserDataFile( id );
            string filePath = Path.Combine( Constants.LogFileCatalogue , id + ".json" ) ;
            RemoveHiddenAttribute( filePath );
            File.WriteAllText( filePath , json );
            SetHiddenAttribute( filePath );

            // 添加到所有游戏结果列表中
            allGameResults.Add( gameResult );
        }

        /// <summary>
        /// 按指定方式降序排序游戏结果
        /// </summary>
        /// <param name="gameResults">游戏结果列表</param>
        /// <param name="isByScore">是否按分数排序, 否则按时间排序, 默认为 true</param>
        public static void SortGameResults( ref List<GameResult> gameResults , bool isByScore = true )
        {
            gameResults.Sort( ( a , b ) => -a.CompareTo( b , isByScore ) );
        }

        /// <summary>
        /// 加密密码
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <returns>加密后的密码</returns>
        public static string EncryptPassword( string password )
        {
            try
            {
                using Aes aesAlg = Aes.Create() ;
                aesAlg.Key = Key ;
                aesAlg.IV = IV ;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor( aesAlg.Key , aesAlg.IV ) ;

                using MemoryStream msEncrypt = new() ;
                using CryptoStream csEncrypt = new( msEncrypt , encryptor , CryptoStreamMode.Write ) ;
                using ( StreamWriter swEncrypt = new( csEncrypt ) )
                {
                    swEncrypt.Write( password );
                }
                return Convert.ToBase64String( msEncrypt.ToArray() );
            }
            catch ( Exception e )
            {
                Console.Clear();
                Console.WriteLine( e.Message );
                Methods.ReturnByKey();
                Environment.Exit( 1 );
                return "";
            }
        }
 
        /// <summary>
        /// 解密密码
        /// </summary>
        /// <param name="password">加密后的密码</param>
        /// <returns>原始密码</returns>
        public static string DecryptPassword( string password )
        {
            try
            {
                using Aes aesAlg = Aes.Create() ;
                aesAlg.Key = Key ;
                aesAlg.IV = IV ;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor( aesAlg.Key , aesAlg.IV ) ;

                using MemoryStream msDecrypt = new( Convert.FromBase64String( password ) ) ;
                using CryptoStream csDecrypt = new( msDecrypt , decryptor , CryptoStreamMode.Read ) ;
                using StreamReader srDecrypt = new( csDecrypt ) ;
                return srDecrypt.ReadToEnd();
            }
            catch ( Exception e )
            {
                Console.Clear();
                Console.WriteLine( e.Message );
                Methods.ReturnByKey();
                Environment.Exit( 1 );
                return "";
            }
        }

        /// <summary>
        /// 移除文件隐藏属性
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void RemoveHiddenAttribute( string filePath )
        {
            File.SetAttributes( filePath , File.GetAttributes( filePath ) & ~FileAttributes.Hidden );
        }

        /// <summary>
        /// 设置文件隐藏属性
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void SetHiddenAttribute( string filePath )
        {
            File.SetAttributes( filePath , File.GetAttributes( filePath ) | FileAttributes.Hidden );
        }

        /// <summary>
        /// 移除文件夹隐藏属性
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        public static void RemoveHiddenAttributeForDirectory( string directoryPath )
        {
            DirectoryInfo directoryInfo = new( directoryPath ) ;
            directoryInfo.Attributes &= ~FileAttributes.Hidden ;
        }

        /// <summary>
        /// 设置文件夹隐藏属性
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        public static void SetHiddenAttributeForDirectory( string directoryPath )
        {
            DirectoryInfo directoryInfo = new( directoryPath ) ;
            directoryInfo.Attributes |= FileAttributes.Hidden ;
        }
    }
}