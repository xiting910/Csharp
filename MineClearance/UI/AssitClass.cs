using System.Globalization;
using MineClearance.Utilities;

namespace MineClearance.UI;

/// <summary>
/// 自定义双缓冲面板
/// </summary>
internal sealed class DoubleBufferedPanel : Panel
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
internal sealed class DoubleBufferedDataGridView : DataGridView
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
internal sealed class CustomDifficultyDialog : Form
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
        var dialogWidth = (int)(250 * UIConstants.DpiScale);
        var dialogHeight = (int)(175 * UIConstants.DpiScale);

        // 初始化控件和布局
        Text = "自定义难度";
        Size = new(dialogWidth, dialogHeight);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 输入标签和输入框的宽度、高度和位置
        var inputLabelWidth = (int)(60 * UIConstants.DpiScale);
        var inputWidth = (int)(80 * UIConstants.DpiScale);
        var inputHeight = (int)(25 * UIConstants.DpiScale);
        var inputX = (dialogWidth - inputLabelWidth - inputWidth) / 2;
        var inputY = (int)(15 * UIConstants.DpiScale);

        // 两个输入框之间的垂直间距
        var verticalSpacing = (int)(25 * UIConstants.DpiScale);

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
            Maximum = (Constants.MaxBoardWidth * Constants.MaxBoardHeight) - 1,
            Value = 40
        };

        // 创建确定和取消按钮
        okButton = new Button
        {
            Text = "确定",
            Location = new(dialogWidth - (int)(120 * UIConstants.DpiScale), dialogHeight - (int)(65 * UIConstants.DpiScale)),
            Size = new((int)(40 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            DialogResult = DialogResult.OK
        };
        cancelButton = new Button
        {
            Text = "取消",
            Location = new(dialogWidth - (int)(60 * UIConstants.DpiScale), dialogHeight - (int)(65 * UIConstants.DpiScale)),
            Size = new((int)(40 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
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
            _ = MessageBox.Show("地雷数必须小于总格子数！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        CustomDifficulty = (width, height, mineCount);
        DialogResult = DialogResult.OK;
        Close();
    }
}

/// <summary>
/// 提供确认卸载的窗体
/// </summary>
internal sealed class UninstallConfirmDialog : Form
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
        var dialogWidth = (int)(300 * UIConstants.DpiScale);
        var dialogHeight = (int)(120 * UIConstants.DpiScale);

        // 设置对话框属性
        Text = "确认卸载";
        Width = dialogWidth;
        Height = dialogHeight;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        // 信息标签的大小
        var labelWidth = dialogWidth - ((int)(15 * UIConstants.DpiScale) * 2);
        var labelHeight = (int)(20 * UIConstants.DpiScale);

        // 信息标签位置
        var labelX = (dialogWidth - labelWidth) / 2;
        var labelY = (int)(10 * UIConstants.DpiScale);

        // 初始化信息标签
        messageLabel = new()
        {
            Text = message,
            Location = new(labelX, labelY),
            Size = new(labelWidth, labelHeight),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 保留数据复选框大小
        var checkBoxWidth = (int)(100 * UIConstants.DpiScale);
        var checkBoxHeight = (int)(30 * UIConstants.DpiScale);

        // 保留数据复选框位置
        var checkBoxX = (int)(10 * UIConstants.DpiScale);
        var checkBoxY = labelY + labelHeight + (int)(20 * UIConstants.DpiScale);

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
        var buttonWidth = (int)(80 * UIConstants.DpiScale);
        var buttonHeight = (int)(30 * UIConstants.DpiScale);

        // 按钮水平间距
        var buttonSpacing = (int)(10 * UIConstants.DpiScale);

        // 按钮位置
        var buttonX = dialogWidth - (buttonWidth * 2) - (buttonSpacing * 3);
        var buttonY = dialogHeight - buttonHeight - (int)(40 * UIConstants.DpiScale);

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

/// <summary>
/// 提供自定义日期选择对话框
/// </summary>
internal sealed class DatePickerDialog : Form
{
    /// <summary>
    /// 获取选择的日期列表(只读)
    /// </summary>
    public IReadOnlyList<DateTime> SelectedDates => selectedDates.ToList().AsReadOnly();

    /// <summary>
    /// 已选择的日期列表ListBox
    /// </summary>
    private readonly ListBox selectedDatesListBox;

    /// <summary>
    /// 按钮集合
    /// </summary>
    private readonly List<Button> buttons;

    /// <summary>
    /// 提示信息标签
    /// </summary>
    private readonly Label infoLabel;

    /// <summary>
    /// 日期选择器
    /// </summary>
    private readonly DateTimePicker dateTimePicker;

    /// <summary>
    /// 选择的日期列表
    /// </summary>
    private readonly HashSet<DateTime> selectedDates;

    /// <summary>
    /// 构造函数
    /// </summary>
    public DatePickerDialog()
    {
        // 对话框大小
        var dialogWidth = (int)(500 * UIConstants.DpiScale);
        var dialogHeight = (int)(400 * UIConstants.DpiScale);

        // 设置对话框属性
        Text = "选择日期";
        Size = new(dialogWidth, dialogHeight);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 已选择的日期列表框大小
        var listBoxWidth = (int)(480 * UIConstants.DpiScale);
        var listBoxHeight = (int)(280 * UIConstants.DpiScale);

        // 初始化已选择的日期列表框
        selectedDatesListBox = new ListBox
        {
            Location = new((int)(5 * UIConstants.DpiScale), (int)(5 * UIConstants.DpiScale)),
            Size = new(listBoxWidth, listBoxHeight),
            SelectionMode = SelectionMode.MultiExtended
        };

        // 添加提示信息
        _ = selectedDatesListBox.Items.Add("当前已选择的日期:(选中后可点击删除按钮删除)");

        // 初始化添加按钮
        var addButton = new Button { Text = "添加" };
        addButton.Click += AddButton_Click;

        // 初始化删除按钮
        var removeButton = new Button { Text = "删除" };
        removeButton.Click += RemoveButton_Click;

        // 初始化确定按钮
        var okButton = new Button
        {
            Text = "确定",
            DialogResult = DialogResult.OK
        };

        // 初始化取消按钮
        var cancelButton = new Button
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel
        };

        // 添加按钮到列表
        buttons = [addButton, removeButton, okButton, cancelButton];

        // 按钮的水平间距
        var buttonSpacing = (int)(10 * UIConstants.DpiScale);

        // 按钮的宽度和高度
        var buttonWidth = (dialogWidth - ((buttons.Count + 2) * buttonSpacing)) / buttons.Count;
        var buttonHeight = (int)(25 * UIConstants.DpiScale);

        // 按钮位置
        var buttonX = buttonSpacing;
        var buttonY = dialogHeight - buttonHeight - (int)(50 * UIConstants.DpiScale);

        // 设置每个按钮的位置和大小
        foreach (var button in buttons)
        {
            button.Size = new(buttonWidth, buttonHeight);
            button.Location = new(buttonX, buttonY);
            buttonX += buttonWidth + buttonSpacing;
        }

        // 日期选择器的大小
        var datePickerWidth = (int)(300 * UIConstants.DpiScale);
        var datePickerHeight = (int)(15 * UIConstants.DpiScale);

        // 信息提示标签的大小
        var infoLabelWidth = (int)(100 * UIConstants.DpiScale);
        var infoLabelHeight = (int)(15 * UIConstants.DpiScale);

        // 间隔宽度
        var spacing = (int)(10 * UIConstants.DpiScale);

        // 当前X和Y位置
        var currentX = (dialogWidth - datePickerWidth - infoLabelWidth - spacing) / 2;
        var currentY = buttonY - datePickerHeight - spacing;

        // 初始化信息提示标签
        infoLabel = new Label
        {
            Text = "要添加的日期:",
            Location = new(currentX, currentY),
            Size = new(infoLabelWidth, infoLabelHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        currentX += infoLabelWidth + spacing;

        // 初始化日期选择器
        dateTimePicker = new DateTimePicker
        {
            Location = new(currentX, currentY),
            Size = new(datePickerWidth, datePickerHeight),
            Format = DateTimePickerFormat.Long,
            Value = DateTime.Now
        };

        // 添加控件到对话框
        Controls.Add(selectedDatesListBox);
        Controls.AddRange([.. buttons]);
        Controls.Add(infoLabel);
        Controls.Add(dateTimePicker);

        // 初始化选择的日期列表
        selectedDates = [];
    }

    /// <summary>
    /// 添加按钮点击事件处理
    /// </summary>
    private void AddButton_Click(object? sender, EventArgs e)
    {
        // 添加选择的日期到列表
        if (selectedDates.Add(dateTimePicker.Value.Date))
        {
            // 如果添加成功, 则更新列表框
            _ = selectedDatesListBox.Items.Add(dateTimePicker.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// 删除按钮点击事件处理
    /// </summary>
    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        // 获取选中的索引
        var selectedIndices = selectedDatesListBox.SelectedIndices;

        // 从后往前删除，避免索引错乱
        for (var i = selectedIndices.Count - 1; i >= 0; --i)
        {
            // 获取当前索引
            var idx = selectedIndices[i];

            // 如果是第一个提示信息, 则跳过
            if (idx == 0)
            {
                continue;
            }

            // 从 HashSet 中移除对应的日期
            if (DateTime.TryParse(selectedDatesListBox.Items[idx].ToString(), out var date))
            {
                _ = selectedDates.Remove(date);
            }

            // 从 ListBox 移除
            selectedDatesListBox.Items.RemoveAt(idx);
        }
    }
}

/// <summary>
/// 自定义确认对话框, 支持不再提示勾选框, 以及不同按钮的唯一标识
/// </summary>
internal sealed class CustomMessageBox : Form
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
    /// <param name="key">唯一标识（如按钮名）</param>
    public static DialogResult Show(string message, string caption, string key)
    {
        // 如果该key已设置为不再显示，直接返回Yes
        if (_doNotShowAgainDict.TryGetValue(key, out var skip) && skip)
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
            Location = new((int)(5 * UIConstants.DpiScale), (int)(5 * UIConstants.DpiScale))
        };
        form.Controls.Add(label);

        // 创建"本次运行不再提示"勾选框
        form._doNotShowAgainCheckBox = new CheckBox
        {
            Text = "本次运行不再提示",
            Location = new((int)(5 * UIConstants.DpiScale), label.Bottom + (int)(10 * UIConstants.DpiScale)),
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
        var width = Math.Max(label.Width + (int)(10 * UIConstants.DpiScale), form._doNotShowAgainCheckBox.Width + btnYes.Width + btnNo.Width + (int)(40 * UIConstants.DpiScale));
        var height = form._doNotShowAgainCheckBox.Bottom + (int)(15 * UIConstants.DpiScale);

        // 设置按钮位置
        btnNo.Location = new(width - btnNo.Width - (int)(10 * UIConstants.DpiScale), label.Bottom + (int)(10 * UIConstants.DpiScale));
        btnYes.Location = new(btnNo.Left - btnYes.Width - (int)(10 * UIConstants.DpiScale), btnNo.Top);

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