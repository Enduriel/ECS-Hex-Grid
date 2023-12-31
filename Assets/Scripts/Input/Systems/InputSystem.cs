using System;
using System.Collections;
using System.Collections.Generic;
using MyNamespace.Input.Enums;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Ray = Unity.Physics.Ray;

namespace MyNamespace
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial class InputSystem : SystemBase
    {
        private DefaultMovementActions _movementActions;
        private Entity _playerEntity;

        private NativeHashMap<int, ButtonState> _lastFrameState = new(2, Allocator.Persistent)
        {
            {(int)InputType.Select, ButtonState.None},
            {(int)InputType.Drag, ButtonState.None}
        };

        private NativeHashMap<int, bool> _lastFrameInput = new (2, Allocator.Persistent)
        {
            {(int)InputType.Scroll, false},
            {(int)InputType.Move, false},
            {(int)InputType.MouseMove, false}
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
            // todo verify that this actually needs to happen immediately,
            // pretty sure there's a way it doesn't which would be cleaner
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);
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
            UpdateComponentWithValue<UserMouseMovement, UserFloat2InputValue, float2>(ecb, InputType.MouseMove, new UserFloat2InputValue
            {
                Value = _movementActions.DefaultMap.MouseMovement.ReadValue<Vector2>()
            });
            
            UpdateComponent<UserSelect>(ecb, InputType.Select, Mouse.current.leftButton);
            UpdateComponent<UserDrag>(ecb, InputType.Drag, Mouse.current.middleButton);
        }
        
        private void UpdateComponent<T>(EntityCommandBuffer ecb, InputType inputType, ButtonControl button)
            where T : unmanaged, IComponentData, IUserInput
        {
            var value = ButtonState.None;
            if (button.isPressed)
            {
                if ((_lastFrameState[(int)inputType] & ButtonState.PressedOrHeld) == ButtonState.None)
                    value = ButtonState.Pressed;
                else
                    value = ButtonState.Held;
            }
            else if ((_lastFrameState[(int)inputType] & ButtonState.PressedOrHeld) != ButtonState.None)
            {
                value = ButtonState.Released;
            }
            
            _lastFrameState[(int)inputType] = value;
            if (value == ButtonState.None)
            {
                ecb.RemoveComponent<T>(_playerEntity);
            }
            else
            {
                ecb.AddComponent<T>(_playerEntity);
                ecb.SetComponent(_playerEntity, new T
                {
                    SetState = value
                });
            }
        }

        private void UpdateComponentWithValue<T, T1, T2>(EntityCommandBuffer ecb, InputType inputType, T1 value)
            where T : unmanaged, IComponentData, IUserInputWithValue<T1>
            where T1 : IUserInputValue<T2>
        {
            if (value.IsNotZero())
            {
                if (!_lastFrameInput[(int)inputType])
                {
                    ecb.AddComponent<T>(_playerEntity);
                    _lastFrameInput[(int)inputType] = true;
                }
                ecb.SetComponent(_playerEntity, new T
                {
                    SetValue = value
                });
            }
            else if (_lastFrameInput[(int)inputType])
            {
                ecb.RemoveComponent<T>(_playerEntity);
                _lastFrameInput[(int)inputType] = false;
            }
        }

        protected override void OnStopRunning()
        {
            _movementActions.Disable();
        }

        protected override void OnDestroy()
        {
            _lastFrameState.Dispose();
            _lastFrameInput.Dispose();
            base.OnDestroy();
        }
    }
}

