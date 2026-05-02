using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.SpeechSynthesis;

namespace VietTTS;

public sealed class MainForm : Form
{
    // ── Win32 drag ────────────────────────────────────────────────────────────
    [DllImport("user32.dll")] static extern bool ReleaseCapture();
    [DllImport("user32.dll")] static extern int  SendMessage(IntPtr h, int msg, int w, int l);
    private const int WM_NCLBUTTONDOWN = 0xA1, HTCAPTION = 2;

    // ── TTS state ─────────────────────────────────────────────────────────────
    private const string CapBasic = "Language.Basic~~~vi-VN~0.0.1.0";
    private const string CapTts   = "Language.TextToSpeech~~~vi-VN~0.0.1.0";

    private readonly SpeechSynthesizer        synth  = new();
    private readonly List<VoiceInformation>   voices = new();
    private SoundPlayer _player;
    private bool        _playing;
    private int         _session;

    // ── Controls ──────────────────────────────────────────────────────────────
    private DarkTextBox  txtInput;
    private ComboBox     cmbVoice;
    private ThinSlider   sldSpeed;
    private PillButton   btnPlay;
    private PillButton   btnSave;
    private PillButton   btnInstall;
    private StatusBar    statusBar;
    private Label        lblSpeedVal;
    private Label        lblClose;

    public MainForm()
    {
        BuildUI();
        LoadVoices();
    }

    // ── UI Construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        // Form
        Text            = "VietTTS";
        ClientSize      = new Size(520, 520);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = Pal.Bg;
        ShowInTaskbar   = true;

        // Drag on form background
        MouseDown += DragForm;

