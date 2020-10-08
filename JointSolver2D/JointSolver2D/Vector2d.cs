using System;

public struct Vector2d
{
    public static readonly Vector2d Zero = new Vector2d();
    public static readonly Vector2d UnitX = new Vector2d(1,0);
    public static readonly Vector2d UnitY = new Vector2d(0,1);

    public double X;
    public double Y;

    public Vector2d(double x, double y)
    {
        this.X = (float)x;
        this.Y = (float)y;
    }

    public static Vector2d operator +(Vector2d v1, Vector2d v2)
    {
        return new Vector2d(v1.X + v2.X, v1.Y + v2.Y);
    }

    public static Vector2d operator -(Vector2d v1, Vector2d v2)
    {
        return new Vector2d(v1.X - v2.X, v1.Y - v2.Y);
    }

    public static Vector2d operator *(Vector2d v1, float m)
    {
        return new Vector2d(v1.X * m, v1.Y * m);
    }

    public static double operator *(Vector2d v1, Vector2d v2)
    {
        return v1.X * v2.X + v1.Y * v2.Y;
    }

    public static Vector2d operator /(Vector2d v1, float m)
    {
        return new Vector2d(v1.X / m, v1.Y / m);
    }

    public static bool operator ==(Vector2d v1, Vector2d v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y;
    }
    public static bool operator !=(Vector2d v1, Vector2d v2)
    {
        return v1.X != v2.X || v1.Y != v2.Y;
    }

    public static float Distance(Vector2d v1, Vector2d v2)
    {
        return (float)Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
    }

    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y);
    }

    public override string ToString()
    {
        return $"Vector2d {{{X},{Y}}}";
    }

    public string ToString_Short(string format = "")
    {
        return $"[{X.ToString(format)},{Y.ToString(format)}]";
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector2d v)
        {
            if (v.X == X && v.Y == Y)
                return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Returns the vector which is rotated anticlockwise by 90 degrees
    /// </summary>
    public Vector2d Perpendicular()
    {
        return new Vector2d(Y, -X);
    }

    /// <summary>
    /// Returns the Z component of a 3D dot product
    /// </summary>
    public static double CrossProduct(Vector2d v1, Vector2d v2)
    {
        return (v1.X* v2.Y) - (v1.Y* v2.X);
    }

    /// <summary>
    /// Returns the dot product of 2 vectors
    /// </summary>
    public static double DotProduct(Vector2d v1, Vector2d v2)
    {
        return (v1.X * v2.X) + (v1.Y * v2.Y);
    }

    /// <summary>
    /// Gets the angle between this vector and the +ve x axis
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>Angle in degrees</returns>
    public static double AngleDegrees(Vector2d vector)
    {
        return (180 * Angle(vector) / Math.PI);
    }

    /// <summary>
    /// Gets the angle between this vector and the +ve x axis
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>Angle in radians</returns>
    public static double Angle(Vector2d vector)
    {
        return (1 + Math.Atan2(vector.Y, vector.X));
    }

    /// <summary>
    /// Gets the angle between 2 vectors
    /// </summary>
    /// <returns>Angle in degrees</returns>
    public static double AngleBetween(Vector2d vector1, Vector2d vector2)
    {
        return (180 * (1 + Math.Atan2((vector1.Y - vector2.Y), (vector1.X - vector2.X)) / Math.PI));
    }

    /// <summary>
    /// Rotate (anticlockwise) a point about an origin
    /// </summary>
    /// <param name="point">Point to be rotated</param>
    /// <param name="origin">Rotation origin</param>
    /// <param name="angle">Anticlockwise angle in radians</param>
    /// <returns></returns>
    public static Vector2d RotatePoint(Vector2d point, Vector2d origin, double angle)
    {
        double X = origin.X + ((point.X - origin.X) * Math.Cos(angle) - (point.Y - origin.Y) * Math.Sin(angle));
        double Y = origin.Y + ((point.X - origin.X) * Math.Sin(angle) + (point.Y - origin.Y) * Math.Cos(angle));
        return new Vector2d(X, Y);
    }

    /// <summary>
    /// Get a new point which has been offset from the original point by a distance at an anticlockwise angle.
    /// </summary>
    /// <param name="origin">Orignal point</param>
    /// <param name="distance">Distance to offset by</param>
    /// <param name="angle">Anti clockwise angle in radians</param>
    /// <returns>New offset point</returns>
    public static Vector2d GetOffsetPoint(Vector2d origin, float distance, float angle)
    {
        return new Vector2d(origin.X + distance * Math.Cos(angle), origin.Y + distance * Math.Sin(angle));
    }


}




