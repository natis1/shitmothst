using System;
using UnityEngine;

namespace shitmothst
{
    public class dash_antics
    {
        public GameObject voidKnight;

        private double theta;

        private double time;
        private bool setupSpiral;
        private double spiralAngel;
        
        private const double maxTime = 0.5;
        private const double timeIncrement = 0.02;
        private double relX;
        private double relY;

        
        
        
        

        public void resetAngelMemes()
        {
            theta = 0;
            time = 0;
            relX = 0;
            relY = 0;
            setupSpiral = false;
        }

        public Vector2 logSpiral(Vector2 initDirection)
        {
            const double angleMult = -0.4;
            
            if (!setupSpiral)
            {
                relX = (dash_hooks.rng.NextDouble() - 0.5) * 20.0;
                relY = (dash_hooks.rng.NextDouble() - 0.5) * 20.0;

                spiralAngel = Math.Atan2(initDirection.y, initDirection.x);
                Modding.Logger.Log("Angel is " + spiralAngel + " and relX is " + relX + " and relY is " + relY);
                
                setupSpiral = true;
            }
            
            // so x on a parametric log spiral is given by the following:
            // x = a e^(b * -t) cos(t)
            // a should always be equal to relX
            // b of -0.4 gives a good spiral in my opinion
            // the partial deriv of this with respect to t is
            
            // e^(-0.4 t) * (-2.6 a sin(2.6 t) - 0.4 a cos(2.6 t) )
            
            Vector2 velocityVec = new Vector2(
                (float) (Math.Pow(Math.E, (-0.4 * time)) * (-2.6 * relX * Math.Sin(2.6 * time + spiralAngel)
                                                           -0.4 * relX * Math.Cos(2.6 * time + spiralAngel) ) ),
                
                (float) (Math.Pow(Math.E, (-0.4 * time)) * ( 2.6 * relY * Math.Cos(2.6 * time + spiralAngel)
                                                           -0.4 * relY * Math.Sin(2.6 * time + spiralAngel) ) )
                );

            time += timeIncrement;

            return velocityVec;
        }
        
        // Turning my direction.
        public Vector2 diagonalDash(Vector2 currentDirection)
        {
            const double turnPerRun = 4.0 * Math.PI * (2.0 / 50.0);
            float magnitude = currentDirection.magnitude;

            
            // dashing down or to the left? send the user up and to the right
            if (currentDirection.y < 0 || currentDirection.x < 0)
            {
                currentDirection.x = (float) (magnitude * Math.Sin(theta)) * 2.0f;
                currentDirection.y = (float) (magnitude * Math.Sin(theta)) * 2.0f;
            }
            // dashing up? send the user down and to the right
            else if (currentDirection.y > 0)
            {
                currentDirection.x = (float) (magnitude *  Math.Sin(theta)) * 2.0f;
                currentDirection.y = (float) (magnitude * -Math.Sin(theta)) * 2.0f;
            }
            // dashing to the right? Send the user up and to the left
            else
            {
                currentDirection.x = (float) (magnitude * -Math.Sin(theta)) * 2.0f;
                currentDirection.y = (float) (magnitude *  Math.Sin(theta)) * 2.0f;
            }

            theta += turnPerRun;
            return currentDirection;
        }


        public Vector2 inverseDash(Vector2 currentDirection)
        {
            currentDirection *= -1;
            return currentDirection;
        }

        public Vector2 waveDash(Vector2 currentDirection)
        {
            const double turnPerRun = 4 * Math.PI * (2.0 / 50.0);
            float magnitude = currentDirection.magnitude;
            
            if (Math.Abs(currentDirection.y) < 0.001f)
            {
                currentDirection.x = currentDirection.x + (float) (magnitude * Math.Sin(theta));
                currentDirection.y = (float) (magnitude * Math.Sin(theta));
            } else if (Math.Abs(currentDirection.x) < 0.001f)
            {
                currentDirection.x = (float) (magnitude * Math.Sin(theta));
                currentDirection.y = currentDirection.y + (float) (magnitude * Math.Sin(theta));
            }
            else
            {
                currentDirection.x = currentDirection.x + (float) (magnitude * Math.Sin(theta));
                currentDirection.y = currentDirection.y + (float) (magnitude * Math.Sin(theta));
            }

            theta += turnPerRun;
            return currentDirection;
        }
        
        



    }
}