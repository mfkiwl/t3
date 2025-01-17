using System.Collections.Generic;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_63e6e642_827b_4518_ac64_9ab0a8d4391e
{
    public class PickFloat : Instance<PickFloat>
    {
        [Output(Guid = "72ADD436-84AA-4332-B061-BE8D50981C77")]
        public readonly Slot<float> Selected = new Slot<float>();

        public PickFloat()
        {
            Selected.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var connections = FloatValues.GetCollectedTypedInputs();
            FloatValues.DirtyFlag.Clear();
            
            if (connections == null || connections.Count == 0)
                return;

            var index = Index.GetValue(context);
            if (index < 0)
                index = -index;

            index %= connections.Count;
            Selected.Value = connections[index].GetValue(context);

            // Clear dirty flag
            if (_isFirstUpdate)
            {
                foreach (var c in connections)
                {
                    c.GetValue(context);
                }

                _isFirstUpdate = false;
            }
        }

        private bool _isFirstUpdate = true; 

        [Input(Guid = "D7EF7F1A-A6BD-4F94-A29A-BB19E2854001")]
        public readonly MultiInputSlot<float> FloatValues = new MultiInputSlot<float>();

        [Input(Guid = "465B4FC3-899C-4B97-9892-F237FA6613E8")]
        public readonly InputSlot<int> Index = new InputSlot<int>(0);
    }
}