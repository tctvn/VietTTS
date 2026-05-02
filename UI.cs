using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VietTTS;

// ── Palette ───────────────────────────────────────────────────────────────────
internal static class Pal
{
    public static readonly Color Bg         = Color.FromArgb(10, 10, 20);
    public static readonly Color Surface    = Color.FromArgb(22, 22, 38);
    public static readonly Color SurfaceHi  = Color.FromArgb(32, 32, 52);
    public static readonly Color Border     = Color.FromArgb(55, 255, 255, 255);
    public static readonly Color BorderFoc  = Color.FromArgb(10, 132, 255);
    public static readonly Color TextPri    = Color.FromArgb(242, 242, 247);
    public static readonly Color TextSec    = Color.FromArgb(110, 110, 130);
    public static readonly Color Blue       = Color.FromArgb(10,  132, 255);
    public static readonly Color Green      = Color.FromArgb(48,  209,  88);
    public static readonly Color Orange     = Color.FromArgb(255, 159,  10);
    public static readonly Color Red        = Color.FromArgb(255,  69,  58);
}

// ── Helper ────────────────────────────────────────────────────────────────────
internal static class Draw
{
    public static GraphicsPath RoundRect(Rectangle r, int rad)
    {
        int d = Math.Max(1, rad * 2);
        var p = new GraphicsPath();
        p.AddArc(r.X,           r.Y,            d, d, 180, 90);
        p.AddArc(r.Right - d,   r.Y,            d, d, 270, 90);
        p.AddArc(r.Right - d,   r.Bottom - d,   d, d,   0, 90);
        p.AddArc(r.X,           r.Bottom - d,   d, d,  90, 90);
        p.CloseFigure();
        return p;
    }

    public static void SetupGraphics(Graphics g)
    {
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.PixelOffsetMode   = PixelOffsetMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    }
}

// ── Pill Button ───────────────────────────────────────────────────────────────
internal sealed class PillButton : Button
{
    public Color AccentColor { get; set; } = Pal.Blue;

    private bool _hover, _press;

    public PillButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor    = Cursors.Hand;
        Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        ForeColor = Color.White;
        Height    = 48;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _press = true;  Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e)   { _press = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Draw.SetupGraphics(g);

        var rc  = new Rectangle(0, 0, Width - 1, Height - 1);
        int rad = Height / 2;

        Color c = Enabled ? AccentColor : Color.FromArgb(50, 50, 65);
        if (_press) c = Darken(c, 0.20f);
        else if (_hover) c = Lighten(c, 0.12f);

        // Fill
        using (var path = Draw.RoundRect(rc, rad))
        using (var br = new LinearGradientBrush(rc, Lighten(c, 0.10f), Darken(c, 0.08f), LinearGradientMode.Vertical))
            g.FillPath(br, path);

        // Specular top highlight
        var hiRc = new Rectangle(rc.X + 3, rc.Y + 2, rc.Width - 6, rc.Height / 2 - 2);
        if (hiRc.Width > 0 && hiRc.Height > 0)
        {
            using var hiPath = Draw.RoundRect(hiRc, rad);
            using var hiBr   = new LinearGradientBrush(hiRc,
                Color.FromArgb(_press ? 10 : 45, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical);
            g.FillPath(hiBr, hiPath);
        }

        // Border
        using (var path = Draw.RoundRect(rc, rad))
        using (var pen  = new Pen(Color.FromArgb(_hover ? 70 : 40, 255, 255, 255), 1f))
            g.DrawPath(pen, path);

        // Text
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var tb = new SolidBrush(Enabled ? Color.White : Color.FromArgb(90, 90, 105));
        g.DrawString(Text, Font, tb, new RectangleF(0, 0, Width, Height), sf);
    }

    private static Color Lighten(Color c, float t) =>
        Color.FromArgb(c.A, Math.Min(255,(int)(c.R+(255-c.R)*t)),
                            Math.Min(255,(int)(c.G+(255-c.G)*t)),
                            Math.Min(255,(int)(c.B+(255-c.B)*t)));
    private static Color Darken(Color c, float t) =>
        Color.FromArgb(c.A, (int)(c.R*(1-t)), (int)(c.G*(1-t)), (int)(c.B*(1-t)));
}

// ── Card Panel ────────────────────────────────────────────────────────────────
internal sealed class CardPanel : Panel
{
    public int Radius { get; set; } = 20;

    public CardPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Pal.Surface;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Draw.SetupGraphics(g);
        var rc = new Rectangle(0, 0, Width - 1, Height - 1);

        using (var path = Draw.RoundRect(rc, Radius))
        using (var br   = new SolidBrush(Pal.Surface))
            g.FillPath(br, path);

        // Top shimmer
        var shimRc = new Rectangle(1, 1, rc.Width - 2, Radius * 2);
        if (shimRc.Height > 0)
        {
            using var sp = Draw.RoundRect(shimRc, Radius - 1);
            using var sb = new LinearGradientBrush(shimRc,
                Color.FromArgb(18, 255, 255, 255), Color.FromArgb(0, 255, 255, 255),
                LinearGradientMode.Vertical);
            g.FillPath(sb, sp);
        }

