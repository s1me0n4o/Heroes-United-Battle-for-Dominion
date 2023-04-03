using System;
using TMPro;
using UnityEngine;

namespace GridSystem
{
	public class Grid<T>
	{
		private readonly float _cellSize;
		private readonly T[,] _gridArray;
		private readonly int _height;
		private readonly Vector3 _originPosition;
		private readonly Transform _parent;

		private readonly int _width;
		private TextMeshPro[,] _debugTextArray;

		public Grid(int width, int height, float cellSize, Vector3 originPosition, Transform parent,
			Func<Grid<T>, int, int, T> createGridObj) // passing the grid object, x and z
		{
			_width = width;
			_height = height;
			_cellSize = cellSize;
			_originPosition = originPosition;
			_parent = parent;

			_gridArray = new T[width, height];

			for (int x = 0; x < _gridArray.GetLength(0); x++)
			{
				for (int z = 0; z < _gridArray.GetLength(1); z++)
				{
					_gridArray[x, z] = createGridObj(this, x, z);
				}
			}
		}

		public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

		public bool IsValidGridPosition(GridPosition gridPos)
		{
			return gridPos.x >= 0 &&
			       gridPos.z >= 0 &&
			       gridPos.x < _width &&
			       gridPos.z < _height;
		}

		public int GetWorldIndex(int x, int z)
		{
			return x + z * _width;
		}

		public float GetCellSize()
		{
			return _cellSize;
		}

		public int GetGridWidth()
		{
			return _width;
		}

		public int GetGridHeight()
		{
			return _height;
		}

		public T[,] GetGridArray()
		{
			return _gridArray;
		}

		public void SetGridObject(GridPosition gridPosition, T value)
		{
			if (gridPosition.x >= 0 && gridPosition.z >= 0 && gridPosition.x < _width && gridPosition.z < _height)
			{
				_gridArray[gridPosition.x, gridPosition.z] = value;

				if (OnGridValueChanged != null)
				{
					OnGridValueChanged(this,
						new OnGridValueChangedEventArgs { x = gridPosition.x, z = gridPosition.z });
				}
			}
		}

		public void TriggerOnGridObjectChanged(int x, int z)
		{
			OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, z = z });
		}

		public GridPosition GetGridPosition(Vector3 worldPos)
		{
			return new GridPosition(Mathf.FloorToInt(worldPos.x / _cellSize), Mathf.FloorToInt(worldPos.z / _cellSize));
		}

		private void GetXandZ(Vector3 worldPosition, out int x, out int z)
		{
			x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
			z = Mathf.FloorToInt((worldPosition - _originPosition).z / _cellSize);
		}

		public void SetGridObject(Vector3 worldPosition, T value)
		{
			int x, z;
			var gridPosition = GetGridPosition(worldPosition);
			SetGridObject(gridPosition, value);
		}

		private T GetGridObject(int x, int z)
		{
			if (x >= 0 && z >= 0 && x < _width && z < _height)
			{
				return _gridArray[x, z];
			}
			else
			{
				return default(T);
			}
		}

		public T GetGridObject(GridPosition gridPosition)
		{
			return GetGridObject(gridPosition.x, gridPosition.z);
		}

		public Vector3 GetWorldPosition(int x, int z) => new Vector3(x, 0, z) * _cellSize + _originPosition;

		public void CreateDebugObjects(Transform debugPrefab, bool useGameObject = false)
		{
			_debugTextArray = new TextMeshPro[_width, _height];

			var offset = new Vector3(_cellSize, _cellSize) * 0.45f;
			var fontSize = (int)_cellSize * 5;

			for (int x = 0; x < _gridArray.GetLength(0); x++)
			{
				for (int z = 0; z < _gridArray.GetLength(1); z++)
				{
					if (useGameObject)
					{
						var gridPos = new GridPosition(x, z);
						var transform =
							GameObject.Instantiate(debugPrefab, GetWorldPosition(x, z), Quaternion.identity);
						transform.localPosition = GetWorldPosition(x, z) + offset;
						var gridObjVisual = transform.GetComponent<GridObjectVisual>();
						gridObjVisual.SetGridObject(GetGridObject(gridPos) as GridObject);
					}
					else
					{
						_debugTextArray[x, z] = CreateWorldText(_parent, $"{x}, {z}", GetWorldPosition(x, z) + offset,
							fontSize,
							Color.black, TextAlignmentOptions.Center, 0);
					}


					Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.black, 100f);
					Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.black, 100f);
				}
			}

			Debug.DrawLine(GetWorldPosition(0, _height), GetWorldPosition(_width, _height), Color.black, 100f);
			Debug.DrawLine(GetWorldPosition(_width, 0), GetWorldPosition(_width, _height), Color.black, 100f);

			OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
			{
				_debugTextArray[eventArgs.x, eventArgs.z].text = _gridArray[eventArgs.x, eventArgs.z]?.ToString();
			};
		}

		private TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize,
			Color color,
			TextAlignmentOptions textAlignment, int sortingOrder)
		{
			var gameObject = new GameObject("World_Text", typeof(TextMeshPro));
			var transform = gameObject.transform;
			transform.SetParent(parent, false);
			transform.localPosition = localPosition;

			var tmp = gameObject.GetComponent<TextMeshPro>();
			// rect transform width and hight
			tmp.rectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);
			tmp.rectTransform.eulerAngles = new Vector3(90, 0, 0);

			tmp.alignment = textAlignment;
			tmp.text = text;
			tmp.fontSize = fontSize;
			tmp.color = color;
			tmp.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
			return tmp;
		}

		public class OnGridValueChangedEventArgs : EventArgs
		{
			public int x;
			public int z;
		}
	}
}

public struct GridPosition : IEquatable<GridPosition>
{
	public int x;
	public int z;

	public GridPosition(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public override string ToString()
	{
		return $"x: {x}; z: {z}";
	}

	public static bool operator ==(GridPosition a, GridPosition b)
	{
		return a.x == b.x && a.z == b.z;
	}

	public static bool operator !=(GridPosition a, GridPosition b)
	{
		return !(a == b);
	}

	public bool Equals(GridPosition other)
	{
		return x == other.x && z == other.z;
	}

	public override bool Equals(object obj)
	{
		return obj is GridPosition other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, z);
	}

	public static GridPosition operator +(GridPosition a, GridPosition b)
	{
		return new GridPosition(a.x + b.x, a.z + b.z);
	}

	public static GridPosition operator -(GridPosition a, GridPosition b)
	{
		return new GridPosition(a.x - b.x, a.z - b.z);
	}
}