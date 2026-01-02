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
    Private iniLastWriteTime As DateTime = DateTime.MinValue
    Private isDailyRefreshDone As Boolean = False
    Private WithEvents tmrLog1 As New System.Windows.Forms.Timer()
    Private WithEvents tmrLog2 As New System.Windows.Forms.Timer()
    Private rnd As New Random()

    Private systemWords As New List(Of String) From {
        "Initializing kernel...",
        "System.Core.dll loaded.",
        "Authenticating user...",
        "Access granted.",
        "Compiling shader module...",
        "Memory allocation: 0x7FFD... OK",
        "Connecting to secure node...", "Firewall rule updated.", "Packet sent... ACK received.",
        "Decrypting data block...", "Running diagnostics...", "CPU usage: normal",
        "Disk I/O check... PASS", "Syncing with NTP server...", "Host resolved.",
        "Establishing TLSv1.3 connection...", "Handshake complete.", "Daemon process started.",
        "Querying database...", "Record found.", "Closing connection.", "Buffer overflow detected... patched.",
        "데이터베이스 연결 시도: MSSQL Server...", "마스터 데이터베이스 연결 성공.",
        "Executing query : 1개 행 반환.",
        "Transaction started.",
        "트랜잭션 시작.",
        "Commit complete.",
        "교착 상태 감지. 자동 롤백 실행.",
        "Index 재구성 작업 완료.",
        "데이터 수신: INFRARED_02 ACTIVATED"
    }


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
    Private GongDongCallUrl As String
    Private lastCallDate_GongDong As DateTime = DateTime.MinValue ' 공동 과금 호출 일자를 기록

    ' 강좌 자동연장 호출 상수 및 변수 (2026-01-02 15:42:00 코드 작성 시작)
    Private Const GANGJWA_CALL_HOUR As Integer = 1 ' 오전 1시
    Private Const GANGJWA_CALL_MINUTE As Integer = 0 ' 0분
    Private Const GANGJWA_CALL_SECOND As Integer = 0 ' 0초
    Private GangJwaCallUrl As String
    Private lastCallDate_GangJwa As DateTime = DateTime.MinValue ' 강좌 자동연장 호출 일자를 기록
    ' 강좌 자동연장 호출 상수 및 변수 (2026-01-02 15:42:00 코드 작성 종료)

    ' 사물함 자동연장 호출 상수 및 변수 (2026-01-02 18:05:10 코드 작성 시작)
    Private Const SAMULHAM_CALL_HOUR As Integer = 1 ' 오전 1시
    Private Const SAMULHAM_CALL_MINUTE As Integer = 0 ' 0분
    Private Const SAMULHAM_CALL_SECOND As Integer = 0 ' 0초
    Private SamulhamCallUrl As String
    Private lastCallDate_Samulham As DateTime = DateTime.MinValue ' 사물함 자동연장 호출 일자를 기록
    ' 사물함 자동연장 호출 상수 및 변수 (2026-01-02 18:05:10 코드 작성 종료)

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' 폼 설정
        Me.Text = "ArgosAPT Demon Server"
        '  Me.Size = New System.Drawing.Size(1024, 768)

        If Config_Load() = False Then
            Me.Close()
            Return
        End If

        If System.IO.File.Exists(gAppPath) Then
            iniLastWriteTime = System.IO.File.GetLastWriteTime(gAppPath)
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
        TmrFormLoad()

    End Sub
    Private Sub TmrFormLoad()

        ' 첫 번째 타이머 설정
        tmrLog1.Interval = rnd.Next(2000, 5000)  ' 2초~5초사이
        tmrLog1.Start()

        ' 두 번째 타이머 설정
        tmrLog2.Interval = rnd.Next(3000, 6000)  ' 3초~6초사이 
        tmrLog2.Start()

    End Sub
    ' 첫 번째 타이머 이벤트: 랜덤 단어 표시
    Private Sub tmrLog1_Tick(sender As Object, e As EventArgs) Handles tmrLog1.Tick
        ' 랜덤 단어 선택
        Dim randomIndex As Integer = rnd.Next(0, systemWords.Count)
        Dim randomWord As String = systemWords(randomIndex)

        ' 텍스트 박스에 추가
        AppendLog(txtGolfDemonDisp, randomWord)

        ' 다음 실행 간격을 다시 랜덤으로 설정
        tmrLog1.Interval = rnd.Next(2000, 5000)
    End Sub
    ' 두 번째 타이머 이벤트: 랜덤 단어 표시
    Private Sub tmrLog2_Tick(sender As Object, e As EventArgs) Handles tmrLog2.Tick
        ' 랜덤 단어 선택
        Dim randomIndex As Integer = rnd.Next(0, systemWords.Count)
        Dim randomWord As String = systemWords(randomIndex)

        ' 텍스트 박스에 추가
        AppendLog(txtGolfWebView, randomWord)

        ' 다음 실행 간격을 다시 랜덤으로 설정
        tmrLog2.Interval = rnd.Next(3000, 6000)
    End Sub
    ' 텍스트 박스에 로그를 추가하는 공통 함수
    Private Sub AppendLog(textBox As TextBox, message As String)
        ' 타임스탬프와 함께 메시지 추가
        Dim logEntry As String = $"[{DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}{Environment.NewLine}"
        textBox.AppendText(logEntry)

        If textBox.Lines.Length > 200 Then  ' 텍스트박스에 200라인 이상이면 50라인을 삭제
            Dim newLines As String() = textBox.Lines.Skip(50).ToArray()
            textBox.Text = String.Join(Environment.NewLine, newLines)
        End If

        ' 스크롤을 항상 맨 아래로 이동
        textBox.SelectionStart = textBox.Text.Length
        textBox.ScrollToCaret()   ' 스크롤을 맨 아래로 이동
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
    Private Function Config_Load(Optional isSilent As Boolean = False) As Boolean
        Try
            gAppPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), INI_FILENAME)

            ' INI 값 읽기
            gTime = GetIni("Settings", "Time", gAppPath)
            gGongDongLastCallDate = GetIni("Settings", "LastGongDongCallDate", gAppPath)
            gUrl = GetIni("Settings", "Url", gAppPath)

            ' URL 설정
            If gUrl <> "" Then
                GongDongCallUrl = $"{gUrl}api/GongDongGwaGeum/GwaGeumAuto/GwaGeumAuto.asp"
            Else
                GongDongCallUrl = ""
            End If

            ' 마지막 호출 날짜 로드
            If DateTime.TryParse(gGongDongLastCallDate, lastCallDate_GongDong) Then
                WriteLog($"[Config] 마지막 공동 과금 호출 일자 로드됨: {lastCallDate_GongDong.ToShortDateString()}", "Log", "GongDongLog")
                WriteLogDisp(txtGolfWebView, $"[Config] 마지막 공동 과금 호출 일자 로드됨: {lastCallDate_GongDong.ToShortDateString()}", True, "GolfWebView.log")
            Else
                lastCallDate_GongDong = DateTime.MinValue
            End If

            ' 강좌 자동연장 설정 로드 (2026-01-02 15:42:00 코드 작성 시작)
            gGangJwaLastCallDate = GetIni("Settings", "LastGangJwaCallDate", gAppPath)
            GangJwaCallUrl = $"{gUrl}api/CS/AutoExtend/GangjwaAutoExtend.asp"

            If DateTime.TryParse(gGangJwaLastCallDate, lastCallDate_GangJwa) Then
                WriteLog($"[Config] 마지막 강좌 자동연장 호출 일자 로드됨: {lastCallDate_GangJwa.ToShortDateString()}", "Log", "GangJwaLog")
            Else
                lastCallDate_GangJwa = DateTime.MinValue
            End If
            ' 강좌 자동연장 설정 로드 (2026-01-02 15:42:00 코드 작성 종료)

            ' 사물함 자동연장 설정 로드 (2026-01-02 18:05:10 코드 작성 시작)
            gSamulhamLastCallDate = GetIni("Settings", "LastSamulhamCallDate", gAppPath)
            SamulhamCallUrl = $"{gUrl}api/CS/AutoExtend/SamulhamAutoExtend.asp"

            If DateTime.TryParse(gSamulhamLastCallDate, lastCallDate_Samulham) Then
                WriteLog($"[Config] 마지막 사물함 자동연장 호출 일자 로드됨: {lastCallDate_Samulham.ToShortDateString()}", "Log", "SamulhamLog")
            Else
                lastCallDate_Samulham = DateTime.MinValue
            End If
            ' 사물함 자동연장 설정 로드 (2026-01-02 18:05:10 코드 작성 종료)

            ' 시간 설정 확인 
            If String.IsNullOrWhiteSpace(gTime) Then
                MessageBox.Show("시간 설정이 비어있습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
                WriteLogDisp(txtGolfWebView, "시간 설정이 비어있습니다.", True, "GolfWebView.log")
                Return False
            End If

            Dim timeParts() As String = gTime.Split(":")

            ' 파싱 시도 및 전역 변수 할당
            If timeParts.Length = 3 AndAlso
                Integer.TryParse(timeParts(0), targetHour) AndAlso
                Integer.TryParse(timeParts(1), targetMinute) AndAlso
                Integer.TryParse(timeParts(2), targetSecond) Then
                Return True
            Else
                MessageBox.Show("시간 값의 형식이 잘못되었습니다. 'HH:mm:ss' 형식으로 입력해주세요. ex) 09:00:01", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
                WriteLogDisp(txtGolfWebView, "시간 값의 형식이 잘못되었습니다. 'HH:mm:ss' 형식으로 입력해주세요. ex) 09:00:01", True, "GolfWebView.log")
                Return False
            End If

        Catch ex As Exception
            MessageBox.Show("설정 파일을 읽는 중 오류가 발생했습니다: " & ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
            WriteLogDisp(txtGolfWebView, "설정 파일을 읽는 중 오류가 발생했습니다: " & ex.Message, True, "GolfWebView.log")
            Return False
        End Try
    End Function

    ' 스케줄된 작업을 처리하는 타이머 콜백 메서드
    Private Async Sub CheckAllScheduledTasks(state As Object)
        Dim nowTime As DateTime = DateTime.Now

        ' 설정파일 변경 감지하는 부분
        Try
            Dim currentLastWrite As DateTime = System.IO.File.GetLastWriteTime(gAppPath)
            ' 파일이 수정되었다면
            If currentLastWrite > iniLastWriteTime Then
                iniLastWriteTime = currentLastWrite
                WriteLog("설정 파일 변경 감지됨. 재로드 시도...", "Log", "SystemLog")
                Me.Invoke(Sub()
                              Config_Load(True)
                          End Sub)
            End If
        Catch ex As Exception
        End Try


        ' Golf WebView 새로고침 
        If nowTime.Hour = targetHour AndAlso nowTime.Minute = targetMinute AndAlso nowTime.Second = targetSecond Then
            ' 1초 안에 여러 번 호출되는 것을 방지하기 위해 플래그로 판단
            If Not isDailyRefreshDone Then
                WriteLog("Golf WebView 새로고침 시간 호출 조건 충족", "Log", "TaseokLog")

                Me.Invoke(Async Sub()
                              If WebView21 IsNot Nothing AndAlso WebView21.CoreWebView2 IsNot Nothing Then
                                  WebView21.Source = New Uri($"{gUrl}api/GolfTaseokRealTime")
                                  Dim jsCode As String = $"$.fnDefaultSetting();"
                                  Await WebView21.ExecuteScriptAsync(jsCode)
                                  WriteLog("Golf WebView 웹페이지 호출 명령 실행!!", "Log", "TaseokLog")
                              Else
                                  WriteLog("Golf WebView WebView2 인스턴스가 유효하지 않아 호출 불가.", "Error", "TaseokLog")
                              End If
                          End Sub)

                isDailyRefreshDone = True
            End If
        Else
            isDailyRefreshDone = False
        End If




        ' 매월 1일 공동 과금 자동연장 API 호출
        Dim targetGongDongTime As New DateTime(nowTime.Year, nowTime.Month, 1, GONGDONG_CALL_HOUR, GONGDONG_CALL_MINUTE, GONGDONG_CALL_SECOND)
        If (lastCallDate_GongDong.Year <> nowTime.Year OrElse lastCallDate_GongDong.Month <> nowTime.Month) AndAlso
            nowTime >= targetGongDongTime Then

            lastCallDate_GongDong = nowTime.Date
            WriteLog($"[자동과금] 공동 과금 호출 조건 충족 실행). 목표: {targetGongDongTime}, 현재: {nowTime}", "Log", "GongDongLog")

            Await Task.Run(Async Sub()
                               Try
                                   Await CallGongDongGwaGeumApi()
                               Catch ex As Exception
                                   WriteLog($"공동과금 자동 연장 호출 중 오류: {ex.Message}", "Error", "GongDongLog")
                               End Try
                           End Sub)
        End If


        ' 매월 1일 강좌 자동연장 API 호출 (2026-01-02 15:42:00 코드 작성 시작)
        Dim targetGangJwaTime As New DateTime(nowTime.Year, nowTime.Month, 1, GANGJWA_CALL_HOUR, GANGJWA_CALL_MINUTE, GANGJWA_CALL_SECOND)
        If (lastCallDate_GangJwa.Year <> nowTime.Year OrElse lastCallDate_GangJwa.Month <> nowTime.Month) AndAlso
            nowTime >= targetGangJwaTime Then

            lastCallDate_GangJwa = nowTime.Date
            WriteLog($"[강좌자동연장] 호출 조건 충족 실행. 목표: {targetGangJwaTime}, 현재: {nowTime}", "Log", "GangJwaLog")

            Await Task.Run(Async Sub()
                               Try
                                   Await CallGangJwaAutoExtensionApi()
                               Catch ex As Exception
                                   WriteLog($"강좌 자동 연장 호출 중 오류: {ex.Message}", "Error", "GangJwaLog")
                               End Try
                           End Sub)
        End If
        ' 매월 1일 강좌 자동연장 API 호출 (2026-01-02 15:42:00 코드 작성 종료)


        ' 매월 1일 사물함 자동연장 API 호출 (2026-01-02 18:05:10 코드 작성 시작)
        Dim targetSamulhamTime As New DateTime(nowTime.Year, nowTime.Month, 1, SAMULHAM_CALL_HOUR, SAMULHAM_CALL_MINUTE, SAMULHAM_CALL_SECOND)
        If (lastCallDate_Samulham.Year <> nowTime.Year OrElse lastCallDate_Samulham.Month <> nowTime.Month) AndAlso
            nowTime >= targetSamulhamTime Then

            lastCallDate_Samulham = nowTime.Date
            WriteLog($"[사물함자동연장] 호출 조건 충족 실행. 목표: {targetSamulhamTime}, 현재: {nowTime}", "Log", "SamulhamLog")

            Await Task.Run(Async Sub()
                               Try
                                   Await CallSamulhamAutoExtensionApi()
                               Catch ex As Exception
                                   WriteLog($"사물함 자동 연장 호출 중 오류: {ex.Message}", "Error", "SamulhamLog")
                               End Try
                           End Sub)
        End If
        ' 매월 1일 사물함 자동연장 API 호출 (2026-01-02 18:05:10 코드 작성 종료)


    End Sub
    ' 공동 과금 API 호출 메서드
    Private Async Function CallGongDongGwaGeumApi() As Task
        Using client As New HttpClient()
            Try
                Dim response As HttpResponseMessage = Await client.GetAsync(GongDongCallUrl)
                response.EnsureSuccessStatusCode()

                Dim responseBody As String = Await response.Content.ReadAsStringAsync()

                WriteLog($"[GongDong API] 응답 수신: {responseBody}", "Log", "GongDongLog")

                Dim cleanedResponse As String = responseBody.Replace("rw ", "").Trim()

                ' 파싱작업..
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

    ' 강좌 자동연장 API 호출 메서드 (2026-01-02 15:42:00 코드 작성 시작)
    Private Async Function CallGangJwaAutoExtensionApi() As Task
        Using client As New HttpClient()
            Try
                ' URL이 유효한지 체크 (Base URL이 없으면 실행 불가)
                If String.IsNullOrEmpty(gUrl) Then
                    WriteLog("[GangJwa API] Base URL이 설정되지 않아 호출을 건너뜁니다.", "Error", "GangJwaLog")
                    Return
                End If

                Dim queryDate As String = DateTime.Now.ToString("yyyy-MM-dd")
                Dim finalUrl As String = $"{GangJwaCallUrl}?yyyy_mm_dd={queryDate}"

                WriteLog($"[GangJwa API] 호출 시도: {finalUrl}", "Log", "GangJwaLog")

                Dim response As HttpResponseMessage = Await client.GetAsync(finalUrl)
                response.EnsureSuccessStatusCode()

                Dim responseBody As String = Await response.Content.ReadAsStringAsync()

                Dim cleanedResponse As String = responseBody.Trim()
                WriteLog($"[GangJwa API] 응답 수신: {cleanedResponse}", "Log", "GangJwaLog")

                'json 파싱 
                Try
                    Dim jsonObject As Object = Newtonsoft.Json.JsonConvert.DeserializeObject(cleanedResponse)

                    Dim intResult As String = If(jsonObject?("intResult") IsNot Nothing, jsonObject("intResult").ToString(), "N/A")
                    Dim strResult As String = If(jsonObject?("strResult") IsNot Nothing, jsonObject("strResult").ToString(), "N/A")

                    WriteLog($"[GangJwa API] 파싱 결과 - intResult: {intResult}, strResult: {strResult}", "Log", "GangJwaLog")
                Catch ex As Exception
                    WriteLog($"[GangJwa API] JSON 파싱 중 오류: {ex.Message}", "Error", "GangJwaLog")
                End Try

                lastCallDate_GangJwa = DateTime.Now.Date ' 성공적으로 호출했음을 기록
                PutIni("Settings", "LastGangJwaCallDate", lastCallDate_GangJwa.ToShortDateString(), gAppPath)  ' 호출 일자를 INI 파일에 저장

            Catch ex As HttpRequestException
                WriteLog($"[GangJwa API] HTTP 요청 오류 발생: {ex.Message} (상태 코드: {ex.StatusCode})", "Error", "GangJwaLog")
            Catch ex As Exception
                WriteLog($"[GangJwa API] 호출 중 예상치 못한 오류 발생: {ex.Message}", "Error", "GangJwaLog")
            End Try
        End Using
    End Function
    ' 강좌 자동연장 API 호출 메서드 (2026-01-02 15:42:00 코드 작성 종료)

    ' 사물함 자동연장 API 호출 메서드 (2026-01-02 18:05:10 코드 작성 시작)
    Private Async Function CallSamulhamAutoExtensionApi() As Task
        Using client As New HttpClient()
            Try
                ' URL이 유효한지 체크 (Base URL이 없으면 실행 불가)
                If String.IsNullOrEmpty(gUrl) Then
                    WriteLog("[Samulham API] Base URL이 설정되지 않아 호출을 건너뜁니다.", "Error", "SamulhamLog")
                    Return
                End If

                Dim queryDate As String = DateTime.Now.ToString("yyyy-MM-dd")
                Dim finalUrl As String = $"{SamulhamCallUrl}?yyyy_mm_dd={queryDate}"

                WriteLog($"[Samulham API] 호출 시도: {finalUrl}", "Log", "SamulhamLog")

                Dim response As HttpResponseMessage = Await client.GetAsync(finalUrl)
                response.EnsureSuccessStatusCode()

                Dim responseBody As String = Await response.Content.ReadAsStringAsync()

                ' 공백 등 정리
                Dim cleanedResponse As String = responseBody.Trim()
                WriteLog($"[Samulham API] 응답 수신: {cleanedResponse}", "Log", "SamulhamLog")

                ' JSON 파싱
                Try
                    Dim jsonObject As Object = Newtonsoft.Json.JsonConvert.DeserializeObject(cleanedResponse)

                    Dim intResult As String = If(jsonObject?("intResult") IsNot Nothing, jsonObject("intResult").ToString(), "N/A")
                    Dim strResult As String = If(jsonObject?("strResult") IsNot Nothing, jsonObject("strResult").ToString(), "N/A")

                    WriteLog($"[Samulham API] 파싱 결과 - intResult: {intResult}, strResult: {strResult}", "Log", "SamulhamLog")
                Catch ex As Exception
                    WriteLog($"[Samulham API] JSON 파싱 중 오류: {ex.Message}", "Error", "SamulhamLog")
                End Try

                lastCallDate_Samulham = DateTime.Now.Date ' 성공적으로 호출했음을 기록
                PutIni("Settings", "LastSamulhamCallDate", lastCallDate_Samulham.ToShortDateString(), gAppPath)  ' 호출 일자를 INI 파일에 저장

            Catch ex As HttpRequestException
                WriteLog($"[Samulham API] HTTP 요청 오류 발생: {ex.Message} (상태 코드: {ex.StatusCode})", "Error", "SamulhamLog")
            Catch ex As Exception
                WriteLog($"[Samulham API] 호출 중 예상치 못한 오류 발생: {ex.Message}", "Error", "SamulhamLog")
            End Try
        End Using
    End Function
    ' 사물함 자동연장 API 호출 메서드 (2026-01-02 18:05:10 코드 작성 종료)

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
    Private Sub btnEnv_Click(sender As Object, e As EventArgs) Handles btnEnv.Click
        If AuthenticateWithPassword() = True Then
            Using frm As New frmEnv()
                frm.ShowDialog()
            End Using
        End If
    End Sub
    ''' <summary>
    ''' 로그를 지정된 텍스트 박스에 표시하고, 선택적으로 파일에 날짜별로 기록합니다.
    ''' </summary>
    ''' <param name="targetTextBox">로그를 표시할 TextBox 컨트롤입니다.</param>
    ''' <param name="message">기록할 로그 메시지입니다.</param>
    ''' <param name="writeToFile">로그를 파일에 기록할지 여부 (True/False)입니다.</param>
    ''' <param name="baseFileName">로그 파일의 기본 이름입니다. (예: "FingerAuth.log")</param>
    Private Sub WriteLogDisp(targetTextBox As TextBox, message As String, Optional writeToFile As Boolean = False, Optional baseFileName As String = "AppLog.log")
        ' 1. 타임스탬프가 포함된 전체 로그 메시지 생성
        Dim logEntry As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}"

        ' 2. 화면의 텍스트 박스에 로그 추가 (UI 스레드 접근 처리 포함)
        If targetTextBox.InvokeRequired Then
            targetTextBox.Invoke(New Action(Sub()
                                                targetTextBox.AppendText(logEntry & Environment.NewLine)
                                            End Sub))
        Else
            targetTextBox.AppendText(logEntry & Environment.NewLine)
        End If

        ' 3. 파일에 로그 기록 (writeToFile이 True일 경우)
        If writeToFile Then
            Try
                ' --- 파일명에 날짜를 추가하는 로직 ---
                Dim fileNameWithoutExt As String = IO.Path.GetFileNameWithoutExtension(baseFileName)
                Dim fileExtension As String = IO.Path.GetExtension(baseFileName)
                Dim currentDate As String = DateTime.Now.ToString("yyyy-MM-dd")
                Dim datedFileName As String = $"{fileNameWithoutExt}_{currentDate}{fileExtension}"

                Dim logDirectory As String = My.Application.Info.DirectoryPath
                Dim logFilePath As String = IO.Path.Combine(logDirectory, datedFileName)

                Using writer As New IO.StreamWriter(logFilePath, True)
                    writer.WriteLine(logEntry)
                End Using

            Catch ex As Exception
                ' 파일 쓰기 실패 시 화면에만 오류 로그를 남깁니다.
                Dim errorLog As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FILE LOGGING ERROR: {ex.Message}"
                If targetTextBox.InvokeRequired Then
                    targetTextBox.Invoke(New Action(Sub()
                                                        targetTextBox.AppendText(errorLog & Environment.NewLine)
                                                    End Sub))
                Else
                    targetTextBox.AppendText(errorLog & Environment.NewLine)
                End If
            End Try
        End If
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Me.Close()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        ' 이 코드는 사용자가 어떤 방식으로든 폼을 닫으려고 할 때 실행됩니다 (X 버튼 포함).
        ' 비밀번호 인증 함수를 호출해서 인증에 실패했다면(False),
        If AuthenticateWithPassword() = False Then
            ' 폼이 닫히는 이벤트를 취소합니다.
            e.Cancel = True
            Return
        End If
    End Sub

End Class