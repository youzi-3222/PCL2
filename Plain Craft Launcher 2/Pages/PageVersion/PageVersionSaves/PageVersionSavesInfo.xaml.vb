Imports System.Xml.XPath
Imports PlainNamedBinaryTag

Class PageVersionSavesInfo
    Implements IRefreshable

    Private Sub IRefreshable_Refresh() Implements IRefreshable.Refresh
        Refresh()
    End Sub
    Public Sub Refresh()
        RefreshInfo()
    End Sub

    Private _loaded As Boolean
    Private Sub Init() Handles Me.Loaded
        PanBack.ScrollToHome()

        RefreshInfo()

        _loaded = True
        If _loaded Then Return

    End Sub

    Private Sub RefreshInfo()
        Try
            Dim saveDatPath = IO.Path.Combine(PageVersionSavesLeft.CurrentSave, "level.dat")
            Dim isCompressed As Boolean
            Using fs As New FileStream(saveDatPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Using saveInfo = VbNbtReaderCreator.FromStreamAutoDetect(fs, isCompressed)
                    Dim outRes As NbtType
                    Dim xData = saveInfo.ReadNbtAsXml(outRes)
                    Dim levelData = xData.XPathSelectElement("//TCompound[@Name='Data']")
                    ClearInfoTable()
                    Dim GetDataInfoByPath = Function(path As String) As String
                                                Return levelData.XPathSelectElement(path).Value
                                            End Function
                    AddInfoTable("存档名称", GetDataInfoByPath("//TString[@Name='LevelName']"))
                    Dim versionName = GetDataInfoByPath("//TCompound[@Name='Version']/TString[@Name='Name']")
                    Dim versionId = GetDataInfoByPath("//TCompound[@Name='Version']/TInt32[@Name='Id']")
                    AddInfoTable("存档版本", $"{versionName} ({versionId})")
                    AddInfoTable("种子", GetDataInfoByPath($"//TCompound[@Name='WorldGenSettings']/TInt64[@Name='seed']"), True)
                    AddInfoTable("最后一次游玩", New DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Long.Parse(GetDataInfoByPath("//TInt64[@Name='LastPlayed']"))).ToLocalTime().ToString())
                    Dim spawnX = GetDataInfoByPath("//TInt32[@Name='SpawnX']")
                    Dim spawnY = GetDataInfoByPath("//TInt32[@Name='SpawnY']")
                    Dim spawnZ = GetDataInfoByPath("//TInt32[@Name='SpawnZ']")
                    AddInfoTable("出生点 (X/Y/Z)", $"{spawnX} / {spawnY} / {spawnZ}")
                    Dim difficulty = GetDataInfoByPath("//TInt8[@Name='Difficulty']")
                    Dim isDifficultyLocked As Boolean = GetDataInfoByPath("//TInt8[@Name='DifficultyLocked']") <> "0"
                    AddInfoTable("困难度", $"{difficulty} (是否已锁定难度：{isDifficultyLocked})")
                End Using
            End Using
        Catch ex As Exception
            Log(ex, $"获取存档信息失败", LogLevel.Msgbox)
        End Try
    End Sub

    Private Sub ClearInfoTable()
        PanList.Children.Clear()
        PanList.RowDefinitions.Clear()
    End Sub

    Private Sub AddInfoTable(head As String, content As String, Optional allowCopy As Boolean = False)
        Dim headTextBlock As New TextBlock With {.Text = head, .Margin = New Thickness(0, 3, 0, 3)}
        Dim contentTextBlock As UIElement
        If allowCopy Then
            Dim thisBtn = New MyTextButton With {.Text = content, .Margin = New Thickness(0, 3, 0, 3)}
            contentTextBlock = thisBtn
            AddHandler thisBtn.Click, Sub()
                                          Try
                                              ClipboardSet(content)
                                          Catch ex As Exception
                                              Log(ex, "复制到剪贴板失败", LogLevel.Hint)
                                          End Try
                                      End Sub
        Else
            contentTextBlock = New TextBlock With {.Text = content, .Margin = New Thickness(0, 3, 0, 3)}
        End If
        PanList.Children.Add(headTextBlock)
        PanList.Children.Add(contentTextBlock)
        Dim targetRow = New RowDefinition
        PanList.RowDefinitions.Add(targetRow)
        Dim rowIndex = PanList.RowDefinitions.IndexOf(targetRow)
        Grid.SetRow(headTextBlock, rowIndex)
        Grid.SetColumn(headTextBlock, 0)
        Grid.SetRow(contentTextBlock, rowIndex)
        Grid.SetColumn(contentTextBlock, 2)
    End Sub

End Class
