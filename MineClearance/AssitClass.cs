namespace MineClearance;

/// <summary>
/// 自定义双缓冲面板
/// </summary>
public class DoubleBufferedPanel : Panel
{
    /// <summary>
    /// 构造函数, 初始化双缓冲面板
    /// </summary>
    public DoubleBufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
    }
}

/// <summary>
/// 提供下载进度的窗体
/// </summary>
public class DownloadProgressForm : Form
{
    /// <summary>
    /// 下载进度条和状态标签
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
        Text = "下载更新中......(请不要中途切换网络)";
        Size = new Size(500, 120);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        ProgressBar = new ProgressBar
        {
            Location = new Point(10, 20),
            Size = new Size(460, 23),
            Minimum = 0,
            Maximum = 100
        };
        StatusLabel = new Label
        {
            Location = new Point(10, 55),
            Size = new Size(340, 23),
            Text = "准备下载..."
        };
        PauseResumeButton = new Button
        {
            Location = new Point(355, 50),
            Size = new Size(75, 23),
            Text = "暂停/继续"
        };
        CancelButton = new Button
        {
            Location = new Point(435, 50),
            Size = new Size(40, 23),
            Text = "取消下载"
        };

        Controls.Add(ProgressBar);
        Controls.Add(StatusLabel);
        Controls.Add(PauseResumeButton);
        Controls.Add(CancelButton);
    }
}

/// <summary>
/// 提供自定义难度设置对话框
/// </summary>
public partial class CustomDifficultyDialog : Form
{
    /// <summary>
    /// 自定义难度设置, 包含宽度、高度和地雷数
    /// </summary>
    public (int width, int height, int mineCount) CustomDifficulty { get; private set; }

    private readonly NumericUpDown widthInput;
    private readonly NumericUpDown heightInput;
    private readonly NumericUpDown mineCountInput;
    private readonly Button okButton;
    private readonly Button cancelButton;

