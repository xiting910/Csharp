namespace MineClearance.UI;

/// <summary>
/// 提供下载进度的窗体
/// </summary>
internal sealed class DownloadProgressForm : Form
{
    /// <summary>
    /// 信息标签
    /// </summary>
    public Label InfoLabel { get; }

    /// <summary>
    /// 下载进度条
    /// </summary>
    public ProgressBar ProgressBar { get; }

    /// <summary>
    /// 状态标签, 显示下载状态信息
    /// </summary>
    public Label StatusLabel { get; }

    /// <summary>
    /// 暂停/继续下载按钮
    /// </summary>
    public Button PauseResumeButton { get; }

    /// <summary>
    /// 取消下载按钮
    /// </summary>
    public new Button CancelButton { get; }

    /// <summary>
    /// 构造函数, 初始化下载进度窗体
    /// </summary>
    public DownloadProgressForm()
    {
        // 窗体宽度和高度
        var formWidth = (int)(500 * UIConstants.DpiScale);
        var formHeight = (int)(140 * UIConstants.DpiScale);

        // 设置窗体属性
        Text = "下载更新";
        Size = new(formWidth, formHeight);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        // 初始化控件
        InfoLabel = new()
        {
            Text = "正在下载更新, 请不要中途切换网络或关闭程序......",
            Location = new((int)(5 * UIConstants.DpiScale), (int)(5 * UIConstants.DpiScale)),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter
        };
        ProgressBar = new()
        {
            Location = new((int)(5 * UIConstants.DpiScale), (int)(35 * UIConstants.DpiScale)),
            Size = new(formWidth - (int)(25 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            Minimum = 0,
            Maximum = 100
        };
        StatusLabel = new()
        {
            Location = new((int)(5 * UIConstants.DpiScale), (int)(75 * UIConstants.DpiScale)),
            Size = new((int)(325 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            Text = "准备下载..."
        };
        PauseResumeButton = new()
        {
            Location = new(formWidth - (int)(170 * UIConstants.DpiScale), (int)(70 * UIConstants.DpiScale)),
            Size = new((int)(75 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            Text = "暂停/继续"
        };
        CancelButton = new()
        {
            Location = new(formWidth - (int)(90 * UIConstants.DpiScale), (int)(70 * UIConstants.DpiScale)),
            Size = new((int)(75 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            Text = "取消下载"
        };

        // 添加控件到窗体
        Controls.Add(InfoLabel);
        Controls.Add(ProgressBar);
        Controls.Add(StatusLabel);
        Controls.Add(PauseResumeButton);
        Controls.Add(CancelButton);
    }
}

/// <summary>
/// 提供下载长时间无进度后的提示窗体
/// </summary>
internal sealed class TimeoutMessageBox : Form
{
    /// <summary>
    /// 重试按钮
    /// </summary>
    private readonly Button btnRetry;

    /// <summary>
    /// 取消按钮
    /// </summary>
    private readonly Button btnCancel;

    /// <summary>
    /// 消息标签
    /// </summary>
    private readonly Label lblMsg;

    /// <summary>
    /// 定时器, 用于自动选择重试
    /// </summary>
    private readonly System.Windows.Forms.Timer timer;

    /// <summary>
    /// 剩余时间(秒)
    /// </summary>
    private int secondsLeft;

    /// <summary>
    /// 显示提示框
    /// </summary>
    /// <param name="text">提示文本</param>
    /// <param name="caption">标题</param>
    /// <param name="timeoutSeconds">超时时间(秒)</param>
    /// <returns>用户选择的结果</returns>
    public static DialogResult Show(string text, string caption, int timeoutSeconds)
    {
        using var box = new TimeoutMessageBox(text, caption, timeoutSeconds);
        return box.ShowDialog();
    }

    /// <summary>
    /// 构造函数, 初始化提示框
    /// </summary>
    /// <param name="text">提示文本</param>
    /// <param name="caption">标题</param>
    /// <param name="timeoutSeconds">超时时间(秒)</param>
    private TimeoutMessageBox(string text, string caption, int timeoutSeconds)
    {
        Text = caption;
        Size = new((int)(200 * UIConstants.DpiScale), (int)(100 * UIConstants.DpiScale));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        ControlBox = false;

        lblMsg = new()
        {
            Text = text,
            Location = new((int)(5 * UIConstants.DpiScale), (int)(5 * UIConstants.DpiScale)),
            AutoSize = true,
        };
        btnRetry = new()
        {
            Text = "重试",
            Location = new((int)(25 * UIConstants.DpiScale), (int)(35 * UIConstants.DpiScale)),
            Size = new((int)(40 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            DialogResult = DialogResult.Yes
        };
        btnCancel = new()
        {
            Text = "取消",
            Location = new((int)(100 * UIConstants.DpiScale), (int)(35 * UIConstants.DpiScale)),
            Size = new((int)(40 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            DialogResult = DialogResult.No
        };

        btnRetry.Click += (s, e) => { DialogResult = DialogResult.Yes; Close(); };
        btnCancel.Click += (s, e) => { DialogResult = DialogResult.No; Close(); };

        Controls.Add(lblMsg);
        Controls.Add(btnRetry);
        Controls.Add(btnCancel);

        secondsLeft = timeoutSeconds;
        timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (s, e) =>
        {
            secondsLeft--;
            Text = $"{caption}（{secondsLeft}秒后自动重试）";
            if (secondsLeft <= 0)
            {
                timer.Stop();
                DialogResult = DialogResult.Yes;
                Close();
            }
        };
        timer.Start();
    }
}