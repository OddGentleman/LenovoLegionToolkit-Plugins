using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Controllers.GodMode;
using LenovoLegionToolkit.Lib.Settings;

namespace LenovoLegionToolkit.Plugin.CustomFanCurve
{
    // Smart Auto Power Range: Full-load watts come either from the plugin's own settings or, with sync on, from the power mode
    // that is active: the Custom mode preset in Legion Toolkit, or the firmware's defaults for Quiet, Balance, Performance and
    // Extreme. CPU full load is the long-term power limit; GPU full load is the configurable TGP plus Dynamic Boost.
    internal static class PowerRangeSource
    {
        public readonly record struct PowerLimits(int? CpuFullW, int? GpuTgpW, int? GpuBoostW)
        {
            public int? GpuFullW => GpuTgpW is > 0 ? GpuTgpW + (GpuBoostW ?? 0) : null;
            public bool HasAny => CpuFullW != null || GpuFullW != null;
        }

        private static volatile object? _currentMode; // boxed PowerModeState; null until the service has read the mode
        private static volatile Dictionary<PowerModeState, PowerLimits>? _firmwareDefaults;
        private static int _defaultsRequested;

        public static PowerModeState? CurrentMode => _currentMode is PowerModeState mode ? mode : null;

        // Called by the service whenever it reads the power mode, so the lookup per sensor cycle stays synchronous.
        public static void SetPowerMode(PowerModeState mode)
        {
            _currentMode = mode;
            EnsureFirmwareDefaultsLoaded();
        }

        // The firmware's per-mode defaults are fixed, so they are read once in the background.
        public static void EnsureFirmwareDefaultsLoaded()
        {
            if (Interlocked.Exchange(ref _defaultsRequested, 1) != 0)
                return;

            _ = Task.Run(async () =>
            {
                try
                {
                    _firmwareDefaults = await ReadFirmwareDefaultsAsync().ConfigureAwait(false);
                    foreach (var kv in _firmwareDefaults)
                        Logger.Debug($"Power range defaults: {kv.Key} cpuFull={kv.Value.CpuFullW} gpuFull={kv.Value.GpuFullW}");
                }
                catch (Exception ex)
                {
                    Logger.Debug($"Firmware power mode defaults unavailable: {ex.Message}");
                    _firmwareDefaults = new Dictionary<PowerModeState, PowerLimits>();
                }
            });
        }

        public static PowerLimits GetLimits(PowerModeState mode)
        {
            if (mode == PowerModeState.GodMode)
                return ReadCustomMode();

            return _firmwareDefaults is { } defaults && defaults.TryGetValue(mode, out var limits) ? limits : default;
        }

        public static PowerLimits GetCurrentLimits() => CurrentMode is { } mode ? GetLimits(mode) : default;

        // GodModeSettings is a singleton shared with the Legion Toolkit UI, so edits there are picked up without a restart.
        public static PowerLimits ReadCustomMode()
        {
            try
            {
                return ReadCustomModeCore();
            }
            catch (Exception ex)
            {
                Logger.Debug($"Custom mode power limits unavailable: {ex.Message}");
                return default;
            }
        }

        public static int GetCpuFullPowerW(CustomFanCurveSettings settings) =>
            settings.SyncPowerRangeWithPowerMode && GetCurrentLimits().CpuFullW is { } w ? w : settings.SmartCpuFullPowerW;

        public static int GetGpuFullPowerW(CustomFanCurveSettings settings) =>
            settings.SyncPowerRangeWithPowerMode && GetCurrentLimits().GpuFullW is { } w ? w : settings.SmartGpuFullPowerW;

        // Kept out of line so that a host build without these members fails inside the callers' try blocks.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static PowerLimits ReadCustomModeCore()
        {
            var store = IoCContainer.TryResolve<GodModeSettings>()?.Store;
            if (store == null || !store.Presets.TryGetValue(store.ActivePresetId, out var preset))
                return default;

            return new PowerLimits(
                Positive(preset.CPULongTermPowerLimit?.Value),
                Positive(preset.GPUConfigurableTGP?.Value),
                Positive(preset.GPUPowerBoost?.Value));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static async Task<Dictionary<PowerModeState, PowerLimits>> ReadFirmwareDefaultsAsync()
        {
            var result = new Dictionary<PowerModeState, PowerLimits>();
            var controller = IoCContainer.TryResolve<IGodModeController>();
            if (controller == null)
                return result;

            var defaults = await controller.GetDefaultsInOtherPowerModesAsync().ConfigureAwait(false);
            foreach (var kv in defaults)
            {
                result[kv.Key] = new PowerLimits(
                    Positive(kv.Value.CPULongTermPowerLimit),
                    Positive(kv.Value.GPUConfigurableTGP),
                    Positive(kv.Value.GPUPowerBoost));
            }

            return result;
        }

        private static int? Positive(int? value) => value is > 0 ? value : null;
    }
}
