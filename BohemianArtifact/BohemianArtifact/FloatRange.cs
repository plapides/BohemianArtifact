using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BohemianArtifact
{
    // a class that varies a float up and down between a minimum and maximum at a given speed
    // the min/max and speed are varied slightly each time to make the range a bit more random
    public class FloatRange
    {
        public delegate void ChangeDirection();

        float f;
        float initial;

        float numSeconds;
        int movementDirection;
        float movementSpeed, initialMovementSpeed;

        float max, min; // values that are added to f
        float startValue, currentTarget;

        bool allowRandomSpeed, allowRandomTarget;

        private static Random random = new Random();

        event ChangeDirection changeDirectionEvent;

        public FloatRange(float initialValue, float numSeconds, float movementSpeed, float percentageMinMax)
            : this(initialValue, numSeconds, movementSpeed, -initialValue * percentageMinMax, initialValue * percentageMinMax)
        { }

        // initialValue: where you want the float to start
        // numSeconds: how long you want it to take to hit the min or max
        // movementSpeed: a percentage value indicating how fast you want the movement to occur: 1 means it takes numSeconds to do a cycle
        // minValue, maxValue are added to initialValue, so make them relative, not absolute values
        public FloatRange(float initialValue, float numSeconds, float movementSpeed, float minValue, float maxValue)
        {
            initial = initialValue;
            this.numSeconds = numSeconds;
            this.movementSpeed = initialMovementSpeed = movementSpeed;

            movementDirection = 1 * (random.Next(2) == 0 ? -1 : 1);

            max = maxValue;
            min = minValue;

            changeDirectionEvent = null;
            allowRandomSpeed = allowRandomTarget = true;

            initialize();
        }

        public void initialize()
        {
            f = currentTarget = initial;
            setTarget();
            setMovementSpeed();
        }

        public ChangeDirection ChangeDirectionEvent
        {
            set { changeDirectionEvent = value; }
        }

        public float Value
        {
            get { return f; }
        }

        public float InitialValue
        {
            get { return initial; }
            set { initial = value; }
        }

        public float MovementSpeed
        {
            get { return movementSpeed; }
            set
            {
                initialMovementSpeed = value;
                setMovementSpeed();
            }
        }

        public bool AllowRandomSpeed
        {
            get { return allowRandomSpeed; }
            set { allowRandomSpeed = value; }
        }

        public bool AllowRandomTarget
        {
            get { return allowRandomTarget; }
            set { allowRandomTarget = value; }
        }

        public int MovementDirection
        {
            get { return movementDirection; }
            set { movementDirection = value; }
        }

        public float Min
        {
            get { return min; }
            set { min = value; }
        }

        public float Max
        {
            get { return max; }
            set { max = value; }
        }

        private float getRandomBetween(float start, float end)
        {
            return (float)(start + (end - start) * random.NextDouble());
        }

        private void setTarget()
        {
            float percentage = 1;
            if (allowRandomTarget)
                percentage = getRandomBetween(0.5f, 1); // some random value between 50% and 100% of either min or max will be the new target
            startValue = currentTarget;
            currentTarget = initial + (movementDirection > 0 ? max : min) * percentage;
        }

        private void setMovementSpeed()
        {
            float percentage = 1;
            if (allowRandomSpeed)
                percentage = getRandomBetween(0.8f, 1.2f);
            movementSpeed = percentage * initialMovementSpeed;
        }

        public void performTimestep(double deltaTime)
        {
            f += (float)((currentTarget - startValue) / ((numSeconds * 60 / movementSpeed) * (deltaTime * 60)));
            if ((f >= currentTarget && movementDirection > 0) || (f <= currentTarget && movementDirection < 0))
            {
                f = currentTarget;
                movementDirection *= -1;
                setTarget();

                if (changeDirectionEvent != null)
                    changeDirectionEvent();
            }
        }

    }
}
