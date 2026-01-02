Module modConst

    Public Const INI_FILENAME = "ArgosAPTDemonServer.ini"   ''ini 파일명
    Public Const SELECT_TIMEOUT = 60   ''조회시 기본 타임아웃 값 

    Public gAppPath As String
    Public gTime As String
    Public gGongDongLastCallDate As String
    ' 강좌 자동연장 전역변수 (2026-01-02 15:42:00 코드 작성 시작)
    Public gGangJwaLastCallDate As String
    ' 강좌 자동연장 전역변수 (2026-01-02 15:42:00 코드 작성 종료)
    ' 사물함 자동연장 전역변수 (2026-01-02 18:05:10 코드 작성 시작)
    Public gSamulhamLastCallDate As String
    ' 사물함 자동연장 전역변수 (2026-01-02 18:05:10 코드 작성 종료)
    Public gUrl As String


End Module
