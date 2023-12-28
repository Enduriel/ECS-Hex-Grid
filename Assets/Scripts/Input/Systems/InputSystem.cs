using System;
using System.Collections;
using System.Collections.Generic;
using MyNamespace.Input.Enums;
using Unity.Collections;
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

        private Dictionary<InputType, bool> _lastFrameInput = new Dictionary<InputType, bool>()
        {
            {InputType.Select, false},
            {InputType.Drag, false},
            {InputType.Scroll, false},
            {InputType.Move, false}
        };
        
        // private Dictionary<InputType, Func<>>
        
        protected override void OnCreate()
        {
            _movementActions = new DefaultMovementActions();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var singleton = ecb.CreateEntity();
            ecb.SetName(singleton, new FixedString64Bytes("UserInputSingleton"));
            ecb.AddComponent<UserMouseInfo>(singleton);
            ecb.SetComponent(singleton, new UserMouseInfo());
            ecb.Playback(EntityManager);
            _playerEntity = SystemAPI.GetSingletonEntity<UserMouseInfo>();
        }
        // Start is called before the first frame update
        protected override void OnStartRunning()
        {
            _movementActions.Enable();
        }

        protected override void OnUpdate()
        {
            var mousePos = Mouse.current.position.ReadValue();
            // why is it a vector3 and not a vector2?
            // why is there no way to immediately get an unmanaged one?
            // or at least easily cast?
            // who tf knows
            var managedRay = Camera.main!.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.SetComponent(_playerEntity, new UserMouseInfo
            {
                Position = mousePos,
                Ray = new Ray
                {
                    Origin = managedRay.origin,
                    Displacement = managedRay.direction
                }
            });

            UpdateComponentWithValue<UserMovement, UserFloat2InputValue, float2>(ecb, InputType.Move, new UserFloat2InputValue
            {
                Value = _movementActions.DefaultMap.Movement.ReadValue<Vector2>()
            });
            UpdateComponentWithValue<UserScroll, UserIntInputValue, int>(ecb, InputType.Scroll, new UserIntInputValue
            {
                Value = (int)Mouse.current.scroll.ReadValue().normalized.y
            });
            UpdateComponent<UserSelect>(ecb, InputType.Select, Mouse.current.leftButton.ReadValue());
            UpdateComponent<UserDrag>(ecb, InputType.Drag, Mouse.current.middleButton.ReadValue());
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
        
        private void UpdateComponent<T>(EntityCommandBuffer ecb, InputType inputType, float value) where T : unmanaged, IComponentData
        {
            if (value != 0)
            {
                if (!_lastFrameInput[inputType])
                {
                    ecb.AddComponent<T>(_playerEntity);
                    _lastFrameInput[inputType] = true;
                }
            }
            else if (_lastFrameInput[inputType])
            {
                ecb.RemoveComponent<T>(_playerEntity);
                _lastFrameInput[inputType] = false;
            }
        }

        private void UpdateComponentWithValue<T, T1, T2>(EntityCommandBuffer ecb, InputType inputType, T1 value)
            where T : unmanaged, IComponentData, IUserInputWithValue<T1>
            where T1 : IUserInputValue<T2>
        {
            if (value.IsNotZero())
            {
                if (!_lastFrameInput[inputType])
                {
                    ecb.AddComponent<T>(_playerEntity);
                    _lastFrameInput[inputType] = true;
                }
                ecb.SetComponent(_playerEntity, new T
                {
                    ValueFunc = value
                });
            }
            else if (_lastFrameInput[inputType])
            {
                ecb.RemoveComponent<T>(_playerEntity);
                _lastFrameInput[inputType] = false;
            }
        }

        protected override void OnStopRunning()
        {
            _movementActions.Disable();
        }
    }
}

