<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        WebView21 = New Microsoft.Web.WebView2.WinForms.WebView2()
        pnlGolfWebView = New Panel()
        txtGolfWebView = New TextBox()
        pnlGolfDemon = New Panel()
        txtGolfDemonDisp = New TextBox()
        pnlMain = New Panel()
        btnExit = New Button()
        btnEnv = New Button()
        WebView22 = New Microsoft.Web.WebView2.WinForms.WebView2()
        CType(WebView21, ComponentModel.ISupportInitialize).BeginInit()
        pnlGolfWebView.SuspendLayout()
        pnlGolfDemon.SuspendLayout()
        pnlMain.SuspendLayout()
        CType(WebView22, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' WebView21
        ' 
        WebView21.AllowExternalDrop = True
        WebView21.BackColor = Color.FromArgb(CByte(45), CByte(45), CByte(48))
        WebView21.CreationProperties = Nothing
        WebView21.DefaultBackgroundColor = Color.White
        WebView21.ForeColor = Color.White
        WebView21.Location = New Point(162, 58)
        WebView21.Name = "WebView21"
        WebView21.Size = New Size(116, 43)
        WebView21.TabIndex = 0
        WebView21.Visible = False
        WebView21.ZoomFactor = 1R
        ' 
        ' pnlGolfWebView
        ' 
        pnlGolfWebView.BorderStyle = BorderStyle.FixedSingle
        pnlGolfWebView.Controls.Add(txtGolfWebView)
        pnlGolfWebView.Location = New Point(12, 87)
        pnlGolfWebView.Name = "pnlGolfWebView"
        pnlGolfWebView.Size = New Size(467, 557)
        pnlGolfWebView.TabIndex = 1
        ' 
        ' txtGolfWebView
        ' 
        txtGolfWebView.BackColor = Color.FromArgb(CByte(45), CByte(45), CByte(48))
        txtGolfWebView.ForeColor = Color.White
        txtGolfWebView.Location = New Point(8, 9)
        txtGolfWebView.Multiline = True
        txtGolfWebView.Name = "txtGolfWebView"
        txtGolfWebView.Size = New Size(446, 538)
        txtGolfWebView.TabIndex = 1
        ' 
        ' pnlGolfDemon
        ' 
        pnlGolfDemon.BorderStyle = BorderStyle.FixedSingle
        pnlGolfDemon.Controls.Add(txtGolfDemonDisp)
        pnlGolfDemon.Location = New Point(500, 87)
        pnlGolfDemon.Name = "pnlGolfDemon"
        pnlGolfDemon.Size = New Size(467, 557)
        pnlGolfDemon.TabIndex = 2
        ' 
        ' txtGolfDemonDisp
        ' 
        txtGolfDemonDisp.BackColor = Color.FromArgb(CByte(45), CByte(45), CByte(48))
        txtGolfDemonDisp.ForeColor = Color.White
        txtGolfDemonDisp.Location = New Point(9, 9)
        txtGolfDemonDisp.Multiline = True
        txtGolfDemonDisp.Name = "txtGolfDemonDisp"
        txtGolfDemonDisp.Size = New Size(446, 538)
        txtGolfDemonDisp.TabIndex = 0
        ' 
        ' pnlMain
        ' 
        pnlMain.Controls.Add(btnExit)
        pnlMain.Controls.Add(btnEnv)
        pnlMain.Location = New Point(12, 12)
        pnlMain.Name = "pnlMain"
        pnlMain.Size = New Size(956, 69)
        pnlMain.TabIndex = 3
        ' 
        ' btnExit
        ' 
        btnExit.BackColor = Color.MidnightBlue
        btnExit.Font = New Font("맑은 고딕", 16F, FontStyle.Bold)
        btnExit.ForeColor = Color.White
        btnExit.Location = New Point(832, 13)
        btnExit.Name = "btnExit"
        btnExit.Size = New Size(116, 48)
        btnExit.TabIndex = 0
        btnExit.Text = "종 료"
        btnExit.UseVisualStyleBackColor = False
        ' 
        ' btnEnv
        ' 
        btnEnv.BackColor = Color.MidnightBlue
        btnEnv.Font = New Font("맑은 고딕", 16F, FontStyle.Bold)
        btnEnv.ForeColor = Color.White
        btnEnv.Location = New Point(9, 13)
        btnEnv.Name = "btnEnv"
        btnEnv.Size = New Size(116, 48)
        btnEnv.TabIndex = 0
        btnEnv.Text = "환경설정"
        btnEnv.UseVisualStyleBackColor = False
        btnEnv.Visible = False
        ' 
        ' WebView22
        ' 
        WebView22.AllowExternalDrop = True
        WebView22.BackColor = Color.FromArgb(CByte(45), CByte(45), CByte(48))
        WebView22.CreationProperties = Nothing
        WebView22.DefaultBackgroundColor = Color.White
        WebView22.ForeColor = Color.White
        WebView22.Location = New Point(555, 58)
        WebView22.Name = "WebView22"
        WebView22.Size = New Size(116, 43)
        WebView22.TabIndex = 4
        WebView22.Visible = False
        WebView22.ZoomFactor = 1R
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(45), CByte(45), CByte(48))
        ClientSize = New Size(980, 657)
        Controls.Add(WebView22)
        Controls.Add(WebView21)
        Controls.Add(pnlMain)
        Controls.Add(pnlGolfDemon)
        Controls.Add(pnlGolfWebView)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        Name = "Form1"
        StartPosition = FormStartPosition.CenterScreen
        Text = "ArgosAPT Demon"
        CType(WebView21, ComponentModel.ISupportInitialize).EndInit()
        pnlGolfWebView.ResumeLayout(False)
        pnlGolfWebView.PerformLayout()
        pnlGolfDemon.ResumeLayout(False)
        pnlGolfDemon.PerformLayout()
        pnlMain.ResumeLayout(False)
        CType(WebView22, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents WebView21 As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents pnlGolfWebView As Panel
    Friend WithEvents pnlGolfDemon As Panel
    Friend WithEvents pnlMain As Panel
    Friend WithEvents btnEnv As Button
    Friend WithEvents btnExit As Button
    Friend WithEvents txtGolfDemonDisp As TextBox
    Friend WithEvents txtGolfWebView As TextBox
    Friend WithEvents WebView22 As Microsoft.Web.WebView2.WinForms.WebView2

End Class
