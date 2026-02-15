namespace WindowsUpdateManager;

/// <summary>
/// 提供自定义日期选择对话框
/// </summary>
internal sealed class DatePickerDialog : Form
{
    /// <summary>
    /// 对话框宽度
    /// </summary>
    private static readonly int DialogWidth = (int)(200 * Constants.DpiScale);

    /// <summary>
    /// 对话框高度
    /// </summary>
    private static readonly int DialogHeight = (int)(100 * Constants.DpiScale);

    /// <summary>
    /// 日期选择器
    /// </summary>
    private readonly DateTimePicker _dateTimePicker;

    /// <summary>
    /// 确定按钮
    /// </summary>
    private readonly Button _okButton;

    /// <summary>
    /// 取消按钮
    /// </summary>
    private readonly Button _cancelButton;

    /// <summary>
    /// 获取选择的日期
    /// </summary>
    public DateTime SelectedDate => _dateTimePicker.Value.Date;

    /// <summary>
    /// 构造函数, 初始化自定义日期选择对话框
    /// </summary>
    public DatePickerDialog()
    {
        // 设置对话框属性
        Text = "选择日期";
        Size = new(DialogWidth, DialogHeight);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 明天
        var tomorrow = DateTime.Now.Date.AddDays(1);

        // 初始化日期选择器
        _dateTimePicker = new()
        {
            Width = (int)(120 * Constants.DpiScale),
            Format = DateTimePickerFormat.Long,
            MaxDate = Constants.MaxPauseUpdateDate,
            MinDate = tomorrow,
            Value = tomorrow
        };

        // 初始化确定按钮
        _okButton = new()
        {
            Text = "确定",
            AutoSize = true,
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.OK
        };

        // 初始化取消按钮
        _cancelButton = new()
        {
            Text = "取消",
            AutoSize = true,
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.Cancel
        };

        // 右侧留空宽度
        var rightMargin = (int)(5 * Constants.DpiScale);

        // 日期选择器X、Y坐标
        var dateTimePickerX = (ClientSize.Width - _dateTimePicker.Width - rightMargin) / 2;
        var dateTimePickerY = (int)(5 * Constants.DpiScale);
        _dateTimePicker.Location = new(dateTimePickerX, dateTimePickerY);

        // 按钮水平间距
        var buttonSpacing = (int)(10 * Constants.DpiScale);

        // 按钮总宽度
        var buttonTotalWidth = _okButton.Width + _cancelButton.Width + buttonSpacing;

        // 按钮当前X、Y位置
        var buttonX = (ClientSize.Width - buttonTotalWidth - rightMargin) / 2;
        var buttonY = _dateTimePicker.Bottom + (int)(10 * Constants.DpiScale);

        // 设置按钮位置
        _okButton.Location = new(buttonX, buttonY);
        buttonX += _okButton.Width + buttonSpacing;
        _cancelButton.Location = new(buttonX, buttonY);

        // 添加控件到对话框
        Controls.Add(_dateTimePicker);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);
    }
}
