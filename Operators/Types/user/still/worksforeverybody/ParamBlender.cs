using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_957961ad_797c_48ac_b9d6_7f2fa2ce17eb
{
    public class ParamBlender : Instance<ParamBlender>
    {

        [Output(Guid = "eea2e2e1-540f-4388-a626-b9885c77a29c")]
        public readonly Slot<float> A = new Slot<float>();

        [Output(Guid = "4571000e-b180-4f69-a3c2-a2af1fa40660")]
        public readonly Slot<float> B = new Slot<float>();

        [Output(Guid = "dc403770-bd73-4dba-a67f-d94054e01d03")]
        public readonly Slot<float> AnimParam1Out = new Slot<float>();

        [Input(Guid = "726b3606-9d03-4b67-af4c-70e144b5c471")]
        public readonly InputSlot<int> SceneIndex = new InputSlot<int>();

        [Input(Guid = "534229d1-8728-4925-bd46-3bb351e4e95b")]
        public readonly InputSlot<float> ADefault = new InputSlot<float>();

        [Input(Guid = "dec2dd0d-ea78-4475-8b26-76bf6d8b724c")]
        public readonly InputSlot<System.Numerics.Vector2> A_ExporeRange = new InputSlot<System.Numerics.Vector2>();

        [Input(Guid = "e997cb93-de68-4430-9c06-32bb46836d98")]
        public readonly InputSlot<float> B_Default = new InputSlot<float>();

        [Input(Guid = "46801ba5-4e1d-4148-aa91-fe2491822e26")]
        public readonly InputSlot<System.Numerics.Vector2> B_ExploreRange = new InputSlot<System.Numerics.Vector2>();

        [Input(Guid = "ec71dd37-2aa2-45a2-96d9-a32853572855")]
        public readonly InputSlot<float> AnimParam1 = new InputSlot<float>();

    }
}

