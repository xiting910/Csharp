namespace GluttonousSnake
{
    /// <summary>
    /// 错误类, 用于存储错误信息
    /// </summary>
    public static class Error
    {
        private static Exception _exception = new() ;

        /// <summary>
        /// 错误信息
        /// </summary>
        public static Exception Exception
        {
            get => _exception ;
            set => _exception = value ;
        }
    }
}