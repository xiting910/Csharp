namespace GluttonousSnake
{
    public class Program
    {
        public static void Main()
        {
            // 设置控制台的编码格式为UTF-8，以支持中文字符
            Console.OutputEncoding = System.Text.Encoding.UTF8 ;

            // 设置控制台的标题为"贪吃蛇"
            Console.Title = "贪吃蛇" ;

            // 隐藏光标
            Console.CursorVisible = false ;
            
            // 创建一个线程用于加载程序
            Thread loadThread = new( Methods.LoadProgram ) ;
            loadThread.Start();

            // 程序基于用户系统运行
            UserSystem.Work( loadThread );
     
            Methods.WriteColorMessage("\n感谢您的使用!\n" , ConsoleColor.DarkGreen );
            Methods.ReturnByKey( "按任意键退出..." );
        }
    }
}