﻿using System;
using System.Numerics;
using ImGuiNET;
using T3.Core;
using T3.Core.Operator;
using T3.Gui.ChildUi.Animators;
using T3.Gui.Styling;
using T3.Operators.Types.Id_23794a1f_372d_484b_ac31_9470d0e77819;
using UiHelpers;

namespace T3.Gui.ChildUi
{
    public static class CounterUi
    {
        public static bool DrawChildUi(Instance instance, ImDrawListPtr drawList, ImRect selectableScreenRect)
        {
            if (!(instance is Jitter2d jitter2d))
                return false;

            var innerRect = selectableScreenRect;
            innerRect.Expand(-4);
            var h = innerRect.GetWidth() * 0.2f;

            var modified= RateLabel.Draw(ref jitter2d.Rate.Value, innerRect, drawList);
            DrawMicroGraph();
            return true;

            void DrawMicroGraph()
            {
                var graphRect = innerRect;
                graphRect.Min.X = graphRect.Max.X - GraphWidthRatio * h;

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
                var dx = new Vector2(jitter2d.Fragment * width - 1, 0);
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

                GraphLinePoints[2].X = lv1.X + jitter2d.Smoothing.Value.Clamp(0, 1) * width;
                GraphLinePoints[2].Y = y;

                GraphLinePoints[3].X = graphRect.Max.X + 1;
                GraphLinePoints[3].Y = y;
                drawList.AddPolyline(ref GraphLinePoints[0], 4, CurveLineColor, false, 1);

                // Draw offset label
                if (h > 14)
                {
                    ImGui.PushFont(Fonts.FontSmall);

                    var label = $"±{jitter2d.JumpDistance.Value:0.0}"; // + jitter2d.JumpDistance.Value.ToString(Formatter, CultureInfo.InvariantCulture);
                    var labelSize = ImGui.CalcTextSize(label);

                    drawList.AddText(MathUtils.Floor(new Vector2(graphRect.Max.X - 3 - labelSize.X,
                                                                 lh1.Y - labelSize.Y / 2 - 2
                                                                )), Color.White, label);
                    ImGui.PopFont();
                }

                // Draw interaction
                ImGui.SetCursorScreenPos(graphRect.Min);
                ImGui.InvisibleButton("drag", graphRect.GetSize());
                if (ImGui.IsItemHovered() || ImGui.IsItemActive())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
                }

                if (ImGui.IsItemActive() && ImGui.IsMouseDragging(0))
                {
                    var dragDelta = ImGui.GetMouseDragDelta(0, 1);
                    jitter2d.Smoothing.Value = (jitter2d.Smoothing.Value + dragDelta.X / 100f).Clamp(0, 1);
                    if (Math.Abs(dragDelta.Y) > 0.1f)
                    {
                        jitter2d.JumpDistance.Value =
                            (jitter2d.JumpDistance.Value * (dragDelta.Y < 0 ? JumpDistanceDragScale : 1 / JumpDistanceDragScale)).Clamp(0.01f, 100);
                    }

                    ImGui.ResetMouseDragDelta();
                }
            }
        }

        private static readonly Color GraphLineColor = new Color(0, 0, 0, 0.3f);
        private static readonly Color FragmentLineColor = Color.Orange;
        private static readonly Color CurveLineColor = new Color(1, 1, 1, 0.5f);
        private const float JumpDistanceDragScale = 1.05f;

        // ReSharper disable once UseStringInterpolation
        private static readonly string Formatter = string.Format("G{0:D}", 4);
        private const float GraphWidthRatio = 2;
        private static readonly Vector2[] GraphLinePoints = new Vector2[4];
    }
}