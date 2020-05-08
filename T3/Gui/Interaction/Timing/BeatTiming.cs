﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using ImGuiNET;
using T3.Core.Logging;

namespace T3.Gui.Interaction.Timing
{
    /// <summary>
    /// A helper to provide continuous a BeatTime for live si tuations.
    /// The timing can be driven by BPM detection or beat tapping.
    /// </summary>
    /// <remarks>
    /// The code is partly inspired by early work on tooll.io</remarks>
    public class BeatTiming
    {
        public void TriggerSyncTap() => _syncTriggered = true;
        public void TriggerReset() =>   _resetTriggered = true;
        public void TriggerDelaySync() => _delayTriggered = true;
        public void TriggerAdvanceSync() =>_advanceTriggered = true;
        
        public static SystemAudioInput SystemAudioInput;

        public float Bpm => (float)(60f / _beatDuration);
        public float DampedBpm => (float)(60f / _dampedBeatDuration);

        public void SetBpmFromSystemAudio()
        {
            if (SystemAudioInput.LastIntLevel == 0)
            {
                Log.Warning("Sound seems to be stuck. Trying restart.");
                SystemAudioInput.Restart();
            }
            
            if (!_bpmDetection.HasSufficientSampleData)
            {
                Log.Warning("Insufficient sample data");
                return;
            }

            var bpm = _bpmDetection.ComputeBpmRate();
            if (!(bpm > 0))
            {
                Log.Warning("Computing bpm-rate failed");
                return;
            }

            Log.Debug("Setting bpm to " + bpm);
            _beatDuration = 60f / bpm;
            _dampedBeatDuration = _beatDuration;
            _lastResyncTime = 0;    // Prevent bpm stretching on resync
        } 
        
        public double GetSyncedBeatTiming()
        {
            if(SystemAudioInput == null)
                SystemAudioInput = new SystemAudioInput();
            
            return SyncedTime;
        }

        
        
        public void Update()
        {
            if (_syncTriggered)
            {
            }

            UpdatedBpmDetection();
            UpdateTapping();
        }

        private void UpdatedBpmDetection()
        {
            if (SystemAudioInput == null || _bpmDetection == null)
                return;

            _bpmDetection.AddFffSample(SystemAudioInput.LastFftBuffer);
        }

