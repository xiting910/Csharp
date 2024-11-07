namespace GluttonousSnake
{
    /// <summary>
    /// 游戏结果类, 用于存储游戏结果
    /// </summary>
    /// <param name="username">玩家用户名</param>
    /// <param name="score">得分</param>
    /// <param name="endTime">游戏结束时间</param>
    public class GameResult( string username , int score , DateTime endTime )
    {
        private readonly string _username = username ;
        private readonly int _score = score ;
        private readonly DateTime _endTime = endTime ;

        /// <summary>
        /// 玩家用户名
        /// </summary>
        public string Username => _username ;

        /// <summary>
        /// 得分
        /// </summary>
        public int Score => _score ;

        /// <summary>
        /// 游戏结束时间
        /// </summary>
        public DateTime EndTime => _endTime ;

        public int CompareTo( GameResult other , bool isByScore = true )
        {
            if ( isByScore )
            {
                return _score.CompareTo( other.Score );
            }
            else
            {
                return _endTime.CompareTo( other.EndTime );
            }
        }

        public override string ToString()
        {
            return $"玩家: {_username}   分数: {_score}   时间: {_endTime}" ;
        }

        public override bool Equals( object? obj )
        {
            if ( obj is GameResult gameResult )
            {
                return _username == gameResult.Username && _score == gameResult.Score && _endTime == gameResult.EndTime ;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine( _username , _score , _endTime );
        }
    }
}