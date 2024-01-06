using TMPro;
using Trideria.View;
using Unity.Entities;
using UnityEngine;

namespace Trideria.HexGrid
{
	public class HexGridCanvas<T> : EntityFollower<T> where T : unmanaged, IHexGridData, IComponentData
	{
		public TextMeshProUGUI cellLabelPrefab;
		protected override void TryAwake()
		{
			base.TryAwake();
			if (IsAwake)
			{
				var grid = Manager.GetComponentData<T>(_myTarget);
				foreach (var hexCoords in grid.GetHexes())
				{
					var label = Instantiate(cellLabelPrefab, transform, false);
					label.rectTransform.anchoredPosition =
						HexHelpers.GetRelativePosition(HexCoordinates.Zero, hexCoords).xz;
					label.text = $"{hexCoords.Q}\n{hexCoords.R}\n{hexCoords.S}";
				}
			}
		}

		protected override void Update()
		{
			base.Update();
			transform.position += Vector3.up * 0.1f;
			transform.rotation *= Quaternion.Euler(90, 0, 0);
		}
	}
}