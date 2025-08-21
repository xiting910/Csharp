using MineClearance.Models;
using MineClearance.Services;
using MineClearance.Utilities;

namespace MineClearance.UI;

/// <summary>
/// 设置窗口类
/// </summary>
internal sealed class SettingForm : Form
{
    /// <summary>
    /// 设置窗口实例
    /// </summary>
    private static SettingForm? _instance;

    /// <summary>
    /// 显示设置窗口
    /// </summary>
    public static void ShowForm()
    {
        // 如果实例已存在且未释放, 则直接显示
        if (_instance != null && !_instance.IsDisposed)
        {
            // 如果当前窗口状态为最小化, 则恢复到正常状态
            if (_instance.WindowState == FormWindowState.Minimized)
            {
                _instance.WindowState = FormWindowState.Normal;
            }

            // 如果当前窗口没有获得焦点, 则激活窗口
            if (!_instance.ContainsFocus)
            {
                _instance.Activate();
            }
            return;
        }

        // 创建新的实例并显示
        _instance = new();
        _instance.Show();
    }

    /// <summary>
    /// 自动启动复选框
    /// </summary>
    private readonly CheckBox _autoStartCheckBox;

    /// <summary>
    /// 按钮列表
    /// </summary>
    private readonly List<Button> _buttons;

    /// <summary>
    /// 构造函数, 初始化设置窗口
    /// </summary>
    private SettingForm()
    {
        // 设置窗口属性
        Text = "设置";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new(UIConstants.SettingFormMinWidth, UIConstants.SettingFormMinHeight);

        // 初始化自动启动复选框
        _autoStartCheckBox = new()
        {
            TextAlign = ContentAlignment.MiddleCenter,
            Checked = AutoStartHelper.IsAutoStartEnabled(),
            Appearance = Appearance.Button,
            FlatStyle = FlatStyle.Flat
        };
        UpdateAutoStartCheckBox();
        _autoStartCheckBox.CheckedChanged += OnAutoStartCheckBoxCheckedChanged;

        // 初始化按钮列表
        _buttons = [];

        // 初始化创建桌面快捷方式按钮
        var createShortcutButton = new Button
        {
            Text = "创建桌面快捷方式",
            BackColor = Color.LightBlue,
            FlatStyle = FlatStyle.Flat
        };
        _buttons.Add(createShortcutButton);
        createShortcutButton.Click += OnCreateShortcutButtonClick;

        // 初始化重置按钮
        var resetButton = new Button
        {
            Text = "重置设置",
            BackColor = Color.Yellow,
            FlatStyle = FlatStyle.Flat
        };
        _buttons.Add(resetButton);
        resetButton.Click += OnResetButtonClick;

        // 添加控件到窗口
        Controls.Add(_autoStartCheckBox);
        Controls.AddRange([.. _buttons]);

        // 订阅窗口大小变化事件
        Resize += (sender, e) => ResizeControls();
    }

