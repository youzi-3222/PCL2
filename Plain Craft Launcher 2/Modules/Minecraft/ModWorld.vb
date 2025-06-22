Imports System.IO.Compression

Public Module ModWorld

#Region "压缩包处理"
    ''' <summary>
    ''' 尝试处理存档。
    ''' </summary>
    ''' <exception cref="CancelledException">确定这是一个存档文件（夹），但存档文件损坏时抛出的异常。</exception>
    ''' <exception cref="Exception"></exception>
    Public Sub ReadWorld(SavePath As String)
        If File.Exists(SavePath) Then
            Dim ExtractPath As String = $"{PathTemp}Cache\{RandomInteger(0, 1000_0000)}\"
            If Directory.Exists(ExtractPath) Then DeleteDirectory(ExtractPath)
            ExtractFile(SavePath, ExtractPath)
            SavePath = ExtractPath
        End If
        Dim world As New McWorld(SavePath)
        If Not File.Exists(world.LevelDatPath) Then Throw New Exception("无效的 Minecraft 存档")
        If Not world.Read Then
            Hint("存档文件可能已损坏，无法读取！", HintType.Critical)
            Throw New CancelledException()
        End If
        Dim sb As New StringBuilder
        If world.VersionName IsNot Nothing Then sb.AppendLine($"存档版本：{world.VersionName}")
        If world.VersionId IsNot Nothing Then sb.AppendLine($"存档数据版本：{world.VersionId}")
        If sb.Length = 0 Then sb.AppendLine("无法获取存档的版本信息，存档版本可能低于 15w32a（对应正式版 1.9）！")
        MyMsgBox(sb.ToString, "存档版本信息")
    End Sub
#End Region

#Region "存档"
    ''' <summary>
    ''' 存档。
    ''' </summary>
    Public Class McWorld
        ''' <summary>
        ''' 存档路径。文件夹，以 “\” 结尾。
        ''' </summary>
        Public SavePath As String
        ''' <summary>
        ''' 版本名。
        ''' </summary>
        Public VersionName As String
        ''' <summary>
        ''' 版本 ID。
        ''' </summary>
        Public VersionId As String
        ''' <summary>
        ''' 存档。
        ''' </summary>
        ''' <param name="SavePath">存档路径。文件夹，以 “\” 结尾。</param>
        Public Sub New(SavePath As String)
            If Not SavePath.EndsWithF("\") Then SavePath = SavePath & "\"
            Me.SavePath = SavePath
        End Sub
        Public ReadOnly Property LevelDatPath
            Get
                Return SavePath & "level.dat"
            End Get
        End Property
        ''' <summary>
        ''' 读取存档。返回是否成功。
        ''' </summary>
        Public Function Read() As Boolean
            Try
                Log($"[World] 读取存档：{SavePath}")
                If Not File.Exists(LevelDatPath) Then
                    Log("[World] 存档没有 level.dat 文件，读取失败")
                    Return False
                End If
                Using fileStream As FileStream = File.OpenRead(LevelDatPath)
                    Using gzipStream As New GZipStream(fileStream, CompressionMode.Decompress)
                        '读取 NBT 数据
                        Dim reader As New NbtReader(gzipStream)
                        Dim rootTag As Dictionary(Of String, Object) = reader.ReadTag().Value

                        If rootTag Is Nothing OrElse Not rootTag.ContainsKey("Data") Then
                            Log("[World] 根 NBT 标签存在问题，读取失败")
                            Return False
                        End If
                        Dim dataTag As Dictionary(Of String, Object) = rootTag("Data")

                        If dataTag.ContainsKey("Version") Then
                            Dim versionTag As Dictionary(Of String, Object) = dataTag("Version")
                            If versionTag.ContainsKey("Name") Then VersionName = versionTag("Name").ToString()
                            If versionTag.ContainsKey("Id") Then VersionId = versionTag("Id").ToString()
                        End If

                        Return True
                    End Using
                End Using
            Catch ex As Exception
                Log(ex, "读取存档时出错")
                Return False
            End Try
        End Function
    End Class
#End Region

