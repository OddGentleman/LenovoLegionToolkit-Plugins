using System;
using LenovoLegionToolkit.Lib;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LenovoLegionToolkit.Plugin.CustomFanCurve
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly CustomFanCurveConfigManager _configManager;
        public bool IsLegionDevice { get; }
        public bool ShowApplyToAllPowerModes { get; }

        public SettingsViewModel(CustomFanCurveConfigManager configManager, bool isLegionDevice, bool isITSModeDevice)
        {
            _configManager = configManager;
            IsLegionDevice = isLegionDevice;
            ShowApplyToAllPowerModes = isLegionDevice || isITSModeDevice;
            var s = configManager.Settings;

            _isCustomFanEnabled = s.IsCustomFanEnabled;
            _applyToAllPowerModes = s.ApplyToAllPowerModes;
            _debugMode = s.DebugMode;
            _sensorIntervalMs = s.SensorIntervalMs;
            _calculationDelayMs = s.CalculationDelayMs;
            _isSmartAutoEnabled = s.IsSmartAutoEnabled;
            _smartSpikeFilterEnabled = s.SmartSpikeFilterEnabled;
            _curveSpikeFilterEnabled = s.CurveSpikeFilterEnabled;
            _smartHoldMinimumAtIdle = s.SmartHoldMinimumAtIdle;
            _syncFanLevel = s.SyncFanLevel;
            _ignoreZeroTemperature = s.IgnoreZeroTemperature;
            _alwaysWriteRpm = s.AlwaysWriteRpm;
            _writeRpmInHundreds = s.WriteRpmInHundreds;
            _forceWriteWhenRpmZero = s.ForceWriteWhenRpmZero;
            _minimumRpmChangeToApply = s.MinimumRpmChangeToApply;
            _minimumSpeedPercent = s.MinimumSpeedPercent;
            _spinUpBoostEnabled = s.SpinUpBoostEnabled;
            _spinUpBoostRpm = s.SpinUpBoostRpm;
            _spinUpBoostDurationMs = s.SpinUpBoostDurationMs;
            _forceRefreshOnModeSwitch = s.ForceRefreshOnModeSwitch;
            _modeSwitchRefreshCount = s.ModeSwitchRefreshCount;
            _modeSwitchRefreshDelayMs = s.ModeSwitchRefreshDelayMs;
            _forceRefreshOnEnable = s.ForceRefreshOnEnable;
            _clearCachedStateWhenLeavingCustomMode = s.ClearCachedStateWhenLeavingCustomMode;
            _uiUpdateIntervalMs = s.UiUpdateIntervalMs;
            _useCachedSnapshotForForcedRefresh = s.UseCachedSnapshotForForcedRefresh;
            _enableMaxFanWriteEachCycle = s.EnableMaxFanWriteEachCycle;
            
            _emaAlpha = s.EmaAlpha;
            _stepDownRateRpmPerSec = s.StepDownRateRpmPerSec;
            _stepDownSpamProtectionDelta = s.StepDownSpamProtectionDelta;
            _uiDebounceDelayMs = s.UiDebounceDelayMs;
            _safeMinTemp = s.SafeMinTemp;
            _safeMaxTemp = s.SafeMaxTemp;
            _criticalTemp = s.CriticalTemp;
            _smartSpikeIgnoreSeconds = s.SmartSpikeIgnoreSeconds;
            _smartRampUpRpmPerSec = s.SmartRampUpRpmPerSec;
            _smartUrgentRampUpRpmPerSec = s.SmartUrgentRampUpRpmPerSec;
            _smartUrgencyStartTemp = s.SmartUrgencyStartTemp;
            _smartUrgencyFullTemp = s.SmartUrgencyFullTemp;
            _smartDecreaseHoldSeconds = s.SmartDecreaseHoldSeconds;
            _smartRampDownRpmPerSec = s.SmartRampDownRpmPerSec;
            _smartSustainedWindowSeconds = s.SmartSustainedWindowSeconds;
            _smartSustainedTemp = s.SmartSustainedTemp;
            _smartSustainedHoldSeconds = s.SmartSustainedHoldSeconds;
            _smartSustainedRampDownRpmPerSec = s.SmartSustainedRampDownRpmPerSec;
            _smartSustainedRampUpRpmPerSec = s.SmartSustainedRampUpRpmPerSec;
            _smartCpuIdlePowerW = s.SmartCpuIdlePowerW;
            _smartCpuFullPowerW = s.SmartCpuFullPowerW;
            _smartGpuIdlePowerW = s.SmartGpuIdlePowerW;
            _smartGpuFullPowerW = s.SmartGpuFullPowerW;
            _syncPowerRangeWithPowerMode = s.SyncPowerRangeWithPowerMode;
            _criticalTempHoldSeconds = s.CriticalTempHoldSeconds;
            _safeMaxPercentAtMaxTemp = s.SafeMaxPercentAtMaxTemp;

            _enableAcousticOffset = s.EnableAcousticOffset;
            _acousticOffsetDeltaRpm = s.AcousticOffsetDeltaRpm;
            _acousticOffsetAddRpm = s.AcousticOffsetAddRpm;
            _hysteresisDeadzoneTemp = s.HysteresisDeadzoneTemp;
            _derivativeSpikeThreshold = s.DerivativeSpikeThreshold;
            _derivativeLookaheadSeconds = s.DerivativeLookaheadSeconds;

            _enablePredictiveEngine = s.EnablePredictiveEngine;
            _enableHysteresis = s.EnableHysteresis;
            _enableStepDownGlide = s.EnableStepDownGlide;
            _enableThermalSafetyNet = s.EnableThermalSafetyNet;
            _enableEma = s.EnableEma;
            
            _enableMinimumRpmChangeToApply = s.EnableMinimumRpmChangeToApply;
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SaveAll()
        {
            // One file write and one refresh for the whole window, instead of one per setting.
            using var batch = _configManager.BeginBatch();
            Save(nameof(CustomFanCurveSettings.IsCustomFanEnabled), _isCustomFanEnabled);
            Save(nameof(CustomFanCurveSettings.ApplyToAllPowerModes), _applyToAllPowerModes);
            Save(nameof(CustomFanCurveSettings.DebugMode), _debugMode);
            Save(nameof(CustomFanCurveSettings.SensorIntervalMs), _sensorIntervalMs);
            Save(nameof(CustomFanCurveSettings.CalculationDelayMs), _calculationDelayMs);
            Save(nameof(CustomFanCurveSettings.IsSmartAutoEnabled), _isSmartAutoEnabled);
            Save(nameof(CustomFanCurveSettings.SmartSpikeFilterEnabled), _smartSpikeFilterEnabled);
            Save(nameof(CustomFanCurveSettings.CurveSpikeFilterEnabled), _curveSpikeFilterEnabled);
            Save(nameof(CustomFanCurveSettings.SmartHoldMinimumAtIdle), _smartHoldMinimumAtIdle);
            Save(nameof(CustomFanCurveSettings.SyncFanLevel), _syncFanLevel);
            Save(nameof(CustomFanCurveSettings.IgnoreZeroTemperature), _ignoreZeroTemperature);
            Save(nameof(CustomFanCurveSettings.AlwaysWriteRpm), _alwaysWriteRpm);
            Save(nameof(CustomFanCurveSettings.WriteRpmInHundreds), _writeRpmInHundreds);
            Save(nameof(CustomFanCurveSettings.ForceWriteWhenRpmZero), _forceWriteWhenRpmZero);
            Save(nameof(CustomFanCurveSettings.MinimumRpmChangeToApply), _minimumRpmChangeToApply);
            Save(nameof(CustomFanCurveSettings.MinimumSpeedPercent), _minimumSpeedPercent);
            Save(nameof(CustomFanCurveSettings.SpinUpBoostEnabled), _spinUpBoostEnabled);
            Save(nameof(CustomFanCurveSettings.SpinUpBoostRpm), _spinUpBoostRpm);
            Save(nameof(CustomFanCurveSettings.SpinUpBoostDurationMs), _spinUpBoostDurationMs);
            Save(nameof(CustomFanCurveSettings.ForceRefreshOnModeSwitch), _forceRefreshOnModeSwitch);
            Save(nameof(CustomFanCurveSettings.ModeSwitchRefreshCount), _modeSwitchRefreshCount);
            Save(nameof(CustomFanCurveSettings.ModeSwitchRefreshDelayMs), _modeSwitchRefreshDelayMs);
            Save(nameof(CustomFanCurveSettings.ForceRefreshOnEnable), _forceRefreshOnEnable);
            Save(nameof(CustomFanCurveSettings.ClearCachedStateWhenLeavingCustomMode), _clearCachedStateWhenLeavingCustomMode);
            Save(nameof(CustomFanCurveSettings.UiUpdateIntervalMs), _uiUpdateIntervalMs);
            Save(nameof(CustomFanCurveSettings.UseCachedSnapshotForForcedRefresh), _useCachedSnapshotForForcedRefresh);
            Save(nameof(CustomFanCurveSettings.EnableMaxFanWriteEachCycle), _enableMaxFanWriteEachCycle);
            
            Save(nameof(CustomFanCurveSettings.EmaAlpha), _emaAlpha);
            Save(nameof(CustomFanCurveSettings.StepDownRateRpmPerSec), _stepDownRateRpmPerSec);
            Save(nameof(CustomFanCurveSettings.StepDownSpamProtectionDelta), _stepDownSpamProtectionDelta);
            Save(nameof(CustomFanCurveSettings.UiDebounceDelayMs), _uiDebounceDelayMs);
            Save(nameof(CustomFanCurveSettings.SafeMinTemp), _safeMinTemp);
            Save(nameof(CustomFanCurveSettings.SafeMaxTemp), _safeMaxTemp);
            Save(nameof(CustomFanCurveSettings.CriticalTemp), _criticalTemp);
            Save(nameof(CustomFanCurveSettings.SmartSpikeIgnoreSeconds), _smartSpikeIgnoreSeconds);
            Save(nameof(CustomFanCurveSettings.SmartRampUpRpmPerSec), _smartRampUpRpmPerSec);
            Save(nameof(CustomFanCurveSettings.SmartUrgentRampUpRpmPerSec), _smartUrgentRampUpRpmPerSec);
            Save(nameof(CustomFanCurveSettings.SmartUrgencyStartTemp), _smartUrgencyStartTemp);
            Save(nameof(CustomFanCurveSettings.SmartUrgencyFullTemp), _smartUrgencyFullTemp);
            Save(nameof(CustomFanCurveSettings.SmartDecreaseHoldSeconds), _smartDecreaseHoldSeconds);
            Save(nameof(CustomFanCurveSettings.SmartRampDownRpmPerSec), _smartRampDownRpmPerSec);
            Save(nameof(CustomFanCurveSettings.SmartSustainedWindowSeconds), _smartSustainedWindowSeconds);
            Save(nameof(CustomFanCurveSettings.SmartSustainedTemp), _smartSustainedTemp);
            Save(nameof(CustomFanCurveSettings.SmartSustainedHoldSeconds), _smartSustainedHoldSeconds);
            Save(nameof(CustomFanCurveSettings.SmartSustainedRampDownRpmPerSec), _smartSustainedRampDownRpmPerSec);
            Save(nameof(CustomFanCurveSettings.SmartSustainedRampUpRpmPerSec), _smartSustainedRampUpRpmPerSec);
            Save(nameof(CustomFanCurveSettings.SmartCpuIdlePowerW), _smartCpuIdlePowerW);
            Save(nameof(CustomFanCurveSettings.SmartCpuFullPowerW), _smartCpuFullPowerW);
            Save(nameof(CustomFanCurveSettings.SmartGpuIdlePowerW), _smartGpuIdlePowerW);
            Save(nameof(CustomFanCurveSettings.SmartGpuFullPowerW), _smartGpuFullPowerW);
            Save(nameof(CustomFanCurveSettings.SyncPowerRangeWithPowerMode), _syncPowerRangeWithPowerMode);
            Save(nameof(CustomFanCurveSettings.CriticalTempHoldSeconds), _criticalTempHoldSeconds);
            Save(nameof(CustomFanCurveSettings.SafeMaxPercentAtMaxTemp), _safeMaxPercentAtMaxTemp);

            Save(nameof(CustomFanCurveSettings.EnableAcousticOffset), _enableAcousticOffset);
            Save(nameof(CustomFanCurveSettings.AcousticOffsetDeltaRpm), _acousticOffsetDeltaRpm);
            Save(nameof(CustomFanCurveSettings.AcousticOffsetAddRpm), _acousticOffsetAddRpm);
            Save(nameof(CustomFanCurveSettings.HysteresisDeadzoneTemp), _hysteresisDeadzoneTemp);
            Save(nameof(CustomFanCurveSettings.DerivativeSpikeThreshold), _derivativeSpikeThreshold);
            Save(nameof(CustomFanCurveSettings.DerivativeLookaheadSeconds), _derivativeLookaheadSeconds);

            Save(nameof(CustomFanCurveSettings.EnablePredictiveEngine), _enablePredictiveEngine);
            Save(nameof(CustomFanCurveSettings.EnableHysteresis), _enableHysteresis);
            Save(nameof(CustomFanCurveSettings.EnableStepDownGlide), _enableStepDownGlide);
            Save(nameof(CustomFanCurveSettings.EnableThermalSafetyNet), _enableThermalSafetyNet);
            Save(nameof(CustomFanCurveSettings.EnableEma), _enableEma);
            
            Save(nameof(CustomFanCurveSettings.EnableMinimumRpmChangeToApply), _enableMinimumRpmChangeToApply);
        }

        private void Save<T>(string name, T value) => _configManager.UpdateSetting(name, value);

        private bool _isCustomFanEnabled;
        public bool IsCustomFanEnabled
        {
            get => _isCustomFanEnabled;
            set
            {
                if (_isCustomFanEnabled != value)
                {
                    _isCustomFanEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _applyToAllPowerModes;
        public bool ApplyToAllPowerModes
        {
            get => _applyToAllPowerModes;
            set
            {
                if (_applyToAllPowerModes != value)
                {
                    _applyToAllPowerModes = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _debugMode;
        public bool DebugMode
        {
            get => _debugMode;
            set
            {
                if (_debugMode != value)
                {
                    _debugMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _sensorIntervalMs;
        public int SensorIntervalMs
        {
            get => _sensorIntervalMs;
            set
            {
                if (_sensorIntervalMs != value)
                {
                    _sensorIntervalMs = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _calculationDelayMs;
        public int CalculationDelayMs
        {
            get => _calculationDelayMs;
            set
            {
                if (_calculationDelayMs != value)
                {
                    _calculationDelayMs = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSmartAutoEnabled;
        public bool IsSmartAutoEnabled
        {
            get => _isSmartAutoEnabled;
            set
            {
                if (_isSmartAutoEnabled != value)
                {
                    _isSmartAutoEnabled = value;
                    OnPropertyChanged();
                    OnSmartAutoModeChanged();
                }
            }
        }

        private bool _smartSpikeFilterEnabled;
        public bool SmartSpikeFilterEnabled
        {
            get => _smartSpikeFilterEnabled;
            set
            {
                if (_smartSpikeFilterEnabled != value)
                {
                    _smartSpikeFilterEnabled = value;
                    OnPropertyChanged();
                    OnSmartAutoModeChanged();
                }
            }
        }

        private bool _curveSpikeFilterEnabled;
        public bool CurveSpikeFilterEnabled
        {
            get => _curveSpikeFilterEnabled;
            set
            {
                if (_curveSpikeFilterEnabled != value)
                {
                    _curveSpikeFilterEnabled = value;
                    OnPropertyChanged();
                    OnSmartAutoModeChanged();
                }
            }
        }

        private bool _smartHoldMinimumAtIdle;
        public bool SmartHoldMinimumAtIdle
        {
            get => _smartHoldMinimumAtIdle;
            set
            {
                if (_smartHoldMinimumAtIdle != value)
                {
                    _smartHoldMinimumAtIdle = value;
                    OnPropertyChanged();
                    OnSmartAutoModeChanged();
                }
            }
        }

        // Settings that Smart Auto ignores are greyed out in the UI, with a note explaining why.
        // Hysteresis only exists in curve mode.
        public bool IsCurveOnlySettingsEnabled => !_isSmartAutoEnabled;
        // The spike filter replaces calculation throttling and step-down gliding, in whichever mode it is on.
        public bool IsCalcSchedulingIgnored => _isSmartAutoEnabled ? _smartSpikeFilterEnabled : _curveSpikeFilterEnabled;
        public bool IsCalcSchedulingEnabled => !IsCalcSchedulingIgnored;
        // The spike filter timings apply to whichever spike filter is on: Smart Auto's or the curve's.
        public bool IsSpikeFilterActive => IsCalcSchedulingIgnored;
        // Spin-up boost only fires from 0 RPM, which Smart Auto never writes while holding the minimum.
        public bool IsSpinUpIgnored => _isSmartAutoEnabled && _smartHoldMinimumAtIdle;
        public bool IsSpinUpEnabled => !IsSpinUpIgnored;

        private void OnSmartAutoModeChanged()
        {
            OnPropertyChanged(nameof(IsCurveOnlySettingsEnabled));
            OnPropertyChanged(nameof(IsCalcSchedulingIgnored));
            OnPropertyChanged(nameof(IsCalcSchedulingEnabled));
            OnPropertyChanged(nameof(IsSpikeFilterActive));
            OnPropertyChanged(nameof(IsSpinUpIgnored));
            OnPropertyChanged(nameof(IsSpinUpEnabled));
        }

        private bool _syncFanLevel;
        public bool SyncFanLevel
        {
            get => _syncFanLevel;
            set
            {
                if (_syncFanLevel != value)
                {
                    _syncFanLevel = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _ignoreZeroTemperature;
        public bool IgnoreZeroTemperature
        {
            get => _ignoreZeroTemperature;
            set
            {
                if (_ignoreZeroTemperature != value)
                {
                    _ignoreZeroTemperature = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _alwaysWriteRpm;
        public bool AlwaysWriteRpm
        {
            get => _alwaysWriteRpm;
            set
            {
                if (_alwaysWriteRpm != value)
                {
                    _alwaysWriteRpm = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _writeRpmInHundreds;
        public bool WriteRpmInHundreds
        {
            get => _writeRpmInHundreds;
            set
            {
                if (_writeRpmInHundreds != value)
                {
                    _writeRpmInHundreds = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _forceWriteWhenRpmZero;
        public bool ForceWriteWhenRpmZero
        {
            get => _forceWriteWhenRpmZero;
            set
            {
                if (_forceWriteWhenRpmZero != value)
                {
                    _forceWriteWhenRpmZero = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minimumRpmChangeToApply;
        public int MinimumRpmChangeToApply
        {
            get => _minimumRpmChangeToApply;
            set
            {
                if (_minimumRpmChangeToApply != value)
                {
                    _minimumRpmChangeToApply = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minimumSpeedPercent;
        public int MinimumSpeedPercent
        {
            get => _minimumSpeedPercent;
            set
            {
                int clamped = Math.Clamp(value, 0, 100);
                if (_minimumSpeedPercent != clamped)
                {
                    _minimumSpeedPercent = clamped;
                    OnPropertyChanged();
                }
            }
        }

        private bool _spinUpBoostEnabled;
        public bool SpinUpBoostEnabled
        {
            get => _spinUpBoostEnabled;
            set
            {
                if (_spinUpBoostEnabled != value)
                {
                    _spinUpBoostEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _spinUpBoostRpm;
        public int SpinUpBoostRpm
        {
            get => _spinUpBoostRpm;
            set
            {
                if (_spinUpBoostRpm != value)
                {
                    _spinUpBoostRpm = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _spinUpBoostDurationMs;
        public int SpinUpBoostDurationMs
        {
            get => _spinUpBoostDurationMs;
            set
            {
                if (_spinUpBoostDurationMs != value)
                {
                    _spinUpBoostDurationMs = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _forceRefreshOnModeSwitch;
        public bool ForceRefreshOnModeSwitch
        {
            get => _forceRefreshOnModeSwitch;
            set
            {
                if (_forceRefreshOnModeSwitch != value)
                {
                    _forceRefreshOnModeSwitch = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _modeSwitchRefreshCount;
        public int ModeSwitchRefreshCount
        {
            get => _modeSwitchRefreshCount;
            set
            {
                if (_modeSwitchRefreshCount != value)
                {
                    _modeSwitchRefreshCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _modeSwitchRefreshDelayMs;
        public int ModeSwitchRefreshDelayMs
        {
            get => _modeSwitchRefreshDelayMs;
            set
            {
                if (_modeSwitchRefreshDelayMs != value)
                {
                    _modeSwitchRefreshDelayMs = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _forceRefreshOnEnable;
        public bool ForceRefreshOnEnable
        {
            get => _forceRefreshOnEnable;
            set
            {
                if (_forceRefreshOnEnable != value)
                {
                    _forceRefreshOnEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _clearCachedStateWhenLeavingCustomMode;
        public bool ClearCachedStateWhenLeavingCustomMode
        {
            get => _clearCachedStateWhenLeavingCustomMode;
            set
            {
                if (_clearCachedStateWhenLeavingCustomMode != value)
                {
                    _clearCachedStateWhenLeavingCustomMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _uiUpdateIntervalMs;
        public int UiUpdateIntervalMs
        {
            get => _uiUpdateIntervalMs;
            set
            {
                if (_uiUpdateIntervalMs != value)
                {
                    _uiUpdateIntervalMs = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _useCachedSnapshotForForcedRefresh;
        public bool UseCachedSnapshotForForcedRefresh
        {
            get => _useCachedSnapshotForForcedRefresh;
            set
            {
                if (_useCachedSnapshotForForcedRefresh != value)
                {
                    _useCachedSnapshotForForcedRefresh = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableMaxFanWriteEachCycle;
        public bool EnableMaxFanWriteEachCycle
        {
            get => _enableMaxFanWriteEachCycle;
            set
            {
                if (_enableMaxFanWriteEachCycle != value)
                {
                    _enableMaxFanWriteEachCycle = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _emaAlpha;
        public double EmaAlpha
        {
            get => _emaAlpha;
            set { var v = Math.Clamp(value, 0.0001, 1.0); if (Math.Abs(_emaAlpha - v) > 0.00001) { _emaAlpha = v; OnPropertyChanged(); } }
        }

        private int _stepDownRateRpmPerSec;
        public int StepDownRateRpmPerSec
        {
            get => _stepDownRateRpmPerSec;
            set { if (_stepDownRateRpmPerSec != value) { _stepDownRateRpmPerSec = value; OnPropertyChanged(); } }
        }

        private int _stepDownSpamProtectionDelta;
        public int StepDownSpamProtectionDelta
        {
            get => _stepDownSpamProtectionDelta;
            set { if (_stepDownSpamProtectionDelta != value) { _stepDownSpamProtectionDelta = value; OnPropertyChanged(); } }
        }

        private int _uiDebounceDelayMs;
        public int UiDebounceDelayMs
        {
            get => _uiDebounceDelayMs;
            set { if (_uiDebounceDelayMs != value) { _uiDebounceDelayMs = value; OnPropertyChanged(); } }
        }

        private int _safeMinTemp;
        public int SafeMinTemp
        {
            get => _safeMinTemp;
            set { if (_safeMinTemp != value) { _safeMinTemp = value; OnPropertyChanged(); } }
        }

        private int _safeMaxTemp;
        public int SafeMaxTemp
        {
            get => _safeMaxTemp;
            set { if (_safeMaxTemp != value) { _safeMaxTemp = value; OnPropertyChanged(); } }
        }

        private int _criticalTemp;
        public int CriticalTemp
        {
            get => _criticalTemp;
            set { if (_criticalTemp != value) { _criticalTemp = value; OnPropertyChanged(); } }
        }

        // Spike filter, sustained heat, Smart Auto power range and critical hold, previously editable only in the config file.
        private double _smartSpikeIgnoreSeconds;
        public double SmartSpikeIgnoreSeconds
        {
            get => _smartSpikeIgnoreSeconds;
            set { var v = Math.Max(0, value); if (Math.Abs(_smartSpikeIgnoreSeconds - v) > 0.0001) { _smartSpikeIgnoreSeconds = v; OnPropertyChanged(); } }
        }

        private int _smartRampUpRpmPerSec;
        public int SmartRampUpRpmPerSec
        {
            get => _smartRampUpRpmPerSec;
            set { var v = Math.Max(0, value); if (_smartRampUpRpmPerSec != v) { _smartRampUpRpmPerSec = v; OnPropertyChanged(); } }
        }

        private int _smartUrgentRampUpRpmPerSec;
        public int SmartUrgentRampUpRpmPerSec
        {
            get => _smartUrgentRampUpRpmPerSec;
            set { var v = Math.Max(0, value); if (_smartUrgentRampUpRpmPerSec != v) { _smartUrgentRampUpRpmPerSec = v; OnPropertyChanged(); } }
        }

        private int _smartUrgencyStartTemp;
        public int SmartUrgencyStartTemp
        {
            get => _smartUrgencyStartTemp;
            set { var v = Math.Max(0, value); if (_smartUrgencyStartTemp != v) { _smartUrgencyStartTemp = v; OnPropertyChanged(); } }
        }

        private int _smartUrgencyFullTemp;
        public int SmartUrgencyFullTemp
        {
            get => _smartUrgencyFullTemp;
            set { var v = Math.Max(0, value); if (_smartUrgencyFullTemp != v) { _smartUrgencyFullTemp = v; OnPropertyChanged(); } }
        }

        private double _smartDecreaseHoldSeconds;
        public double SmartDecreaseHoldSeconds
        {
            get => _smartDecreaseHoldSeconds;
            set { var v = Math.Max(0, value); if (Math.Abs(_smartDecreaseHoldSeconds - v) > 0.0001) { _smartDecreaseHoldSeconds = v; OnPropertyChanged(); } }
        }

        private int _smartRampDownRpmPerSec;
        public int SmartRampDownRpmPerSec
        {
            get => _smartRampDownRpmPerSec;
            set { var v = Math.Max(0, value); if (_smartRampDownRpmPerSec != v) { _smartRampDownRpmPerSec = v; OnPropertyChanged(); } }
        }

        private double _smartSustainedWindowSeconds;
        public double SmartSustainedWindowSeconds
        {
            get => _smartSustainedWindowSeconds;
            set { var v = Math.Max(0, value); if (Math.Abs(_smartSustainedWindowSeconds - v) > 0.0001) { _smartSustainedWindowSeconds = v; OnPropertyChanged(); } }
        }

        private int _smartSustainedTemp;
        public int SmartSustainedTemp
        {
            get => _smartSustainedTemp;
            set { var v = Math.Max(0, value); if (_smartSustainedTemp != v) { _smartSustainedTemp = v; OnPropertyChanged(); } }
        }

        private double _smartSustainedHoldSeconds;
        public double SmartSustainedHoldSeconds
        {
            get => _smartSustainedHoldSeconds;
            set { var v = Math.Max(0, value); if (Math.Abs(_smartSustainedHoldSeconds - v) > 0.0001) { _smartSustainedHoldSeconds = v; OnPropertyChanged(); } }
        }

        private int _smartSustainedRampDownRpmPerSec;
        public int SmartSustainedRampDownRpmPerSec
        {
            get => _smartSustainedRampDownRpmPerSec;
            set { var v = Math.Max(0, value); if (_smartSustainedRampDownRpmPerSec != v) { _smartSustainedRampDownRpmPerSec = v; OnPropertyChanged(); } }
        }

        private int _smartSustainedRampUpRpmPerSec;
        public int SmartSustainedRampUpRpmPerSec
        {
            get => _smartSustainedRampUpRpmPerSec;
            set { var v = Math.Max(0, value); if (_smartSustainedRampUpRpmPerSec != v) { _smartSustainedRampUpRpmPerSec = v; OnPropertyChanged(); } }
        }

        private int _smartCpuIdlePowerW;
        public int SmartCpuIdlePowerW
        {
            get => _smartCpuIdlePowerW;
            set { var v = Math.Max(0, value); if (_smartCpuIdlePowerW != v) { _smartCpuIdlePowerW = v; OnPropertyChanged(); } }
        }

        private int _smartCpuFullPowerW;
        public int SmartCpuFullPowerW
        {
            get => _smartCpuFullPowerW;
            set { var v = Math.Max(0, value); if (_smartCpuFullPowerW != v) { _smartCpuFullPowerW = v; OnPropertyChanged(); } }
        }

        private int _smartGpuIdlePowerW;
        public int SmartGpuIdlePowerW
        {
            get => _smartGpuIdlePowerW;
            set { var v = Math.Max(0, value); if (_smartGpuIdlePowerW != v) { _smartGpuIdlePowerW = v; OnPropertyChanged(); } }
        }

        private int _smartGpuFullPowerW;
        public int SmartGpuFullPowerW
        {
            get => _smartGpuFullPowerW;
            set { var v = Math.Max(0, value); if (_smartGpuFullPowerW != v) { _smartGpuFullPowerW = v; OnPropertyChanged(); } }
        }

        private bool _syncPowerRangeWithPowerMode;
        public bool SyncPowerRangeWithPowerMode
        {
            get => _syncPowerRangeWithPowerMode;
            set
            {
                if (_syncPowerRangeWithPowerMode != value)
                {
                    _syncPowerRangeWithPowerMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPowerRangeManual));
                    OnPropertyChanged(nameof(PowerModePowerRangeText));
                }
            }
        }

        // The full-load fields are greyed out while they follow the power mode.
        public bool IsPowerRangeManual => !_syncPowerRangeWithPowerMode;

        public string PowerModePowerRangeText
        {
            get
            {
                PowerRangeSource.EnsureFirmwareDefaultsLoaded();
                var current = PowerRangeSource.CurrentMode;
                var lines = new System.Collections.Generic.List<string>();
                foreach (var mode in new[] { PowerModeState.Quiet, PowerModeState.Balance, PowerModeState.Performance, PowerModeState.Extreme, PowerModeState.GodMode })
                {
                    var limits = PowerRangeSource.GetLimits(mode);
                    if (!limits.HasAny) continue;
                    string name = mode == PowerModeState.GodMode ? Resources.Resource.SyncPowerRangeCustomMode : mode.ToString();
                    string cpu = limits.CpuFullW is { } c ? $"{c} W" : "–";
                    string gpu = limits.GpuFullW is { } g ? $"{g} W ({limits.GpuTgpW} + {limits.GpuBoostW ?? 0})" : "–";
                    lines.Add((mode == current ? "▶ " : "   ") + string.Format(Resources.Resource.SyncPowerRangeValues, name, cpu, gpu));
                }

                return lines.Count == 0 ? Resources.Resource.SyncPowerRangeUnavailable : string.Join("\n", lines);
            }
        }

        private double _criticalTempHoldSeconds;
        public double CriticalTempHoldSeconds
        {
            get => _criticalTempHoldSeconds;
            set { var v = Math.Max(0, value); if (Math.Abs(_criticalTempHoldSeconds - v) > 0.0001) { _criticalTempHoldSeconds = v; OnPropertyChanged(); } }
        }

        private int _safeMaxPercentAtMaxTemp;
        public int SafeMaxPercentAtMaxTemp
        {
            get => _safeMaxPercentAtMaxTemp;
            set
            {
                int clamped = Math.Clamp(value, 0, 100);
                if (_safeMaxPercentAtMaxTemp != clamped)
                {
                    _safeMaxPercentAtMaxTemp = clamped;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableAcousticOffset;
        public bool EnableAcousticOffset
        {
            get => _enableAcousticOffset;
            set { if (_enableAcousticOffset != value) { _enableAcousticOffset = value; OnPropertyChanged(); } }
        }

        private int _acousticOffsetDeltaRpm;
        public int AcousticOffsetDeltaRpm
        {
            get => _acousticOffsetDeltaRpm;
            set { var v = Math.Max(0, value); if (_acousticOffsetDeltaRpm != v) { _acousticOffsetDeltaRpm = v; OnPropertyChanged(); } }
        }

        private int _acousticOffsetAddRpm;
        public int AcousticOffsetAddRpm
        {
            get => _acousticOffsetAddRpm;
            set { var v = Math.Max(0, value); if (_acousticOffsetAddRpm != v) { _acousticOffsetAddRpm = v; OnPropertyChanged(); } }
        }

        private int _hysteresisDeadzoneTemp;
        public int HysteresisDeadzoneTemp
        {
            get => _hysteresisDeadzoneTemp;
            set { var v = Math.Max(0, value); if (_hysteresisDeadzoneTemp != v) { _hysteresisDeadzoneTemp = v; OnPropertyChanged(); } }
        }

        private int _derivativeSpikeThreshold;
        public int DerivativeSpikeThreshold
        {
            get => _derivativeSpikeThreshold;
            set { var v = Math.Max(0, value); if (_derivativeSpikeThreshold != v) { _derivativeSpikeThreshold = v; OnPropertyChanged(); } }
        }

        private int _derivativeLookaheadSeconds;
        public int DerivativeLookaheadSeconds
        {
            get => _derivativeLookaheadSeconds;
            set { var v = Math.Max(0, value); if (_derivativeLookaheadSeconds != v) { _derivativeLookaheadSeconds = v; OnPropertyChanged(); } }
        }

        private bool _enablePredictiveEngine;
        public bool EnablePredictiveEngine
        {
            get => _enablePredictiveEngine;
            set { if (_enablePredictiveEngine != value) { _enablePredictiveEngine = value; OnPropertyChanged(); } }
        }

        private bool _enableHysteresis;
        public bool EnableHysteresis
        {
            get => _enableHysteresis;
            set { if (_enableHysteresis != value) { _enableHysteresis = value; OnPropertyChanged(); } }
        }

        private bool _enableStepDownGlide;
        public bool EnableStepDownGlide
        {
            get => _enableStepDownGlide;
            set { if (_enableStepDownGlide != value) { _enableStepDownGlide = value; OnPropertyChanged(); } }
        }

        private bool _enableThermalSafetyNet;
        public bool EnableThermalSafetyNet
        {
            get => _enableThermalSafetyNet;
            set { if (_enableThermalSafetyNet != value) { _enableThermalSafetyNet = value; OnPropertyChanged(); } }
        }

        private bool _enableEma;
        public bool EnableEma
        {
            get => _enableEma;
            set { if (_enableEma != value) { _enableEma = value; OnPropertyChanged(); } }
        }

        private bool _enableMinimumRpmChangeToApply;
        public bool EnableMinimumRpmChangeToApply
        {
            get => _enableMinimumRpmChangeToApply;
            set { if (_enableMinimumRpmChangeToApply != value) { _enableMinimumRpmChangeToApply = value; OnPropertyChanged(); } }
        }

        public ICommand ResetToDefaultsCommand { get; }

        public void ResetToDefaults()
        {
            var defaultSettings = new CustomFanCurveSettings();

            IsCustomFanEnabled = defaultSettings.IsCustomFanEnabled;
            ApplyToAllPowerModes = defaultSettings.ApplyToAllPowerModes;
            DebugMode = defaultSettings.DebugMode;
            SensorIntervalMs = defaultSettings.SensorIntervalMs;
            CalculationDelayMs = defaultSettings.CalculationDelayMs;
            IsSmartAutoEnabled = defaultSettings.IsSmartAutoEnabled;
            SmartSpikeFilterEnabled = defaultSettings.SmartSpikeFilterEnabled;
            CurveSpikeFilterEnabled = defaultSettings.CurveSpikeFilterEnabled;
            SmartHoldMinimumAtIdle = defaultSettings.SmartHoldMinimumAtIdle;
            SyncFanLevel = defaultSettings.SyncFanLevel;
            IgnoreZeroTemperature = defaultSettings.IgnoreZeroTemperature;
            AlwaysWriteRpm = defaultSettings.AlwaysWriteRpm;
            WriteRpmInHundreds = defaultSettings.WriteRpmInHundreds;
            ForceWriteWhenRpmZero = defaultSettings.ForceWriteWhenRpmZero;
            MinimumRpmChangeToApply = defaultSettings.MinimumRpmChangeToApply;
            MinimumSpeedPercent = defaultSettings.MinimumSpeedPercent;
            SpinUpBoostEnabled = defaultSettings.SpinUpBoostEnabled;
            SpinUpBoostRpm = defaultSettings.SpinUpBoostRpm;
            SpinUpBoostDurationMs = defaultSettings.SpinUpBoostDurationMs;
            ForceRefreshOnModeSwitch = defaultSettings.ForceRefreshOnModeSwitch;
            ModeSwitchRefreshCount = defaultSettings.ModeSwitchRefreshCount;
            ModeSwitchRefreshDelayMs = defaultSettings.ModeSwitchRefreshDelayMs;
            ForceRefreshOnEnable = defaultSettings.ForceRefreshOnEnable;
            ClearCachedStateWhenLeavingCustomMode = defaultSettings.ClearCachedStateWhenLeavingCustomMode;
            UiUpdateIntervalMs = defaultSettings.UiUpdateIntervalMs;
            UseCachedSnapshotForForcedRefresh = defaultSettings.UseCachedSnapshotForForcedRefresh;
            EnableMaxFanWriteEachCycle = defaultSettings.EnableMaxFanWriteEachCycle;
            
            EmaAlpha = defaultSettings.EmaAlpha;
            StepDownRateRpmPerSec = defaultSettings.StepDownRateRpmPerSec;
            StepDownSpamProtectionDelta = defaultSettings.StepDownSpamProtectionDelta;
            UiDebounceDelayMs = defaultSettings.UiDebounceDelayMs;
            SafeMinTemp = defaultSettings.SafeMinTemp;
            SafeMaxTemp = defaultSettings.SafeMaxTemp;
            CriticalTemp = defaultSettings.CriticalTemp;
            SmartSpikeIgnoreSeconds = defaultSettings.SmartSpikeIgnoreSeconds;
            SmartRampUpRpmPerSec = defaultSettings.SmartRampUpRpmPerSec;
            SmartUrgentRampUpRpmPerSec = defaultSettings.SmartUrgentRampUpRpmPerSec;
            SmartUrgencyStartTemp = defaultSettings.SmartUrgencyStartTemp;
            SmartUrgencyFullTemp = defaultSettings.SmartUrgencyFullTemp;
            SmartDecreaseHoldSeconds = defaultSettings.SmartDecreaseHoldSeconds;
            SmartRampDownRpmPerSec = defaultSettings.SmartRampDownRpmPerSec;
            SmartSustainedWindowSeconds = defaultSettings.SmartSustainedWindowSeconds;
            SmartSustainedTemp = defaultSettings.SmartSustainedTemp;
            SmartSustainedHoldSeconds = defaultSettings.SmartSustainedHoldSeconds;
            SmartSustainedRampDownRpmPerSec = defaultSettings.SmartSustainedRampDownRpmPerSec;
            SmartSustainedRampUpRpmPerSec = defaultSettings.SmartSustainedRampUpRpmPerSec;
            SmartCpuIdlePowerW = defaultSettings.SmartCpuIdlePowerW;
            SmartCpuFullPowerW = defaultSettings.SmartCpuFullPowerW;
            SmartGpuIdlePowerW = defaultSettings.SmartGpuIdlePowerW;
            SmartGpuFullPowerW = defaultSettings.SmartGpuFullPowerW;
            SyncPowerRangeWithPowerMode = defaultSettings.SyncPowerRangeWithPowerMode;
            CriticalTempHoldSeconds = defaultSettings.CriticalTempHoldSeconds;
            SafeMaxPercentAtMaxTemp = defaultSettings.SafeMaxPercentAtMaxTemp;

            EnableAcousticOffset = defaultSettings.EnableAcousticOffset;
            AcousticOffsetDeltaRpm = defaultSettings.AcousticOffsetDeltaRpm;
            AcousticOffsetAddRpm = defaultSettings.AcousticOffsetAddRpm;
            HysteresisDeadzoneTemp = defaultSettings.HysteresisDeadzoneTemp;
            DerivativeSpikeThreshold = defaultSettings.DerivativeSpikeThreshold;
            DerivativeLookaheadSeconds = defaultSettings.DerivativeLookaheadSeconds;

            EnablePredictiveEngine = defaultSettings.EnablePredictiveEngine;
            EnableHysteresis = defaultSettings.EnableHysteresis;
            EnableStepDownGlide = defaultSettings.EnableStepDownGlide;
            EnableThermalSafetyNet = defaultSettings.EnableThermalSafetyNet;
            EnableEma = defaultSettings.EnableEma;

            EnableMinimumRpmChangeToApply = defaultSettings.EnableMinimumRpmChangeToApply;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
