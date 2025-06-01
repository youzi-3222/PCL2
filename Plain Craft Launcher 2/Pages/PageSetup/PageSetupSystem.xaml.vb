﻿Class PageSetupSystem

    Private Shadows IsLoaded As Boolean = False

    Private Sub PageSetupSystem_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        '重复加载部分
        PanBack.ScrollToHome()

        '非重复加载部分
        If IsLoaded Then Return
        IsLoaded = True

        AniControlEnabled += 1
        Reload()
        SliderLoad()
        AniControlEnabled -= 1

    End Sub
    Public Sub Reload()

        '下载
        SliderDownloadThread.Value = Setup.Get("ToolDownloadThread")
        SliderDownloadSpeed.Value = Setup.Get("ToolDownloadSpeed")
        ComboDownloadSource.SelectedIndex = Setup.Get("ToolDownloadSource")
        ComboDownloadVersion.SelectedIndex = Setup.Get("ToolDownloadVersion")
        CheckDownloadAutoSelectVersion.Checked = Setup.Get("ToolDownloadAutoSelectVersion")

        'Mod 与整合包
        ComboDownloadTranslateV2.SelectedIndex = Setup.Get("ToolDownloadTranslateV2")
        ComboDownloadMod.SelectedIndex = Setup.Get("ToolDownloadMod")
        ComboModLocalNameStyle.SelectedIndex = Setup.Get("ToolModLocalNameStyle")
        CheckDownloadIgnoreQuilt.Checked = Setup.Get("ToolDownloadIgnoreQuilt")
        CheckDownloadClipboard.Checked = Setup.Get("ToolDownloadClipboard")

        'Minecraft 更新提示
        CheckUpdateRelease.Checked = Setup.Get("ToolUpdateRelease")
        CheckUpdateSnapshot.Checked = Setup.Get("ToolUpdateSnapshot")

        '辅助设置
        CheckHelpChinese.Checked = Setup.Get("ToolHelpChinese")

        '系统设置
        ComboSystemUpdate.SelectedIndex = Setup.Get("SystemSystemUpdate")
        If Val(Environment.OSVersion.Version.ToString().Split(".")(2)) >= 19042 Then
            ComboSystemUpdateBranch.SelectedIndex = Setup.Get("SystemSystemUpdateBranch")
        Else '不满足系统要求
            ComboSystemUpdateBranch.Items.Clear()
            ComboSystemUpdateBranch.Items.Add("Legacy")
            ComboSystemUpdateBranch.SelectedIndex = 0
            ComboSystemUpdateBranch.ToolTip = "由于你的 Windows 版本过低，不满足新版本要求，只能获取 Legacy 分支的更新。&#xa;升级到 Windows 10 20H2 或以上版本以获取最新更新。"
            ComboSystemUpdateBranch.IsEnabled = False
        End If
        ComboSystemActivity.SelectedIndex = Setup.Get("SystemSystemActivity")
        TextSystemCache.Text = Setup.Get("SystemSystemCache")
        CheckSystemDisableHardwareAcceleration.Checked = Setup.Get("SystemDisableHardwareAcceleration")
        SliderAniFPS.Value = Setup.Get("UiAniFPS")
        SliderMaxLog.Value = Setup.Get("SystemMaxLog")
        CheckSystemTelemetry.Checked = Setup.Get("SystemTelemetry")

        '网络
        TextSystemHttpProxy.Text = Setup.Get("SystemHttpProxy")
        CheckDownloadCert.Checked = Setup.Get("ToolDownloadCert")
        CheckUseDefaultProxy.Checked = Setup.Get("SystemUseDefaultProxy")

        '调试选项
        CheckDebugMode.Checked = Setup.Get("SystemDebugMode")
        SliderDebugAnim.Value = Setup.Get("SystemDebugAnim")
        CheckDebugDelay.Checked = Setup.Get("SystemDebugDelay")
        CheckDebugSkipCopy.Checked = Setup.Get("SystemDebugSkipCopy")

    End Sub

    '初始化
    Public Sub Reset()
        Try
            Setup.Reset("ToolDownloadThread")
            Setup.Reset("ToolDownloadSpeed")
            Setup.Reset("ToolDownloadSource")
            Setup.Reset("ToolDownloadVersion")
            Setup.Reset("ToolDownloadTranslateV2")
            Setup.Reset("ToolDownloadIgnoreQuilt")
            Setup.Reset("ToolDownloadClipboard")
            Setup.Reset("ToolDownloadMod")
            Setup.Reset("ToolDownloadAutoSelectVersion")
            Setup.Reset("ToolModLocalNameStyle")
            Setup.Reset("ToolUpdateRelease")
            Setup.Reset("ToolUpdateSnapshot")
            Setup.Reset("ToolHelpChinese")
            Setup.Reset("SystemDebugMode")
            Setup.Reset("SystemDebugAnim")
            Setup.Reset("SystemDebugDelay")
            Setup.Reset("SystemDebugSkipCopy")
            Setup.Reset("SystemSystemCache")
            Setup.Reset("SystemSystemUpdate")
            Setup.Reset("SystemSystemActivity")
            Setup.Reset("SystemDisableHardwareAcceleration")
            Setup.Reset("SystemHttpProxy")
            Setup.Reset("ToolDownloadCert")
            Setup.Reset("SystemUseDefaultProxy")
            Setup.Reset("UiAniFPS")

            Log("[Setup] 已初始化启动器页设置")
            Hint("已初始化启动器页设置！", HintType.Finish, False)
        Catch ex As Exception
            Log(ex, "初始化启动器页设置失败", LogLevel.Msgbox)
        End Try

        Reload()
    End Sub

    '将控件改变路由到设置改变
    Private Shared Sub CheckBoxChange(sender As MyCheckBox, e As Object) Handles CheckDebugMode.Change, CheckDebugDelay.Change, CheckDebugSkipCopy.Change, CheckUpdateRelease.Change, CheckUpdateSnapshot.Change, CheckHelpChinese.Change, CheckDownloadIgnoreQuilt.Change, CheckDownloadCert.Change, CheckDownloadClipboard.Change, CheckSystemDisableHardwareAcceleration.Change, CheckUseDefaultProxy.Change, CheckDownloadAutoSelectVersion.Change, CheckSystemTelemetry.Change
        If AniControlEnabled = 0 Then Setup.Set(sender.Tag, sender.Checked)
    End Sub
    Private Shared Sub SliderChange(sender As MySlider, e As Object) Handles SliderDebugAnim.Change, SliderDownloadThread.Change, SliderDownloadSpeed.Change, SliderAniFPS.Change, SliderMaxLog.Change
        If AniControlEnabled = 0 Then Setup.Set(sender.Tag, sender.Value)
    End Sub
    Private Shared Sub ComboChange(sender As MyComboBox, e As Object) Handles ComboDownloadVersion.SelectionChanged, ComboModLocalNameStyle.SelectionChanged, ComboDownloadTranslateV2.SelectionChanged, ComboSystemUpdate.SelectionChanged, ComboSystemActivity.SelectionChanged, ComboDownloadSource.SelectionChanged, ComboSystemUpdateBranch.SelectionChanged, ComboDownloadMod.SelectionChanged
        If AniControlEnabled = 0 Then Setup.Set(sender.Tag, sender.SelectedIndex)
    End Sub
    Private Shared Sub TextBoxChange(sender As MyTextBox, e As Object) Handles TextSystemCache.ValidatedTextChanged, TextSystemHttpProxy.TextChanged
        If AniControlEnabled = 0 Then Setup.Set(sender.Tag, sender.Text)
    End Sub

    Private Sub StartClipboardListening() Handles CheckDownloadClipboard.Change
        If CheckDownloadClipboard.Checked Then
            RunInNewThread(Sub() CompClipboard.ClipboardListening())
        End If
    End Sub

    '滑动条
    Private Sub SliderLoad()
        SliderDownloadThread.GetHintText = Function(v) v + 1
        SliderDownloadSpeed.GetHintText =
        Function(v)
            Select Case v
                Case Is <= 14
                    Return (v + 1) * 0.1 & " M/s"
                Case Is <= 31
                    Return (v - 11) * 0.5 & " M/s"
                Case Is <= 41
                    Return (v - 21) & " M/s"
                Case Else
                    Return "无限制"
            End Select
        End Function
        SliderDebugAnim.GetHintText = Function(v) If(v > 29, "关闭", (v / 10 + 0.1) & "x")
        SliderAniFPS.GetHintText =
            Function(v)
                Return $"{v + 1} FPS"
            End Function
        SliderMaxLog.GetHintText =
            Function(v)
                'y = 10x + 50 (0 <= x <= 5, 50 <= y <= 100)
                'y = 50x - 150 (5 < x <= 13, 100 < y <= 500)
                'y = 100x - 800 (13 < x <= 28, 500 < y <= 2000)
                Select Case v
                    Case Is <= 5
                        Return v * 10 + 50
                    Case Is <= 13
                        Return v * 50 - 150
                    Case Is <= 28
                        Return v * 100 - 800
                    Case Else
                        Return "无限制"
                End Select
            End Function
    End Sub
    Private Sub SliderDownloadThread_PreviewChange(sender As Object, e As RouteEventArgs) Handles SliderDownloadThread.PreviewChange
        If SliderDownloadThread.Value < 100 Then Return
        If Not Setup.Get("HintDownloadThread") Then
            Setup.Set("HintDownloadThread", True)
            MyMsgBox("如果设置过多的下载线程，可能会导致下载时出现非常严重的卡顿。" & vbCrLf &
                     "一般设置 64 线程即可满足大多数下载需求，除非你知道你在干什么，否则不建议设置更多的线程数！", "警告", "我知道了", IsWarn:=True)
        End If
    End Sub

    '硬件加速
    Private Sub Check_DisableHardwareAcceleration(sender As Object, user As Boolean) Handles CheckSystemDisableHardwareAcceleration.Change
        Hint("此项变更将在重启 PCL 后生效")
    End Sub

    '调试模式
    Private Sub CheckDebugMode_Change() Handles CheckDebugMode.Change
        If AniControlEnabled = 0 Then Hint("部分调试信息将在刷新或启动器重启后切换显示！",, False)
    End Sub

    '自动更新
    Private Sub ComboSystemActivity_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboSystemActivity.SelectionChanged
        If AniControlEnabled <> 0 Then Return
        If ComboSystemActivity.SelectedIndex <> 2 Then Return
        If MyMsgBox("若选择此项，即使在将来出现严重问题时，你也无法获取相关通知。" & vbCrLf &
                    "例如，如果发现某个版本游戏存在严重 Bug，你可能就会因为无法得到通知而导致无法预知的后果。" & vbCrLf & vbCrLf &
                    "一般选择 仅在有重要通知时显示公告 就可以让你尽量不受打扰了。" & vbCrLf &
                    "除非你在制作服务器整合包，或时常手动更新启动器，否则极度不推荐选择此项！", "警告", "我知道我在做什么", "取消", IsWarn:=True) = 2 Then
            ComboSystemActivity.SelectedItem = e.RemovedItems(0)
        End If
    End Sub
    Private Sub ComboSystemUpdate_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboSystemUpdate.SelectionChanged
        If AniControlEnabled <> 0 Then Return
        If ComboSystemUpdate.SelectedIndex <> 3 Then Return
        If MyMsgBox("若选择此项，即使在启动器将来出现严重问题时，你也无法获取更新并获得修复。" & vbCrLf &
                    "例如，如果官方修改了登录方式，从而导致现有启动器无法登录，你可能就会因为无法更新而无法开始游戏。" & vbCrLf & vbCrLf &
                    "一般选择 仅在有重大漏洞更新时显示提示 就可以让你尽量不受打扰了。" & vbCrLf &
                    "除非你在制作服务器整合包，或时常手动更新启动器，否则极度不推荐选择此项！", "警告", "我知道我在做什么", "取消", IsWarn:=True) = 2 Then
            ComboSystemUpdate.SelectedItem = e.RemovedItems(0)
        End If
    End Sub
    Private Sub ComboSystemUpdateBranch_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboSystemUpdateBranch.SelectionChanged
        If AniControlEnabled <> 0 Then Exit Sub
        If ComboSystemUpdateBranch.SelectedIndex <> 1 Then Exit Sub
        If MyMsgBox("你正在切换启动器更新通道到 Fast Ring。" & vbCrLf &
                    "Fast Ring 可以提供下个版本更新内容的预览，但可能会包含未经充分测试的功能，稳定性欠佳。" & vbCrLf & vbCrLf &
                    "在升级到 Fast Ring 版本后，如果你选择切换到 Slow Ring，需要等待下一个 Slow Ring 版本发布，在这期间不会提供更新。" & vbCrLf &
                    "该选项仅推荐具有一定基础知识和能力的用户选择。如果你正在制作整合包，请使用 Slow Ring！", "继续之前...", "我已知晓", "取消", IsWarn:=True) = 2 Then
            ComboSystemUpdateBranch.SelectedItem = e.RemovedItems(0)
        Else
            UpdateCheckByButton()
        End If
    End Sub
    Private Sub BtnSystemUpdate_Click(sender As Object, e As EventArgs) Handles BtnSystemUpdate.Click
        UpdateCheckByButton()
    End Sub
    Private Sub BtnSystemMirrorChyanKey_Click(sender As Object, e As EventArgs) Handles BtnSystemMirrorChyanKey.Click
        Dim ret = MyMsgBoxInput("设置 Mirror 酱 CDK", $"Mirror 酱(https://mirrorchyan.com/)是一个第三方应用分发平台{vbCrLf}如果你购买了他们的服务，可以让 PCL CE 使用他们的高速下载源下载版本更新，同时也可以减轻社区更新服务器的压力……")
        If String.IsNullOrWhiteSpace(ret) Then Exit Sub
        Setup.Set("SystemMirrorChyanKey", ret)
        Hint("设置 Mirror 酱 CDK 成功！", HintType.Finish)
    End Sub
    Private Sub BtnSystemMirrorChyanGetKey_Click(sender As Object, e As EventArgs) Handles BtnSystemMirrorChyanGetKey.Click
        OpenWebsite("https://mirrorchyan.com/")
    End Sub
    ''' <summary>
    ''' 启动器是否已经是最新版？
    ''' 若返回 Nothing，则代表无更新缓存文件或出错。
    ''' </summary>
    Public Shared Function IsLauncherNewest() As Boolean?
        Try
            Return IsVerisonLatest()
        Catch ex As Exception
            Log(ex, "确认启动器更新失败", LogLevel.Feedback)
            Return Nothing
        End Try
    End Function

#Region "导出 / 导入设置"

    Private Sub BtnSystemSettingExp_Click(sender As Object, e As MouseButtonEventArgs) Handles BtnSystemSettingExp.Click
        Dim savePath As String = SelectSaveFile("选择保存位置", "PCL 全局配置.json", "PCL 配置文件(*.json)|*.json", Path).Replace("/", "\")
        If savePath = "" Then Exit Sub
        File.Copy(PathAppdataConfig & "Config.json", savePath, True)
        Hint("配置导出成功！", HintType.Finish)
        OpenExplorer(savePath)
    End Sub
    Private Sub BtnSystemSettingImp_Click(sender As Object, e As MouseButtonEventArgs) Handles BtnSystemSettingImp.Click
        Dim sourcePath As String = SelectFile("PCL 配置文件(*.json)|*.json", "选择配置文件")
        If sourcePath = "" Then Exit Sub
        File.Copy(sourcePath, PathAppdataConfig & "Config.json", True)
        MyMsgBox("配置导入成功！请重启 PCL 以应用配置……", Button1:="重启", ForceWait:=True)
        Process.Start(New ProcessStartInfo(PathWithName))
        FormMain.EndProgramForce(ProcessReturnValues.Success)
    End Sub

#End Region

End Class
