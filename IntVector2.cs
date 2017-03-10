using System;
using UnityEngine;

/*
UnityEngine.Vector2 has two problems
First of its not serializeable, which causes all kinds of problems when sending information over network and for creating save-files.
The other is that it is using float-points, which makes it prone to precision errors and making it unsuitable for using in data-structures such as maps.
IntVector2 solves these two problems and has a few utility functions to increase use-ability.
Many more functions could be added, but as a solo developer actually trying to get a final product, I have a strict principle of only writing functions on demand.
*/

[System.Serializable]
public struct IntVector2 : IEquatable<IntVector2>
{
	public int x;
	public int y;

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public bool Equals(IntVector2 other)
	{
		return other.x == x && other.y == y;
	}

	public static IntVector2 RoundFrom(Vector2 v2)
	{
		return new IntVector2(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
	}

	public static IntVector2 FloorFrom(Vector2 v2)
	{
		return new IntVector2(Mathf.FloorToInt(v2.x), Mathf.FloorToInt(v2.y));
	}

	static public IntVector2 zero
	{
		get
		{
			return new IntVector2(0, 0);
		}
	}

	static public IntVector2 operator * (IntVector2 a, int b)
	{
		return new IntVector2(a.x * b, a.y * b);
	}

	public static explicit operator Vector2(IntVector2 a)
	{
		return new Vector2(a.x, a.y);
	}

	public static explicit operator Vector3(IntVector2 a)
	{
		return new Vector3(a.x, a.y);
	}

	public override string ToString()
	{
		return string.Format("[X = {0}; Y = {1};]", x, y);
	}
}
