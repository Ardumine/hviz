using System;
using System.Numerics;
class Meth
{

    /// <summary>
    /// Calcula a diferença entre dois angulos.
    /// Ex:
    /// calculateDifferenceBetweenAngles(0, 350) = -10
    /// Meu ang é 0 e tenho de ir para 350 graus. tenho de mexer 10 graus.
    /// </summary>
    /// <param name="firstAngle">Angulo 1 em deg</param>
    /// <param name="secondAngle">Angulo 2 em deg</param>
    /// <returns>Grau entre -180 e 180 em deg</returns>
    public static double calcDifBetweenAngles(double firstAngle, double secondAngle)
    {
        double difference = secondAngle - firstAngle;
        while (difference < -180) difference += 360;
        while (difference > 180) difference -= 360;
        return difference;
    }

    public static double Deg4Rad(double degrees)
    {
        //double radians = (Math.PI / 180) * (degrees);
        //return radians;
        return double.DegreesToRadians(degrees);

    }
    public static double Rad4Deg(double radians)
    {
        //double degrees = (180 / Math.PI) * radians;
        //return degrees - (int)(degrees / 360) * 360;
        return double.RadiansToDegrees(radians);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="posSrc"></param>
    /// <param name="angSrcRad">Deg</param>
    /// <param name="posObj"></param>
    /// <returns>Angulo de dif em Deg</returns>
    public static double ObterDifAng(System.Numerics.Vector2 posSrc, double angSrcRad, System.Numerics.Vector2 posObj)
    {
        // Calculate the difference vector between target and current positions
        double dx = posObj.X - posSrc.X;
        double dy = posObj.Y - posSrc.Y;

        // Calculate the angle to the target point in radians
        double targetAngleRadians = Math.Atan2(dy, dx);

        // Convert both angles to degrees for easier understanding and calculation
        double targetAngleDegrees = Rad4Deg(targetAngleRadians);

        // Calculate the difference in angle between current orientation and target position
        double angleDifference = targetAngleDegrees - Meth.Rad4Deg(angSrcRad);

        // Normalize the angle difference to be within [-180, 180] degrees
        if (angleDifference > 180) { angleDifference -= 360; }
        else if (angleDifference < -180) { angleDifference += 360; }
        return angleDifference;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="posSrc"></param>
    /// <param name="posObj"></param>
    /// <returns>Angulo de dif em rad</returns>
    public static double ObterDifAngRad(System.Numerics.Vector2 posSrc, System.Numerics.Vector2 posObj)
    {
        // Calculate the difference vector between target and current positions
        double dx = posObj.X - posSrc.X;
        double dy = posObj.Y - posSrc.Y;

        // Calculate the angle to the target point in radians
        double targetAngleRadians = Math.Atan2(dy, dx);

        return targetAngleRadians;
    }


    public static int SpeedControl(int desiredSpeed, int MaxSpeed)
    {
        // Clamp the desired speed to the range of [-MaxSpeed, MaxSpeed]
        if (desiredSpeed > MaxSpeed)
        {
            return MaxSpeed;
        }
        else if (desiredSpeed < -MaxSpeed)
        {
            return -MaxSpeed;
        }
        else
        {
            return desiredSpeed;
        }
    }
    public static Godot.Vector2 Lerp(Godot.Vector2 First, Godot.Vector2 Second, float Amount)
    {
        float retX = Lerp(First.X, Second.X, Amount);
        float retY = Lerp(First.Y, Second.Y, Amount);
        return new Godot.Vector2(retX, retY);
    }
    public static Godot.Vector3 Lerp(Godot.Vector3 First, Godot.Vector3 Second, float Amount)
    {
        float retX = Lerp(First.X, Second.X, Amount);
        float retY = Lerp(First.Y, Second.Y, Amount);
        float retZ = Lerp(First.Z, Second.Z, Amount);
        return new Godot.Vector3(retX, retY, retZ);
    }
    public static float Lerp(float First, float Second, float Amount)
    {
        return First * (1 - Amount) + Second * Amount;
    }

    public static double ObterDist(Vector2 v1, Vector2 v2)
    {
        return Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
    }
    public static double ObterDist(System.Drawing.Point v1, System.Drawing.Point v2)
    {
        return Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
    }


}