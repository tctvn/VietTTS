# VietTTS

VietTTS là ứng dụng WinForms nhỏ để chuyển văn bản tiếng Việt thành giọng nói bằng giọng Windows **Microsoft An**.

Ứng dụng target **.NET Framework 4.8**, phù hợp để chạy trên Windows 10/11 mà người dùng thường không cần cài thêm .NET Runtime riêng.

## Tính năng

- Chỉ dùng giọng tiếng Việt, ưu tiên **Microsoft An**.
- Đọc văn bản bằng một nút **Phát/Dừng**.
- Điều chỉnh tốc độ đọc.
- Lưu âm thanh ra file WAV.
- Sau khi lưu, có thể mở file bằng trình phát mặc định của Windows.
- Nếu máy chưa có giọng tiếng Việt, app có nút **Tự cài giọng Việt**.

## Yêu cầu khi chạy

- Windows 10 hoặc Windows 11.
- .NET Framework 4.8 có sẵn trên hầu hết máy Windows 10/11 đã cập nhật.
- Giọng tiếng Việt **Microsoft An**.

Nếu máy chưa có Microsoft An, bấm **Tự cài giọng Việt** trong app. Windows sẽ yêu cầu quyền Administrator và tải gói:

```powershell
Language.Basic~~~vi-VN~0.0.1.0
Language.TextToSpeech~~~vi-VN~0.0.1.0
```

Việc cài giọng cần Windows Update hoặc mạng Internet.

## Cách chạy

Mở file:

```text
VietTTS\bin\Debug\net48\VietTTS.exe
```

Khi phát hành cho người dùng, chỉ cần gửi thư mục chứa `VietTTS.exe` sau khi build Release.

## Build

Trên máy phát triển cần Visual Studio hoặc Build Tools có hỗ trợ .NET Framework 4.8 Developer Pack.

Build Debug:

```powershell
dotnet build VietTTS.csproj
```

Build Release:

```powershell
dotnet build VietTTS.csproj -c Release
```

File chạy Release nằm ở:

```text
bin\Release\net48\VietTTS.exe
```

## Ghi chú kỹ thuật

- App dùng `Windows.Media.SpeechSynthesis` để đọc được voice OneCore như **Microsoft An**.
- Không dùng `System.Speech`, vì API đó thường chỉ thấy các voice Desktop cũ như David/Zira và có thể không thấy Microsoft An.
- Project target `.NET Framework 4.8` để giảm yêu cầu cài thêm runtime cho người dùng Windows 10/11.
