using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
/*
* 0為紅色 1為黑色
* Heightspac 異動 72 73
* int Mode 0 正常可讓棋 1戰俘可讓棋 2正常不讓 3戰俘不讓 4單人Debug
* CGi.Self 0 紅 1黑 紅先
* int Phase 0開起房間階段 1初始並讓棋 2等候開始 3開始 等待重起 從1開始
*/
namespace Gi
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D Chessboard;
        Texture2D Chess;
        Texture2D EmptyChess;
        SpriteFont Font,Font2;
        SoundEffect SE;
        Song Songs;
        String IP = "";
        static List<CGi>[] All = new List<CGi>[2];
        List<CGi> DespiseList = new List<CGi>();
        static CGi Empty;
        CGi Pick, Pick2;
        MouseState NPoint, LPoint;
        KeyboardState LastCin,Cin;
        Vector2 Target;
        int Xremain = 0, Yremain = 0, Mode = 4, Phase = 0;
        public static int Round = 0,Winner=4;
        const int XBound = 58, YBound = 56, Heightspac = 73, Widthspac = 72;
        bool Pickup = false, Captive = false, Ready = false;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            Window.AllowUserResizing = false;
            graphics.ApplyChanges();
            IsMouseVisible = true;
            NPoint = Mouse.GetState();
            LPoint = NPoint;
            CGi.Self = 0;
            Empty = new CGi();
            Empty.Position = new Vector2(-128, -128);
            Pick = Pick2 = Empty;
            Pick.Position = Target;
            Empty.Picture = new Rectangle(5, 5, 5, 5);
            All[0] = new List<CGi>();
            All[1] = new List<CGi>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Chessboard = Content.Load<Texture2D>("Chessboard");
            Chess = Content.Load<Texture2D>("ChineseChess");
            EmptyChess = Content.Load<Texture2D>("emptyChess");
            Font = Content.Load<SpriteFont>("Chinese");
            Songs = Content.Load<Song>("BGM");
            SE = Content.Load<SoundEffect>("Sound");
            Font2 = Content.Load<SpriteFont>("AA");
            MediaPlayer.Play(Songs);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Phase == 0) //開房
                KeyboardDetect();
            else if (Phase == 1)//初始
            {
                KeyboardDetect();
                if ((Mode < 2) || (Mode == 4))
                {
                    MouseDetect();
                    Despise();
                }
                if (Ready)
                {
                    Phase++;
                    Pick = Empty;
                    Pick2 = Empty;
                    Target = Vector2.Zero;
                }
            }
            else if (Phase == 2)//等候
            {
                Winner = 2;
                if (Mode == 4)
                {
                    Phase++;
                    DespiseList.Clear();
                }
                KeyboardDetect();
                if (!Ready)
                    Phase--;
                ;//等待對方完成
            }
            else if (Phase == 3)//開始
            {
                Ready = false;
                CaptiveMode();
                if (Mode == 4)
                    CGi.Self = Round % 2;
                if (Winner < 2)
                {
                    Phase = 1;
                    if (Mode == 4)
                        CGi.Self = 0;
                    Round = 0;
                    ReStart();
                }
            }
           
            base.Update(gameTime);
        }
        public void Despise()
        {
            if (PickSelf())
            {
                if (Pick.Picture.X == 0)
                    return;
                DespiseList.Add(Pick);
                All[CGi.Self].Remove(Pick);
                return;
            }
            else
            {
                foreach (CGi i in DespiseList)
                    if (i.Position==Target)
                    {
                        All[CGi.Self].Add(i);
                        return;
                    }
            }
        }
        
        public void CaptiveMode()
        {
            if (!Captive)
            {
                MouseDetect();
                if (!PickSelf())
                    PickToCGi();
            }
            else
            {
                MouseDetect();
                if (Target != Vector2.Zero && CheckPosition())
                {
                    All[1 - CGi.Self].Remove(Pick2);
                    Pick2.Picture = new Rectangle(Pick2.Picture.X, CGi.Self * 64, 64, 64);
                    Pick2.Position = Target;
                    All[CGi.Self].Add(Pick2);
                        Captive = false;
                    Pick = Empty;
                    Pick2 = Empty;
                    SE.Play();
                    Round++; 
                }
            }
        }
        /*
* 0為紅色 1為黑色
* Heightspac 異動 72 73
* int Mode 0 正常可讓棋 1戰俘可讓棋 2正常不讓 3戰俘不讓 4單人Debug
* CGi.Self 0 紅 1黑 紅先
* int Phase 0開起房間階段 1初始並讓棋 2等候開始 3開始 等待重起 從1開始
*/
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            if (Phase == 0)
            {
                if (Mode == 0)
                    spriteBatch.DrawString(Font, "正常可讓棋", new Vector2(512, 300), Color.Red);
                else
                    spriteBatch.DrawString(Font, "正常可讓棋", new Vector2(512, 300), Color.White);
                if(Mode==1)
                    spriteBatch.DrawString(Font, "戰俘可讓棋", new Vector2(512, 330), Color.Red);
                else
                    spriteBatch.DrawString(Font, "戰俘可讓棋", new Vector2(512, 330), Color.White);
                if(Mode==2)
                    spriteBatch.DrawString(Font, "正常不讓", new Vector2(512, 360), Color.Red);
                else
                    spriteBatch.DrawString(Font, "正常不讓", new Vector2(512, 360), Color.White);
                if(Mode==3)
                    spriteBatch.DrawString(Font, "戰俘不讓", new Vector2(512, 390), Color.Red);
                else
                    spriteBatch.DrawString(Font, "戰俘不讓", new Vector2(512, 390), Color.White);
                if(Mode==4)
                    spriteBatch.DrawString(Font, "單人Debug", new Vector2(512, 420), Color.Red);
                else
                    spriteBatch.DrawString(Font, "單人Debug", new Vector2(512, 420), Color.White);
                if(Mode==5)
                    spriteBatch.DrawString(Font2, "離開遊戲", new Vector2(512, 450), Color.Red);
                else
                    spriteBatch.DrawString(Font2, "離開遊戲", new Vector2(512, 450), Color.White);

            }
            if ((Phase == 2) || (Phase == 1))
            {
                if (Ready)
                    spriteBatch.DrawString(Font, "準備就緒", new Vector2(800, 100), Color.Red);
                else
                    spriteBatch.DrawString(Font, "R鍵準備就緒", new Vector2(800, 100), Color.White);
                spriteBatch.DrawString(Font, "將要移除的棋子\r\n點住小拖移 就可去除\r\n要復原只要在原點\r\n點住小拖移", new Vector2(700, 200), Color.White);
                if(Winner==0)
                    spriteBatch.DrawString(Font, "紅色獲勝", new Vector2(800, 35), Color.Red);
                else if(Winner==1)
                    spriteBatch.DrawString(Font, "黑色獲勝", new Vector2(800, 35), Color.Black);
            }
            if ((Phase == 1) || (Phase == 2) || (Phase==3))
            {
                if((Round%2)==0)
                    spriteBatch.DrawString(Font, "紅色回合", new Vector2(800, 5), Color.Red);
                else
                    spriteBatch.DrawString(Font, "黑色回合", new Vector2(800, 5), Color.Black);
                spriteBatch.Draw(Chessboard, new Vector2(42, 42), null, Color.White, 0, Vector2.Zero, 0.75f, SpriteEffects.FlipVertically, 0);
                foreach (CGi i in All[0])
                    spriteBatch.Draw(Chess, i.Position, i.Picture, Color.White);
                foreach (CGi i in All[1])
                    spriteBatch.Draw(Chess, i.Position, i.Picture, Color.White);
                if (Pick != Empty)
                    spriteBatch.Draw(EmptyChess, Pick.Position, Color.Yellow);
                else
                    spriteBatch.Draw(EmptyChess, new Vector2(-64, -64), Color.Yellow);
                if (Captive)
                    spriteBatch.Draw(Chess, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Pick2.Picture, Color.White);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
        //MouseDetect 改變Target
        public void MouseDetect()
        {
            NPoint = Mouse.GetState();
            Target = Vector2.Zero;
            if (NPoint.LeftButton != LPoint.LeftButton)
            {
                int X = NPoint.X, Y = NPoint.Y;
                if ((X < 42) || (X > 648) || (Y < 43) || (Y > 723))
                    return;
                Xremain = (X - XBound) % Widthspac;
                if (Xremain < 20)
                {
                    Yremain = (Y - YBound) % Heightspac;
                    if (Yremain < 20)
                        Target = Coordinate(X, Y, 0, 0);
                    else if (Yremain > 52)
                        Target = Coordinate(X, Y, 0, 1);
                }
                else if (Xremain > 52)
                {
                    Yremain = (Y - YBound) % Heightspac;
                    if (Yremain < 20)
                        Target = Coordinate(X, Y, 1, 0);
                    else if (Yremain > 52)
                         Target = Coordinate(X, Y, 1, 1);
                }
            }
            LPoint = NPoint;
        }

        public void MenuMouse()
        {
            
        }
        
        public static Vector2 Coordinate(int XArgu, int YArgu, int XWeight, int YWeight)
        {
            int X = (XArgu - XBound) / Widthspac + XWeight, Y = (YArgu - YBound) / Heightspac + YWeight;
            return new Vector2(X * Widthspac + XBound - 32, Y * Heightspac + YBound - 32 - Y / 2);
        }
        public static Vector2 Coordinate(int X, int Y)
        {
            return new Vector2(X * Widthspac + XBound - 32, Y * Heightspac + YBound - 32 - Y / 2);
        }
        public bool PickSelf()
        {
            foreach (CGi i in All[CGi.Self])
                if (Target == i.Position)
                {
                    Pick = i;
                    Pickup = true;
                    return true;
                }
            return false;
        }
        public void PickToCGi()
        {
            Pick2 = Empty;
            foreach(CGi i in All[1-CGi.Self])
                if (Target == i.Position)
                    Pick2 = i;
            if (Pickup)
                MoveOrEat();
        }
        public void MoveOrEat()
        {
            if (Pick2 == Empty)
            {
                if (Pick.Move(Target))
                {
                    Round++;
                    SE.Play();
                    Pick = Empty;
                    Pick2 = Empty;
                    return;
                }
            }
            if (Pick.Move(Pick2)&&((Mode==1)||(Mode==3)||(Mode==4)))
            {
                Captive = true;
            }
        }
        public bool CheckPosition()
        {
            foreach (CGi i in All[0])
                if(Target==i.Position)
                    return false;
            foreach (CGi i in All[1])
                if (Target == i.Position)
                    return false;
            return true;
        }
        public static int Collision(Vector2 PointA, Vector2 PointB, bool SameIsX)
        {
            int Value = 0;
            if (SameIsX)
            {
                float Same = PointA.X;
                int Max = Math.Max((int)PointA.Y, (int)(PointB.Y)), Min = Math.Min((int)PointA.Y, (int)(PointB.Y));
                foreach (CGi i in All[0])
                    if ((Same == i.Position.X) && (i.Position.Y < Max) && (i.Position.Y > Min))
                        Value++;
                foreach (CGi i in All[1])
                    if ((Same == i.Position.X) && (i.Position.Y < Max) && (i.Position.Y > Min))
                        Value++;
            }
            else
            {
                float Same = PointA.Y;
                int Max = Math.Max((int)PointA.X, (int)(PointB.X)), Min = Math.Min((int)PointA.X, (int)(PointB.X));
                foreach (CGi i in All[0])
                    if ((Same == i.Position.Y) && (i.Position.X < Max) && (i.Position.X > Min))
                        Value++;
                foreach (CGi i in All[1])
                    if ((Same == i.Position.Y) && (i.Position.X < Max) && (i.Position.X > Min))
                        Value++;
            }
            return Value;
        }

        void ReStart()
        {
            All[0].Clear();
            All[1].Clear();
            Pick = Empty;
            Pick2 = Empty;
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 5; i++)
                    All[j].Add(new Pawn(j, i));
                for (int i = 0; i < 2; i++)
                {
                    All[j].Add(new Cannon(j, i));
                    All[j].Add(new Horse(j, i));
                    All[j].Add(new Rook(j, i));
                    All[j].Add(new Bishop(j, i));
                    All[j].Add(new Bodyguard(j, i));
                }
                All[j].Add(new Generals(j));
            }
        }

        private void KeyboardDetect()
        {
            Cin = Keyboard.GetState();
            if (Cin.GetPressedKeys().FirstOrDefault<Keys>() != LastCin.GetPressedKeys().FirstOrDefault<Keys>())
            {
                if (Cin.IsKeyDown(Keys.NumPad0))
                    IP += '0';
                else if (Cin.IsKeyDown(Keys.NumPad1))
                    IP += '1';
                else if (Cin.IsKeyDown(Keys.NumPad2))
                    IP += '2';
                else if (Cin.IsKeyDown(Keys.NumPad3))
                    IP += '3';
                else if (Cin.IsKeyDown(Keys.NumPad4))
                    IP += '4';
                else if (Cin.IsKeyDown(Keys.NumPad5))
                    IP += '5';
                else if (Cin.IsKeyDown(Keys.NumPad6))
                    IP += '6';
                else if (Cin.IsKeyDown(Keys.NumPad7))
                    IP += '7';
                else if (Cin.IsKeyDown(Keys.NumPad8))
                    IP += '8';
                else if (Cin.IsKeyDown(Keys.NumPad9))
                    IP += '9';
                else if (Cin.IsKeyDown(Keys.Decimal))
                    IP += '.';
                else if (Cin.IsKeyDown(Keys.Divide))
                    IP += '/';
                else if (Cin.IsKeyDown(Keys.Back) && IP.Length > 0)
                    IP = IP.Remove(IP.Length - 1);
                else if (Cin.IsKeyDown(Keys.Down))
                {
                    if (++Mode > 5)
                        Mode = 5;
                }
                else if (Cin.IsKeyDown(Keys.Up))
                {
                    if (--Mode < 0)
                        Mode = 0;
                }
                else if (Cin.IsKeyDown(Keys.Left))
                    CGi.Self = 0;
                else if (Cin.IsKeyDown(Keys.Right))
                    CGi.Self = 1;
                else if (Cin.IsKeyDown(Keys.Enter))
                {
                        Phase++;
                        if (Mode == 4)
                            CGi.Self = 0;
                        if (Mode == 5)
                            Exit();
                        ReStart();  
                    ;//Connect to  
                }
                else if (Cin.IsKeyDown(Keys.Space))
                    IP += ' ';
                else if (Cin.IsKeyDown(Keys.R) && ((Phase == 1) || (Phase == 2))) 
                {
                    if (Ready)
                        Ready = false;
                    else
                        Ready = true;
                }
            }
            LastCin = Cin;
        }
    }
}



 