    /// <summary>
    /// 自动启动复选框状态变化事件处理
    /// </summary>
    private void OnAutoStartCheckBoxCheckedChanged(object? sender, EventArgs e)
    {
        if (sender is CheckBox cb)
        {
            try
            {
                if (cb.Checked)
                {
                    // 启用自动启动
                    AutoStartHelper.EnableAutoStart();
                }
                else
                {
                    // 禁用自动启动
                    AutoStartHelper.DisableAutoStart();
                }
            }
            catch (Exception ex)
            {
                // 如果发生异常, 切换复选框状态并显示错误信息
                cb.CheckedChanged -= OnAutoStartCheckBoxCheckedChanged;
                cb.Checked = !cb.Checked;
                cb.CheckedChanged += OnAutoStartCheckBoxCheckedChanged;
                Methods.LogException(ex);
                _ = MessageBox.Show($"设置开机自启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 更新复选框文本和颜色
                UpdateAutoStartCheckBox();
            }
        }
    }

    /// <summary>
    /// 根据是否启用开机自动启动来切换复选框文本和颜色
    /// </summary>
    private void UpdateAutoStartCheckBox()
    {
        if (_autoStartCheckBox.Checked)
        {
            _autoStartCheckBox.Text = "开机自动启动: 已启用";
            _autoStartCheckBox.BackColor = Color.LightGreen;
        }
        else
        {
            _autoStartCheckBox.Text = "开机自动启动: 已禁用";
            _autoStartCheckBox.BackColor = Color.LightGray;
        }
    }

    /// <summary>
    /// 创建桌面快捷方式按钮点击事件处理
    /// </summary>
    private void OnCreateShortcutButtonClick(object? sender, EventArgs e)
    {
        // 弹窗提示用户输入快捷方式名称
        var shortcutName = Microsoft.VisualBasic.Interaction.InputBox("请输入快捷方式名称:", "创建桌面快捷方式", Constants.ExecutableFileName, -1, -1);

        if (!string.IsNullOrWhiteSpace(shortcutName))
        {
            // 创建快捷方式
            try
            {
                ShortcutCreator.CreateDesktopShortcut(Constants.ExecutableFilePath, shortcutName);
                _ = MessageBox.Show("快捷方式创建成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Methods.LogException(ex);
                _ = MessageBox.Show($"创建快捷方式失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 重置设置按钮点击事件处理
    /// </summary>
    private void OnResetButtonClick(object? sender, EventArgs e)
    {
        // 重置配置为默认值
        Settings.ModifyConfig(_ => new());

        // 关闭自动启动
        _autoStartCheckBox.Checked = false;

        // 显示重置成功提示
        _ = MessageBox.Show("设置已重置为默认值！", "重置设置成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 根据窗口大小动态调整控件位置和大小
    /// </summary>
    private void ResizeControls()
    {
        // 获取当前窗口的宽度和高度
        var width = ClientSize.Width;
        var height = ClientSize.Height;

        // 控件X位置
        var controlX = (int)(10 * UIConstants.DpiScale);

        // 控件宽度
        var controlWidth = width - (int)(30 * UIConstants.DpiScale);

        // 控件垂直间距
        var controlSpacing = (int)(10 * UIConstants.DpiScale);

        // 控件数量为按钮数量+复选框
        var controlCount = _buttons.Count + 1;

        // 间隔数量
        var controlSpacingCount = controlCount + 1;

        // 每个控件的高度
        var heightPerControl = (height - (controlSpacingCount * controlSpacing)) / controlCount;

        // 当前Y位置
        var controlY = controlSpacing;

        // 设置自动启动复选框位置和大小
        _autoStartCheckBox.Location = new(controlX, controlY);
        _autoStartCheckBox.Size = new(controlWidth, heightPerControl);
        controlY += heightPerControl + controlSpacing;

        // 调整每个按钮的位置和大小
        foreach (var button in _buttons)
        {
            button.Location = new(controlX, controlY);
            button.Size = new(controlWidth, heightPerControl);
            controlY += heightPerControl + controlSpacing;
        }
    }

    /// <summary>
    /// 重写OnLoad方法, 用于加载设置
    /// </summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // 加载配置数据
        Settings.LoadConfig();

        // 恢复窗口位置和大小
        if (Settings.Config.SettingForm != null)
        {
            // 保存的位置
            var left = Settings.Config.SettingForm.Left;
            var top = Settings.Config.SettingForm.Top;

            // 当前屏幕的工作区域
            var workingArea = Screen.GetWorkingArea(this);

            // 确保位置在工作区域内
            if (left >= 0 && top >= 0 && left < workingArea.Width && top < workingArea.Height)
            {
                Left = left;
                Top = top;
            }

            // 如果配置的宽度和高度小于最小值，则使用最小值
            Width = Math.Max(Settings.Config.SettingForm.Width, UIConstants.SettingFormMinWidth);
            Height = Math.Max(Settings.Config.SettingForm.Height, UIConstants.SettingFormMinHeight);
        }

        // 调整控件位置和大小
        ResizeControls();
    }

    /// <summary>
    /// 重写OnFormClosing方法, 用于保存设置
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 保存窗口位置和大小
        Settings.ModifyConfig(config =>
        {
            return config with
            {
                SettingForm = new SettingFormConfig
                {
                    Left = Left,
                    Top = Top,
                    Width = Width,
                    Height = Height
                }
            };
        });
    }

    /// <summary>
    /// 重写FormClosed方法, 用于释放实例
    /// </summary>
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        // 释放唯一实例
        _instance = null;
    }
}