﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using T3.Core.Operator.Slots;

namespace T3.Core.Operator
{
    using InputDefinitionId = Guid;

    /// <summary>
    /// Represents an instance of a <see cref="Symbol"/> within a combined symbol group.
    /// </summary>
    public class SymbolChild
    {
        /// <summary>A reference to the <see cref="Symbol"/> this is an instance from.</summary>
        public Symbol Symbol { get; }

        public Guid Id { get; }

        public string Name { get; set; } = string.Empty;

        public string ReadableName => string.IsNullOrEmpty(Name) ? Symbol.Name : Name;

        public Dictionary<InputDefinitionId, Input> InputValues { get; } = new Dictionary<InputDefinitionId, Input>();
        public Dictionary<Guid, Output> Outputs { get; } = new Dictionary<Guid, Output>();

        public SymbolChild(Symbol symbol, Guid childId)
        {
            Symbol = symbol;
            Id = childId;

            foreach (var inputDefinition in symbol.InputDefinitions)
            {
                InputValues.Add(inputDefinition.Id, new Input(inputDefinition));
            }

            foreach (var outputDefinition in symbol.OutputDefinitions)
            {
                var outputData = (outputDefinition.OutputDataType != null) ? (Activator.CreateInstance(outputDefinition.OutputDataType) as IOutputData) : null;
                var output = new Output(outputDefinition, outputData);
                Outputs.Add(outputDefinition.Id, output);
            }
        }

        #region sub classes =============================================================

        public class Output
        {
            public Symbol.OutputDefinition OutputDefinition { get; }
            public IOutputData OutputData { get; }
            public DirtyFlagTrigger DirtyFlagTrigger { get; set; } = DirtyFlagTrigger.None;

            public Output(Symbol.OutputDefinition outputDefinition, IOutputData outputData)
            {
                OutputDefinition = outputDefinition;
                OutputData = outputData;
            }
        }

        public class Input
        {
            public Symbol.InputDefinition InputDefinition { get; }
            public InputValue DefaultValue => InputDefinition.DefaultValue;

            public string Name => InputDefinition.Name;

            /// <summary>The input value used for this symbol child</summary>
            public InputValue Value { get; }

            public bool IsDefault { get; set; }

            public Input(Symbol.InputDefinition inputDefinition)
            {
                InputDefinition = inputDefinition;
                Value = DefaultValue.Clone();
                IsDefault = true;
            }

            public void SetCurrentValueAsDefault()
            {
                DefaultValue.Assign(Value);
                IsDefault = true;
            }

            public void ResetToDefault()
            {
                Value.Assign(DefaultValue);
                IsDefault = true;
            }
        }

        #endregion
    }
}
