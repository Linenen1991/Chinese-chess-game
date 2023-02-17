using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Gi;
namespace Gi
{
    //Pawn Bodyguard Gen Bishop 恐受地形影響
    //Cannon Generals override move
    
    public class CGi
    {
        public static int Self;
        public static Vector2 DeadPoint = new Vector2(-64, -64);
        public Vector2 Position;
        public int Number, XStride, Subjection, XStart, YStride;
        public Rectangle Picture;
        public virtual bool Move(CGi To)
        {
            if(Rule(To.Position))
            {
                if (To.Picture.X == 0)
                    Game1.Winner = this.Subjection;
                this.Position = To.Position;
                To.Position = DeadPoint;
                return true;
            }
            return false;
        }
        public virtual bool Move(Vector2 To)
        {
            if (Rule(To))
            {
                this.Position = To;
                return true;
            }
            return false;
        }
        public virtual bool Rule(Vector2 To) { return false; }
        public void Initialize()
        {
            if (Subjection == Self) YStride=9-YStride;
            Position = Game1.Coordinate(XStart + Number * XStride, YStride);
        }
    }
    
    public class Pawn : CGi
    {
        public Pawn(int sub, int ArguNumber)
        {
            Subjection = sub;
            Number = ArguNumber;
            XStart = 0;
            XStride = 2;
            YStride = 3;
            Picture = new Rectangle(384, Subjection * 64, 64, 64);
            Initialize();
        }
        public override bool Rule(Vector2 To)
        {
            int X = (int)(To.X - this.Position.X), Y = (int)(To.Y - this.Position.Y);
            if (CGi.Self == 0)
            {
                if (this.Position.Y > 380)
                {
                    if ((X == 0) && (Y == -72 || (Y == -73)))
                        return true;
                    else
                        return false;
                }
            }
            else
            {
                if (this.Position.Y < 380)
                {
                    if ((X == 0) && ((Y == 72) || (Y == 73)))
                        return true;
                    return false;
                }
            }
            if ((Y > 0) || (Math.Abs(X) + Math.Abs(Y) > 74))
                return false;
            return true;

        }
    }
    
    public class Cannon : CGi
    {
        
        public Cannon(int sub, int ArguNumber)
        {
            Subjection = sub;
            Number = ArguNumber;
            Picture = new Rectangle(320, Subjection * 64, 64, 64);
            XStart = 1;
            XStride = 6;
            YStride = 2;
            Initialize();
        }
        public override bool Move(CGi To)
        {
            
            bool XisSame;
            if(To.Position.X==this.Position.X)
                XisSame = true;
            else if(To.Position.Y==this.Position.Y)
                XisSame = false;
            else
                return false;
            int Number = Game1.Collision(this.Position, To.Position, XisSame);
            if (Number == 1)
            {
                if (To.Picture.X == 0)
                    Game1.Winner = this.Subjection;
                this.Position = To.Position;
                To.Position = DeadPoint;
                return true;
            }
            return false;
        }
        public override bool Move(Vector2 To)
        {
           
            bool Flag = false;
            if (To.X == this.Position.X)
                Flag = true;
            else if (To.Y == this.Position.Y)
                Flag = false;
            else
                return false;
            int Number = Game1.Collision(this.Position, To, Flag);
            if (Number == 0)
            {
                this.Position = To;
                return true;
            }
            return false;
        }
    }
    
    public class Rook : CGi
    {
        public Rook(int sub, int ArguNumber)
        {
            Subjection = sub;
            Number = ArguNumber;
            Picture = new Rectangle(192, Subjection * 64, 64, 64);
            XStart = 0;
            XStride = 8;
            YStride = 0;
            Initialize();
        }
        public override bool Rule(Vector2 To)
        {
            
            bool XisSame;
            if (To.X == this.Position.X)
                XisSame = true;
            else if (To.Y == this.Position.Y)
                XisSame = false;
            else
                return false;
            int Number = Game1.Collision(this.Position, To, XisSame);
            if (Number == 0)
                return true;
            return false;
        }
    }