        using (var path = Draw.RoundRect(rc, Radius))
        using (var pen  = new Pen(Pal.Border, 1f))
            g.DrawPath(pen, path);
    }
}

// ── Thin Slider ───────────────────────────────────────────────────────────────
internal sealed class ThinSlider : Control
{
    public int  Minimum { get; set; } = -10;
    public int  Maximum { get; set; } =  10;
    public event EventHandler ValueChanged;

    private int  _val;
    private bool _drag;

    public int Value
    {
        get => _val;
        set
        {
            _val = Math.Max(Minimum, Math.Min(Maximum, value));
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public ThinSlider()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Pal.SurfaceHi;
        Height    = 34;
        Cursor    = Cursors.Hand;
    }

    protected override void OnMouseDown(MouseEventArgs e) { _drag = true;  Hit(e.X); base.OnMouseDown(e); }
    protected override void OnMouseMove(MouseEventArgs e) { if (_drag) Hit(e.X); base.OnMouseMove(e); }
    protected override void OnMouseUp(MouseEventArgs e)   { _drag = false; base.OnMouseUp(e); }

    private void Hit(int x)
    {
        int pad = 14, w = Width - pad * 2;
        float pct = Math.Max(0f, Math.Min(1f, (x - pad) / (float)w));
        Value = Minimum + (int)Math.Round(pct * (Maximum - Minimum));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Draw.SetupGraphics(g);

        int pad = 14, cy = Height / 2, tw = Width - pad * 2, th = 4;
        float pct   = (float)(_val - Minimum) / (Maximum - Minimum);
        int   thumbX = pad + (int)(pct * tw);

        // Track bg
        var tRc = new Rectangle(pad, cy - th / 2, tw, th);
        using (var tp = Draw.RoundRect(tRc, th / 2))
        using (var tb = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
            g.FillPath(tb, tp);

        // Filled track
        if (thumbX > pad)
        {
            var fRc = new Rectangle(pad, cy - th / 2, thumbX - pad, th);
            if (fRc.Width > 0)
            {
                using var fp = Draw.RoundRect(fRc, th / 2);
                using var fb = new LinearGradientBrush(fRc, Pal.Blue, Color.FromArgb(80, 180, 255), LinearGradientMode.Horizontal);
                g.FillPath(fb, fp);
            }
        }

        // Thumb
        int r = 9;
        var knob = new Rectangle(thumbX - r, cy - r, r * 2, r * 2);
        using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            g.FillEllipse(shadow, new Rectangle(knob.X + 1, knob.Y + 2, r * 2, r * 2));
        using (var wb = new SolidBrush(Color.White))
            g.FillEllipse(wb, knob);
        using (var wp = new Pen(Color.FromArgb(25, 0, 0, 0), 1f))
            g.DrawEllipse(wp, knob);
    }
}

// ── Dark TextBox ──────────────────────────────────────────────────────────────
internal sealed class DarkTextBox : TextBox
{
    public DarkTextBox()
    {
        BackColor     = Pal.SurfaceHi;
        ForeColor     = Pal.TextPri;
        BorderStyle   = BorderStyle.None;
        Font          = new Font("Segoe UI", 10.5f);
        Multiline     = true;
        ScrollBars    = ScrollBars.Vertical;
        AcceptsReturn = true;
        Padding       = new Padding(4);
    }
}

// ── Section Label ─────────────────────────────────────────────────────────────
internal sealed class SectionLabel : Label
{
    public SectionLabel(string text)
    {
        Text      = text;
        Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        ForeColor = Pal.TextSec;
        AutoSize  = true;
        BackColor = Color.Transparent;
    }
}

// ── Status Bar ────────────────────────────────────────────────────────────────
internal sealed class StatusBar : Control
{
    private string _text = "Sẵn sàng";
    private Color  _dot  = Pal.Green;

    public new string Text  { get => _text; set { _text = value; Invalidate(); } }
    public Color DotColor   { get => _dot;  set { _dot  = value; Invalidate(); } }

    public StatusBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Pal.Surface;
        Height    = 36;
        Font      = new Font("Segoe UI", 9.5f);
        ForeColor = Pal.TextPri;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Draw.SetupGraphics(g);

        // Dot
        int dotR = 6, dotX = 18, dotY = Height / 2;
        using (var db = new SolidBrush(_dot))
            g.FillEllipse(db, dotX - dotR, dotY - dotR, dotR * 2, dotR * 2);

        // Glow
        using (var gp = new GraphicsPath())
        {
            gp.AddEllipse(dotX - dotR - 3, dotY - dotR - 3, (dotR + 3) * 2, (dotR + 3) * 2);
            using var gb = new PathGradientBrush(gp);
            gb.CenterColor    = Color.FromArgb(60, _dot);
            gb.SurroundColors = new[] { Color.FromArgb(0, _dot) };
            g.FillPath(gb, gp);
        }

        // Text
        using var tb = new SolidBrush(Pal.TextPri);
        g.DrawString(_text, Font, tb, dotX + dotR + 8, (Height - Font.Height) / 2f);
    }
}