#Region "NBT 读取"
    Public Class NbtReader
        Private ReadOnly stream As Stream

        Public Sub New(stream As Stream)
            Me.stream = stream
        End Sub

        Public Function ReadTag() As KeyValuePair(Of String, Object)
            Dim tagType As Byte = ReadByte()
            If tagType = 0 Then Return Nothing
            Dim length As Short = ReadShort()
            Dim key(length - 1) As Byte
            stream.Read(key, 0, length)
            Return New KeyValuePair(Of String, Object)(Encoding.ASCII.GetString(key), ReadObject(tagType))
        End Function
        Public Function ReadObject(tagType As Byte) As Object
            If tagType = 0 Then Return Nothing

            Select Case tagType
                Case 1 ' TAG_Byte
                    Return ReadByte()
                Case 2 ' TAG_Short
                    Return ReadShort()
                Case 3 ' TAG_Int
                    Return ReadInt()
                Case 4 ' TAG_Long
                    Return ReadLong()
                Case 5 ' TAG_Float
                    Return ReadFloat()
                Case 6 ' TAG_Double
                    Return ReadDouble()
                Case 7 ' TAG_Byte_Array
                    Dim arrayLength As Integer = ReadInt()
                    Dim bytes(arrayLength - 1) As Byte
                    stream.Read(bytes, 0, arrayLength)
                    Return bytes
                Case 8 ' TAG_String
                    Dim stringLength As Short = ReadShort()
                    Dim bytes(stringLength - 1) As Byte
                    stream.Read(bytes, 0, stringLength)
                    Return Encoding.UTF8.GetString(bytes)
                Case 9 ' TAG_List
                    Dim listType As Byte = stream.ReadByte()
                    Dim listLength As Integer = ReadInt()
                    Dim list As New List(Of Object)

                    For i As Integer = 0 To listLength - 1
                        list.Append(ReadObject(listType))
                    Next

                    Return list
                Case 10 ' TAG_Compound
                    Dim compound As New List(Of KeyValuePair(Of String, Object))
                    Dim currentTag As KeyValuePair(Of String, Object)
                    Do
                        currentTag = ReadTag()
                        If currentTag.Value IsNot Nothing Then
                            compound.Add(currentTag)
                        End If
                    Loop While currentTag.Value IsNot Nothing

                    Return compound.ToDictionary(Function(x) x.Key, Function(x) x.Value)
                Case 11 ' TAG_Int_Array
                    Dim arrayLength As Integer = ReadInt()
                    Dim ints(arrayLength - 1) As Integer

                    For i As Integer = 0 To arrayLength - 1
                        ints(i) = ReadInt()
                    Next

                    Return ints
                Case 12 ' TAG_Long_Array
                    Dim arrayLength As Integer = ReadInt()
                    Dim longs(arrayLength - 1) As Integer

                    For i As Integer = 0 To arrayLength - 1
                        longs(i) = ReadLong()
                    Next

                    Return longs
                Case Else
                    Throw New NotSupportedException("未知的 NBT 标签类型: " & tagType)
            End Select
        End Function

        Private Function ReadByte() As Byte
            Return stream.ReadByte()
        End Function

        Private Function ReadShort() As Short
            Dim bytes(1) As Byte
            stream.Read(bytes, 0, 2)
            If BitConverter.IsLittleEndian Then Array.Reverse(bytes)
            Return BitConverter.ToInt16(bytes, 0)
        End Function

        Private Function ReadInt() As Integer
            Dim bytes(3) As Byte
            stream.Read(bytes, 0, 4)
            If BitConverter.IsLittleEndian Then Array.Reverse(bytes)
            Return BitConverter.ToInt32(bytes, 0)
        End Function

        Private Function ReadLong() As Long
            Dim bytes(7) As Byte
            stream.Read(bytes, 0, 8)
            If BitConverter.IsLittleEndian Then Array.Reverse(bytes)
            Return BitConverter.ToInt64(bytes, 0)
        End Function

        Private Function ReadFloat() As Single
            Dim bytes(3) As Byte
            stream.Read(bytes, 0, 4)
            If BitConverter.IsLittleEndian Then Array.Reverse(bytes)
            Return BitConverter.ToSingle(bytes, 0)
        End Function

        Private Function ReadDouble() As Double
            Dim bytes(7) As Byte
            stream.Read(bytes, 0, 8)
            If BitConverter.IsLittleEndian Then Array.Reverse(bytes)
            Return BitConverter.ToDouble(bytes, 0)
        End Function
    End Class
#End Region

End Module
