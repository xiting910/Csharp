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
/// 自定义双缓冲DataGridView
/// </summary>
public class DoubleBufferedDataGridView : DataGridView
{
    /// <summary>
    /// 构造函数, 初始化双缓冲DataGridView
    /// </summary>
    public DoubleBufferedDataGridView()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
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
        // 对话框宽度和高度
        var dialogWidth = (int)(250 * Constants.DpiScale);
        var dialogHeight = (int)(175 * Constants.DpiScale);

        // 初始化控件和布局
        Text = "自定义难度";
        Size = new(dialogWidth, dialogHeight);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 输入标签和输入框的宽度、高度和位置
        var inputLabelWidth = (int)(60 * Constants.DpiScale);
        var inputWidth = (int)(80 * Constants.DpiScale);
        var inputHeight = (int)(25 * Constants.DpiScale);
        var inputX = (dialogWidth - inputLabelWidth - inputWidth) / 2;
        var inputY = (int)(15 * Constants.DpiScale);

        // 两个输入框之间的垂直间距
        var verticalSpacing = (int)(25 * Constants.DpiScale);

        // 创建宽度输入标签和输入框
        var widthLabel = new Label
        {
            Text = "宽度:",
            Location = new(inputX, inputY),
            Size = new(inputLabelWidth, inputHeight)
        };
        widthInput = new NumericUpDown
        {
            Location = new(inputX + inputLabelWidth, inputY),
            Size = new(inputWidth, inputHeight),
            Minimum = 1,
            Maximum = Constants.MaxBoardWidth,
            Value = 16
        };
        inputY += verticalSpacing;

        // 创建高度输入标签和输入框
        var heightLabel = new Label
        {
            Text = "高度:",
            Location = new(inputX, inputY),
            Size = new(inputLabelWidth, inputHeight)
        };
        heightInput = new NumericUpDown
        {
            Location = new(inputX + inputLabelWidth, inputY),
            Size = new(inputWidth, inputHeight),
            Minimum = 1,
            Maximum = Constants.MaxBoardHeight,
            Value = 16
        };
        inputY += verticalSpacing;

        // 创建地雷数输入标签和输入框
        var mineLabel = new Label
        {
            Text = "地雷数:",
            Location = new(inputX, inputY),
            Size = new(inputLabelWidth, inputHeight)
        };
        mineCountInput = new NumericUpDown
        {
            Location = new(inputX + inputLabelWidth, inputY),
            Size = new(inputWidth, inputHeight),
            Minimum = 1,
            Maximum = Constants.MaxBoardWidth * Constants.MaxBoardHeight - 1,
            Value = 40
        };

        // 创建确定和取消按钮
        okButton = new Button
        {
            Text = "确定",
            Location = new(dialogWidth - (int)(120 * Constants.DpiScale), dialogHeight - (int)(65 * Constants.DpiScale)),
            Size = new((int)(40 * Constants.DpiScale), (int)(25 * Constants.DpiScale)),
            DialogResult = DialogResult.OK
        };
        cancelButton = new Button
        {
            Text = "取消",
            Location = new(dialogWidth - (int)(60 * Constants.DpiScale), dialogHeight - (int)(65 * Constants.DpiScale)),
            Size = new((int)(40 * Constants.DpiScale), (int)(25 * Constants.DpiScale)),
            DialogResult = DialogResult.Cancel
        };

        // 添加OK按钮的点击事件处理
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
        Size = new((int)(250 * Constants.DpiScale), (int)(90 * Constants.DpiScale));
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        ProgressBar progressBar = new()
        {
            Dock = DockStyle.Top,
            Height = (int)(20 * Constants.DpiScale),
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
/// 提供确认卸载的窗体
/// </summary>
public class UninstallConfirmDialog : Form
{
    /// <summary>
    /// 获取是否保留数据的选项
    /// </summary>
    public bool KeepData => keepDataCheckBox.Checked;

    /// <summary>
    /// 信息标签
    /// </summary>
    private readonly Label messageLabel;

    /// <summary>
    /// 保留数据复选框
    /// </summary>
    private readonly CheckBox keepDataCheckBox;

    /// <summary>
    /// 确认按钮
    /// </summary>
    private readonly Button okButton;

    /// <summary>
    /// 取消按钮
    /// </summary>
    private readonly Button cancelButton;

    /// <summary>
    /// 构造函数, 初始化确认卸载对话框
    /// </summary>
    /// <param name="message">对话框显示的消息</param>
    public UninstallConfirmDialog(string message)
    {
        // 对话框大小
        var dialogWidth = (int)(300 * Constants.DpiScale);
        var dialogHeight = (int)(120 * Constants.DpiScale);

        // 设置对话框属性
        Text = "确认卸载";
        Width = dialogWidth;
        Height = dialogHeight;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        // 信息标签的大小
        var labelWidth = dialogWidth - (int)(15 * Constants.DpiScale) * 2;
        var labelHeight = (int)(20 * Constants.DpiScale);

        // 信息标签位置
        var labelX = (dialogWidth - labelWidth) / 2;
        var labelY = (int)(10 * Constants.DpiScale);

        // 初始化信息标签
        messageLabel = new()
        {
            Text = message,
            Location = new(labelX, labelY),
            Size = new(labelWidth, labelHeight),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 保留数据复选框大小
        var checkBoxWidth = (int)(100 * Constants.DpiScale);
        var checkBoxHeight = (int)(30 * Constants.DpiScale);

        // 保留数据复选框位置
        var checkBoxX = (int)(10 * Constants.DpiScale);
        var checkBoxY = labelY + labelHeight + (int)(20 * Constants.DpiScale);

        // 初始化保留数据复选框
        keepDataCheckBox = new()
        {
            Text = "保留数据",
            Left = checkBoxX,
            Top = checkBoxY,
            Width = checkBoxWidth,
            Height = checkBoxHeight
        };

        // 按钮大小
        var buttonWidth = (int)(80 * Constants.DpiScale);
        var buttonHeight = (int)(30 * Constants.DpiScale);

        // 按钮水平间距
        var buttonSpacing = (int)(10 * Constants.DpiScale);

        // 按钮位置
        var buttonX = dialogWidth - buttonWidth * 2 - buttonSpacing * 3;
        var buttonY = dialogHeight - buttonHeight - (int)(40 * Constants.DpiScale);

        // 初始化确认按钮
        okButton = new()
        {
            Text = "确定",
            DialogResult = DialogResult.OK,
            Left = buttonX,
            Top = buttonY,
            Width = buttonWidth,
            Height = buttonHeight
        };
        buttonX += buttonWidth + buttonSpacing;

        // 初始化取消按钮
        cancelButton = new()
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel,
            Left = buttonX,
            Top = buttonY,
            Width = buttonWidth,
            Height = buttonHeight
        };

        // 添加控件到对话框
        Controls.Add(messageLabel);
        Controls.Add(keepDataCheckBox);
        Controls.Add(okButton);
        Controls.Add(cancelButton);

        // 设置默认按钮
        AcceptButton = okButton;
        CancelButton = cancelButton;
    }
}