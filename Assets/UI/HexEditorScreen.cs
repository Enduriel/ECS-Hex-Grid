using System;
using Trideria.HexGrid;
using Trideria.Input;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace Trideria.UI
{
	public class HexEditorScreen : MonoBehaviour
	{
		private EntityQuery _playerQuery;
		public void Awake()
		{
			var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_playerQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UserMouseInfo>().Build(manager);
			var root = GetComponent<UIDocument>().rootVisualElement;
			var hexColorSelector = root.Q<EnumField>("HexColorSelector");
			manager.AddComponent<HexEditorInputComponent>(_playerQuery.GetSingletonEntity());
			hexColorSelector.RegisterValueChangedCallback(evt =>
			{
				// doing it this way is bad but I can't wrap my head around a clean ECS compatible
				// MVVM system so this will have to do for now
				var data = manager.GetComponentData<HexEditorInputComponent>(_playerQuery.GetSingletonEntity());
				data.Color = (AllowedColor)evt.newValue;
				manager.SetComponentData(_playerQuery.GetSingletonEntity(), data);
			});

			var hexHeightSelector = root.Q<SliderInt>("HexHeightSelector");
			hexHeightSelector.RegisterValueChangedCallback(evt =>
			{
				var data = manager.GetComponentData<HexEditorInputComponent>(_playerQuery.GetSingletonEntity());
				data.Height = evt.newValue;
				manager.SetComponentData(_playerQuery.GetSingletonEntity(), data);
			});
		}
	}
}