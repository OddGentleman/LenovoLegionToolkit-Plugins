# LenovoLegionToolkit Plugins: CustomFanCurve fixes for Legion 5 16IRX9

This is a fork of [LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins](https://github.com/LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins). It fixes the **CustomFanCurve** plugin on Lenovo Legion models whose firmware handles WMI fan control differently from what the plugin expects.

## Why this fork exists

On a **Legion 5 16IRX9** (machine type 83DG, BIOS NMCN34WW), the upstream plugin did not work:

- **Custom curve mode:** the CPU fan ran at 100% whatever the curve said.
- **Smart Auto (AI) mode:**
  - At idle, the fans burst by up to +1,500 RPM every 5–10 seconds.
  - The fans never settled. At idle the BIOS took over and wandered between 1,700 and 1,900 RPM.
  - In games, the fans swung up and down with every temperature peak.

The suggested cause was that the machine is incompatible with WMI mode and needs EC (embedded controller) mode. That turned out to be wrong. WMI fan control works on this machine; the plugin was sending values that this firmware reads differently.

Only the 16IRX9 has been tested. Other Legion models with the same firmware behaviour are likely affected too.

## What was wrong in the upstream plugin

**1. Fan speeds were written in the wrong units.**
This firmware reads the `SetFeatureValue` fan speed as a single byte in units of 100 RPM. It keeps only the lowest byte of the raw RPM value the plugin sends. Measured over WMI:

| Plugin writes | Firmware keeps | Measured fan speed |
|---|---|---|
| 1048 | 24 | 2,400 RPM |
| 1800 | 8 | 800 RPM |
| 1500 – 5000 | 136 – 220 | full speed (5,600 RPM) |

Most curve targets therefore landed on a high byte and ran the fan at full speed. Reading fan speeds was never affected, since reads return plain RPM.

**2. Keep-alive writes made the fans oscillate.**
With "Always write RPM" on, the plugin re-sent the same value every 2.5 seconds. Each write restarted the firmware's speed regulation. At a steady target the fan dipped and overshot between 1,200 and 1,500 RPM, in step with the writes.

**3. Smart Auto reacted to short power spikes.**
Up to 40% of the fan speed came straight from the momentary CPU power draw, with no smoothing. Opening an app was enough to make the fans burst.

**4. Smart Auto handed idle control to the BIOS.**
At idle, the Smart Auto target (about 885 RPM) was below the fan minimum. The plugin then wrote `0`, which on Lenovo firmware means "BIOS, take over". It re-sent that every 2.5 seconds, so the BIOS's own fluctuating idle speed ran the fans.

**5. Smart Auto's power range was fixed at 20–65 W.**
The GPU in this laptop draws up to 140 W, so any game counted as full load. That kept the GPU fan near maximum regardless of how hard the GPU was actually working.

## What was fixed and how

| Problem | Fix |
|---|---|
| 1. Wrong units | New `WriteRpmInHundreds` setting. When on, every write is converted to hundreds of RPM (1,800 → 18). |
| 2. Keep-alive oscillation | Turn off the existing `AlwaysWriteRpm` setting. No code change needed. |
| 3. Power spikes | New *ramp gate* between Smart Auto and the fans, described below. |
| 4. BIOS idle handoff | Smart Auto now holds the fan minimum instead of writing `0`. Custom curve mode is unchanged. |
| 5. Fixed power range | Idle and full-load watts are now settings, separately for CPU and GPU. |

The ramp gate works like this:
- **Short spikes are ignored.** A higher demand must last 5 seconds before the fans respond. The wait shrinks as the temperature approaches 85°C, so a real heat-up, such as a game starting, gets a fast response. A single hot reading never triggers it.
- **Ramps are smooth.** The fans ramp up gradually. When demand drops, they hold for 3 seconds, then glide down.
- **Games get steadier fans.** Repeated ramp-ups within 2 minutes switch to longer holds (15 s), a slower glide down and gentler climbs. The fans then stay steady between peaks instead of following every one.
- **Safety isn't delayed.** At 90°C the fans respond instantly, and the existing 95°C critical override is unchanged.

**Results on the 16IRX9:**
- Idle: a flat 1,000 RPM instead of the BIOS's 1,700–1,900 RPM.
- Light use: no fan bursts.
- Gaming: fans respond in seconds and stay steadier between peaks.

### Enabling the fixes

All new settings default to the upstream behaviour, so nothing changes until you set them. They are not in the plugin's settings screen yet. With Legion Toolkit closed, add or adjust them in `%LOCALAPPDATA%\LenovoLegionToolkit\Plugins\Configs\CustomFanCurve\custom_fan_curve_settings.json`:

```json
"WriteRpmInHundreds": true,
"AlwaysWriteRpm": false,
"SmartCpuIdlePowerW": 20,
"SmartCpuFullPowerW": 70,
"SmartGpuIdlePowerW": 20,
"SmartGpuFullPowerW": 140,
```

Set the power values to your own limits: the CPU long-term power limit, and the GPU TGP plus Dynamic Boost.

Your machine probably needs `WriteRpmInHundreds` if custom curves run the fans at full speed, or if the measured fan speeds bear no relation to the targets shown in the plugin.

The changes are on the [`fix/wmi-units-and-smart-auto`](https://github.com/OddGentleman/LenovoLegionToolkit-Plugins/tree/fix/wmi-units-and-smart-auto) branch as separate commits, intended for upstream. For build instructions, see the [upstream README](https://github.com/LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins#building).
