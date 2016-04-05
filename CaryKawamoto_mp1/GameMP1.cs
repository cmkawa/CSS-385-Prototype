
#region Reference to system libraries
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.GamerServices;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Net;
    using Microsoft.Xna.Framework.Storage;
#endregion

// Reference to XNACS1Lib
using XNACS1Lib;

namespace CaryKawamoto_NameSpace
{
    // Subclass from XNACS1Base
    public class GameMP1 : XNACS1Base
    {
        #region Instance Variables

        // Set the speed of the ball
        const float SPEED = 0.5f;

        // Initialize game objects
        XNACS1Circle m_Ball;
        XNACS1Rectangle m_Paddle;

        string ballLocation = "";
        int paddleCount = 0;
        bool sliceMode = false;
        string sliceText = "";

        #endregion 

        protected override void InitializeWorld()
        {
            // Initialize window with origin (0, 0) and width 100
            World.SetWorldCoordinate(new Vector2(0f, 0f), 100.0f);
            World.SetBackgroundTexture("tarantula-nebula");

            // Background music
            PlayBackgroundAudio("Meteor Junction loop", 0.2f);

            // Ball is initially invisible in the center of the screen
            Vector2 centerPos = (World.WorldMax + World.WorldMin) / 2.0f;
            m_Ball = new XNACS1Circle(centerPos, 2.0f, "asteroid");
            m_Ball.RemoveFromAutoDrawSet();

            // Create paddle
            Vector2 aPos = new Vector2(45.0f, 5.0f);
            Vector2 bPos = new Vector2(55.0f, 5.0f);
            m_Paddle = new XNACS1Rectangle(aPos, bPos, 2.0f, "trak2_trim4a");
        }
        
        protected override void UpdateWorld()
        {
            if (GamePad.ButtonBackClicked())
                this.Exit();

            #region A Button: Reset ball, status, and paddle count
            if (GamePad.ButtonAClicked())
            {
                m_Ball.Center = (World.WorldMax + World.WorldMin) / 2.0f;
                m_Ball.AddToAutoDrawSet();
                m_Ball.VelocityX = RandomFloat(0.5f, 2.0f);
                m_Ball.VelocityY = RandomFloat(0.5f, 2.0f);
                m_Ball.ShouldTravel = true;

                ballLocation = "Ball is in play!";
                paddleCount = 0;
            }
            #endregion

            #region Y Button: Toggle Slice Mode on/off
            // Paddle bounce velocity affected by paddle velocity
            if (GamePad.ButtonYClicked())
            {
                sliceMode = !sliceMode;

                if (sliceMode)
                    sliceText = " - Slice Mode ON";
                else
                    sliceText = "";
            }
            #endregion

            #region Right stick: Moves paddle left and right
            m_Paddle.CenterX += GamePad.ThumbSticks.Right.X;
            World.ClampAtWorldBound(m_Paddle);
            #endregion

            #region Bound collision - change velocity, determine collision
            BoundCollideStatus collideStatus = World.CollideWorldBound(m_Ball);
            switch (collideStatus)
            {
                case BoundCollideStatus.CollideTop:
                    PlayACue("Bounce");
                    World.ClampAtWorldBound(m_Ball);
                    m_Ball.VelocityY = -m_Ball.VelocityY;
                    ballLocation = "Bounce TOP";
                    break;
                case BoundCollideStatus.CollideLeft:
                    PlayACue("Bounce");
                    World.ClampAtWorldBound(m_Ball);
                    m_Ball.VelocityX = -m_Ball.VelocityX;
                    ballLocation = "Bounce LEFT";
                    break;
                case BoundCollideStatus.CollideRight:
                    PlayACue("Bounce");
                    World.ClampAtWorldBound(m_Ball);
                    m_Ball.VelocityX = -m_Ball.VelocityX;
                    ballLocation = "Bounce RIGHT";
                    break;
                case BoundCollideStatus.CollideBottom:
                    PlayACue("Crash");
                    m_Ball.RemoveFromAutoDrawSet();
                    // reset ball, so that the cue does not loop
                    m_Ball.Center = (World.WorldMax + World.WorldMin) / 2.0f;
                    break;
            }
            #endregion

            #region Paddle collision - change velocity, increment, slice mode
            bool paddleBounce = m_Ball.Collided(m_Paddle);
            if (paddleBounce)
            {
                PlayACue("Crunch");
                m_Ball.VelocityY = -m_Ball.VelocityY;

                if (sliceMode)
                    m_Ball.VelocityX += GamePad.ThumbSticks.Right.X;

                paddleCount++;
            }
            #endregion

            #region Display bounce status and number of paddle hits
            if (m_Ball.IsInAutoDrawSet())
            {
                EchoToTopStatus(ballLocation);
            }
            else
            {
                EchoToTopStatus("Ball not in the world.");
            }

            EchoToBottomStatus(paddleCount.ToString() + sliceText);
            #endregion
        }
    }
}
