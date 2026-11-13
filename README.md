# LenovoLegionToolkit Plugins: CustomFanCurve fixes for Legion 5 16IRX9

This is a fork of [LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins](https://github.com/LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins). It fixes the **CustomFanCurve** plugin on Lenovo Legion models whose firmware handles WMI fan control differently from what the plugin expects.

## Why this fork exists

On a **Legion 5 16IRX9** (machine type 83DG, BIOS NMCN34WW), the upstream plugin's Smart Auto (AI) mode did not work well:

- At idle, the fans burst by up to +1,500 RPM from any powerdraw rise although temps rose only by 1-2C.
- The fans never settled. At idle the BIOS took over and wandered between 1,700 and 1,900 RPM.
- Target 0 RPM is not supported.
- In games, the fans swung up and down with every temperature peak.

This README covers only what this fork changes. For everything else, see the [upstream README](https://github.com/LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins).

Only the 16IRX9 has been tested. Other Legion models with the same firmware behaviour are likely affected too.

## What was wrong in the upstream plugin

**1. Keep-alive writes made the fans oscillate.**
With "Always write RPM" on, the plugin re-sent the same value every 2.5 seconds. Each write restarted the firmware's speed regulation. At a steady target the fan dipped and overshot between 1,200 and 1,500 RPM, in step with the writes.

**2. Smart Auto reacted to short power spikes.**
Up to 40% of the fan speed came straight from the momentary CPU power draw, with no smoothing. Opening an app was enough to make the fans burst.

**3. Smart Auto handed idle control to the BIOS.**
At idle, the Smart Auto target (about 885 RPM) was below the fan minimum. The plugin then wrote `0`, which on Lenovo firmware means "BIOS, take over". It re-sent that every 2.5 seconds, so the BIOS's own fluctuating idle speed ran the fans.

**4. Smart Auto's power range was fixed at 20–65 W.**
The GPU in this laptop draws up to 140 W, so any game counted as full load. That kept the GPU fan near maximum regardless of how hard the GPU was actually working.

## What was fixed and how

| Problem | Fix |
|---|---|
| 1. Keep-alive oscillation | Turn off the existing `AlwaysWriteRpm` setting. No code change needed. |
| 2. Power spikes | New *ramp gate* between Smart Auto and the fans, described below. On by default, with a **Spike Filter** switch under Settings → Advanced. |
| 3. BIOS idle handoff | New **Hold Minimum Speed at Idle** switch under Settings → Advanced (off by default). When on, Smart Auto holds the fan minimum instead of writing `0`. Custom curve mode is unchanged. |
| 4. Fixed power range | Idle and full-load watts are now settings, separately for CPU and GPU. |
| Confusing settings | Settings that Smart Auto ignores are now greyed out with a "Not used while Smart Auto is on" note: calculation interval, hysteresis, step-down gliding and spin-up boost. |
| Sensor source hidden in Smart Auto | Smart Auto reads each fan's sensor source, but turning it on hid the curve editor, and the sensor dropdown with it. A fan left on "Average of CPU / GPU" from curve mode kept that setting with no way to see or change it. The Smart Auto dashboard now shows a sensor dropdown for each fan. The options were renamed to say what they follow: the old "System Default (Auto)" is now named after the fan's own chip (**CPU temperature** or **GPU temperature**), and the others read **Hotter of CPU and GPU** and **Average of CPU and GPU**. |
| Fans stopped after Fn+Q | Switching from Custom to Balance mode with Fn+Q left both fans stopped and the CPU throttled to about 400 MHz. The firmware takes the fans back by itself about 3 s after a mode switch, and the plugin's hand-back write, sent 0.3 s after the switch, interfered. The plugin now waits until 5 s have passed since the last mode switch (each further Fn+Q press restarts the wait) and hands back only if the fans are still at its speeds. With "apply to all power modes" on, it re-sends its targets once the firmware has settled. |
| Fans stopped after a mode switch | In 3 of 8 logged switches out of Custom mode, the firmware did not take the fans back and left both at 0 RPM. The plugin read that as "firmware has taken over" and left them stopped. A fan the plugin was driving that reads 0 RPM after the settle wait now also gets the hand-back. 5 s later the debug log records each fan's speed (`ModeSwitch: after hand-back`). |
| Plugin active after wakeup outside Custom mode | On resume, Legion Toolkit tells fan plugins they control the fans again, in every power mode. The plugin then switched on even in Quiet or Balance with "apply to all power modes" off, and the next mode switch could leave the fans stopped. It now re-checks the power mode on resume. |

The ramp gate works like this:
- **Short spikes are ignored.** A higher demand must last 5 seconds before the fans respond. The wait shrinks as the temperature approaches 85°C, so a real heat-up, such as a game starting, gets a fast response. A single hot reading never triggers it.
- **Ramps are smooth.** The fans ramp up gradually. When demand drops, they hold for 3 seconds, then glide down.
- **Games get steadier fans.** Under sustained heat (30-second average of the hotter chip at 72°C or above), the gate switches to longer holds (15 s), a slower glide down and gentler climbs. The fans then stay steady between peaks instead of following every one. In the test game the chip averaged 75–78°C; desktop bursts such as app launches and update checks averaged 67–70°C. An earlier version counted ramp-ups instead (two within 2 minutes), and ordinary desktop use triggered it.
- **Safety isn't delayed.** At 90°C the fans respond instantly, and the existing 95°C critical override is unchanged.

**Results on the 16IRX9:**
- Idle: a flat 1,000 RPM instead of the BIOS's 1,700–1,900 RPM.
- Light use: no fan bursts.
- Gaming: fans respond in seconds and stay steadier between peaks.

### Further tuning in this fork only

These changes are on `main` only, not in the upstream proposal. They were checked against 3.5 hours of debug logs from the 16IRX9.

**Duplicate writes are skipped.** In hundreds mode, two different RPM targets can land on the same firmware step (for example 3,110 and 3,140 both become 31). Re-sending the same step restarts the firmware's speed regulation, the same wobble described in problem 1. About 16% of all writes did that. The plugin now treats a change within the same step as no change. A fan that has drifted far from its target is still re-sent its value.

**Fan Speed Sync no longer re-writes every cycle.** Whenever Sync raised a fan's target, it forced a write even when the fan already had that value. It now writes only real changes.

**Acoustic Tuning was adapted.** It offsets the second fan slightly so the two fans don't hum at almost the same pitch (beating).
- **It's skipped at idle.** As shipped, it would have fired in 45% of cycles, and 95% of those were both fans idling at minimum. There it only made the idle GPU fan louder.
- **It no longer toggles.** Once active, it stays on until the fans drift further apart, instead of switching on and off around the 100 RPM threshold.
- **It works in whole firmware steps.** In hundreds mode, a 150 RPM offset rounded unpredictably to +100 or +200 RPM. The second fan is now placed whole 100 RPM steps away, or left alone if the fans already land on different steps.

**EMA temperature smoothing is time-based.** The smoothing factor used to apply per sensor reading, but readings arrive anywhere from 0.5 to over 6 seconds apart. `EmaAlpha` is now the weight per second. At 0.35/s, on this laptop's CPU, it removes about 70% of reading-to-reading jitter (sharp 4°C+ jumps went from 82 to 3 in the test log). On a real heat-up it lags only 2–4°C for a few seconds.

**With EMA on, the safety paths use the smoothed temperature too.** That covers the Thermal Safety Net floor, the ramp urgency and the 90°C emergency bypass. On this CPU an app launch boosts it to 88–97°C for 1–3 seconds. With raw temperatures, the Safety Net floor alone pushed the fans to about 70% for each launch, even though fans can't cool a 2-second spike: the heat sink takes tens of seconds to respond. The 95°C critical override still uses raw readings, as the last line of defence.

**Custom curves can use the spike filter too.** In curve mode the fans followed every short spike. In one logged app launch, a single 78°C CPU reading lifted the CPU fan from 1,850 to 2,200 RPM at once. Curve mode's own settings can't fully prevent that:
- Hysteresis only shifts the curve by 3°C while the fan spins.
- Step-Down Glide slows decreases but not increases, and never holds.
- A lower EMA alpha filters spikes, but it delays real heat-ups and the safety paths too.

A new **Curve Spike Filter** switch under Settings → Advanced (off by default) runs the curve's target through the same ramp gate as Smart Auto, with the same timings. Short spikes are ignored, rises ramp up gradually, decreases hold before gliding down, and sustained heat gets longer holds. The emergency bypass and critical override still apply. Calculation Delay and Step-Down Glide are greyed out while it's on.

**Settings for custom curves on this laptop:** turn on **Curve Spike Filter**, keep EMA at 0.35, and leave Hysteresis and Predictive Engine off. Keep the curve flat up to a few degrees above the idle temperature (this CPU idles around 56–61°C), so idle wobble doesn't move the fans.

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
| GPU fan sensor source | **GPU temperature** | "Hotter of CPU and GPU" made the GPU fan follow the CPU at idle (about 1,400 instead of 1,000 RPM) for only about 1°C lower CPU idle temperature. GPU temperature keeps the GPU fan silent, and the Thermal Safety Net still brings it in when either chip is above 75°C. Set per fan on the Smart Auto dashboard, or as `SensorSource` in the config (0 own chip, 1 hotter, 2 average). |
| CPU fan sensor source | CPU temperature or Average | Your choice. With the 1,000 RPM idle hold and the CPU under 20 W, the CPU fan rises above 1,000 RPM at about 60°C on **CPU temperature**. The CPU idles around 56°C, so light tasks lift it often. On **Average of CPU and GPU** it rises when the average reaches about 60°C, roughly 120 − GPU temperature on the CPU: about 77°C while the GPU sleeps at 43°C. That's quieter but slower to react to CPU-only heat. Either way, the Thermal Safety Net brings it in at about 78°C. These thresholds are calculated from the Smart Auto formula, not measured. |

**Debug log fields.** With Debug Mode on, each `recalc` line in `%LOCALAPPDATA%\LenovoLegionToolkit\log\plugin_LenovoLegionToolkit.Plugin.CustomFanCurve.CustomFanCurveProvider.log` also records:

| Field | Meaning |
|---|---|
| `rawTemp` | The fan's raw sensor temperature |
| `hwMaxRaw` | Raw temperature of the hotter chip |
| `safetyTemp` | The (smoothed) temperature the safety paths use |
| `safeMin` | The Thermal Safety Net's minimum fan speed |
| `cpuW` / `gpuW` | CPU and GPU power draw (−1 when the GPU is powered down) |

### Enabling the fixes

The Spike Filter is on by default. Every other new setting defaults to the upstream behaviour, so nothing else changes until you set it.

- **In the settings screen** (Settings → Advanced): turn on **Hold Minimum Speed at Idle** for Smart Auto, and **Curve Spike Filter** for custom curves. On the 16IRX9, also turn on upstream's **Write RPM in Hundreds** and set **Minimum Fan Speed** to 17% (see below).
- **In the config file:** the rest aren't in the settings screen yet. With Legion Toolkit closed, add or adjust them in `%LOCALAPPDATA%\LenovoLegionToolkit\Plugins\Configs\CustomFanCurve\custom_fan_curve_settings.json`:

```json
"SmartHoldMinimumAtIdle": true,
"AlwaysWriteRpm": false,
"SmartCpuIdlePowerW": 20,
"SmartCpuFullPowerW": 70,
"SmartGpuIdlePowerW": 20,
"SmartGpuFullPowerW": 140,
"EnableEma": true,
"EmaAlpha": 0.35,
"SmartSustainedWindowSeconds": 30,
"SmartSustainedTemp": 72,
```

If your config came from an earlier build of this fork, check `SmartSustainedWindowSeconds`. It used to mean "two ramp-ups within this many seconds" (saved as 120). It now sets the averaging time for sustained-heat detection, and 30 is the tested value.

Set the power values to your own limits: the CPU long-term power limit, and the GPU TGP plus Dynamic Boost.

**Minimum fan speed.** The 16IRX9 firmware reports a fan minimum of 0, so the minimum comes from upstream's **Minimum Fan Speed** percentage. 17% of the 5,600 RPM maximum gives 1,000 RPM, the speed the idle hold was tested at; the default of 10% gives 600 RPM. Unlike upstream, which raises every target below the minimum up to it, this fork keeps its own handling: curve targets below the minimum hand the fan back to the BIOS, and Smart Auto with **Hold Minimum Speed at Idle** holds the minimum.

Fixes 1–4 are on the [`fix/wmi-units-and-smart-auto`](https://github.com/OddGentleman/LenovoLegionToolkit-Plugins/tree/fix/wmi-units-and-smart-auto) branch as separate commits, intended for upstream. That branch also carries a units fix that upstream has since added as **Write RPM in Hundreds**. The further tuning is on `main` only. For build instructions, see the [upstream README](https://github.com/LenovoLegionToolkit-Team/LenovoLegionToolkit-Plugins#building).
