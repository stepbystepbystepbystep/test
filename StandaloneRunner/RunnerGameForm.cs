using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StandaloneRunner;

public class RunnerGameForm : Form
{
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 16 };
    private readonly Random random = new();

    private readonly List<Obstacle> obstacles = new();
    private readonly List<JetpackPickup> jetpacks = new();

    private float playerY = 560;
    private int laneIndex = 1;
    private float speed = 5.5f;
    private float chaserY = 700;
    private float chaserLane = 1f;
    private float score;

    private int jetpackFrames;
    private bool gameOver;

    private float nextObstacleRowY;
    private float nextJetpackY;
    private int previousSafeLane = 1;

    private Image? runnerImage;
    private Image? chaserImage;
    private Image? obstacleImage;
    private Image? jetpackImage;

    public RunnerGameForm()
    {
        DoubleBuffered = true;
        Width = 720;
        Height = 900;
        Text = "TopDownRunner";
        KeyPreview = true;
        BackColor = Color.FromArgb(25, 25, 30);

        ResetSpawnState();
        LoadImages();

        timer.Tick += (_, _) => TickGame();
        timer.Start();

        KeyDown += OnKeyDown;
    }

    private void LoadImages()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        runnerImage = TryLoadImage(Path.Combine(baseDir, "runner.png"));
        chaserImage = TryLoadImage(Path.Combine(baseDir, "chaser.png"));
        obstacleImage = TryLoadImage(Path.Combine(baseDir, "obstacle.png"));
        jetpackImage = TryLoadImage(Path.Combine(baseDir, "jetpack.png"));
    }

    private static Image? TryLoadImage(string path)
    {
        return File.Exists(path) ? Image.FromFile(path) : null;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (gameOver && e.KeyCode == Keys.R)
        {
            Restart();
            return;
        }

        if (gameOver)
        {
            return;
        }

        if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
        {
            laneIndex = Math.Max(0, laneIndex - 1);
        }

        if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
        {
            laneIndex = Math.Min(2, laneIndex + 1);
        }
    }

    private void TickGame()
    {
        if (gameOver)
        {
            Invalidate();
            return;
        }

        speed = Math.Min(11f, speed + 0.001f);
        score += speed;
        playerY -= speed;

        if (jetpackFrames > 0)
        {
            jetpackFrames--;
        }

        SpawnContent();
        MoveObjects();
        CheckCollisions();

        float desiredChaserY = playerY + 120;
        chaserY += (desiredChaserY - chaserY) * 0.05f;
        chaserLane += (laneIndex - chaserLane) * 0.07f;

        if ((chaserY - playerY) < 34f)
        {
            gameOver = true;
        }

        Invalidate();
    }

    private void SpawnContent()
    {
        float obstacleSpawnThreshold = playerY - 1400f;
        while (nextObstacleRowY > obstacleSpawnThreshold)
        {
            SpawnObstacleRow(nextObstacleRowY);
            nextObstacleRowY -= random.Next(190, 280);
        }

        float jetpackSpawnThreshold = playerY - 1700f;
        while (nextJetpackY > jetpackSpawnThreshold)
        {
            jetpacks.Add(new JetpackPickup(random.Next(0, 3), nextJetpackY));
            nextJetpackY -= random.Next(650, 980);
        }
    }

    private void SpawnObstacleRow(float worldY)
    {
        int minLane = Math.Max(0, previousSafeLane - 1);
        int maxLane = Math.Min(2, previousSafeLane + 1);
        int safeLane = random.Next(minLane, maxLane + 1);
        previousSafeLane = safeLane;

        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == safeLane)
            {
                continue;
            }

            obstacles.Add(new Obstacle(lane, worldY));
        }
    }

    private void MoveObjects()
    {
        foreach (var obstacle in obstacles)
        {
            obstacle.Y += speed;
        }

        foreach (var jetpack in jetpacks)
        {
            jetpack.Y += speed;
        }

        obstacles.RemoveAll(o => o.Y > playerY + 520);
        jetpacks.RemoveAll(j => j.Y > playerY + 520);
    }

    private void CheckCollisions()
    {
        RectangleF playerRect = GetPlayerRect();

        if (jetpackFrames == 0 && obstacles.Any(o => playerRect.IntersectsWith(GetObstacleRect(o))))
        {
            gameOver = true;
        }

        foreach (var jetpack in jetpacks.Where(j => playerRect.IntersectsWith(GetJetpackRect(j))).ToList())
        {
            jetpackFrames = 260;
            chaserY += 170;
            jetpacks.Remove(jetpack);
        }
    }

    private RectangleF GetPlayerRect() => new(GetLaneX(laneIndex) - 30, 650, 60, 82);
    private RectangleF GetObstacleRect(Obstacle obstacle) => new(GetLaneX(obstacle.Lane) - 34, WorldToScreenY(obstacle.Y), 68, 68);
    private RectangleF GetJetpackRect(JetpackPickup jetpack) => new(GetLaneX(jetpack.Lane) - 24, WorldToScreenY(jetpack.Y), 48, 48);

    private float GetLaneX(float lane) => 180 + lane * 180;
    private float WorldToScreenY(float worldY) => 650 + (worldY - playerY);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        DrawEnvironment(g);

        foreach (var obstacle in obstacles)
        {
            RectangleF obstacleRect = GetObstacleRect(obstacle);
            DrawObstacle(g, obstacleRect);
        }

        foreach (var jetpack in jetpacks)
        {
            RectangleF jetpackRect = GetJetpackRect(jetpack);
            DrawJetpack(g, jetpackRect);
        }

        float chaseDelta = chaserY - playerY;
        float chaserScreenY = 780 - Math.Clamp((120f - chaseDelta) * 2.2f, -40f, 150f);
        RectangleF chaserRect = new(GetLaneX(chaserLane) - 30, chaserScreenY, 60, 78);

        if (chaserImage != null)
        {
            g.DrawImage(chaserImage, chaserRect);
        }
        else
        {
            DrawPixelCharacter(g, chaserRect, Color.MediumPurple);
        }

        RectangleF playerRect = GetPlayerRect();
        if (runnerImage != null)
        {
            g.DrawImage(runnerImage, playerRect);
        }
        else
        {
            DrawPixelCharacter(g, playerRect, jetpackFrames > 0 ? Color.LightGoldenrodYellow : Color.DeepSkyBlue);
        }

        g.DrawString($"Score: {(int)score}", Font, Brushes.White, 15, 12);
        if (jetpackFrames > 0)
        {
            g.DrawString($"Jetpack: {jetpackFrames / 60f:0.0}s", Font, Brushes.Gold, 15, 36);
        }

        if (gameOver)
        {
            using var shade = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
            g.FillRectangle(shade, ClientRectangle);
            g.DrawString("Game Over", new Font(Font.FontFamily, 30, FontStyle.Bold), Brushes.White, 230, 320);
            g.DrawString("R - Restart", new Font(Font.FontFamily, 16), Brushes.White, 285, 380);
        }
    }

    private void DrawEnvironment(Graphics g)
    {
        g.Clear(Color.FromArgb(20, 22, 30));

        using var sideBrush = new SolidBrush(Color.FromArgb(32, 40, 52));
        g.FillRectangle(sideBrush, 0, 0, 90, Height);
        g.FillRectangle(sideBrush, 630, 0, 90, Height);

        using var roadBrush = new SolidBrush(Color.FromArgb(45, 48, 62));
        g.FillRectangle(roadBrush, 90, 0, 540, Height);

        using var lanePen = new Pen(Color.FromArgb(100, 190, 210, 240), 2);
        g.DrawLine(lanePen, 90, 0, 90, Height);
        g.DrawLine(lanePen, 270, 0, 270, Height);
        g.DrawLine(lanePen, 450, 0, 450, Height);
        g.DrawLine(lanePen, 630, 0, 630, Height);

        for (int i = 0; i < 20; i++)
        {
            float y = (i * 80 + (playerY * -0.35f)) % Height;
            g.FillRectangle(Brushes.WhiteSmoke, 266, y, 8, 26);
            g.FillRectangle(Brushes.WhiteSmoke, 446, y, 8, 26);
        }

        for (int i = 0; i < 12; i++)
        {
            float y = (i * 120 + (playerY * -0.20f)) % Height;
            g.FillRectangle(Brushes.DarkSlateGray, 14, y, 48, 60);
            g.FillRectangle(Brushes.DarkSlateGray, 658, y, 48, 60);
        }
    }

    private void DrawObstacle(Graphics g, RectangleF rect)
    {
        if (obstacleImage != null)
        {
            g.DrawImage(obstacleImage, rect);
            return;
        }

        DrawPixelBlock(g, rect, Color.FromArgb(172, 78, 78), Color.FromArgb(115, 50, 50));
    }

    private void DrawJetpack(Graphics g, RectangleF rect)
    {
        if (jetpackImage != null)
        {
            g.DrawImage(jetpackImage, rect);
            return;
        }

        DrawPixelBlock(g, rect, Color.Gold, Color.DarkGoldenrod);
        g.DrawString("J", Font, Brushes.Black, rect.X + 15, rect.Y + 12);
    }

    private static void DrawPixelCharacter(Graphics g, RectangleF rect, Color color)
    {
        DrawPixelBlock(g, rect, color, ControlPaint.Dark(color));
    }

    private static void DrawPixelBlock(Graphics g, RectangleF rect, Color baseColor, Color darkColor)
    {
        int cell = 8;
        using var light = new SolidBrush(baseColor);
        using var dark = new SolidBrush(darkColor);

        for (int y = 0; y < rect.Height; y += cell)
        {
            for (int x = 0; x < rect.Width; x += cell)
            {
                bool darkCell = ((x / cell) + (y / cell)) % 2 == 0;
                g.FillRectangle(darkCell ? dark : light, rect.X + x, rect.Y + y, cell, cell);
            }
        }

        using var outlinePen = new Pen(Color.Black, 2);
        g.DrawRectangle(outlinePen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    private void Restart()
    {
        obstacles.Clear();
        jetpacks.Clear();

        playerY = 560;
        chaserY = 700;
        chaserLane = 1f;
        speed = 5.5f;
        score = 0;
        laneIndex = 1;
        jetpackFrames = 0;
        gameOver = false;

        ResetSpawnState();
    }

    private void ResetSpawnState()
    {
        nextObstacleRowY = playerY - 820;
        nextJetpackY = playerY - 1150;
        previousSafeLane = laneIndex;
    }

    private sealed class Obstacle
    {
        public int Lane { get; }
        public float Y { get; set; }

        public Obstacle(int lane, float y)
        {
            Lane = lane;
            Y = y;
        }
    }

    private sealed class JetpackPickup
    {
        public int Lane { get; }
        public float Y { get; set; }

        public JetpackPickup(int lane, float y)
        {
            Lane = lane;
            Y = y;
        }
    }
}
