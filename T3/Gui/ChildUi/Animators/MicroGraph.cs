﻿using System;
using System.Numerics;
using ImGuiNET;
using T3.Core;
using T3.Core.Logging;
using T3.Gui.Styling;
using UiHelpers;

namespace T3.Gui.ChildUi.Animators
{
    public static class MicroGraph
    {
        public static bool Draw(ref float offset, ref float smoothing, float fragment, ImRect innerRect, ImDrawListPtr drawList, string valueText)
        {
            var modified = false;
            var h = innerRect.GetHeight();
            var graphRect = innerRect;
            graphRect.Min.X = graphRect.Max.X - GraphWidthRatio * h;

            // Draw interaction
            ImGui.SetCursorScreenPos(graphRect.Min);
            ImGui.InvisibleButton("dragMicroGraph", graphRect.GetSize());
            
            
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsItemHovered() || _dragState != DragMode.Off)
            { 
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            }

            var isActive = ImGui.IsItemActive();
            if (isActive)
            {
                 var dragDelta = ImGui.GetMouseDragDelta(0, 1);
                 switch (_dragState)
                 {
                     case DragMode.Off:
                     case DragMode.Undecided:
                     {
                         if (dragDelta.LengthSquared() > 10)
                         {
                             _dragState = Math.Abs(dragDelta.X) > Math.Abs(dragDelta.Y) 
                                              ? DragMode.DraggingHorizontally : 
                                              DragMode.DraggingVertically;
                         }

                         break;
                     }
                     
                     case DragMode.DraggingHorizontally:
                         if (Math.Abs(dragDelta.X) > 0.5f)
                         {
                             smoothing = (smoothing + dragDelta.X / 100f).Clamp(0, 1);
                             ImGui.ResetMouseDragDelta();
                             Log.Debug("horizontally");
                             modified = true;
                         }
                         break;
                
                     case DragMode.DraggingVertically:
                         if (Math.Abs(dragDelta.Y) > 0.5f)
                         {
                             offset = (offset * (dragDelta.Y < 0
                                                     ? JumpDistanceDragScale
                                                     : 1 / JumpDistanceDragScale)).Clamp(0.01f, 100);
                             ImGui.ResetMouseDragDelta();
                             Log.Debug("vertically");
                             modified = true;
                         }
                         break;
                 }
            }
            else if (ImGui.IsItemDeactivated())
            {
                _dragState = DragMode.Off;
            }
            
            
            // horizontal line
            var lh1 = graphRect.Min + Vector2.UnitY * h / 2;
            var lh2 = new Vector2(graphRect.Max.X, lh1.Y + 1);
            drawList.AddRectFilled(lh1, lh2, GraphLineColor);

            // Vertical start line
            const float leftPaddingH = 0.25f;
            var lv1 = graphRect.Min + Vector2.UnitX * (int)(leftPaddingH * h + 0.5f);
            var lv2 = new Vector2(lv1.X + 1, graphRect.Max.Y);
            drawList.AddRectFilled(lv1, lv2, GraphLineColor);

            // Fragment line 
            var width = h * (GraphWidthRatio - leftPaddingH);
            var dx = new Vector2(fragment * width - 1, 0);
            drawList.AddRectFilled(lv1 + dx, lv2 + dx, FragmentLineColor);

            // Draw graph
            //        lv
            //        |  2-------3    y
            //        | /
            //  0-----1 - - - - - -   lh
            //        |
            //        |
            GraphLinePoints[0] = lh1;
            GraphLinePoints[1].X = lv1.X;
            GraphLinePoints[1].Y = lh1.Y;

            const float yLineRatioH = 0.25f;
            var y = graphRect.Min.Y + yLineRatioH * h;

            GraphLinePoints[2].X = lv1.X + smoothing.Clamp(0, 1) * width;
            GraphLinePoints[2].Y = y;

            GraphLinePoints[3].X = graphRect.Max.X + 1;
            GraphLinePoints[3].Y = y;

            var curveLineColor = isActive && _dragState == DragMode.DraggingHorizontally ? Color.Red : CurveLineColor;
            drawList.AddPolyline(ref GraphLinePoints[0], 4, curveLineColor, false, 1);

            // Draw offset label
            if (h > 14)
            {
                ImGui.PushFont(Fonts.FontSmall);
                
                var labelSize = ImGui.CalcTextSize(valueText);

                var color = isActive && _dragState == DragMode.DraggingVertically ? Color.Red : Color.White;
                drawList.AddText(MathUtils.Floor(new Vector2(graphRect.Max.X - 3 - labelSize.X,
                                                             lh1.Y - labelSize.Y / 2 - 2
                                                            )), color, valueText);
                ImGui.PopFont();
            }
            return modified;
        }

        private static DragMode _dragState;

        private enum DragMode
        {
            Off,
            Undecided,
            DraggingVertically,
            DraggingHorizontally,
        }

        private static readonly Color GraphLineColor = new Color(0, 0, 0, 0.3f);
        private static readonly Color FragmentLineColor = Color.Orange;
        private static readonly Color CurveLineColor = new Color(1, 1, 1, 0.5f);
        private const float JumpDistanceDragScale = 1.05f;
        
        private const float GraphWidthRatio = 2;
        private static readonly Vector2[] GraphLinePoints = new Vector2[4];
    }
}