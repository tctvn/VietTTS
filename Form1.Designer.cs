using System.Drawing;
using System.Windows.Forms;

namespace VietTTS;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        txtText = new TextBox();
        btnSpeak = new Button();
        btnSave = new Button();
        cmbVoices = new ComboBox();
        lblVoice = new Label();
        lblStatus = new Label();
        trackBarSpeed = new TrackBar();
        lblSpeed = new Label();
        btnInstall = new Button();
        label1 = new Label();
        ((System.ComponentModel.ISupportInitialize)trackBarSpeed).BeginInit();
        SuspendLayout();

        // txtText
        txtText.Location = new Point(12, 36);
        txtText.Multiline = true;
        txtText.Name = "txtText";
        txtText.Size = new Size(560, 118);
        txtText.TabIndex = 0;
        txtText.Text = "Xin chào! Đây là ứng dụng chuyển văn bản thành giọng nói tiếng Việt.";

        // btnSpeak
        btnSpeak.BackColor = Color.FromArgb(0, 123, 255);
        btnSpeak.ForeColor = Color.White;
        btnSpeak.Location = new Point(12, 235);
        btnSpeak.Name = "btnSpeak";
        btnSpeak.Size = new Size(170, 40);
        btnSpeak.TabIndex = 6;
        btnSpeak.Text = "Phát";
        btnSpeak.UseVisualStyleBackColor = false;
        btnSpeak.Click += btnSpeak_Click;

        // btnSave
        btnSave.BackColor = Color.FromArgb(40, 167, 69);
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(196, 235);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(170, 40);
        btnSave.TabIndex = 7;
        btnSave.Text = "Lưu WAV";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += btnSave_Click;

        // cmbVoices
        cmbVoices.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbVoices.FormattingEnabled = true;
        cmbVoices.Location = new Point(12, 186);
        cmbVoices.Name = "cmbVoices";
        cmbVoices.Size = new Size(286, 28);
        cmbVoices.TabIndex = 2;

        // lblVoice
        lblVoice.AutoSize = true;
        lblVoice.Location = new Point(12, 163);
        lblVoice.Name = "lblVoice";
        lblVoice.Size = new Size(113, 20);
        lblVoice.TabIndex = 1;
        lblVoice.Text = "Giọng tiếng Việt";

        // lblStatus
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(12, 294);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(69, 20);
        lblStatus.TabIndex = 8;
        lblStatus.Text = "Sẵn sàng";

        // trackBarSpeed
        trackBarSpeed.Location = new Point(330, 180);
        trackBarSpeed.Maximum = 10;
        trackBarSpeed.Minimum = -10;
        trackBarSpeed.Name = "trackBarSpeed";
        trackBarSpeed.Size = new Size(242, 56);
        trackBarSpeed.TabIndex = 4;
        trackBarSpeed.Value = 0;
        trackBarSpeed.Scroll += trackBarSpeed_Scroll;

        // lblSpeed
        lblSpeed.AutoSize = true;
        lblSpeed.Location = new Point(330, 163);
        lblSpeed.Name = "lblSpeed";
        lblSpeed.Size = new Size(72, 20);
        lblSpeed.TabIndex = 3;
        lblSpeed.Text = "Tốc độ: 0";

        // btnInstall
        btnInstall.BackColor = Color.FromArgb(255, 193, 7);
        btnInstall.ForeColor = Color.Black;
        btnInstall.Location = new Point(408, 282);
        btnInstall.Name = "btnInstall";
        btnInstall.Size = new Size(164, 36);
        btnInstall.TabIndex = 9;
        btnInstall.Text = "Tự cài giọng Việt";
        btnInstall.UseVisualStyleBackColor = false;
        btnInstall.Visible = false;
        btnInstall.Click += btnInstall_Click;

        // label1
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label1.Location = new Point(12, 12);
        label1.Name = "label1";
        label1.Size = new Size(180, 20);
        label1.TabIndex = 10;
        label1.Text = "Nhập văn bản tiếng Việt:";

        // Form1
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(584, 331);
        Controls.Add(label1);
        Controls.Add(btnInstall);
        Controls.Add(lblSpeed);
        Controls.Add(trackBarSpeed);
        Controls.Add(lblStatus);
        Controls.Add(lblVoice);
        Controls.Add(cmbVoices);
        Controls.Add(btnSave);
        Controls.Add(btnSpeak);
        Controls.Add(txtText);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "VietTTS - Giọng nói tiếng Việt";
        ((System.ComponentModel.ISupportInitialize)trackBarSpeed).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private TextBox txtText;
    private Button btnSpeak;
    private Button btnSave;
    private ComboBox cmbVoices;
    private Label lblVoice;
    private Label lblStatus;
    private TrackBar trackBarSpeed;
    private Label lblSpeed;
    private Button btnInstall;
    private Label label1;
}
