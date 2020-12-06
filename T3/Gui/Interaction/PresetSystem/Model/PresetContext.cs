﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace T3.Gui.Interaction.PresetSystem.Model
{
    /// <summary>
    /// Model of a single composition
    /// </summary>
    public class PresetContext
    {
        public List<PresetScene> Scenes = new List<PresetScene>();
        public List<ParameterGroup> ParameterGroups = new List<ParameterGroup>();
        public Preset[,] Presets = new Preset[4, 4];
        public PresetAddress ViewWindow;
        public Guid ActiveGroupId = Guid.Empty;
        public Guid ActiveSceneId = Guid.Empty;
        public Guid CompositionId = Guid.Empty;

        public Preset TryGetPreset(PresetAddress address)
        {
            if (!address.IsValidForContext(this))
                return null;

            var p = Presets[address.ParameterGroupColumn, address.SceneRow];
            return p ?? _mockPreset;
        }

        private static readonly Preset _mockPreset = new Preset() { IsPlaceholder = true };

        public ParameterGroup ActiveGroup => ParameterGroups.SingleOrDefault(g => g.Id == ActiveGroupId);

        public ParameterGroup GetGroupAtIndex(int index)
        {
            return ParameterGroups.Count <= index ? null : ParameterGroups[index];
        }
        
        public void SetPresetAt(Preset preset, PresetAddress address)
        {
            var needToExtendGrid = !address.IsValidForContext(this);
            if (needToExtendGrid)
            {
                Presets = ResizeArray(Presets,
                                      Math.Max(address.ParameterGroupColumn + 1, Presets.GetLength(0)),
                                      Math.Max(address.SceneRow + 1, Presets.GetLength(1)));
            }

            Presets[address.ParameterGroupColumn, address.SceneRow] = preset;
        }

        protected T[,] ResizeArray<T>(T[,] original, int x, int y)
        {
            T[,] newArray = new T[x, y];
            int minX = Math.Min(original.GetLength(0), newArray.GetLength(0));
            int minY = Math.Min(original.GetLength(1), newArray.GetLength(1));

            for (int i = 0; i < minY; ++i)
                Array.Copy(original, i * original.GetLength(0), newArray, i * newArray.GetLength(0), minX);

            return newArray;
        }

        public ParameterGroup CreateNewGroup(string _nameForNewGroup)
        {
            var newGroup = new ParameterGroup()
                               {
                                   Title = _nameForNewGroup,
                                   Parameters = new List<GroupParameter>(),
                               };

            // insert
            var freeSlotIndex = ParameterGroups.IndexOf(null);
            if (freeSlotIndex == -1)
            {
                ParameterGroups.Add(newGroup);
            }
            else
            {
                ParameterGroups[freeSlotIndex] = newGroup;
            }

            return newGroup;
        }
    }
}