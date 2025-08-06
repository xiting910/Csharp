using MineClearance.Models;
using MineClearance.Services;
using MineClearance.Utilities;

namespace MineClearance.UI;

/// <summary>
/// 设置窗口类
/// </summary>
public partial class SettingForm : Form
{
    /// <summary>
    /// 设置窗口实例
    /// </summary>
    private static SettingForm? instance;

    /// <summary>
    /// 显示设置窗口
    /// </summary>
    public static void ShowForm()
    {
        // 如果实例已存在且未释放, 则直接显示
        if (instance != null && !instance.IsDisposed)
        {
            if (instance.WindowState == FormWindowState.Minimized)
            {
                instance.WindowState = FormWindowState.Normal;
            }
            instance.Activate();
            return;
        }

        // 创建新的实例并显示
        instance = new();
        instance.Show();
    }

    /// <summary>
    /// 自动启动复选框
    /// </summary>
    private readonly CheckBox autoStartCheckBox;

    /// <summary>
    /// 隐藏更新详细提示复选框
    /// </summary>
    private readonly CheckBox hideUpdateDetailsCheckBox;

    /// <summary>
    /// 按钮列表
    /// </summary>
    private readonly List<Button> buttons;

    /// <summary>
    /// 构造函数, 初始化设置窗口
    /// </summary>
    private SettingForm()
    {
        // 设置窗口属性
        Text = "设置";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new(Constants.SettingFormMinWidth, Constants.SettingFormMinHeight);

        // 初始化自动启动复选框
        autoStartCheckBox = new()
        {
            TextAlign = ContentAlignment.MiddleCenter,
            Checked = AutoStartHelper.IsAutoStartEnabled(),
            Appearance = Appearance.Button,
            FlatStyle = FlatStyle.Flat
        };
        UpdateAutoStartCheckBox();
        autoStartCheckBox.CheckedChanged += AutoStartCheckBox_CheckedChanged;

        // 初始化隐藏更新详细提示复选框
        hideUpdateDetailsCheckBox = new()
        {
            TextAlign = ContentAlignment.MiddleCenter,
            Checked = Settings.Config.HideUpdateDetails,
            Appearance = Appearance.Button,
            FlatStyle = FlatStyle.Flat
        };
        UpdateHideUpdateDetailsCheckBox();
        hideUpdateDetailsCheckBox.CheckedChanged += HideUpdateDetailsCheckBox_CheckedChanged;

        // 初始化按钮列表
        buttons = [];

        // 初始化创建桌面快捷方式按钮
        var createShortcutButton = new Button
        {
            Text = "创建桌面快捷方式",
            BackColor = Color.LightBlue,
            FlatStyle = FlatStyle.Flat
        };
        buttons.Add(createShortcutButton);
        createShortcutButton.Click += CreateShortcutButton_Click;

        // 初始化重置按钮
        var resetButton = new Button
        {
            Text = "重置设置",
            BackColor = Color.Yellow,
            FlatStyle = FlatStyle.Flat
        };
        buttons.Add(resetButton);
        resetButton.Click += ResetButton_Click;

        // 初始化卸载按钮
        var uninstallButton = new Button
        {
            Text = "卸载",
            BackColor = Color.DarkRed,
            FlatStyle = FlatStyle.Flat
        };
        buttons.Add(uninstallButton);
        uninstallButton.Click += UninstallButton_Click;

        // 添加控件到窗口
        Controls.Add(autoStartCheckBox);
        Controls.Add(hideUpdateDetailsCheckBox);
        Controls.AddRange([.. buttons]);

        // 订阅窗口大小变化事件
        Resize += FormResized;
    }

    /// <summary>
    /// 自动启动复选框状态变化事件处理
    /// </summary>
    private void AutoStartCheckBox_CheckedChanged(object? sender, EventArgs e)
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
                // 如果发生异常，切换复选框状态并显示错误信息
                cb.CheckedChanged -= AutoStartCheckBox_CheckedChanged;
                cb.Checked = !cb.Checked;
                cb.CheckedChanged += AutoStartCheckBox_CheckedChanged;
                MessageBox.Show($"设置开机自启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        if (autoStartCheckBox.Checked)
        {
            autoStartCheckBox.Text = "开机自动启动: 已启用";
            autoStartCheckBox.BackColor = Color.LightGreen;
        }
        else
        {
            autoStartCheckBox.Text = "开机自动启动: 已禁用";
            autoStartCheckBox.BackColor = Color.LightGray;
        }
    }

