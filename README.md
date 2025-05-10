<p align="center">
  <img src="docs/images/swell_logo_def.png" alt="SWELL logo" width="200"/>
</p>

# SWELL - Simulated Waves of Energy, Light and Life

[![License: CC BY-NC 3.0](https://img.shields.io/badge/License-CC%20BY--NC%203.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc/3.0/)
[![Platform](https://img.shields.io/badge/platform-Windows--only-blue)](https://microsoft.com)  
[![Language](https://img.shields.io/badge/language-R%20%7C%20C%23-purple)](https://cran.r-project.org/)  
![Status](https://img.shields.io/badge/status-active-brightgreen)

---

## 📖 Overview

**SWELL** is a phenology model for simulating the daily vegetation index dynamics of remote sensing pixels in **deciduous forest ecosystems**, based on **photoperiod and temperature-driven physiological cues**.

The model divides plant phenology into **dormancy and growing seasons**, representing phenophase transitions through ecophysiological functions to offer a biologically meaningful alternative to traditional statistical NDVI fitting techniques.

> **Note:** SWELL uses compiled **C# routines**, which are invoked from R. It is currently available only on **Windows** but cross-platform capabilities are under development.

---

## Table of Contents

- [Highlights](#highlights)
- [Description](#description)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Support](#support)
- [License](#license)
- [How It Works](#how-it-works)
  
---

## Highlights 

- 🌿 SWELL simulates NDVI and EVI using photothermal phenological cues  
- 🧠 Process-based approach to deciduous vegetation dynamics  
- 📡 Accurate reproduction of satellite NDVI and EVI data through automatic calibration  
- 🔬 Analysis of phenophase timing, duration and shift across environment 
- ⚙️ Hybrid architecture: **R interface + C# computation core**  
- 🪟 **Windows-only** (MacOS and Linux coming soon)
  
---

## Description

Vegetation phenology is essential for understanding how ecosystems respond to climate change. Remote sensing provides large-scale phenological insights — but traditional curve-fitting methods often lack physiological relevance.

**SWELL** addresses this gap with a process-based approach that simulates NDVI and EVI as a function of:

- **Photoperiod**
- **Air temperature**
- **Species-specific photothermal thresholds**

Validated on European beech (*Fagus sylvatica*) using MODIS data (2012–2021), SWELL showed strong performance across diverse ecoregions and seasons, matching or exceeding statistical fitting benchmarks. 

📄 **See the preprint of the SWELL presentation paper**: [Simulated Waves of Energy, Light and Life (SSRN, 2024)](https://papers.ssrn.com/sol3/papers.cfm?abstract_id=5078831)

<figure>
  <p align="center">
    <img src="./docs/images/swell_schema.png" width="600">
  </p>
  <figcaption align="center"><em>Schematic of simulated NDVI curve across seasonal phases</em></figcaption>
</figure>

SWELL uses mathematical functions to simulate phenological phases:

### 🌙 Dormancy Season

- **Dormancy Induction**: Triggered by cold, short days.  
- **Endodormancy**: Simulated through chilling unit accumulation.  
- **Ecodormancy**: Promoted by warming temperatures and longer photoperiods.

### ☀️ Growing Season

- **Growth**: Rapid NDVI/EVI rise from leaf-out to seasonal peak.  
- **Greendown**: Plateau or slight decrease.  
- **Decline**: Sharp NDVI/EVI drop during senescence.

### 🌿 NDVI/EVI Simulation
NDVI or EVI are modeled daily, combining understory and overstory vegetation signals in a biologically interpretable framework.

> 📘 Full details and equations are provided in the [⚙️ How It Works](#how-it-works) section of this readme.

---

## Installation

> ⚠️ **Platform notice:**  
> SWELL currently runs **only on Windows** due to its use of compiled C# executables.  
> Cross-platform support (macOS/Linux) is under active development.

### 1. Install R

Make sure **R (version ≥ 4.0)** is installed:  
🔗 [https://cran.r-project.org/](https://cran.r-project.org/)

### 2. Install Required R Packages

Before installing SWELL, make sure the following R packages are installed:

```r
install.packages(c("devtools", "data.table", "jsonlite"))
```

### 3. Install SWELL from GitHub

You can install the SWELL package directly using devtools:

```r
devtools::install_github("GeoModelLab/SWELL")
```

This will:

    Download the SWELL R package

    Compile and install it locally

    Include the precompiled C# executable required for model execution (Windows only)

### Access Documentation    
All core functions (swellCalibration, swellValidation, etc.) include Roxygen-style documentation. You can access help directly from R:

```r
?swellCalibration
?swellValidation
```
Or use the RStudio help viewer by placing your cursor inside the function and pressing F1.

The C# source code for the SWELL computational engine is included in the repository under the /src directory. A precompiled Windows .exe is also bundled under inst/extdata/Windows/.

    📦 The R functions handle all the configuration and execution automatically by calling this backend executable.

---

## Getting Started

The SWELL model consists of two main processes: **calibration** and **validation**. For each process, two functions are available in the R package to allow users executing SWELL on a dataframe with multiple pixels or to perform batch executions for heavy simulation jobs. 

---

### 1. `swellCalibration()`

Performs **calibration** of the SWELL model using NDVI or EVI time series and weather data via a **multi-start simplex algorithm**. Returns all outputs in R as structured data frames.

**Usage:**
```r
result <- swellCalibration(
  weather_data = your_weather_df,
  vegetation_data = your_ndvi_or_evi_df,
  vegetationIndex = "NDVI",
  SWELLparameters = parameter_list,
  species = "beech",
  start_year = 2012,
  end_year = 2021,
  simplexes = 3,
  iterations = 1000
)

# Output structure:
# result$calibration_results     → Daily NDVI simulations and rates
# result$parameters_pixels       → Calibrated parameters by pixel
# result$parameters_group        → Mean ± SD grouped by vegetation Group
```
### 2. `swellCalibrationBatch()`

Performs batch calibration. Ideal for automated pipelines or high-performance computing setups. Saves CSV outputs to disk (no in-memory R return).

**Usage:**
```r
swellCalibrationBatch(
  weather_data = your_weather_df,
  vegetation_data = your_ndvi_or_evi_df,
  vegetationIndex = "EVI",
  SWELLparameters = parameter_list,
  species = "beech",
  start_year = 2012,
  end_year = 2021,
  simplexes = 5,
  iterations = 500,
  outPath = "path/to/output/"
)
```
📂 Output files:

    parameters_group.csv – Calibrated mean ± SD per group
    results_by_pixel.csv – Time series of NDVI simulation

### 3. swellValidation()

Performs model validation using calibrated parameters and weather/NDVI input. Returns simulation results as an R data frame.

Usage:
```r
val <- swellValidation(
  weather_data = your_weather_df,
  vegetation_data = your_ndvi_or_evi_df,
  vegetationIndex = "NDVI",
  SWELLparameters = parameter_list,
  SWELLparametersCalibrated = param_group_df,
  species = "beech",
  start_year = 2012,
  end_year = 2021,
  validationReplicates = 10
)

# Output: val → Simulated NDVI/EVI with uncertainty bands (percentiles)
```

### 4. swellValidationBatch()

Runs batch validation, ideal for integration with automated systems. Results are saved to disk.
```r
val <- swellValidation(
  weather_data = your_weather_df,
  vegetation_data = your_ndvi_or_evi_df,
  vegetationIndex = "NDVI",
  SWELLparameters = parameter_list,
  SWELLparametersCalibrated = param_group_df,
  species = "beech",
  start_year = 2012,
  end_year = 2021,
  validationReplicates = 10
)

# Output: val → Simulated NDVI/EVI with uncertainty bands (percentiles)
```
---

## Support

Need help?

- Open an issue: https://github.com/GeoModelLab/SWELL/issues
- Contact the maintainer via email (see DESCRIPTION file)

---
## License
This project is licensed under the **Creative Commons Attribution-NonCommercial 3.0 Unported (CC BY-NC 3.0)** license.

You are free to:
- **Share** — copy and redistribute the material in any medium or format
- **Adapt** — remix, transform, and build upon the material

Under the following terms:
- **Attribution** — You must give appropriate credit.
- **NonCommercial** — You may not use the material for commercial purposes.

🔗 [View the full license](https://creativecommons.org/licenses/by-nc/3.0/)

---

## How it works

This section provides a detailed look at the internal mechanics of the SWELL model, including the mathematical and physiological functions used to simulate NDVI/EVI.

> 🧠 Recommended for advanced users and researchers interested in model structure and ecophysiological logic.

These are the functions used by SWELL to simulate the plant response to air temperature and photoperiod in different phenological phases.
<figure>
<p align="center">
  <img src="./docs/images/all_functions.png" width="700">
  </p>
  <figcaption align="center"><em>Functions used to simulate the effect of photoperiod and temperature on plant phenological phases</em></figcaption>
</figure>

Every day and during phenophase _x_, a photothermal unit _PTU<sub>x</sub>_, (day<sup>-1</sup>, eq. 1) is computed, summed, and divided by the phenophase photothermal requirements _PTR<sub>x</sub>_ (days, eq. 2) to derive its percentage of completion _PC<sub>x</sub>_ (%). 

$$PTU_x = PU_x \times TU_x \tag{1}$$
$$PC_x = \frac{\sum_{1}^{n} PTU_x}{PTR_x} \times 100\tag{2}$$

where _PU<sub>x</sub>_ (0-1, day<sup>-1</sup>) and _TU<sub>x</sub>_ (0-1, day<sup>-1</sup>) are the photoperiodic and thermal components of _PTU<sub>x</sub>_, respectively, and _n_ is the number of days elapsed from the start of the _x_ phenophase. 

### Dormancy season
The [Dormancy induction](#dormancy-induction) is stimulated by short and cold days. Once induced, the dormancy season unfolds through [Endodormancy](#endodormancy), when chilling accumulation occurs, and [Ecodormancy](#ecodormancy), whose progress is promoted by longer days and warm temperatures.

#### Dormancy induction
The following equations (3 and 4) are used to draw the logistic function displayed in the figure below, which is used to estimate the photoperiodic unit for dormancy induction _PU<sub>DI</sub>_.

$$
P_{\mathrm{DI\_mid}} = 0.5 \cdot (P_{\mathrm{DI\_l}} + P_{\mathrm{DI\_nl}}) \tag{3}
$$

$$
PU_{\mathrm{DI}} = \begin{cases}
  0 & \text{if } D_l \geq P_{\mathrm{DI\_l}} \\
  1 & \text{if } D_l \leq P_{\mathrm{DI\_nl}} \\
  \frac{1}{1 + e^{10/(P_{\mathrm{DI\_l}} - P_{\mathrm{DI\_nl}}) \cdot (D_l - P_{\mathrm{DI\_mid}})}} & \text{elsewhere}
\end{cases} \tag{4}
$$

where _P<sub>DI\_mid</sub>_ (hours) is the midpoint of the logistic function increasing from _P<sub>DI\_l</sub>_ (hours) to _P<sub>DI\_nl</sub>_ (hours), i.e., the limiting and non-limiting day length for dormancy induction, respectively; _Dl_ (hours) is the day length.

<figure>
<p align="center">
  <img src="./docs/images/PU_DI_doc.png"  width="400">
  </p>
  <figcaption align="center"><em>Photoperiodic unit for dormancy induction. The shades correspond to the 40-60<sup>th</sup> and 25-75<sup>th</sup> percentiles of the distribution generated with limiting photoperiod ranging from 12 to 14.5 hours and non limiting photoperiod from 11.5 to 13 hours.</em></figcaption>
</figure>


The thermal unit for dormancy induction are derived with another logistic function, using equations 5 and 6.

$$
T_{\mathrm{DI\_mid}} = 0.5 \cdot (T_{\mathrm{DI\_l}} + T_{\mathrm{DI\_nl}}) \tag{5}
$$

$$
TU_{\mathrm{DI}} =
\begin{cases}
0 & \text{if } T \geq T_{\mathrm{DI\_l}} \\
1 & \text{if } T_{\mathrm{l}} \leq T_{\mathrm{DI\_nl}} \\
\frac{1}{1 + e^{10/(T_{\mathrm{DI\_l}} - T_{\mathrm{DI\_nl}}) \cdot (T - T_{\mathrm{DI\_mid}})}} & \text{elsewhere}
\end{cases}
\tag{6}
$$

where _T<sub>DI\_mid</sub>_ (°C) is the midpoint of the logistic function increasing from _T<sub>DI\_l</sub>_ (°C) to _T<sub>DI\_nl</sub>_ (°C), i.e., the limiting and non-limiting temperature for dormancy induction, respectively; _T_ (°C) is the average daily air temperature.

<figure>
<p align="center">
  <img src="./docs/images/TU_DI_doc.png"  width="400">
  </p>
   <figcaption align="center"><em>Thermal units for dormancy induction. The shades correspond to the 40-60<sup>th</sup> and 25-75<sup>th</sup> percentiles of the distribution generated with limiting temperature ranging from 17 to 22 °C and non limiting temperatre from 3 to 8 °C.</em></figcaption>
</figure>

Each day, _PU<sub>DI</sub>_ and _TU<sub>DI</sub>_ are multiplied to give the daily photothermal unit of dormancy induction _PTU<sub>DI</sub>_ and the respective completion percentage _PC<sub>DI</sub>_

#### Endodormancy
After dormancy induction is completed, the endodormancy proceeds along with chilling units’ accumulation, computed according to the following equations 7 and 8. The chilling units are computed hourly, therefore the daily dynamic of air temperature is simulated from daily maximum and minimum air temperature. For more details, you can refer to [Soil Physics with BASIC](https://shop.elsevier.com/books/soil-physics-with-basic/campbell/978-0-444-42557-7).

$$
CU_{\text{mid}} = 
\begin{cases}
  0.5 \cdot (T_{\mathrm{EN\_l}\downarrow} + T_{\mathrm{EN\_nl}\downarrow}) & \text{if } T_h \leq T_{\mathrm{EN\_nl}\downarrow} \leq T_{\mathrm{EN\_l}\downarrow}, \\
  0.5 \cdot (T_{\mathrm{EN\_l}\uparrow} + T_{\mathrm{EN\_nl}\uparrow}) & \text{if } T_{\mathrm{EN\_nl}\uparrow} \leq T_h \leq T_{\mathrm{EN\_l}\uparrow}.
\end{cases} \tag{7}
$$

$$
CU_{\text{EN}} =
\begin{cases}
0 & \text{if } T_h \leq T_{\mathrm{EN\_l}\downarrow} \text{ or } T_h \geq T_{\mathrm{EN\_l}\uparrow}, \\
1 & \text{if } T_{\mathrm{EN\_nl}\downarrow} \leq T_h \leq T_{\mathrm{EN\_nl}\uparrow}, \\
\frac{1}{1 + e^{-10/(T_{\mathrm{EN\_l}\downarrow} - T_{\mathrm{EN\_nl}\downarrow}) \cdot (T_h - CU_{\text{mid}})}} & \text{if } T_{\mathrm{EN\_l}\downarrow} < T_h < T_{\mathrm{EN\_nl}\downarrow}, \\
\frac{1}{1 + e^{10/(T_{\mathrm{EN\_l}\uparrow} - T_{\mathrm{EN\_nl}\uparrow}) \cdot (T_h - CU_{\text{mid}})}} & \text{if } T_{\mathrm{EN\_nl}\uparrow} < T_h < T_{\mathrm{EN\_l}\uparrow}.
\end{cases} \tag{8}
$$


where _T<sub>EN\_l↓</sub>_ (°C) and _T<sub>EN\_l↑</sub>_ (°C) are the lower and upper limiting temperature for chilling units accumulation, and _T<sub>EN\_nl↓</sub>_ (°C) and _T<sub>EN\_nl↑</sub>_ (°C) are the lower and upper not-limiting thresholds for chilling units accumulation. Daily chilling units (_CU<sub>EN\_d</sub>_, day<sup>-1</sup>) are computed summing _CU<sub>EN</sub>_ (hour<sup>-1</sup>), and the endodormancy completion (_PC<sub>EN</sub>_, %) is derived as for dormancy induction. 
The resulting function is displayed below.

<figure>
 <p align="center">
  <img src="./docs/images/CU_doc.png"  width="400">
  </p>
  <figcaption align="center"><em>Chilling units accumulation. The shades correspond to the 40-60<sup>th</sup> and 25-75<sup>th</sup> percentiles of the distribution generated with lower limiting chilling temperature ranging from -5 to -2 °C, lower non limiting chilling temperature from 1 to 5°C, upper non limiting temperature from 2 to 6 °C and upper limiting temperature from 9 to 12 °C</em></figcaption>
</figure>


#### Ecodormancy
High _PC<sub>EN</sub>_ values accelerates the progress of the [ecodormancy](#ecodormancy) (_PTU<sub>EC_</sub>, day<sup>-1</sup>), whose completion is stimulated by long days and warm temperatures. The following equations 8-10 are used to estimate photothermal units during the ecodormancy phase.

$$
P_r = \frac{D_l}{P_{\mathrm{EC\_nl}}} \tag{9}
$$

$$
T_{\mathrm{EC\_mid}} = 0.5 \cdot T_{\mathrm{EC\_nl}} + (1 - P_r) \cdot T_{\mathrm{EC\_nl}} \tag{10}
$$

$$
PTU_{\mathrm{EC}} = \frac{PC_{\mathrm{EN}} + (1 - PC_{\mathrm{EN}}) \cdot P_r}{1 + e^{-10/(T_{\mathrm{EC\_nl}} \cdot P_r \cdot (T - T_{\mathrm{EC\_mid}}))}} \cdot \frac{1}{100} \tag{11}
$$

where _Pr_ is the ratio between _Dl_ and the not-limiting photoperiod for ecodormancy (_P<sub>EC\_nl</sub>_, hour); _T<sub>EC\_mid</sub>_ (°C) is the midpoint of the logistic function reproducing the temperature effect. The function asymptote depends both on _PC<sub>EN</sub>_ and _Pr_. The figures below show the behaviour of these equations at two levels of endodormancy completion and different day lengths.

<figure>
  <p align="center">
    <img src="./docs/images/PTU_EC_20_doc.png" width="400">
    <img src="./docs/images/PTU_EC_80_doc.png" width="400">
  </p>
 <figcaption align="center"><em>Photothermal units for ecodormancy progress. The shades correspond to the 40-60<sup>th</sup> and 25-75<sup>th</sup> percentiles of the distribution generated with non limiting  temperature ranging from 18 to 22 °C, and non limiting photoperiod ranging from 10 to 13 hours</em></figcaption>
</figure>

### Growing season
When _PC<sub>EC</sub>_ = 100%, the ecodormancy phase is completed and the growing season begins. 

#### Growth and greendown
Forcing thermal units (_TU<sub>GR</sub>_, day<sup>-1</sup>) are computed with the equation 11, which is taken from [Yan and Hunt, 1999](https://www.ggebiplot.com/Yan-Hunt1999a.pdf)


$$
TU_{\mathrm{GR}} = \left( \frac{T_{\mathrm{max}} - T}{T_{\mathrm{max}} - T_{\mathrm{opt}}} \right) \cdot \left( \frac{T - T_{\mathrm{min}}}{T_{\mathrm{opt}} - T_{\mathrm{min}}} \right)^{\frac{T_{\mathrm{opt}} - T_{\mathrm{min}}}{T_{\mathrm{max}} - T_{\mathrm{opt}}}}
$$

where _T<sub>min</sub>_, _T<sub>opt</sub>_ and _T<sub>max</sub>_ are the minimum, optimum, and maximum tree cardinal forcing temperatures (°C). The growth and greendown phases are simulated as a function of _TU<sub>GR</sub>_ only.
The resulting function is drawn below.


<figure>
 <p align="center">
  <img src="./docs/images/TU_GR_doc.png"  width="400">
  </p>
  <figcaption align="center"><em>Forcing accumulation during growth, greendown and decline. The shades correspond to the 40-60<sup>th</sup> and 25-75<sup>th</sup> percentiles of the distribution generated with minimum temperature ranging from 3 to 7 °C, optimum temperature from 16 to 22°C, and maximum temperature from 28 and 35 °C</em></figcaption>
</figure>

#### Decline
During the decline phenophase, the photothermal unit (_PTU<sub>DE</sub>_, day<sup>-1</sup>) is simulated as the weighted average of _TU<sub>GR</sub>_ and the photothermal unit for [dormancy induction](#dormancy-induction), whose relative contribution depends on the percentage completion of the decline phenophase _PC<sub>DE</sub>_ (%), as in the equation below.

$$
PTU_{\mathrm{DE}} = TU_{\mathrm{GR}} \cdot (1 - PC_{\mathrm{DE}}) + PTU_{\mathrm{DI}} \cdot PC_{\mathrm{DE}} \tag{13}
$$

When _PC<sub>DE</sub>_ = 100%, the growing season ends, and the dormancy season restarts.

### NDVI simulation
SWELL simulates the pixel-level  dynamic (_NDVI<sub>swell</sub>_, unitless) by integrating a daily NDVI rate (day<sup>-1</sup>) within a lower (_NDVI<sub>min</sub>_, unitless) and upper (_NDVI<sub>min</sub>_ + _NDVI<sub>amp</sub>_, unitless) limit:

$$
NDVI_{\text{swell}} = \begin{cases}
  NDVI_{\text{min}} & \text{if } \sum_{1}^{n} NDVI_{r} \leq NDVI_{\text{min}} \\
  \sum_{1}^{n} NDVI_{r} & \text{if } NDVI_{\text{min}} < \sum_{1}^{n} NDVI_{r} < NDVI_{\text{min}} + NDVI_{\text{amp}} \\
  NDVI_{\text{min}} + NDVI_{\text{amp}} & \text{if } \sum_{1}^{n} NDVI_{r} \geq NDVI_{\text{min}} + NDVI_{\text{amp}}
\end{cases}\tag{15}
$$

where _NDVI<sub>amp</sub>_ is the NDVI amplitude and _n_ are the days elapsed from simulation start. During the dormancy season, _NDVI<sub>swell</sub>_ decreases with cold temperatures (_NDVI<sub>r\_D↓</sub>_), and increases when days are lengthening and forcing temperatures favour the awakening of the understory vegetation (_NDVI<sub>r\_D↑</sub>_). The following equations are used for this purpose.

$$
NDVI_{D\downarrow} = \begin{cases}
  0 & \text{if } T \geq T_{\text{min}} + T_{\text{shift}} \\
  NDVI_{D\downarrow*} \cdot \frac{\left| (T_{\text{min}} + T_{\text{shift}}) - T \right|}{10} & \text{if } T < T_{\text{min}} + T_{\text{shift}}
\end{cases}\tag{16}
$$


$$
NDVI_{D\uparrow} = \begin{cases}
  NDVI_{D\uparrow*} \cdot TU_{\text{under}} & \text{if } T \geq T_{\text{min}} + T_{\text{shift}} \text{ and } Dl > Dl_y \\
  0 & \text{if } T < T_{\text{min}} + T_{\text{shift}} \text{ and } Dl > Dl_y
\end{cases}\tag{17}
$$

where _NDVI<sub>D*</sub>_ (unitless) and _NDVI<sub>D*</sub>_ (unitless) are pixel-specific parameters representing the minimum NDVI decrease and the maximum NDVI increase during the dormancy season; _T<sub>shift</sub>_ (°C) represents the pixel-specific sensitivity of the understory vegetation to thermal cues; _TU<sub>under</sub> (day<sup>-1</sup>_) is the understory thermal unit, computed substituting _T<sub>min</sub>_ with _T<sub>min</sub>_ + _T<sub>shift</sub>_ (°C) in the equation for  [forcing](#growth-and-greendown) accumulation; _Dl<sub>y</sub>_ (hours) is the day length of the previous day. On the first dormancy day, the NDVI at senescence is initialized, ensuring it doesn't fall below the minimum allowed value (_NDVI<sub>min</sub>_). When average temperature drops below T_min + T_shift, the endodormancy contribution is calculated using a scaled temperature deficit, modulated by the normalized distance to NDVImin. When day length is increasing and temperatures exceed the thermal threshold, the ecodormancy contribution is computed using thermal forcing units and scaled by the proximity of current NDVI to its maximum (_NDVI<sub>max</sub>_). The final daily _NDVI<sub>r</sub> during dormancy thus combines both endodormancy and ecodormancy contribution.

During the growth phenophase, the daily NDVI rate (_NDVI<sub>GR</sub>_, day<sup>-1</sup>) refers to the dominant plant species and increases as a function of _TU<sub>GR</sub>_ (day<sup>-1</sup>) and _PC<sub>GR</sub>_ (%), i.e., the percentage completion of the growth phenophase.

$$
 NDVI_{\text{GR}} = NDVI_{\text{GR*}} \cdot TU_{\text{GR*}} \cdot \frac{(100 - PC_{\text{GR}})}{100} 
$$

where _NDVI<sub>GR*</sub>_ (day<sup>-1</sup>) is a pixel-specific parameter corresponding to the maximum _NDVI<sub>swell</sub>_ increase during the growth phase. After reaching the seasonal peak, _NDVI<sub>swell</sub>_ decreases during the greendown phenophase (NDVIr,GD, equation 18), and is further reduced during the decline phase (NDVIr,DE, equation 19). 


