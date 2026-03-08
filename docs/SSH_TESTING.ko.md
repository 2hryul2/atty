# SSH 사용 및 테스트 가이드

## 목적

이 문서는 `Atty.Wpf`의 SSH 기능 사용 방법과 연결 문제를 단계적으로 점검하는 절차를 설명합니다.

## 앱 실행

```powershell
cd D:\source\Atty
dotnet run --project .\Atty.Wpf\Atty.Wpf.csproj
```

## 기본 SSH 사용 방법

1. 앱을 실행합니다.
2. 설정 화면에서 아래 값을 입력합니다.
   - `Host`
   - `Port`
   - `Username`
   - `Password`
3. `Open SSH` 또는 `Open AI Terminal`을 클릭합니다.
4. 연결 상태가 `Connected`로 바뀌는지 확인합니다.
5. 터미널 입력 영역에 명령을 입력하고 `Enter` 또는 `Send`를 누릅니다.
6. 세션 종료는 `Disconnect`를 사용합니다.

## 현재 기본 테스트 값

- Host: `127.0.0.1`
- Port: `22`
- Username: `sds`
- Password: 앱에서 직접 입력

## 권장 테스트 명령어

연결 후 아래 명령어로 먼저 확인합니다.

```text
whoami
pwd
hostname
ls -l
```

Windows OpenSSH 대상이면 아래도 사용할 수 있습니다.

```text
whoami
hostname
pwd
dir
```

## 단계별 연결 테스트

### 1. SSH 서버 서비스 확인

Windows:

```powershell
Get-Service sshd
```

기대 결과:
- 서비스 상태가 `Running`

### 2. 로컬 SSH 포트 확인

```powershell
Test-NetConnection localhost -Port 22
Test-NetConnection 127.0.0.1 -Port 22
```

기대 결과:
- `TcpTestSucceeded : True`

### 3. 대상 호스트의 포트 접근 확인

```powershell
Test-NetConnection <host> -Port 22
```

예:

```powershell
Test-NetConnection 127.0.0.1 -Port 22
Test-NetConnection 192.168.0.1 -Port 22
```

해석:
- `True`: SSH 포트까지 TCP 경로가 열려 있음
- `False`: 인증 전에 네트워크, 방화벽, 주소, 서비스 바인딩 문제 존재

### 4. 앱에서 로그인 테스트

포트 `22` 접근이 가능하면 같은 값으로 앱에서 접속을 시도합니다.

기대 결과:
- 상태가 `Connected`로 변경
- 대시보드 화면으로 전환
- 터미널 출력 표시

### 5. 명령 실행 테스트

아래 명령어를 실행합니다.

```text
whoami
```

기대 결과:
- 원격 셸이 로그인 사용자명을 반환

## 실패 사례와 의미

### `Connection failed to establish within 20000 milliseconds`

의미:
- 서버가 제한 시간 내 응답하지 않음
- 호스트 주소가 잘못되었을 수 있음
- `22` 포트가 차단되었을 수 있음
- 방화벽에서 차단 중일 수 있음
- 해당 인터페이스로 SSH 서비스가 열려 있지 않을 수 있음

### `TcpTestSucceeded : False`

의미:
- 문제는 사용자 인증 전 단계에 있음
- 보통 호스트 주소, 라우팅, 방화벽, SSH 포트 노출 문제

### 포트 22는 열려 있는데 로그인 실패

의미:
- SSH 서비스는 도달 가능
- 원인은 보통 아래 중 하나임
  - 잘못된 사용자명
  - 잘못된 비밀번호
  - 서버에서 password login 비허용
  - 계정 로그인 제한

## 문제 진단 체크리스트

1. 호스트 IP가 맞는지 확인
2. SSH 서비스가 실행 중인지 확인
3. `Test-NetConnection <host> -Port 22` 성공 여부 확인
4. 사용자명이 맞는지 확인
5. 비밀번호가 맞는지 확인
6. 서버가 password-based SSH login을 허용하는지 확인
7. 로컬/서버 방화벽에서 `22/tcp`를 막고 있지 않은지 확인

## 참고

- 현재 앱은 비밀번호 기반 SSH 로그인만 지원합니다.
- 개인키 인증은 아직 구현되지 않았습니다.
- AI Assistant 패널은 현재 시각적 보조 UI이며 실제 AI 동작은 하지 않습니다.
