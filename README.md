# 파일 업로드 도우미 (UploadHelper)

## 소개
**UploadHelper**는 Windows 환경에서 파일 업로드 및 관리 작업을 간편하게 도와주는 WPF 기반 데스크탑 애플리케이션입니다.  
드래그 앤 드롭, 복사/붙여넣기, 다국어, 테마, 투명도 등 다양한 편의 기능을 제공합니다.

## 주요 기능
- **파일 추가**: 파일을 드래그 앤 드롭하거나, `파일 추가` 버튼 또는 Ctrl+V(클립보드)로 파일을 리스트에 추가
- **파일 리스트 관리**: 전체 선택, 선택 삭제, 전체 삭제, 정렬, 이름 바꾸기(F2), 붙여넣기 지원
- **설정**: 테마(라이트/다크), 언어(한국어/영어/일본어/중국어), 투명도 조절
- **다국어 지원**: 한국어, 영어, 일본어, 중국어 UI 제공
- **아이콘 표시**: 각 파일의 아이콘 및 정보(이름, 크기, 경로) 표시
- **사용자 설정 저장**: 설정은 AppData 내 `settings.json`에 자동 저장

## 설치 및 실행 방법
1. **빌드 환경**
   - .NET 8.0 (Windows Desktop)
   - Visual Studio 2022 이상 권장

2. **빌드**
   - 솔루션 파일(`UploadHelper.sln`)을 Visual Studio로 열고 빌드합니다.

3. **실행**
   - 빌드 후 생성된 실행 파일(`UploadHelper.exe`)을 실행합니다.

## 폴더 구조
```
UploadHelper/
  ├─ App.xaml, App.xaml.cs         # 앱 진입점 및 리소스 관리
  ├─ MainWindow.xaml, .cs          # 메인 UI 및 로직
  ├─ SettingsWindow.xaml, .cs      # 설정 창
  ├─ Settings.cs                   # 사용자 설정 관리
  ├─ EmptyToVisibleConverter.cs    # 리스트 빈 상태 표시용 컨버터
  ├─ Resources/                    # 다국어 리소스, 아이콘
  ├─ Themes/                       # 라이트/다크 테마
  └─ UploadHelper.csproj           # 프로젝트 파일
```

## 주요 클래스 및 파일
- **MainWindow**: 파일 리스트 관리, 드래그 앤 드롭, 단축키, 파일 아이콘 표시 등 메인 기능 담당
- **SettingsWindow**: 테마, 언어, 투명도 등 사용자 설정 UI
- **Settings**: 설정 파일 로드/저장 및 언어/테마 정보 관리
- **EmptyToVisibleConverter**: 리스트가 비었을 때 안내 문구 표시

## 다국어 및 테마
- `Resources/Strings.ko-KR.xaml` 등에서 각 언어별 UI 문자열 관리
- `Themes/LightTheme.xaml`, `DarkTheme.xaml`에서 테마 리소스 관리

