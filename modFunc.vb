Module modFunc


    '로그쌓기 
    Public Sub WriteLog(ByVal strMsg As String, ByVal strFolder As String, ByVal strFile As String)

        Try
            Dim strCheckFolder As String = ""
            Dim strFileName As String = ""

            strCheckFolder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\"))
            strCheckFolder += "\" + strFolder + "\" + strFile

            If (FileSystem.Dir(strCheckFolder, FileAttribute.Directory) = "") Then
                System.IO.Directory.CreateDirectory(strCheckFolder)
            End If

            strFileName = strCheckFolder + "\" + DateAndTime.Now.ToString("yyyyMMdd") + ".log"

            Dim FileWriter As System.IO.StreamWriter = New System.IO.StreamWriter(strFileName, True)

            FileWriter.Write(DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ::::: " + strMsg + vbCrLf)
            FileWriter.Flush()
            FileWriter.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "로그 작성중 오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

End Module
