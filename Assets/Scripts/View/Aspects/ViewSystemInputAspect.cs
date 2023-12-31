using MyNamespace;
using Unity.Entities;
using View.Components;

namespace View.Aspects
{
    public readonly partial struct ViewSystemInputAspect : IAspect
    {
        public readonly RefRO<UserMouseInfo> UserMouseInfo;
        [Optional] public readonly RefRO<UserMouseMovement> UserMouseMovement;
        [Optional] public readonly RefRO<UserMovement> UserMovement;
        [Optional] public readonly RefRO<UserScroll> UserScroll;
        [Optional] public readonly RefRO<UserDrag> UserDrag;
        public readonly RefRW<ZoomLevel> ZoomLevel;
    }
    
}