        /// <remarks>
        /// This code seems much too complicated, but getting flexible and coherent beat detection
        /// seems to be much trickier, than I though. After playing with a couple of methods, it 
        /// finally settled on keeping a "fragmentTime" counter wrapping over the _beatDuration.
        ///     The fragmentTime is than additionally offset with. Maybe there is a method that works
        /// without the separate offset-variable, but since I wanted to have damped transition is
        /// syncing (no jumps please), keeping both separated seemed to work.
        /// </remarks>
        private void UpdateTapping()
        {
            var time = ImGui.GetTime();
            if (Math.Abs(time - _lastTime) < 0.0001f)
                return;

            var timeDelta = time - _lastTime;
            
            _lastTime = time;

            //var barDuration = _dampedBeatDuration * BeatsPerBar;

            if (_resetTriggered)
            {
                _resetTriggered = false;
                _measureCounter = 0;
                _dampedBeatDuration = 120;
            }

            if (_advanceTriggered)
            {
                _tappedMeasureStartTime += 0.01f;
                _advanceTriggered = false;
            }
            
            if (_delayTriggered)
            {
                _tappedMeasureStartTime -= 0.01f;
                _delayTriggered = false;
            }
            
            
            var measureDuration = _beatDuration * BeatsPerMeasure;
            var barDuration = _beatDuration * BeatsPerBar;
            var timeInMeasure = time - _measureStartTime;
            var timeInBar = timeInMeasure % barDuration;

            if (timeInBar > barDuration / 2)
            {
                timeInBar -= barDuration;
            }

            const float precision = 0.5f;
            SyncPrecision = (float)(Math.Max(0, 1-Math.Abs(timeInBar) / barDuration * 2) - precision) * 1/(1-precision);
            
            if (_syncTriggered)
            {
                _syncTriggered = false;
                AddTapAndShiftTimings(time);

                // Fix BPM if completely out of control
                if (double.IsNaN(Bpm) || Bpm < 20 || Bpm > 200)
                {
                    _beatDuration = 1;
                    _tappedMeasureStartTime = 0;
                    _dampedBeatDuration = 0.5;
                    _tapTimes.Clear();
                }
                
                Log.Debug("Sync Hit precision: " + SyncPrecision);
                var needToJumpSync = SyncPrecision < 0f;
                if (needToJumpSync)
                {
                    _measureStartTime = time;
                }

                _tappedMeasureStartTime = time;

                // Stretch _beatTime
                if (Math.Abs(_lastResyncTime) > 0.001f)
                {
                    var timeSinceResync = time - _lastResyncTime;

                    var barCount = timeSinceResync / barDuration;
                    var barCountInt = Math.Round(barCount);
                    var isNotTappingAndNotAbandoned = barCount > 4 && barCount < 200;
                    if (isNotTappingAndNotAbandoned)
                    {
                        var mod = barCount - barCountInt;
                        if (Math.Abs(mod) < 0.5 && barCountInt > 0)
                        {
                            var barFragment = mod * barDuration / barCountInt;
                            var beatShift = barFragment / BeatsPerBar;
                            _beatDuration += beatShift;
                            Log.Debug("Resync-Offset:" + mod + " shift:" + beatShift + " new BPM" + (60 / _beatDuration));
                        }
                    }
                }

                _lastResyncTime = time;
            }

            // Smooth offset and beat duration to avoid jumps
            _dampedBeatDuration = Lerp(_dampedBeatDuration, _beatDuration, 0.05f);

            // Slide start-time to match last beat-trigger
            //var timeInBar = time - _measureStartTime;
            var tappedTimeInMeasure = time - _tappedMeasureStartTime;
            var differenceToTapping = tappedTimeInMeasure - timeInMeasure; 
            //var tappedBeatTime = (tappedTimeInMeasure / _dampedBeatDuration) % 1f;
            //var beatTime = (timeInBar / _dampedBeatDuration) % 1f;

            var isTimingOff = Math.Abs(differenceToTapping) > 0.03f; 
            if(isTimingOff)
                _measureStartTime += (differenceToTapping > 0) ? -0.01f : 0.01f;   

            // Check for next measure               
            if (timeInMeasure > measureDuration)
            {
                _measureCounter++;
                _measureStartTime += measureDuration;
                timeInMeasure -= measureDuration;
            }

            _tappedMeasureStartTime = time + (_tappedMeasureStartTime - time) % measureDuration;
            _measureProgress = (float)(timeInMeasure / measureDuration);
        }

        private void AddTapAndShiftTimings(double time)
        {
            var newSeriesStarted = _tapTimes.Count == 0 || Math.Abs(time - _tapTimes.Last()) > 16 * _beatDuration;
            if (newSeriesStarted)
                _tapTimes.Clear();
        
            _tapTimes.Add(time);
            
            if (_tapTimes.Count < 4)
                return;
        
            if (_tapTimes.Count > 16)
            {
                _tapTimes.RemoveAt(0);
            }
        
            var sum = 0.0;
            var lastT = 0.0;
        
            foreach (var t in _tapTimes)
            {
                if (Math.Abs(lastT) < 0.001f)
                {
                    lastT = t;
                    continue;
                }
                sum += t - lastT;
                lastT = t;
            }
        
            _beatDuration = sum / (_tapTimes.Count - 1);
            //_tappedMeasureStartTime = time - _beatDuration;
        }
        
        
        private static double Lerp(double a, double b, float t)
        {
            return a * (1 - t) + b * t;
        }
        
        private double SyncedTime => (float)(_measureCounter + _measureProgress) * 4 ;
        private double _lastResyncTime;
        private int _measureCounter;
        private float _measureProgress;

        private double _measureStartTime;

        private double _beatDuration = 0.5;
        private double _dampedBeatDuration = 0.5;

        
        private const int BeatsPerBar = 4;
        private const int BeatsPerMeasure = 16;
        private double _lastTime;
        
        private bool _syncTriggered;
        private bool _resetTriggered;

        readonly List<double> _tapTimes = new List<double>();
        private double _tappedMeasureStartTime;
        private BpmDetection _bpmDetection = new BpmDetection();
        public static float SyncPrecision;


        private bool _delayTriggered;
        private bool _advanceTriggered;
        

    }
}