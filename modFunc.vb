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
    Public Function AuthenticateWithPassword() As Boolean
        ' --- 1. 비밀번호 생성 ---
        Dim currentDay As Integer = Date.Today.Day
        Dim sumOfDigits As Integer = 0
        For Each c As Char In currentDay.ToString()
            sumOfDigits += CInt(c.ToString())
        Next
        Dim calculatedNumber As Integer = sumOfDigits * 7
        Dim correctPassword As String = $"dy{calculatedNumber}0655"

        ' --- 2. 사용자 입력 받기 ---
        Dim userInput As String = InputBox("비밀번호를 입력하세요.", "인증")

        ' --- 3. 비밀번호 확인 ---
        If userInput = correctPassword Then
            Return True ' 인증 성공
        Else
            If Not String.IsNullOrEmpty(userInput) Then
                MessageBox.Show("비밀번호가 올바르지 않습니다.", "인증 실패", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False ' 인증 실패
        End If
    End Function


End Module