    /// <summary>
    /// 隐藏更新详细提示复选框状态变化事件处理
    /// </summary>
    private void HideUpdateDetailsCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is CheckBox cb)
        {
            try
            {
                Settings.ModifyConfig(config =>
                {
                    // 更新配置中的隐藏更新详细提示设置
                    return config with { HideUpdateDetails = cb.Checked };
                });
            }
            catch (Exception ex)
            {
                // 如果发生异常，切换复选框状态并显示错误信息
                cb.CheckedChanged -= HideUpdateDetailsCheckBox_CheckedChanged;
                cb.Checked = !cb.Checked;
                cb.CheckedChanged += HideUpdateDetailsCheckBox_CheckedChanged;
                MessageBox.Show($"设置隐藏更新详细提示失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 更新复选框文本和颜色
                UpdateHideUpdateDetailsCheckBox();
            }
        }
    }

    /// <summary>
    /// 根据是否隐藏更新详细提示来切换复选框文本和颜色
    /// </summary>
    private void UpdateHideUpdateDetailsCheckBox()
    {
        if (hideUpdateDetailsCheckBox.Checked)
        {
            hideUpdateDetailsCheckBox.Text = "隐藏更新详细提示: 已启用";
            hideUpdateDetailsCheckBox.BackColor = Color.LightGreen;
        }
        else
        {
            hideUpdateDetailsCheckBox.Text = "隐藏更新详细提示: 已禁用";
            hideUpdateDetailsCheckBox.BackColor = Color.LightGray;
        }
    }

    /// <summary>
    /// 创建桌面快捷方式按钮点击事件处理
    /// </summary>
    private void CreateShortcutButton_Click(object? sender, EventArgs e)
    {
        // 弹窗提示用户输入快捷方式名称
        var shortcutName = Microsoft.VisualBasic.Interaction.InputBox("请输入快捷方式名称:", "创建桌面快捷方式", Constants.ExecutableFileName, -1, -1);

        if (!string.IsNullOrWhiteSpace(shortcutName))
        {
            // 创建快捷方式
            try
            {
                ShortcutCreator.CreateDesktopShortcut(Constants.ExecutableFilePath, shortcutName);
                MessageBox.Show("快捷方式创建成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建快捷方式失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 重置设置按钮点击事件处理
    /// </summary>
    private void ResetButton_Click(object? sender, EventArgs e)
    {
        // 重置配置为默认值
        Settings.ModifyConfig(_ => new());

        // 关闭自动启动
        autoStartCheckBox.Checked = false;

        // 更新隐藏更新详细提示复选框状态
        hideUpdateDetailsCheckBox.CheckedChanged -= HideUpdateDetailsCheckBox_CheckedChanged;
        hideUpdateDetailsCheckBox.Checked = Settings.Config.HideUpdateDetails;
        hideUpdateDetailsCheckBox.CheckedChanged += HideUpdateDetailsCheckBox_CheckedChanged;
        UpdateHideUpdateDetailsCheckBox();

        // 显示重置成功提示
        MessageBox.Show("设置已重置为默认值！", "重置设置成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 卸载按钮点击事件处理
    /// </summary>
    private void UninstallButton_Click(object? sender, EventArgs e)
    {
        // 弹出确认对话框
        using var dialog = new UninstallConfirmDialog("您确定要卸载吗？");
        var result = dialog.ShowDialog();

        // 如果用户没有点击确定按钮
        if (result != DialogResult.OK)
        {
            return;
        }

        // 再次弹出确认对话框
        var confirmResult = MessageBox.Show($"请再次确认您要卸载吗？\n这将会彻底删除 {Constants.ParentDirectory} 文件夹及其所有内容", "再次确认卸载", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        // 如果用户没有点击确定按钮
        if (confirmResult != DialogResult.OK)
        {
            return;
        }

        // 关闭开机自启
        AutoStartHelper.DisableAutoStart();

        // 设置强制关闭标志
        Methods.IsForceClose = true;

        // 启动卸载脚本
        Script.StartAutoUninstallScript(dialog.KeepData);

        // 退出应用程序
        Application.Exit();
    }

    /// <summary>
    /// 窗口大小变化事件处理
    /// </summary>
    private void FormResized(object? sender, EventArgs e)
    {
        // 调整控件位置和大小
        ResizeControls();
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
        var controlX = (int)(10 * Constants.DpiScale);

        // 控件宽度
        var controlWidth = width - (int)(30 * Constants.DpiScale);

        // 间隔数量
        var controlSpacingCount = buttons.Count + 3;

        // 控件垂直间距
        var controlSpacing = (int)(10 * Constants.DpiScale);

        // 复选框高度
        var checkBoxHeight = (int)(30 * Constants.DpiScale);

        // 按钮总高度
        var buttonHeight = height - checkBoxHeight * 2 - controlSpacing * controlSpacingCount;

        // 每个按钮的高度
        var buttonHeightPerControl = buttonHeight / buttons.Count;

        // 当前Y位置
        var controlY = controlSpacing;

        // 设置自动启动复选框位置和大小
        autoStartCheckBox.Location = new(controlX, controlY);
        autoStartCheckBox.Size = new(controlWidth, checkBoxHeight);
        controlY += checkBoxHeight + controlSpacing;

        // 设置隐藏更新详细提示复选框位置和大小
        hideUpdateDetailsCheckBox.Location = new(controlX, controlY);
        hideUpdateDetailsCheckBox.Size = new(controlWidth, checkBoxHeight);
        controlY += checkBoxHeight + controlSpacing;

        // 调整每个按钮的位置和大小
        foreach (var button in buttons)
        {
            button.Location = new(controlX, controlY);
            button.Size = new(controlWidth, buttonHeightPerControl);
            controlY += buttonHeightPerControl + controlSpacing;
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
            Left = Settings.Config.SettingForm.Left;
            Top = Settings.Config.SettingForm.Top;

            // 如果配置的宽度和高度小于最小值，则使用最小值
            Width = Math.Max(Settings.Config.SettingForm.Width, Constants.SettingFormMinWidth);
            Height = Math.Max(Settings.Config.SettingForm.Height, Constants.SettingFormMinHeight);
        }

        // 调整控件位置和大小
        ResizeControls();
    }

    /// <summary>
    /// 重写OnFormClosing方法, 用于保存设置
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // 解除事件绑定，防止关闭时触发
        Resize -= FormResized;

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

        // 将设置窗口实例设为null
        instance = null;

        base.OnFormClosing(e);
    }
}