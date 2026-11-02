# LenovoLegionToolkit Plugins: CustomFanCurve fixes for Legion 5 16IRX9

This is a fork of [LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins](https://github.com/LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins). It fixes the **CustomFanCurve** plugin on Lenovo Legion models whose firmware handles WMI fan control differently from what the plugin expects.

## Why this fork exists

On a **Legion 5 16IRX9** (machine type 83DG, BIOS NMCN34WW), the upstream plugin did not work:

- **Custom curve mode:** the fan curve didn't apply. CPU fan always at 100%.
- **Smart Auto (AI) mode:**
  - At idle, the fans burst by up to +1,500 RPM from any powerdraw rise although temps rose only by 1-2C.
  - The fans never settled. At idle the BIOS took over and wandered between 1,700 and 1,900 RPM.
  - Target 0 RPM is not supported.
  - In games, the fans swung up and down with every temperature peak.

The plugin was sending values that this firmware reads differently.

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
| 3. Power spikes | New *ramp gate* between Smart Auto and the fans, described below. On by default, with a **Spike Filter** switch under Settings → Advanced. |
| 4. BIOS idle handoff | New **Hold Minimum Speed at Idle** switch under Settings → Advanced (off by default). When on, Smart Auto holds the fan minimum instead of writing `0`. Custom curve mode is unchanged. |
| 5. Fixed power range | Idle and full-load watts are now settings, separately for CPU and GPU. |
| Confusing settings | Settings that Smart Auto ignores are now greyed out with a "Not used while Smart Auto is on" note: calculation interval, temperature/power delta filters, hysteresis, step-down gliding and spin-up boost. |

The ramp gate works like this:
- **Short spikes are ignored.** A higher demand must last 5 seconds before the fans respond. The wait shrinks as the temperature approaches 85°C, so a real heat-up, such as a game starting, gets a fast response. A single hot reading never triggers it.
- **Ramps are smooth.** The fans ramp up gradually. When demand drops, they hold for 3 seconds, then glide down.
- **Games get steadier fans.** Repeated ramp-ups within 2 minutes switch to longer holds (15 s), a slower glide down and gentler climbs. The fans then stay steady between peaks instead of following every one.
- **Safety isn't delayed.** At 90°C the fans respond instantly, and the existing 95°C critical override is unchanged.

**Results on the 16IRX9:**
- Idle: a flat 1,000 RPM instead of the BIOS's 1,700–1,900 RPM.
- Light use: no fan bursts.
- Gaming: fans respond in seconds and stay steadier between peaks.

### Further tuning in this fork only

These changes are on `main` only, not in the upstream proposal. They were checked against 3.5 hours of debug logs from the 16IRX9.

**Duplicate writes are skipped.** In hundreds mode, two different RPM targets can land on the same firmware step (for example 3,110 and 3,140 both become 31). Re-sending the same step restarts the firmware's speed regulation, the same wobble described in problem 2. About 16% of all writes did that. The plugin now treats a change within the same step as no change. A fan that has drifted far from its target is still re-sent its value.

**Fan Speed Sync no longer re-writes every cycle.** Whenever Sync raised a fan's target, it forced a write even when the fan already had that value. It now writes only real changes.

**Acoustic Tuning was adapted.** It offsets the second fan slightly so the two fans don't hum at almost the same pitch (beating).
- **It's skipped at idle.** As shipped, it would have fired in 45% of cycles, and 95% of those were both fans idling at minimum. There it only made the idle GPU fan louder.
- **It no longer toggles.** Once active, it stays on until the fans drift further apart, instead of switching on and off around the 100 RPM threshold.
- **It works in whole firmware steps.** In hundreds mode, a 150 RPM offset rounded unpredictably to +100 or +200 RPM. The second fan is now placed whole 100 RPM steps away, or left alone if the fans already land on different steps.

**EMA temperature smoothing is time-based.** The smoothing factor used to apply per sensor reading, but readings arrive anywhere from 0.5 to over 6 seconds apart. `EmaAlpha` is now the weight per second. At 0.35/s, on this laptop's CPU, it removes about 70% of reading-to-reading jitter (sharp 4°C+ jumps went from 82 to 3 in the test log). On a real heat-up it lags only 2–4°C for a few seconds.

**With EMA on, the safety paths use the smoothed temperature too.** That covers the Thermal Safety Net floor, the ramp urgency and the 90°C emergency bypass. On this CPU an app launch boosts it to 88–97°C for 1–3 seconds. With raw temperatures, the Safety Net floor alone pushed the fans to about 70% for each launch, even though fans can't cool a 2-second spike: the heat sink takes tens of seconds to respond. The 95°C critical override still uses raw readings, as the last line of defence.

**Other settings for Smart Auto on this laptop:**

| Setting | Recommendation | Why |
|---|---|---|
| EMA smoothing | **On, 0.35** | Steadier fan targets from a jittery CPU temperature |
| Acoustic Tuning | Optional | Helps only if you hear the fans beating at similar speeds |
| Fan Speed Sync | Off | The CPU does most daily work; Sync would spin up the idle GPU fan. The Thermal Safety Net already brings in both fans above 75°C. |
| Predictive Engine | **Off** | On this CPU it would have fired 77 times in 3.5 hours of desktop use, sending the fans to maximum 39 times |
| Hysteresis | Off | Only applies to curve mode |
| Asymmetric Step-Down | Off | Smart Auto's ramp gate already glides down |
| Spin-Up Boost | Off | Only fires when a fan starts from 0 RPM, which Smart Auto no longer sends |

### Enabling the fixes

The Spike Filter is on by default. Every other new setting defaults to the upstream behaviour, so nothing else changes until you set it.

- **In the settings screen** (Settings → Advanced): turn on **Hold Minimum Speed at Idle**.
- **In the config file:** the rest aren't in the settings screen yet. With Legion Toolkit closed, add or adjust them in `%LOCALAPPDATA%\LenovoLegionToolkit\Plugins\Configs\CustomFanCurve\custom_fan_curve_settings.json`:

```json
"SmartHoldMinimumAtIdle": true,
"WriteRpmInHundreds": true,
"AlwaysWriteRpm": false,
"SmartCpuIdlePowerW": 20,
"SmartCpuFullPowerW": 70,
"SmartGpuIdlePowerW": 20,
"SmartGpuFullPowerW": 140,
"EnableEma": true,
"EmaAlpha": 0.35,
```

Set the power values to your own limits: the CPU long-term power limit, and the GPU TGP plus Dynamic Boost.

Your machine probably needs `WriteRpmInHundreds` if custom curves run the fans at full speed, or if the measured fan speeds bear no relation to the targets shown in the plugin.

Fixes 1–5 are on the [`fix/wmi-units-and-smart-auto`](https://github.com/OddGentleman/LenovoLegionToolkit-Plugins/tree/fix/wmi-units-and-smart-auto) branch as separate commits, intended for upstream. The further tuning is on `main` only. For build instructions, see the [upstream README](https://github.com/LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins#building).
