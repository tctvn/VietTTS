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

Build Release (xuất 1 file EXE portable duy nhất):

```powershell
dotnet build VietTTS.csproj -c Release
```

Sau khi build, thư mục output chỉ còn **một file duy nhất**:

```text
bin\Release\net48\VietTTS.exe
```

Copy file này đến bất kỳ máy Windows 10/11 nào và chạy thẳng — không cần cài thêm gì.

## Đóng góp

Mọi đóng góp đều được chào đón! Bạn có thể tham gia bằng các cách sau:

- **Fork** repo này về tài khoản của bạn và tự do thử nghiệm, cải tiến.
- **Commit & Pull Request** — sau khi sửa hoặc thêm tính năng, mở Pull Request để tích hợp vào nhánh chính. Vui lòng mô tả rõ thay đổi trong PR.
- **Issues** — nếu gặp lỗi hoặc có ý tưởng mới, hãy [mở Issue](../../issues/new) để thảo luận trước khi code.

> Nếu đây là lần đầu bạn đóng góp mã nguồn mở, hãy tham khảo [hướng dẫn fork & pull request của GitHub](https://docs.github.com/en/get-started/quickstart/contributing-to-projects).

## Ghi chú kỹ thuật

- App dùng `Windows.Media.SpeechSynthesis` để đọc được voice OneCore như **Microsoft An**.
- Không dùng `System.Speech`, vì API đó thường chỉ thấy các voice Desktop cũ như David/Zira và có thể không thấy Microsoft An.
- Project target `.NET Framework 4.8` để giảm yêu cầu cài thêm runtime cho người dùng Windows 10/11.