    public class Horse : CGi
    {
        public Horse(int sub, int ArguNumber)
        {
            Subjection = sub;
            Number = ArguNumber;
            Picture = new Rectangle(256, Subjection * 64, 64, 64);
            XStart = 1;
            XStride = 6;
            YStride = 0;
            Initialize();
        }
        public override bool Rule(Vector2 To)
        {
            int X = (int)(To.X - this.Position.X), Y = (int)(To.Y - this.Position.Y);
            int Value=9;
            if ((Math.Abs(X) == 144) && ((Math.Abs(Y) < 75) && (Math.Abs(Y) > 71)))
                Value = Game1.Collision(this.Position, To, false);
            else if ((Math.Abs(X) == 72) && (Math.Abs(Y) > 143) && (Math.Abs(Y) < 147))
                Value = Game1.Collision(this.Position, To, true);
            if(Value==0)
                return true;
            return false;
        }
    }

    public class Bishop : CGi
    {
        public Bishop(int sub, int ArguNumber)
        {
            Subjection = sub;
            Number = ArguNumber;
            Picture = new Rectangle(128, Subjection * 64, 64, 64);
            XStart = 2;
            XStride = 4;
            YStride = 0;
            Initialize();
        }
        public override bool Rule(Vector2 To)
        {
            int Value = 2;
            int X = (int)(To.X - this.Position.X), Y = (int)(To.Y - this.Position.Y);
            if (CGi.Self == 0)
            {
                if (To.Y < 384)
                    return false;
            }
            else
            {
                if (To.Y > 384)
                    return false;
            }
            if ((Math.Abs(X) == 144) && (Math.Abs(Y) <147) &&(Math.Abs(Y)>143))
                Value=Game1.Collision(new Vector2(this.Position.X + X / 2, this.Position.Y), To, true);
                if(Value==0)
                    return true;
            return false;
        }
    }
    
    public class Bodyguard : CGi
    {
         public Bodyguard(int sub, int ArguNumber)
        {
            Subjection = sub;
            Number = ArguNumber;
            Picture = new Rectangle(64, Subjection * 64, 64, 64);
            XStart = 3;
            XStride = 2;
            YStride = 0;
            Initialize();
        }
         public override bool Rule(Vector2 To)
         {
             int X = (int)To.X, Y = (int)To.Y;
             int XD = X - (int)this.Position.X, YD = Y - (int)this.Position.Y;
             if (CGi.Self==0)
             {
                 if (((X < 201) || (X > 347)) && ((Y < 531) || (Y > 712)))
                     return false;
             }
             else
             {
                 if (((X < 201) || (X > 347)) && ((Y < 55) || (Y > 200)))
                     return false;
             }
             if ((Math.Abs(XD)==72)&&((Math.Abs(YD)==72)||(Math.Abs(YD)==73)))
                 return true;
             return false;
         }
    }

    public class Generals : CGi
     {
        public Generals(int sub)
        {
            Subjection = sub;
            Number = 0;
            Picture = new Rectangle(0, Subjection * 64, 64, 64);
            XStart = 4;
            XStride = 0;
            YStride = 0;
            Initialize();
        }
        public override bool Move(CGi To)
        {
            if (To.Picture.X == 0)
            {
                if (Game1.Collision(this.Position, To.Position, false) == 0)
                {
                    Game1.Winner = this.Subjection;
                    return true;
                }
                return false;
            }
            else
                return base.Move(To);
        }
        public override bool Rule(Vector2 To)
        {
            int X = (int)To.X, Y = (int)To.Y;
            int Distance = Math.Abs(X-(int)this.Position.X)+Math.Abs(Y-(int)this.Position.Y);
            if (CGi.Self == 0)
            {
                if (((X < 201) || (X > 347)) && ((Y < 531) || (Y > 712)))
                    return false;
            }
            else
            {
                if (((X < 201) || (X > 347)) && ((Y < 55) || (Y > 200)))
                    return false;
            }
            if ((Distance>71)&&(Distance<74))
                return true;
            return false;
        }
     }


}