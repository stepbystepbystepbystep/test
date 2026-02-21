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
    private float score;

    private int jetpackFrames;
    private bool gameOver;

    private Image? runnerImage;
    private Image? chaserImage;

    public RunnerGameForm()
    {
        DoubleBuffered = true;
        Width = 720;
        Height = 900;
        Text = "TopDownRunner";
        KeyPreview = true;
        BackColor = Color.FromArgb(25, 25, 30);

        LoadImages();

        timer.Tick += (_, _) => TickGame();
        timer.Start();

        KeyDown += OnKeyDown;
    }

    private void LoadImages()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var runnerPath = Path.Combine(baseDir, "runner.png");
        var chaserPath = Path.Combine(baseDir, "chaser.png");

        if (File.Exists(runnerPath)) runnerImage = Image.FromFile(runnerPath);
        if (File.Exists(chaserPath)) chaserImage = Image.FromFile(chaserPath);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (gameOver && e.KeyCode == Keys.R)
        {
            Restart();
            return;
        }

        if (gameOver) return;

        if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            laneIndex = Math.Max(0, laneIndex - 1);

        if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            laneIndex = Math.Min(2, laneIndex + 1);
    }

    private void TickGame()
    {
        if (gameOver)
        {
            Invalidate();
            return;
        }

        speed = Math.Min(11f, speed + 0.0009f);
        score += speed;
        playerY -= speed;

        if (jetpackFrames > 0)
            jetpackFrames--;

        SpawnContent();
        MoveObjects();
        CheckCollisions();

        var desiredChaserY = playerY + 120;
        chaserY += (desiredChaserY - chaserY) * 0.04f;

        if (Math.Abs(chaserY - playerY) < 35)
            gameOver = true;

        Invalidate();
    }

    private void SpawnContent()
    {
        if (random.NextDouble() < 0.06)
        {
            int safeLane = random.Next(0, 3);
            for (int lane = 0; lane < 3; lane++)
            {
                if (lane == safeLane) continue;
                obstacles.Add(new Obstacle(lane, playerY - 900));
            }
        }

        if (random.NextDouble() < 0.009)
            jetpacks.Add(new JetpackPickup(random.Next(0, 3), playerY - 1000));
    }

    private void MoveObjects()
    {
        foreach (var o in obstacles) o.Y += speed;
        foreach (var j in jetpacks) j.Y += speed;

        obstacles.RemoveAll(o => o.Y > playerY + 500);
        jetpacks.RemoveAll(j => j.Y > playerY + 500);
    }

    private void CheckCollisions()
    {
        var playerRect = GetPlayerRect();

        if (jetpackFrames == 0 && obstacles.Any(o => playerRect.IntersectsWith(GetObstacleRect(o))))
            gameOver = true;

        foreach (var j in jetpacks.Where(j => playerRect.IntersectsWith(GetJetpackRect(j))).ToList())
        {
            jetpackFrames = 240;
            chaserY += 160;
            jetpacks.Remove(j);
        }
    }

    private RectangleF GetPlayerRect() => new(GetLaneX(laneIndex) - 30, 650, 60, 80);
    private RectangleF GetObstacleRect(Obstacle o) => new(GetLaneX(o.Lane) - 34, WorldToScreenY(o.Y), 68, 68);
    private RectangleF GetJetpackRect(JetpackPickup j) => new(GetLaneX(j.Lane) - 26, WorldToScreenY(j.Y), 52, 52);
    private float GetLaneX(int lane) => 180 + lane * 180;
    private float WorldToScreenY(float worldY) => 650 + (worldY - playerY);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        using var lanePen = new Pen(Color.FromArgb(60, 200, 200, 220), 2);
        g.DrawLine(lanePen, 90, 0, 90, Height);
        g.DrawLine(lanePen, 270, 0, 270, Height);
        g.DrawLine(lanePen, 450, 0, 450, Height);
        g.DrawLine(lanePen, 630, 0, 630, Height);

        foreach (var o in obstacles)
        {
            var r = GetObstacleRect(o);
            g.FillRectangle(Brushes.IndianRed, r);
        }

        foreach (var j in jetpacks)
        {
            var r = GetJetpackRect(j);
            g.FillEllipse(Brushes.Gold, r);
            g.DrawString("J", Font, Brushes.Black, r.X + 17, r.Y + 14);
        }

        var chRect = new RectangleF(GetLaneX(laneIndex) - 28, WorldToScreenY(chaserY), 56, 76);
        if (chaserImage != null) g.DrawImage(chaserImage, chRect);
        else g.FillRectangle(Brushes.MediumPurple, chRect);

        var pRect = GetPlayerRect();
        if (runnerImage != null) g.DrawImage(runnerImage, pRect);
        else g.FillRectangle(jetpackFrames > 0 ? Brushes.LightGoldenrodYellow : Brushes.DeepSkyBlue, pRect);

        g.DrawString($"Score: {(int)score}", Font, Brushes.White, 15, 12);
        if (jetpackFrames > 0)
            g.DrawString($"Jetpack: {jetpackFrames / 60f:0.0}s", Font, Brushes.Gold, 15, 36);

        if (gameOver)
        {
            using var b = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
            g.FillRectangle(b, ClientRectangle);
            g.DrawString("Game Over", new Font(Font.FontFamily, 30, FontStyle.Bold), Brushes.White, 230, 320);
            g.DrawString("R - Restart", new Font(Font.FontFamily, 16), Brushes.White, 285, 380);
        }
    }

    private void Restart()
    {
        obstacles.Clear();
        jetpacks.Clear();
        playerY = 560;
        chaserY = 700;
        speed = 5.5f;
        score = 0;
        laneIndex = 1;
        jetpackFrames = 0;
        gameOver = false;
    }

    private sealed class Obstacle
    {
        public int Lane { get; }
        public float Y { get; set; }
        public Obstacle(int lane, float y) { Lane = lane; Y = y; }
    }

    private sealed class JetpackPickup
    {
        public int Lane { get; }
        public float Y { get; set; }
        public JetpackPickup(int lane, float y) { Lane = lane; Y = y; }
    }
}
