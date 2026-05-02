using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.SpeechSynthesis;

namespace VietTTS;

public partial class Form1 : Form
{
    private const string VietnameseBasicCapability = "Language.Basic~~~vi-VN~0.0.1.0";
    private const string VietnameseTtsCapability = "Language.TextToSpeech~~~vi-VN~0.0.1.0";

    private readonly SpeechSynthesizer synthesizer = new();
    private readonly List<VoiceInformation> vietnameseVoices = new();
    private SoundPlayer currentPlayer;
    private bool isPlaying;
    private int playbackSession;

    public Form1()
    {
        InitializeComponent();
        LoadVoices();
    }

    private void LoadVoices()
    {
        cmbVoices.Items.Clear();
        vietnameseVoices.Clear();

        vietnameseVoices.AddRange(SpeechSynthesizer.AllVoices
            .Where(IsVietnameseVoice)
            .OrderByDescending(voice => ContainsIgnoreCase(voice.DisplayName, "An"))
            .ThenBy(voice => voice.DisplayName));

        foreach (VoiceInformation voice in vietnameseVoices)
        {
            cmbVoices.Items.Add($"{voice.DisplayName} ({voice.Language})");
        }

        if (vietnameseVoices.Count > 0)
        {
            int anIndex = vietnameseVoices.FindIndex(voice => ContainsIgnoreCase(voice.DisplayName, "An"));
            cmbVoices.SelectedIndex = anIndex >= 0 ? anIndex : 0;
            lblStatus.Text = "Đã tìm thấy giọng tiếng Việt";
            lblStatus.ForeColor = Color.Green;
            btnInstall.Visible = false;
            SetSpeechControlsEnabled(true);
            return;
        }

        lblStatus.Text = "Chưa có giọng tiếng Việt. Bấm 'Tự cài giọng Việt'";
        lblStatus.ForeColor = Color.Red;
        btnInstall.Visible = true;
        SetSpeechControlsEnabled(false);
    }

    private static bool IsVietnameseVoice(VoiceInformation voice)
    {
        return voice.Language.Equals("vi-VN", StringComparison.OrdinalIgnoreCase)
            || ContainsIgnoreCase(voice.DisplayName, "Microsoft An")
            || ContainsIgnoreCase(voice.DisplayName, "Vietnamese")
            || ContainsIgnoreCase(voice.Description, "Vietnamese");
    }