    public CustomDifficultyDialog()
    {
        // 初始化控件和布局
        Text = "自定义难度";
        Size = new Size(300, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        // 创建控件
        var widthLabel = new Label { Text = "宽度:", Location = new Point(20, 20), Size = new Size(60, 23) };
        widthInput = new NumericUpDown { Location = new Point(90, 20), Size = new Size(100, 23), Minimum = 1, Maximum = 40, Value = 16 };

        var heightLabel = new Label { Text = "高度:", Location = new Point(20, 50), Size = new Size(60, 23) };
        heightInput = new NumericUpDown { Location = new Point(90, 50), Size = new Size(100, 23), Minimum = 1, Maximum = 25, Value = 16 };

        var mineLabel = new Label { Text = "地雷数:", Location = new Point(20, 80), Size = new Size(60, 23) };
        mineCountInput = new NumericUpDown { Location = new Point(90, 80), Size = new Size(100, 23), Minimum = 1, Maximum = 999, Value = 40 };

        okButton = new Button { Text = "确定", Location = new Point(110, 120), Size = new Size(75, 23), DialogResult = DialogResult.OK };
        cancelButton = new Button { Text = "取消", Location = new Point(200, 120), Size = new Size(75, 23), DialogResult = DialogResult.Cancel };

        okButton.Click += OkButton_Click;

        // 添加控件到窗体
        Controls.AddRange([widthLabel, widthInput, heightLabel, heightInput, mineLabel, mineCountInput, okButton, cancelButton]);
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        var width = (int)widthInput.Value;
        var height = (int)heightInput.Value;
        var mineCount = (int)mineCountInput.Value;

        // 验证地雷数不能超过总格子数
        if (mineCount >= width * height)
        {
            MessageBox.Show("地雷数必须小于总格子数！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        CustomDifficulty = (width, height, mineCount);
        DialogResult = DialogResult.OK;
        Close();
    }
}

/// <summary>
/// 提供等待更新事件处理完成的窗体
/// </summary>
public class WaitingForm : Form
{
    public WaitingForm()
    {
        Text = "请稍候";
        Size = new Size(300, 100);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        ProgressBar progressBar = new()
        {
            Dock = DockStyle.Top,
            Height = 20,
            Style = ProgressBarStyle.Marquee
        };
        Label label = new()
        {
            Text = "正在取消更新事件处理，请稍候...",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(progressBar);
        Controls.Add(label);
    }
}

/// <summary>
/// 提供下载长时间无进度后的提示窗体
/// </summary>
public class TimeoutMessageBox : Form
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
    /// 剩余时间（秒）
    /// </summary>
    private int secondsLeft;

    /// <summary>
    /// 显示提示框
    /// </summary>
    /// <param name="text">提示文本</param>
    /// <param name="caption">标题</param>
    /// <param name="timeoutSeconds">超时时间（秒）</param>
    /// <returns>用户选择的结果</returns>
    public static DialogResult Show(string text, string caption, int timeoutSeconds)
    {
        using var box = new TimeoutMessageBox(text, caption, timeoutSeconds);
        return box.ShowDialog();
    }

    /// <summary>
    /// 构造函数, 初始化提示框
    /// </summary>
    private TimeoutMessageBox(string text, string caption, int timeoutSeconds)
    {
        Text = caption;
        Width = 350;
        Height = 150;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        ControlBox = false;

        lblMsg = new Label { Text = text, Left = 20, Top = 20, Width = 300, Height = 40 };
        btnRetry = new Button { Text = "重试", Left = 60, Width = 80, Top = 70, DialogResult = DialogResult.Yes };
        btnCancel = new Button { Text = "取消", Left = 180, Width = 80, Top = 70, DialogResult = DialogResult.No };

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

/// <summary>
/// 自定义确认对话框, 支持不再提示勾选框, 以及不同按钮的唯一标识
/// </summary>
public class CustomMessageBox : Form
{
    /// <summary>
    /// 保存每个弹窗的“不再显示”状态
    /// </summary>
    private static readonly Dictionary<string, bool> _doNotShowAgainDict = [];

    /// <summary>
    /// 不再提示勾选框
    /// </summary>
    private CheckBox? _doNotShowAgainCheckBox;

    /// <summary>
    /// 显示自定义消息框, 如果该key已设置为不再显示，直接返回Yes
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="caption">标题</param>
    /// /// <param name="key">唯一标识（如按钮名）</param>
    public static DialogResult Show(string message, string caption, string key)
    {
        // 如果该key已设置为不再显示，直接返回Yes
        if (_doNotShowAgainDict.TryGetValue(key, out bool skip) && skip)
        {
            return DialogResult.Yes;
        }

        // 创建自定义消息框实例
        using var form = new CustomMessageBox();
        form.Text = caption;

        // 设置消息内容和按钮
        var label = new Label
        {
            Text = message,
            AutoSize = true,
            Location = new(10, 10)
        };
        form.Controls.Add(label);

        // 创建"本次运行不再提示"勾选框
        form._doNotShowAgainCheckBox = new CheckBox
        {
            Text = "本次运行不再提示",
            Location = new(10, label.Bottom + 15),
            AutoSize = true
        };
        form.Controls.Add(form._doNotShowAgainCheckBox);

        // 创建"是"和"否"按钮
        var btnYes = new Button
        {
            Text = "是",
            DialogResult = DialogResult.Yes,
            AutoSize = true
        };
        var btnNo = new Button
        {
            Text = "否",
            DialogResult = DialogResult.No,
            AutoSize = true
        };
        form.Controls.Add(btnYes);
        form.Controls.Add(btnNo);

        // 计算对话框的宽度和高度
        var width = Math.Max(label.Width + 20, form._doNotShowAgainCheckBox.Width + btnYes.Width + btnNo.Width + 40);
        var height = form._doNotShowAgainCheckBox.Bottom + 20;

        // 设置按钮位置
        btnNo.Location = new(width - btnNo.Width - 10, label.Bottom + 10);
        btnYes.Location = new(btnNo.Left - btnYes.Width - 10, btnNo.Top);

        // 设置对话框的大小和位置
        form.ClientSize = new(width, height);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;

        // 显示对话框
        var result = form.ShowDialog();

        // 如果勾选了"不再提示", 设置当前key为不再显示
        if (form._doNotShowAgainCheckBox.Checked)
        {
            _doNotShowAgainDict[key] = true;
        }

        // 返回用户选择的结果
        return result;
    }
}