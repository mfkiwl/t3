﻿using System;

namespace T3.Core.Operator.Types
{
    public class Add : Instance<Add>
    {
        [Output(Guid = "{5CE9C625-F890-4620-9747-C98EAB4B9447}")]
        public readonly Slot<float> Result = new Slot<float>();

        public Add()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            Result.Value = Input1.GetValue(context) + Input2.GetValue(context);
        }

        [FloatInput(DefaultValue = 29.0f, Guid = "{E3550929-8905-4CDF-BC85-C31E97DA4BAA}")]
        public readonly InputSlot<float> Input1 = new InputSlot<float>();

        [FloatInput(DefaultValue = 23.0f, Guid = "{993D59BB-1FC0-4857-A36D-629B0E7AA0D2}")]
        public readonly InputSlot<float> Input2 = new InputSlot<float>();

//         [FloatInput(DefaultValue = 17.0f, Guid = "{D7478BAA-41B4-4F83-873B-6267AA93BFA9}")]
//         public readonly InputSlot<float> Input3 = new InputSlot<float>();
// 
//         [FloatInput(DefaultValue = 13.0f, Guid = "{99A53560-8F62-4240-9ED4-800525CF2EF3}")]
//         public readonly InputSlot<float> Input4 = new InputSlot<float>();
    }
}