    private static bool ContainsIgnoreCase(string value, string text)
    {
        return value != null && value.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void SetSpeechControlsEnabled(bool enabled)
    {
        cmbVoices.Enabled = enabled;
        btnSpeak.Enabled = enabled;
        btnSave.Enabled = enabled;
    }

    private bool TrySelectVietnameseVoice()
    {
        if (cmbVoices.SelectedIndex < 0 || cmbVoices.SelectedIndex >= vietnameseVoices.Count)
        {
            MessageBox.Show(
                "Máy chưa có giọng nói tiếng Việt. Hãy bấm 'Tự cài giọng Việt' để Windows tải và cài Microsoft An.",
                "Thiếu giọng tiếng Việt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        synthesizer.Voice = vietnameseVoices[cmbVoices.SelectedIndex];
        synthesizer.Options.SpeakingRate = Clamp(1.0 + (trackBarSpeed.Value * 0.1), 0.5, 2.0);
        return true;
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private async Task SaveSpeechToFileAsync(string text, string filePath)
    {
        using SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync(text);
        using Stream input = stream.AsStreamForRead();
        using FileStream output = File.Create(filePath);
        await input.CopyToAsync(output);
    }

    private async void btnSpeak_Click(object sender, EventArgs e)
    {
        if (isPlaying)
        {
            StopCurrentPlayback();
            lblStatus.Text = "Đã dừng";
            return;
        }

        await SpeakTextAsync();
    }

    private async Task SpeakTextAsync()
    {
        if (string.IsNullOrWhiteSpace(txtText.Text))
        {
            MessageBox.Show("Vui lòng nhập văn bản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            StopCurrentPlayback();
            if (!TrySelectVietnameseVoice()) return;

            lblStatus.Text = "Đang tạo âm thanh...";
            btnSpeak.Enabled = false;
            btnSave.Enabled = false;
            cmbVoices.Enabled = false;

            string tempFile = Path.Combine(Path.GetTempPath(), $"viettts_{Guid.NewGuid()}.wav");
            await SaveSpeechToFileAsync(txtText.Text, tempFile);
            await PlayTempFileAsync(tempFile);
        }
        catch (Exception ex)
        {
            ResetPlaybackButton();
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!isPlaying)
            {
                SetSpeechControlsEnabled(vietnameseVoices.Count > 0);
            }
        }
    }

    private async Task PlayTempFileAsync(string tempFile)
    {
        int session = ++playbackSession;
        currentPlayer = new SoundPlayer(tempFile);
        isPlaying = true;
        btnSpeak.Enabled = true;
        btnSpeak.Text = "Dừng";
        lblStatus.Text = "Đang phát...";

        try
        {
            await Task.Run(() => currentPlayer.PlaySync());
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }

            if (session == playbackSession)
            {
                StopCurrentPlayback(resetStatus: false);
                lblStatus.Text = "Xong";
            }
        }
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtText.Text))
        {
            MessageBox.Show("Vui lòng nhập văn bản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using SaveFileDialog sfd = new();
        sfd.Filter = "WAV Audio (*.wav)|*.wav";
        sfd.FileName = "tts_output.wav";

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            StopCurrentPlayback();
            if (!TrySelectVietnameseVoice()) return;

            lblStatus.Text = "Đang lưu file...";
            SetSpeechControlsEnabled(false);
            await SaveSpeechToFileAsync(txtText.Text, sfd.FileName);

            lblStatus.Text = $"Đã lưu: {sfd.FileName}";
            lblStatus.ForeColor = Color.Green;

            if (MessageBox.Show("Đã lưu file âm thanh. Bạn có muốn mở file để nghe thử không?", "Mở file âm thanh", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                OpenWithWindows(sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetSpeechControlsEnabled(vietnameseVoices.Count > 0);
        }
    }

    private static void OpenWithWindows(string filePath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });
    }

    private async void btnInstall_Click(object sender, EventArgs e)
    {
        DialogResult confirm = MessageBox.Show(
            "Ứng dụng sẽ yêu cầu quyền Administrator để Windows tải và cài giọng Microsoft An.\n\n" +
            "Quá trình này cần Windows Update/mạng Internet và có thể mất vài phút. Tiếp tục?",
            "Tự cài giọng tiếng Việt",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        try
        {
            btnInstall.Enabled = false;
            lblStatus.Text = "Đang chờ Windows cài giọng tiếng Việt...";
            lblStatus.ForeColor = Color.DarkOrange;

            int exitCode = await Task.Run(InstallVietnameseVoice);

            if (exitCode == 0)
            {
                LoadVoices();

                if (vietnameseVoices.Count > 0)
                {
                    MessageBox.Show("Đã cài và nhận diện giọng tiếng Việt.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Windows báo cài xong nhưng ứng dụng chưa thấy giọng mới. Hãy đóng và mở lại ứng dụng.",
                        "Cần mở lại ứng dụng",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            else
            {
                lblStatus.Text = "Cài giọng tiếng Việt chưa thành công";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show(
                    $"Windows trả về mã lỗi {exitCode}. Hãy kiểm tra Windows Update hoặc thử chạy lại với mạng Internet ổn định.",
                    "Không cài được giọng tiếng Việt",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            lblStatus.Text = "Đã hủy cài đặt";
            lblStatus.ForeColor = Color.Red;
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Cài giọng tiếng Việt chưa thành công";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnInstall.Enabled = true;
        }
    }

    private static int InstallVietnameseVoice()
    {
        string command =
            "$ErrorActionPreference = 'Stop'; " +
            $"Add-WindowsCapability -Online -Name '{VietnameseBasicCapability}'; " +
            $"Add-WindowsCapability -Online -Name '{VietnameseTtsCapability}'";

        using Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Normal
        }) ?? throw new InvalidOperationException("Không mở được PowerShell để cài giọng tiếng Việt.");

        process.WaitForExit();
        return process.ExitCode;
    }

    private void trackBarSpeed_Scroll(object sender, EventArgs e)
    {
        lblSpeed.Text = $"Tốc độ: {trackBarSpeed.Value}";
    }

    private void StopCurrentPlayback(bool resetStatus = true)
    {
        playbackSession++;
        currentPlayer?.Stop();
        currentPlayer?.Dispose();
        currentPlayer = null;
        isPlaying = false;
        ResetPlaybackButton();

        if (resetStatus)
        {
            SetSpeechControlsEnabled(vietnameseVoices.Count > 0);
        }
    }

    private void ResetPlaybackButton()
    {
        btnSpeak.Text = "Phát";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopCurrentPlayback(resetStatus: false);
        synthesizer.Dispose();
        base.OnFormClosing(e);
    }
}
