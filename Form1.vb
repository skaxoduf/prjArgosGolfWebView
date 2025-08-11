Imports Microsoft.Web.WebView2.Core
Imports System.ComponentModel
Imports System.Threading
Imports System.IO ' Path 관련 기능을 위해 추가
Imports System.Net.Http ' HttpClient 사용을 위해 추가
Imports Newtonsoft.Json ' JSON 파싱을 위해 NuGet 패키지 추가 필요


Public Class Form1
    Private WithEvents trayIcon As New NotifyIcon()
    Private WithEvents trayMenu As New ContextMenuStrip()
    Private mainTimer As Timer

    ' 새로고침 설정값
    Private targetHour As Integer
    Private targetMinute As Integer
    Private targetSecond As Integer
    Private isExiting As Boolean = False
    Private lastRefreshDate As DateTime = DateTime.MinValue ' 마지막 새로고침 일자를 기록

    ' 매월 1일 자동 과금 호출 상수
    Private Const GONGDONG_CALL_HOUR As Integer = 1 ' 오전 1시
    Private Const GONGDONG_CALL_MINUTE As Integer = 0 ' 0분
    Private Const GONGDONG_CALL_SECOND As Integer = 0 ' 0초
    Private Const GONGDONG_CALL_URL As String = "http://julist.webpos.co.kr/api/GongDongGwaGeum/GwaGeumAuto/GwaGeumAuto.asp"
    Private lastCallDate_GongDong As DateTime = DateTime.MinValue ' 공동 과금 호출 일자를 기록

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' 폼 설정
        Me.Text = "Argos Golf WebView"
        '  Me.Size = New System.Drawing.Size(1024, 768)


        If Config_Load() = False Then
            Me.Close()
            Return
        End If


        If WebView21 IsNot Nothing Then
            ' WebView2 초기화
            Await WebView21.EnsureCoreWebView2Async(Nothing)
            ' NavigationCompleted 이벤트 핸들러 추가
            AddHandler WebView21.CoreWebView2.NavigationCompleted, AddressOf CoreWebView2_NavigationCompleted
            ' NavigationStarting : 페이지 로드 시도 로그 남기는용도
            AddHandler WebView21.CoreWebView2.NavigationStarting, AddressOf CoreWebView2_NavigationStarting
        Else
            WriteLog("[WebView2] WebView21 컨트롤을 찾을 수 없습니다. WebView2 기능 비활성화.", "Error", "TaseokLog")
        End If



        ' 프로그램 시작시 웹페이지 호출안함 
        'WebView21.Source = New Uri("http://julist.webpos.co.kr/api/GolfTaseokRealTime")

        ' 트레이 메뉴 설정
        trayMenu.Items.Add("화면보기", Nothing, AddressOf ShowWindow)
        trayMenu.Items.Add("종료", Nothing, AddressOf ExitApplication)

        ' 트레이 아이콘 설정
        trayIcon.Icon = Me.Icon
        trayIcon.Text = "Argos Golf WebView"
        trayIcon.Visible = True
        trayIcon.ContextMenuStrip = trayMenu ' 메뉴 연결

        ' 타이머 설정 (1초마다 체크)
        mainTimer = New Timer(AddressOf CheckAllScheduledTasks, Nothing, 0, 1000)

    End Sub
    Private Sub CoreWebView2_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs)
        ' WebView2가 무언가를 로드하려고 시도 중임을 알려줍니다.

        WriteLog($"[WebView2] 탐색 시작: {e.Uri}", "Log", "TaseokLog")
    End Sub

    Private Sub CoreWebView2_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)

        WriteLog($"[WebView2] NavigationCompleted 이벤트 시작. 현재 로드 시도 URL: {WebView21.Source.ToString()}", "Log", "TaseokLog")

        If e.IsSuccess Then
            WriteLog($"[WebView2] 웹페이지 로드 성공: {WebView21.Source.ToString()}", "Log", "TaseokLog")
        Else
            WriteLog($"[WebView2] 웹페이지 로드 실패: {WebView21.Source.ToString()}, 오류 상태: {e.WebErrorStatus}, HTTP 상태: {e.HttpStatusCode}", "Error", "TaseokLog")
            Me.Invoke(Sub()
                          MessageBox.Show($"웹페이지 로드 실패: {WebView21.Source.ToString()}{Environment.NewLine}오류 상태: {e.WebErrorStatus}{Environment.NewLine}HTTP 상태 코드: {e.HttpStatusCode}", "WebView2 오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                      End Sub)

        End If
    End Sub

    Private Function Config_Load() As Boolean
        Try
            gAppPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), INI_FILENAME)
            gTime = GetIni("Settings", "Time", gAppPath)
            gGongDongLastCallDate = GetIni("Settings", "LastGongDongCallDate", gAppPath)  ' 마지막 공동과금 처리일자 
            If DateTime.TryParse(gGongDongLastCallDate, lastCallDate_GongDong) Then ' 문자열을 DateTime으로 파싱하여 lastCallDate_GongDong에 할당
                WriteLog($"[Config] 마지막 공동 과금 호출 일자 로드됨: {lastCallDate_GongDong.ToShortDateString()}", "Log", "GongDongLog")
            Else
                lastCallDate_GongDong = DateTime.MinValue
            End If

            If String.IsNullOrWhiteSpace(gTime) Then
                MessageBox.Show("시간 설정이 잘못되었습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            Dim timeParts() As String = gTime.Split(":")
            If timeParts.Length = 3 AndAlso Integer.TryParse(timeParts(0), targetHour) AndAlso Integer.TryParse(timeParts(1), targetMinute) AndAlso Integer.TryParse(timeParts(2), targetSecond) Then
                Return True
            Else
                MessageBox.Show("시간 값의 형식이 잘못되었습니다. 'HH:mm:ss' 형식으로 입력해주세요. ex) 09:00:01", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
        Catch ex As Exception
            MessageBox.Show("설정 파일을 읽는 중 오류가 발생했습니다: " & ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' 스케줄된 작업을 처리하는 타이머 콜백 메서드
    Private Async Sub CheckAllScheduledTasks(state As Object)
        Dim now As DateTime = DateTime.Now
        Dim jsCode As String

        ' ini에서 변경시 프로그램 재실행 안하고도 바로 적용되게끔 
        gTime = GetIni("Settings", "Time", gAppPath)
        Dim timeParts() As String = gTime.Split(":")
        Integer.TryParse(timeParts(0), targetHour)
        Integer.TryParse(timeParts(1), targetMinute)
        Integer.TryParse(timeParts(2), targetSecond)


        ' 1. Golf WebView 새로고침
        If now.Hour = targetHour AndAlso now.Minute = targetMinute AndAlso now.Second = targetSecond Then

            WriteLog("Golf WebView 새로고침 시간 호출 조건 충족", "Log", "TaseokLog")

            Me.Invoke(Async Sub()
                          If WebView21 IsNot Nothing AndAlso WebView21.CoreWebView2 IsNot Nothing Then
                              WebView21.Source = New Uri("http://julist.webpos.co.kr/api/GolfTaseokRealTime") ' 웹페이지 호출
                              jsCode = $"$.fnDefaultSetting();"
                              Await WebView21.ExecuteScriptAsync(jsCode)  'ExecuteScriptAsync(...) 같은 함수는 Async/Await 필요
                              WriteLog("Golf WebView 웹페이지 호출 명령 실행!!", "Log", "TaseokLog")
                          Else
                              WriteLog("Golf WebView WebView2 인스턴스가 유효하지 않아 호출 불가.", "Error", "TaseokLog")
                          End If
                      End Sub)
        End If

        ' 2. 매월 1일 공동 과금 호출 기능 (새로운 로직)
        ' 현재 날짜가 1일이고, 목표 시간(오전 1시)이 되었으며,
        ' 마지막 호출 일자가 현재 월의 1일이 아니라면 (이번 달에 아직 호출 안 했다면)
        If now.Day = 1 AndAlso
           now.Hour >= GONGDONG_CALL_HOUR AndAlso
           (now.Hour > GONGDONG_CALL_HOUR OrElse now.Minute >= GONGDONG_CALL_MINUTE) AndAlso
           (now.Hour > GONGDONG_CALL_HOUR OrElse now.Minute > GONGDONG_CALL_MINUTE OrElse now.Second >= GONGDONG_CALL_SECOND) AndAlso
           (lastCallDate_GongDong.Year <> now.Year OrElse lastCallDate_GongDong.Month <> now.Month) Then ' 이번 달에 호출했는지 확인

            WriteLog($"공동 과금 호출 시간 조건 충족: {GONGDONG_CALL_URL}", "Log", "GongDongLog")

            ' 비동기 호출을 위해 Invoke 외부에서 Task.Run 사용
            Await Task.Run(Async Sub()
                               Await CallGongDongGwaGeumApi()
                           End Sub)

        End If
    End Sub
    ' 공동 과금 API 호출 메서드
    Private Async Function CallGongDongGwaGeumApi() As Task
        Using client As New HttpClient()
            Try
                Dim response As HttpResponseMessage = Await client.GetAsync(GONGDONG_CALL_URL)
                response.EnsureSuccessStatusCode()

                Dim responseBody As String = Await response.Content.ReadAsStringAsync()

                WriteLog($"[GongDong API] 응답 수신: {responseBody}", "Log", "GongDongLog")

                ' 응답 파싱: rw "{""rowcount"" : """ & ROWCOUNT & """}"
                ' JSON.NET 라이브러리 (Newtonsoft.Json) 사용
                ' 먼저 "rw " 부분을 제거
                Dim cleanedResponse As String = responseBody.Replace("rw ", "").Trim()

                ' 따옴표로 감싸진 JSON 문자열인지 확인 후 파싱
                If cleanedResponse.StartsWith("{") AndAlso cleanedResponse.EndsWith("}") Then
                    Dim jsonObject As Object = Newtonsoft.Json.JsonConvert.DeserializeObject(cleanedResponse)
                    Dim rowCount As String = If(jsonObject?("rowcount") IsNot Nothing, jsonObject("rowcount").ToString(), "N/A")
                    WriteLog($"[GongDong API] ROWCOUNT 파싱 성공: {rowCount}", "Log", "GongDongLog")
                Else
                    WriteLog($"[GongDong API] 응답 형식 오류 (JSON 아님): {cleanedResponse}", "Error", "GongDongLog")
                End If

                lastCallDate_GongDong = DateTime.Now.Date ' 성공적으로 호출했음을 기록
                PutIni("Settings", "LastGongDongCallDate", lastCallDate_GongDong.ToShortDateString(), gAppPath)  ' 호출 일자를 INI 파일에 저장

            Catch ex As HttpRequestException
                WriteLog($"[GongDong API] HTTP 요청 오류 발생: {ex.Message} (상태 코드: {ex.StatusCode})", "Error", "GongDongLog")
            Catch ex As Exception
                WriteLog($"[GongDong API] 호출 중 예상치 못한 오류 발생: {ex.Message}", "Error", "GongDongLog")
            End Try
        End Using
    End Function

    ' 폼 크기 변경 시 트레이로 이동
    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()
            trayIcon.Visible = True
        End If
    End Sub
    Private Sub TrayIcon_DoubleClick(sender As Object, e As EventArgs) Handles trayIcon.DoubleClick
        ShowWindow()
    End Sub
    Private Sub ShowWindow()
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        ' 트레이 상태일 때만 Visible을 false로 변경하는 것이 좋지만,
        ' 단순화를 위해 여기서는 보일 때마다 false 처리
    End Sub
    Private Sub ExitApplication()
        If MessageBox.Show("종료하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Me.Close()
        End If
    End Sub
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            If Not isExiting Then
                If MessageBox.Show("종료하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    isExiting = True
                    trayIcon.Dispose()
                    Application.Exit()
                Else
                    e.Cancel = True
                End If
            End If
        End If
    End Sub


End Class