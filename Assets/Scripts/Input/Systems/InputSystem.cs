using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Ray = Unity.Physics.Ray;

namespace MyNamespace
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        private DefaultMovementActions _movementActions;
        private Entity _playerEntity;

        private bool _clicked;
        private bool _lastClicked;
        
        protected override void OnCreate()
        {
            _movementActions = new DefaultMovementActions();
            var ecb = new EntityCommandBuffer();
            var singleton = ecb.CreateEntity();
            ecb.AddComponent<UserMouseInfo>(singleton);
            ecb.SetComponent(singleton, new UserMouseInfo());
            ecb.AddComponent<UserMovement>(singleton);
            ecb.SetComponent(singleton, new UserMovement());
            ecb.AddComponent<UserZoom>(singleton);
            ecb.AddComponent(singleton, new UserZoom());
            ecb.Playback(EntityManager);
            _playerEntity = SystemAPI.GetSingletonEntity<UserInput>();
        }
        // Start is called before the first frame update
        protected override void OnStartRunning()
        {
            _movementActions.Enable();
            _movementActions.DefaultMap.Click.performed += _ => _clicked = true;
        }

        protected override void OnUpdate()
        {
            var mousePos = Mouse.current.position.ReadValue();
            // why is it a vector3 and not a vector2?
            // why is there no way to immediately get an unmanaged one?
            // or at least easily cast?
            // who tf knows
            var managedRay = Camera.main!.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
            EntityManager.SetComponentData(_playerEntity, new UserMouseInfo
            {
                Position = mousePos,
                Ray = new Ray
                {
                    Origin = managedRay.origin,
                    Displacement = managedRay.direction
                }
            });
            EntityManager.SetComponentData(_playerEntity, new UserZoom()
            {
                Value = _movementActions.DefaultMap.Zoom.ReadValue<sbyte>()
            });
            EntityManager.SetComponentData(_playerEntity, new UserMovement()
            {
                Value = _movementActions.DefaultMap.Movement.ReadValue<Vector2>()
            });
            if (_clicked)
            {
                EntityManager.AddComponent<UserClick>(_playerEntity);
                _clicked = false;
                _lastClicked = true;
            }
            else if (_lastClicked)
            {
                EntityManager.RemoveComponent<UserClick>(_playerEntity);
                _lastClicked = false;
            }
        }

        protected override void OnStopRunning()
        {
            _movementActions.Disable();
        }
    }
}

