using TMPro;
using Trideria.Mesh;
using Trideria.View;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trideria.HexGrid
{
	public class HexGridCanvas<T> : EntityFollower<T> where T : unmanaged, IHexGridData, IComponentData
	{
		public TextMeshProUGUI cellLabelPrefab;

		protected TextMeshProUGUI[] _labels;
		protected int _meshId;

		protected override void TryAwake()
		{
			base.TryAwake();
			if (!IsAwake)
			{
				return;
			}

			if (!Manager.HasBuffer<HexBuffer>(_myTarget) || !Manager.HasComponent<MaterialMeshInfo>(_myTarget))
			{
				_myTarget = Entity.Null;
				UnRegister();
				return;
			}

			_meshId = Manager.GetComponentData<MaterialMeshInfo>(_myTarget).Mesh;
			var grid = Manager.GetComponentData<T>(_myTarget);
			InitLabels(grid);
			UpdateLabels(grid);
		}

		protected virtual void InitLabels(T grid)
		{
			_labels = new TextMeshProUGUI[grid.GetNumHexes()];
			for (var i = 0; i < _labels.Length; i++)
			{
				_labels[i] = Instantiate(cellLabelPrefab, transform, false);
			}
		}

		protected virtual void UpdateLabels(T grid)
		{
			var j = 0;
			foreach (var hex in Manager.GetBuffer<HexBuffer>(_myTarget))
			{
				var label = _labels[j++];
				var pos = HexHelpers.GetRelativePosition(HexCoordinates.Zero, hex.Coords, hex.Height);
				pos.y = -pos.y - 0.1f;
				label.rectTransform.anchoredPosition3D = pos.xzy;
				label.text = $"{hex.Coords.Q}\n{hex.Coords.R}\n{hex.Coords.S}";
			}
		}

		protected override void Update()
		{
			base.Update();
			if (!IsAwake)
			{
				return;
			}
			// transform.position += Vector3.up * 0.1f;
			transform.rotation *= Quaternion.Euler(90, 0, 0);
			if (Manager.GetComponentData<MaterialMeshInfo>(_myTarget).Mesh != _meshId)
			{
				UpdateLabels(Manager.GetComponentData<T>(_myTarget));
			}
		}
	}
}