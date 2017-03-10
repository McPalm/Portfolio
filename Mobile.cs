using UnityEngine;
using System.Collections.Generic;

/*
An A* implementation for a simulation game.
It navigates a 2D map that consist of Tiles.
It lacks logic for updating nodes position in the queue if it find a better path.
It rarely comes up in practice and the impact is minimal, however so addressing that issue is low priority.
*/

public class Mobile : MapObject
{
	[SerializeField, Range(0.1f, 20f)]
	public float speed = 2f;

	Stack<Tile> _path;
	Tile currentDestination;
	Tile lastTile;

	public bool MoveTo(Tile destination)
	{
		if (currentDestination == destination) return true; // If the destination is the same as the current, we dont recalculate the path.
		bool r = FindPath(Tile, destination); // Tile is the Tile we stand on in the parent-class MapObject
		if (r) currentDestination = destination;
		return r;
	}
	void OnDisable()
	{
		currentDestination = null;
	}

	public void Stop()
	{
		currentDestination = null;
	}
	
	// Update is called once per frame
	void Update()
	{
		if(Arrived == false)
		{
			if ( ( _path.Count > 1 && AtCheckpoint(0.3f) )
				||
				( _path.Count == 1 && AtCheckpoint(0.1f) ) )_path.Pop();
			if(_path.Count > 0) MoveToNextWaypoint();
			if (lastTile == null) lastTile = Tile;
			if(lastTile != Tile)
			{
				if (lastTile.Room != Tile.Room)
				{
					if (Tile.Room != null) Tile.Room.EnterRoomEvent.Invoke(this);
					if (lastTile.Room != null) lastTile.Room.LeaveRoomEvent.Invoke(this);
				}
				lastTile = Tile;
			}
		}
	}

	bool AtCheckpoint(float distance)
	{
		return MapUtility.Delta(pt.Position, _path.Peek().transform.position) < distance;
	}

	void MoveToNextWaypoint()
	{
		if (_path.Peek() == null)
		{
			Stop();
		}
		else
		{
			Vector3 delta = _path.Peek().transform.position - pt.Position;
			Vector3 direction = delta.normalized;
			float distance = Time.deltaTime * speed;
			if (distance > delta.magnitude) distance = delta.magnitude;
			pt.Position += direction * distance;
		}
	}

	public bool Arrived { get { return currentDestination == null  || currentDestination != null && _path.Count == 0; } }

	/// <summary>
	/// A* pathfinding implementation
	/// </summary>
	/// <param name="start"></param>
	/// <param name="goal"></param>
	bool FindPath(Tile start, Tile goal)
	{
		if (!start) throw new System.Exception("No start tile for A* pathfinding!");
		if (!goal) throw new System.Exception("No goal tile for A* pathfinding!");
		HashSet<Tile> closed = new HashSet<Tile>();
		PriorityQueue<EvalTile> open = new PriorityQueue<EvalTile>();
		// add start tile
		EvalTile et = new EvalTile(start, 0f, Distance(start.transform.position, goal.transform.position));
		open.Add(et, et.fScore);
		int failsafe = 25000;
		while(!open.IsEmpty && failsafe-- > 0)
		{
			if (failsafe == 0)
			{
				Debug.LogError("Pathfinder Failsafe Activated!");
				print("Closed Size: " + closed.Count);
				print("Open Size: " + open.Count);
				print("Start: " + start);
				print("Goal: " + goal);
			}
			et = open.Dequeue();

			if (failsafe == 0) print("Top of Open Stack: " + et);
			
			closed.Add(et.tile);
			if (et.tile == goal)
			{
				// construct a path
				_path = new Stack<Tile>();
				_path.Push(et.tile);
				while(et.cameFrom != null)
				{
					et = et.cameFrom;
					_path.Push(et.tile);
				}
				return true;
			}
			for(int i = 0; i < 4; i++)
			{
				if (failsafe == 0) print(i);
				Tile neighbor = null;
				if (i == 0) neighbor = et.tile.NE;
				if (i == 1) neighbor = et.tile.SE;
				if (i == 2) neighbor = et.tile.SW;
				if (i == 3) neighbor = et.tile.NW;
				if (failsafe == 0) print(neighbor);
				if (neighbor == null) continue;
				if (closed.Contains(neighbor)) continue;
				if (failsafe == 0) print("added!");
				EvalTile addMe = new EvalTile(neighbor, et.gScore + neighbor.EntranceCost, Distance(et.tile.transform.position, goal.transform.position));
				addMe.cameFrom = et;
				open.Add(addMe, addMe.fScore);
			}
		}
		//print(failsafe);
		//print("No path found!");
		MessageBox.Print(name + " is lost, unable to find a path!");
		return false;
	}

	float Distance(Vector2 a, Vector2 b)
	{
		return Mathf.Abs(a.x - b.x) + 2 * Mathf.Abs(a.y - b.y);
	}

	class EvalTile
	{
		public Tile tile;
		public EvalTile cameFrom;
		// cost of getting to the start to that tile
		public float gScore;
		// distance to goal
		public float hScore;
		// optimistic total distance
		public float fScore { get { return gScore + hScore; } }

		public EvalTile(Tile t, float g, float h)
		{
			tile = t;
			gScore = g;
			hScore = h;
		}

		public override string ToString()
		{
			return string.Format("EvalTile [Tile = {0}, cameFrom = {1}, gScore = {2}, hScore = {3}]", tile, cameFrom.tile, gScore, hScore);
		}
	}
}
