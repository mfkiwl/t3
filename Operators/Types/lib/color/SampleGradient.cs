using System.Numerics;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_8211249d_7a26_4ad0_8d84_56da72a5c536
{
    public class SampleGradient : Instance<SampleGradient>
    {
        
        [Output(Guid = "963611E7-F55E-4C94-96E6-34E195558A2B")]
        public readonly Slot<Vector4> Color = new Slot<Vector4>();

        
        [Output(Guid = "9F3D0701-86E8-436E-8652-918BA23B2CEF")]
        public readonly Slot<Gradient> OutGradient = new Slot<Gradient>();


        public SampleGradient()
        {
            Color.UpdateAction = Update;
            OutGradient.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var t = SamplePos.GetValue(context);
            var gradient = Gradient.GetValue(context);
            var interpolation = (Gradient.Interpolations) Interpolation.GetValue(context);

            gradient.Interpolation = interpolation;
            Color.Value = gradient.Sample(t);
            OutGradient.Value = gradient.TypedClone();    //FIXME: This might not be efficient or required
        }

        [Input(Guid = "a4527e01-f19a-4200-85e5-00144f3ce061")]
        public readonly InputSlot<float> SamplePos = new();
        
        [Input(Guid = "EFF10FAD-CF95-4133-91DB-EFC41258CD1B")]
        public readonly InputSlot<Gradient> Gradient = new();
        
        [Input(Guid = "76CF4A72-2D25-48CB-A1EC-08D0DDABB053", MappedType = typeof(Gradient.Interpolations))]
        public readonly InputSlot<int> Interpolation = new();

    }
}
