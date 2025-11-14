Public Class frmEnv
    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click

        Me.Close()

    End Sub

    Private Sub frmEnv_Load(sender As Object, e As EventArgs) Handles Me.Load
        txtCompanyIDX.Text = DecryptString(GetIni("Settings", "CompanyIdx", gAppPath))
        txtCompanyCode.Text = DecryptString(GetIni("Settings", "CompanyCode", gAppPath))

    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim result As DialogResult = MessageBox.Show("설정을 저장하시겠습니까? 설정 변경후 Demon 프로그램 재 실행 바랍니다.", "설정 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            PutIni("Settings", "CompanyIdx", EncryptString(txtCompanyIDX.Text.Trim), gAppPath)
            PutIni("Settings", "CompanyCode", EncryptString(txtCompanyCode.Text.Trim), gAppPath)
            Me.Close()
        End If
    End Sub

End Class