        // ── Close button ─────────────────────────────────────────────
        lblClose = new Label
        {
            Text      = "✕",
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 100, 120),
            BackColor = Color.Transparent,
            AutoSize  = false,
            Size      = new Size(36, 36),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor    = Cursors.Hand,
            Location  = new Point(ClientSize.Width - 44, 12),
        };
        lblClose.MouseEnter += (s, e) => { lblClose.ForeColor = Color.White; };
        lblClose.MouseLeave += (s, e) => { lblClose.ForeColor = Color.FromArgb(100, 100, 120); };
        lblClose.Click      += (s, e) => Application.Exit();
        Controls.Add(lblClose);

        // ── Header ───────────────────────────────────────────────────
        var lblApp = new Label
        {
            Text      = "VietTTS",
            Font      = new Font("Segoe UI", 20f, FontStyle.Bold),
            ForeColor = Pal.TextPri,
            BackColor = Color.Transparent,
            AutoSize  = true,
            Location  = new Point(24, 18),
        };
        lblApp.MouseDown += DragForm;
        Controls.Add(lblApp);

        var lblSub = new Label
        {
            Text      = "Text-to-speech • Tiếng Việt",
            Font      = new Font("Segoe UI", 9f),
            ForeColor = Pal.TextSec,
            BackColor = Color.Transparent,
            AutoSize  = true,
            Location  = new Point(26, 50),
        };
        lblSub.MouseDown += DragForm;
        Controls.Add(lblSub);

        int y = 82;

        // ── Text input card ───────────────────────────────────────────
        var cardText = new CardPanel { Location = new Point(16, y), Size = new Size(488, 170), Radius = 16 };
        Controls.Add(cardText);

        var secText = new SectionLabel("VĂN BẢN") { Location = new Point(14, 10) };
        cardText.Controls.Add(secText);

        txtInput = new DarkTextBox
        {
            Location = new Point(10, 30),
            Size     = new Size(468, 128),
            Text     = "Xin chào! Đây là ứng dụng chuyển văn bản thành giọng nói tiếng Việt.",
        };
        cardText.Controls.Add(txtInput);
        y += 178;

        // ── Voice + Speed row ─────────────────────────────────────────
        var cardVS = new CardPanel { Location = new Point(16, y), Size = new Size(488, 90), Radius = 16 };
        Controls.Add(cardVS);

        var secVoice = new SectionLabel("GIỌNG ĐỌC") { Location = new Point(14, 10) };
        cardVS.Controls.Add(secVoice);

        cmbVoice = new ComboBox
        {
            DropDownStyle      = ComboBoxStyle.DropDownList,
            FormattingEnabled  = true,
            Location           = new Point(10, 28),
            Size               = new Size(228, 28),
            Font               = new Font("Segoe UI", 10f),
            FlatStyle          = FlatStyle.Flat,
            BackColor          = Pal.SurfaceHi,
            ForeColor          = Pal.TextPri,
        };
        cardVS.Controls.Add(cmbVoice);

        var secSpeed = new SectionLabel("TỐC ĐỘ") { Location = new Point(252, 10) };
        cardVS.Controls.Add(secSpeed);

        lblSpeedVal = new Label
        {
            Text      = "0",
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Pal.Blue,
            BackColor = Color.Transparent,
            AutoSize  = true,
            Location  = new Point(295, 10),
        };
        cardVS.Controls.Add(lblSpeedVal);

        sldSpeed = new ThinSlider
        {
            Location = new Point(248, 28),
            Size     = new Size(230, 34),
            BackColor = Pal.SurfaceHi,
        };
        sldSpeed.ValueChanged += (s, e) =>
        {
            lblSpeedVal.Text = sldSpeed.Value.ToString("+0;-0;0");
            lblSpeedVal.Left = secSpeed.Right + 4;
        };
        cardVS.Controls.Add(sldSpeed);
        y += 98;

        // ── Action buttons ────────────────────────────────────────────
        btnPlay = new PillButton
        {
            Text        = "▶  Phát",
            AccentColor = Pal.Blue,
            Location    = new Point(16, y),
            Size        = new Size(234, 52),
        };
        btnPlay.Click += BtnPlay_Click;
        Controls.Add(btnPlay);

        btnSave = new PillButton
        {
            Text        = "↓  Lưu WAV",
            AccentColor = Pal.Green,
            Location    = new Point(270, y),
            Size        = new Size(234, 52),
        };
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);
        y += 60;

        // ── Install button (hidden) ───────────────────────────────────
        btnInstall = new PillButton
        {
            Text        = "⚙  Máy chưa có giọng Việt — Bấm để tự cài",
            AccentColor = Pal.Orange,
            Location    = new Point(16, y),
            Size        = new Size(488, 48),
            Visible     = false,
        };
        btnInstall.Click += BtnInstall_Click;
        Controls.Add(btnInstall);
        y += 56;

        // ── Status bar ────────────────────────────────────────────────
        statusBar = new StatusBar
        {
            Location  = new Point(16, y),
            Size      = new Size(488, 36),
            BackColor = Pal.Surface,
        };
        Controls.Add(statusBar);
        y += 44;

        // Resize form to fit
        ClientSize = new Size(520, y + 16);
    }

    private void DragForm(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0); }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0) { base.OnPaintBackground(e); return; }
        var g  = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rc = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

        // Base gradient
        using (var br = new LinearGradientBrush(rc, Pal.Bg, Color.FromArgb(14, 12, 30), LinearGradientMode.Vertical))
            g.FillRectangle(br, rc);

        // Subtle orb top-left
        try
        {
            using var gp = new GraphicsPath();
            gp.AddEllipse(-80, -80, 320, 320);
            using var gb = new PathGradientBrush(gp);
            gb.CenterColor    = Color.FromArgb(35, 80, 50, 180);
            gb.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
            g.FillPath(gb, gp);
        }
        catch { }
    }

    // ── Voice loading ─────────────────────────────────────────────────────────
    private void LoadVoices()
    {
        cmbVoice.Items.Clear();
        voices.Clear();

        voices.AddRange(SpeechSynthesizer.AllVoices
            .Where(v => v.Language.Equals("vi-VN", StringComparison.OrdinalIgnoreCase)
                     || v.DisplayName.IndexOf("Microsoft An", StringComparison.OrdinalIgnoreCase) >= 0
                     || v.DisplayName.IndexOf("Vietnamese",  StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderByDescending(v => v.DisplayName.IndexOf("An", StringComparison.OrdinalIgnoreCase) >= 0)
            .ThenBy(v => v.DisplayName));

        foreach (var v in voices)
            cmbVoice.Items.Add($"{v.DisplayName} ({v.Language})");

        if (voices.Count > 0)
        {
            int idx = voices.FindIndex(v => v.DisplayName.IndexOf("An", StringComparison.OrdinalIgnoreCase) >= 0);
            cmbVoice.SelectedIndex = idx >= 0 ? idx : 0;
            SetStatus("Sẵn sàng", Pal.Green);
            SetControls(true);
            btnInstall.Visible = false;
        }
        else
        {
            SetStatus("Chưa có giọng Việt", Pal.Red);
            SetControls(false);
            btnInstall.Visible = true;
        }
    }

    private void SetControls(bool on)
    {
        btnPlay.Enabled  = on;
        btnSave.Enabled  = on;
        cmbVoice.Enabled = on;
    }

    private void SetStatus(string text, Color dot)
    {
        statusBar.Text     = text;
        statusBar.DotColor = dot;
    }

    // ── Play ──────────────────────────────────────────────────────────────────
    private async void BtnPlay_Click(object sender, EventArgs e)
    {
        if (_playing) { StopPlayback(); SetStatus("Đã dừng", Pal.TextSec); return; }
        await SpeakAsync();
    }

    private async Task SpeakAsync()
    {
        if (string.IsNullOrWhiteSpace(txtInput.Text))
        {
            MessageBox.Show("Vui lòng nhập văn bản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            StopPlayback();
            if (!SelectVoice()) return;

            SetStatus("Đang tạo âm thanh…", Pal.Blue);
            SetControls(false);
            btnPlay.Enabled = false;

            string tmp = Path.Combine(Path.GetTempPath(), $"viettts_{Guid.NewGuid()}.wav");
            await SaveWavAsync(txtInput.Text, tmp);

            int ses = ++_session;
            _player  = new SoundPlayer(tmp);
            _playing = true;
            btnPlay.Enabled = true;
            btnPlay.Text    = "⏹  Dừng";
            SetStatus("Đang phát…", Pal.Blue);

            try   { await Task.Run(() => _player.PlaySync()); }
            finally
            {
                try { File.Delete(tmp); } catch { }
                if (ses == _session) { StopPlayback(false); SetStatus("Xong", Pal.Green); }
            }
        }
        catch (Exception ex)
        {
            ResetPlayBtn();
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!_playing) SetControls(voices.Count > 0);
        }
    }

    // ── Save ──────────────────────────────────────────────────────────────────
    private async void BtnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInput.Text))
        {
            MessageBox.Show("Vui lòng nhập văn bản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var sfd = new SaveFileDialog { Filter = "WAV Audio (*.wav)|*.wav", FileName = "output.wav" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            StopPlayback();
            if (!SelectVoice()) return;
            SetStatus("Đang lưu…", Pal.Blue);
            SetControls(false);
            await SaveWavAsync(txtInput.Text, sfd.FileName);
            SetStatus($"Đã lưu: {Path.GetFileName(sfd.FileName)}", Pal.Green);
            if (MessageBox.Show("Mở file để nghe thử?", "Đã lưu", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Process.Start(new ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { SetControls(voices.Count > 0); }
    }

    // ── Install ───────────────────────────────────────────────────────────────
    private async void BtnInstall_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(
            "Ứng dụng sẽ yêu cầu quyền Admin để cài giọng Microsoft An.\nCần mạng Internet. Tiếp tục?",
            "Cài giọng tiếng Việt", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            btnInstall.Enabled = false;
            SetStatus("Đang cài giọng…", Pal.Orange);
            int code = await Task.Run(InstallVoice);
            if (code == 0) { LoadVoices(); MessageBox.Show("Đã cài xong!", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            else           { SetStatus("Cài giọng thất bại", Pal.Red); }
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) { SetStatus("Đã hủy", Pal.TextSec); }
        catch (Exception ex)      { SetStatus("Lỗi", Pal.Red); MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { btnInstall.Enabled = true; }
    }

    private static int InstallVoice()
    {
        string cmd = $"Add-WindowsCapability -Online -Name '{CapBasic}'; Add-WindowsCapability -Online -Name '{CapTts}'";
        using var p = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{cmd}\"",
            UseShellExecute = true, Verb = "runas", WindowStyle = ProcessWindowStyle.Normal,
        }) ?? throw new InvalidOperationException("Không mở được PowerShell.");
        p.WaitForExit();
        return p.ExitCode;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private bool SelectVoice()
    {
        if (cmbVoice.SelectedIndex < 0 || cmbVoice.SelectedIndex >= voices.Count)
        {
            MessageBox.Show("Chưa có giọng tiếng Việt.", "Thiếu giọng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        synth.Voice = voices[cmbVoice.SelectedIndex];
        double rate = 1.0 + sldSpeed.Value * 0.1;
        synth.Options.SpeakingRate = Math.Max(0.5, Math.Min(2.0, rate));
        return true;
    }

    private async Task SaveWavAsync(string text, string path)
    {
        using var stream = await synth.SynthesizeTextToStreamAsync(text);
        using var input  = stream.AsStreamForRead();
        using var output = File.Create(path);
        await input.CopyToAsync(output);
    }

    private void StopPlayback(bool resetStatus = true)
    {
        _session++;
        _player?.Stop();
        _player?.Dispose();
        _player   = null;
        _playing  = false;
        ResetPlayBtn();
        if (resetStatus) SetControls(voices.Count > 0);
    }

    private void ResetPlayBtn() => btnPlay.Text = "▶  Phát";

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopPlayback(false);
        synth.Dispose();
        base.OnFormClosing(e);
    }
}
