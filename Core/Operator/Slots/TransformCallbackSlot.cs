using System;
using T3.Core.Animation;
using T3.Core.Operator.Interfaces;

namespace T3.Core.Operator.Slots
{
    public class TransformCallbackSlot<T> : Slot<T>
    {
        public Action<ITransformable, EvaluationContext> TransformCallback { get; set; }

        public ITransformable TransformableOp { get; set; }

        private new void Update(EvaluationContext context)
        {
            TransformCallback?.Invoke(TransformableOp, context);
            _baseUpdateAction(context);
        }

        private Action<EvaluationContext> _baseUpdateAction;

        public override Action<EvaluationContext> UpdateAction
        {
            set
            {
                _baseUpdateAction = value;
                base.UpdateAction = Update;
            }
        }
        
        protected override void SetDisabled(bool isDisabled)
        {
            if (isDisabled == _isDisabled)
                return;

            if (isDisabled)
            {
                _defaultUpdateAction = _baseUpdateAction;
                base.UpdateAction = EmptyAction;
                DirtyFlag.Invalidate();
            }
            else
            {
                SetUpdateActionBackToDefault();
                DirtyFlag.Invalidate();
            }

            _isDisabled = isDisabled;
        }
    }
}