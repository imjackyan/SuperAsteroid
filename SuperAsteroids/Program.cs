using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Timers;

namespace SuperAsteroids
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]        
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form maingame = new Asteroids();
            Application.Run(maingame);
        }

        public static List<Player> Players = new List<Player>();
        public static Background[] background = new Background[1];
        public static Form[] Maingame = new Form[1];
        public static int[] Time = new int[3]{0,1,0}; //[0] is frame, [1]is seconds
        public static List<Rock> Rocks = new List<Rock>();
        public static List<Boom> Booms = new List<Boom>();

        public static void Initialize(Form maingame)
        {
            Graphics g = maingame.CreateGraphics();
            Maingame[0] = maingame;
            background[0] = new Background();
            background[0].Draw(g);
            Players.Add(new Player(1));
            Players[0].Draw(g); 
            System.Timers.Timer _timer = new System.Timers.Timer(25);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
            


        }

        static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Time[0]++;
            if (Time[0] >= 40) { Time[1]++; Time[0] = 0; Time[2] ++; }

            //if (Players[0].health <= 0) { Maingame[0].Enabled = false; }

            if (Players[0].Controls[0]) { Players[0].speed -= Players[0].accel_rate; }
            if (Players[0].Controls[2]) { Players[0].speed += Players[0].accel_rate; }
            if (Players[0].Controls[1]) { Players[0].Rotate(-Players[0].angle_rotation); }
            if (Players[0].Controls[3]) { Players[0].Rotate(Players[0].angle_rotation); }
            Players[0].Update();

            if (Time[2]==2)
            {
                Rock rock = new Rock();
                Rocks.Add(rock);
                Time[2] = 0;
            }



            //puts images together and draw
            Graphics g = Maingame[0].CreateGraphics();
            using (Bitmap bitmap = new Bitmap(background[0].image))
            {
                using (Graphics grfx = Graphics.FromImage(bitmap))
                {
                    //add images here to merge
                    Players[0].Update();
                    if (Players[0].health > 0)
                    {
                        grfx.DrawImage(Players[0].image, Players[0].Location);
                        if (Players[0].invinc_time != 0) { grfx.DrawImage(Players[0].bubble, Players[0].bubble_location);}
                    }
                    else if (Players[0].health <= 0) 
                    {
                        Label gameover = new Label();
                        gameover.Text = "GAME OVER";
                        gameover.Location = new Point(500, 300);
                        gameover.Size = new Size(100, 30);
                        gameover.Visible = true;
                        //over here mate.
                    }
                    if (Players[0].bullets.Count != 0)
                    {
                        for (int i = 0; i < Players[0].bullets.Count; i++)//adding the bullets
                        {
                            Players[0].bullets[i].Update();
                            if (!Players[0].bullets[i].used) { grfx.DrawImage(Players[0].bullets[i].image, Players[0].bullets[i].location); } //this bullet still exist
                            else if (Players[0].bullets[i].used) { Players[0].bullets.RemoveAt(i); i--; }//dispose of the bullet
                        }
                    }
                    if (Rocks.Count != 0)
                    {
                        for (int i = 0; i < Rocks.Count; i++)
                        {
                            Rocks[i].Update();
                            if (!Rocks[i].used && !Rocks[i].shot) { grfx.DrawImage(Rocks[i].image, Rocks[i].location); }
                            else if (Rocks[i].used || Rocks[i].shot) { Rocks.RemoveAt(i); i--; }
                        }
                    }
                    if (Booms.Count != 0)
                    {
                        for (int i = 0; i < Booms.Count; i++)
                        {
                            if (Booms[i].timer >= 0)
                            {
                                grfx.DrawImage(Booms[i].image, Booms[i].location);
                                Booms[i].timer--;
                            }
                            else if (Booms[i].timer < 0) { Booms.RemoveAt(i); i--; }
                        }
                    }

                        grfx.Save();
                }
                g.DrawImage(bitmap, 0, 0);
            }
        }

    }

    public class Player
    {
        Point location; public int health = 3; string path; public Bitmap image; public int accel_rate = 3; Bitmap original_image; int max_speed = 9;
        bool[] _controls = new bool[4]; public int speed = 0;
        int deaccel = 1;
        public float angle_rotation = 10.0f;
        Point orgin = new Point(0, 0);
        int current_angle = 0;
        public List<Bullet> bullets = new List<Bullet>();
        int bullet_cooldown = 0; Rectangle rect; public int invinc_time = 0; public Bitmap bubble; public Point bubble_location;


        public Player(int player_num)
        {
            if (player_num == 1) { this.location = new Point(600, 600); this.path = Path.Combine(Environment.CurrentDirectory + @"\Data\Player1.png");}
            else { this.location = new Point(500, 600); this.path = Path.Combine(Environment.CurrentDirectory + @"\Data\Player2.png"); }
            this.image = new Bitmap(Image.FromFile(this.path));
            this.original_image = this.image;
            rect = new Rectangle(this.location, new Size(this.image.Width, this.image.Height));

            path = Path.Combine(Environment.CurrentDirectory + @"\Data\bubble.png");
            this.bubble = new Bitmap(Image.FromFile(path), new Size(130,130));
        }
        public void FinalLocation()
        {
            int x = 1; int y = 1; int de_accel = 1;

            if (!this.Controls[0] && speed < 0) { this.speed += de_accel; }
            if (!this.Controls[2] && speed > 0) { this.speed -= de_accel; }

            if (speed > max_speed) { speed = max_speed; }
            else if (speed < -max_speed) { speed = -max_speed; }

            if (current_angle == 0) { x = 0; y = speed; }
            else if (current_angle == 180) { y = -speed; x = 0; }
            else if (current_angle == 90) { x = -speed; y = 0; }
            else if (current_angle == -90) { y = 0; x = speed; }
            else
            {
                int angle = current_angle;
                if (current_angle > 0 && current_angle < 90) { angle = current_angle; x = -1; }
                else if (current_angle > 90 && current_angle < 180) { angle = 180-current_angle; y = -1; x = -1; }
                else if (current_angle < 0 && current_angle > -90) { angle = Math.Abs(current_angle); x = 1; }
                else if (current_angle < -90 && current_angle > -180) { angle = Math.Abs(-180 - current_angle); y = -1; }

                x = x*(int)(speed * Math.Sin(Math.PI * angle / 180.0));
                y = y*(int)(speed * Math.Cos(Math.PI * angle / 180.0));
            }
            this.location = new Point(this.location.X + x, this.location.Y + y);
        }

        public void Rotate(float angle)
        {
            if (current_angle > 180) { current_angle = -360 + current_angle; }
            else if(current_angle < -180) { current_angle = 360 + current_angle; }
            
            this.current_angle += (int)angle;
            orgin = new Point(this.image.Width / 2 + this.location.X, this.image.Height / 2 + this.location.Y);
            this.image = RotateImg.Rotate(this.original_image, (float)this.current_angle);
            this.location = new Point(orgin.X - this.image.Width/2, orgin.Y - this.image.Height/2);
        }

        public void FireBullet()
        {
            if (bullet_cooldown == 0)
            {
                orgin = new Point(this.image.Width / 2 + this.location.X, this.image.Height / 2 + this.location.Y);
                Bullet bullet = new Bullet(1, orgin, this.current_angle);
                bullets.Add(bullet);
                // COOL DOWN
                bullet_cooldown = 20;
            }
        }
        public void Update()
        {
            this.FinalLocation();
            this.rect.Location = this.location;
            if (bullet_cooldown != 0) { this.bullet_cooldown--; }
            if (this.location.X > 970) { this.location.X = 970; }
            else if (this.location.X < 0) { this.location.X = 0; }
            if (this.location.Y > 660) { this.location.Y = 660; }
            else if (this.location.Y < 0) { this.location.Y = 0; }
            if (Program.Rocks.Count != 0 && this.invinc_time == 0)
            {
                for (int i = 0; i < Program.Rocks.Count; i++)
                {
                    if (Distance(this.location, Program.Rocks[i].location) < 300)
                    {
                        if (this.rect.IntersectsWith(Program.Rocks[i].rect))
                        {
                            Program.Rocks[i].shot = true; this.health--;
                            Program.Booms.Add(new Boom(this.location));
                            this.location = new Point(500, 300);
                            this.invinc_time = 160; //4 seconds of invincible period after collision
                            
                        }

                    }
                }
            }
            else if (invinc_time != 0) 
            { invinc_time--;
            orgin = new Point(this.image.Width / 2 + this.location.X, this.image.Height / 2 + this.location.Y);
            bubble_location = new Point(orgin.X-65, orgin.Y -65); }
            
        }
        public int Distance(Point pos1, Point pos2)
        {
            int distance;
            distance = (int)Math.Pow(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2), 0.5);
            return distance;
        }

        public void Draw(Graphics g) { g.DrawImage(this.image, this.location); }

        public bool[] Controls { get { return this._controls; } }
        public Point Location { get { return this.location; } set { location =value; } }
    }
    public class Bullet
    {
        string path; public Bitmap image; public Point location; int speed = -10; int x = 1; int y = 1; public bool used = false; Rectangle rect;
        public Bullet(int bullet_type, Point position, int angle)
        {
            if (bullet_type == 1) { path = Path.Combine(Environment.CurrentDirectory + @"\Data\bullet.png"); }
            this.image = new Bitmap(Image.FromFile(path));
            this.location = position;
            rect = new Rectangle(this.location, new Size(this.image.Width, this.image.Height));

            if (angle == 0) { x = 0; y = speed; }
            else if (angle == 180) { y = -speed; x = 0; }
            else if (angle == 90) { x = -speed; y = 0; }
            else if (angle == -90) { y = 0; x = speed; }
            else
            {
                int _angle = angle;
                if (angle > 0 && angle < 90) { _angle = angle; x = -1; }
                else if (angle > 90 && angle < 180) { _angle = 180 - angle; y = -1; x = -1; }
                else if (angle < 0 && angle > -90) { _angle = Math.Abs(angle); x = 1; }
                else if (angle < -90 && angle > -180) { _angle = Math.Abs(-180 - angle); y = -1; }

                x = x * (int)(speed * Math.Sin(Math.PI * _angle / 180.0));
                y = y * (int)(speed * Math.Cos(Math.PI * _angle / 180.0));
            }
        }
        public void Update()
        {
            if (!this.used)
            {
                this.rect.Location = this.location;
                this.location = new Point(this.location.X + x, this.location.Y + y);
                if (this.location.X < 0 || this.location.X > 1024 || this.location.Y < 0 || this.location.Y > 720) { this.used = true; }
                if (Program.Rocks.Count != 0)
                {
                    for (int i = 0; i < Program.Rocks.Count; i++)
                    {
                        if (Distance(this.location, Program.Rocks[i].location) < 200)
                        {
                            if (this.rect.IntersectsWith(Program.Rocks[i].rect))
                            {
                                Program.Rocks[i].shot = true; this.used = true;
                                Program.Booms.Add(new Boom(this.location));
                            }

                        }
                    }
                }
            }
        }
        private int Distance(Point pos1, Point pos2)
        {
            int distance;
            distance = (int)Math.Pow(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2), 0.5);
            return distance;
        }
    }

    public class Rock
    {
        public Bitmap image; public Point location; string path; int angle; int speed; Random rnd = new Random(); int type; int x = 1; int y = 1; public bool used = false;
        public bool shot = false; public Rectangle rect;

        public Rock()
        {
            type = 0;
            this.path = Path.Combine(Environment.CurrentDirectory + @"\Data\rock.png");            
            this.image = new Bitmap(Image.FromFile(path));
            int choice = rnd.Next(0, 4);
            this.location = LocationGenerate(choice);
            this.rect = new Rectangle(this.location,new Size( this.image.Width, this.image.Height));
            
            if (choice == 0) { x = rnd.Next(1, 7); y = rnd.Next(-3, 4); }
            else if (choice == 2) { x = -rnd.Next(1, 7); y = rnd.Next(-3, 4); }
            else if (choice == 1) { x = rnd.Next(-3, 4); y = rnd.Next(1, 7); }
            else if (choice == 3) { x = rnd.Next(-3, 4); y = -rnd.Next(1, 7); }

        }
        public Rock(Point _location, int num) //second seperation
        {
            type = 1;
            this.path = Path.Combine(Environment.CurrentDirectory + @"\Data\rock.png");
            this.image = new Bitmap(Image.FromFile(path), new Size(50,50));
            this.location = _location;
            this.rect = new Rectangle(this.location, new Size(this.image.Width, this.image.Height));
            int choice = 0;
            if (num == 0) { choice = rnd.Next(0, 2); }
            else { choice = rnd.Next(2, 4); }

            if (choice == 0) { x = rnd.Next(1, 5); y = rnd.Next(-2, 3); }
            else if (choice == 2) { x = -rnd.Next(1, 5); y = rnd.Next(-2, 3); }
            else if (choice == 1) { x = rnd.Next(-2, 3); y = rnd.Next(1, 5); }
            else if (choice == 3) { x = rnd.Next(-2, 3); y = -rnd.Next(1, 5); }
        }
        public Rock(Point _location, int num, int seperation) // third seperation
        {
            type = 2;
            this.path = Path.Combine(Environment.CurrentDirectory + @"\Data\rock.png");
            this.image = new Bitmap(Image.FromFile(path), new Size(25, 25));
            this.location = _location;
            this.rect = new Rectangle(this.location, new Size(this.image.Width, this.image.Height));
            int choice = 0;
            if (num == 0) { choice = rnd.Next(0, 2); }
            else { choice = rnd.Next(2, 4); }

            if (choice == 0) { x = rnd.Next(1, 3); y = rnd.Next(-1, 2); }
            else if (choice == 2) { x = -rnd.Next(1, 2); y = rnd.Next(-1, 2); }
            else if (choice == 1) { x = rnd.Next(-1, 2); y = rnd.Next(1, 3); }
            else if (choice == 3) { x = rnd.Next(-1, 2); y = -rnd.Next(1, 3); }
        }

        private Point LocationGenerate(int choice)
        {
            int x=0; int y=0; Point location;
            if(choice == 0){ x = -100; y = rnd.Next(0, 720);}
            else if(choice == 1){ x = rnd.Next(0, 1024); y = -100; }
            else if(choice == 2){ x = 1024; y = rnd.Next(0, 720); }
            else if(choice == 3){ x = rnd.Next(0, 1024); y = 720; }
            
            location = new Point(x, y);
            return location;
        }
        public void Update()
        {
            if (!this.used && !this.shot)
            {
                this.location = new Point(this.location.X + x, this.location.Y + y);
                this.rect.Location = this.location;
                if (this.location.X > 1350 || this.location.X < -150 || this.location.Y > 800 || this.location.Y < -150) { this.used = true; }
            }
            else if (this.shot && this.type == 0)
            {                
                Program.Rocks.Add(new Rock(this.location,0));
                Program.Rocks.Add(new Rock(this.location,1));
            }
            else if (this.shot && this.type == 1)
            {
                Program.Rocks.Add(new Rock(this.location, 0, 1));
                Program.Rocks.Add(new Rock(this.location, 1, 1));
            }
        }
    }

    public class Boom
    {
        string path; public Bitmap image; public Point location; public int timer = 120;
        public Boom(Point position)
        {
            path = Path.Combine(Environment.CurrentDirectory +@"\Data\boom.png");
            this.image = new Bitmap(Image.FromFile(path),new Size(75,75));
            this.location = position;
        }
    }

    public class Background
    {
        string _path; public Image image; Point location;
        public Background()
        {
            this._path = Path.Combine(Environment.CurrentDirectory + @"\Data\Space.jpg");
            this.image = Image.FromFile(this._path);
            this.location = new Point(0, 0);
        }

        public void Draw(Graphics g) { g.DrawImage(this.image, this.location); }
    }
}
