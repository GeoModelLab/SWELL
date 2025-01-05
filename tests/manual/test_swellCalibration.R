rm(list=ls())
#install.packages("pkgdown")
#if (!requireNamespace("devtools", quietly = TRUE)) {
#  install.packages("devtools")
#}
#set the working directory
setwd(dirname(rstudioapi::getActiveDocumentContext()$path))
remove.packages("SWELL")
devtools::install()
devtools::document()
#devtools::install(build_vignettes = TRUE, force = TRUE)
#devtools::document()
library(SWELL)
?swellValidation
load("..//..//data//SWELLparameters.rda")
print(ls()) # Should show "SWELLparameters" in the environment.

SWELLparameters

weather_data<-read.csv('C:\\Users\\simoneugomaria.brega\\Dropbox\\IO\\data\\e-obs\\data\\45.2498605458953_26.4498602851736.csv')

vegetation_data<-read.csv('C:\\Users\\simoneugomaria.brega\\Dropbox\\IO\\SWELL\\inst\\extdata\\files\\referenceData\\pixelsCalibrationEvi.csv')

start_year<-2011
end_year<-2021
simplexes<-1
iterations<-1
weather_data$Date<-as.Date(weather_data$Date)
library(tidyverse)

SWELLparameters <- SWELL::SWELLparameters %>%
  mutate(max = ifelse(parameter == "maximumNDVI", .9, max),
         max = ifelse(parameter == "minimumNDVI", .2, max),
         min = ifelse(parameter == "maximumNDVI", .6, min))
#SWELLparameters$calibration<-TRUE

pixels <- swellCalibration(weather_data,
                           vegetation_data |> filter(id=='10000a'),
                           vegetationIndex = 'EVI',
                           SWELLparameters,
                        start_year=2011,end_year=2021,
                        simplexes=4,iterations=1000)

results<-pixels$calibration_results
paramPixels<-pixels$parameters_pixels
paramGroups<-pixels$parameters_group

write.csv(paramGroups,'parametersValidationFile.csv', row.names = FALSE, quote = FALSE)



#devtools::check()
library(tidyverse)
ggplot(pixels[[2]]) + geom_boxplot(aes(y = value,fill=group))+
  facet_wrap(~param+class,scales='free_y')

ggplot(pixels[[1]] |> filter(year==2013), aes(x=doy)) +
  stat_summary(geom='line',aes(y = SWELL),size=2)+
  stat_summary(geom='point',aes(y = reference),size=2)+
  scale_color_manual(values=c('green','darkgreen','green3','red','brown'))+
  stat_summary(geom='area',aes(y = dormancyInductionRate),fill='red',alpha=0.2)+
   stat_summary(geom='area',aes(y = endodormancyRate),fill='blue',alpha=0.2)+
  stat_summary(geom='area',aes(y = ecodormancyRate),fill='orange',alpha=0.9)+
  stat_summary(geom='area',aes(y = growthRate),fill='green',alpha=0.2)+
  stat_summary(geom='area',aes(y = greendownRate),fill='darkgreen',alpha=0.2)+
  stat_summary(geom='area',aes(y = declineRate),fill='orange4',alpha=0.2)+
  facet_wrap(~group,nrow=2)



pixelsValid <- swellValidation(weather_data, vegetation_data, vegetationIndex = 'EVI',
                               SWELL::SWELLparameters,
                               paramGroups,
                               start_year=2011,end_year=2021,validationReplicates = 50)
paramGroups
unique(pixelsValid$pixel)

#devtools::check()
library(tidyverse)

ggplot(pixelsValid |> filter(year>=2011), aes(x=date)) +
  stat_summary(geom='line',aes(y = SWELL),size=.4)+
  stat_summary(geom='line',aes(y = SWELL_10),size=.4,color='red')+
  stat_summary(geom='line',aes(y = SWELL_25),size=.4,color='green')+
  stat_summary(geom='line',aes(y = SWELL_40),size=.4,color='blue')+
  stat_summary(geom='line',aes(y = SWELL_60),size=.4,color='blue')+
  stat_summary(geom='line',aes(y = SWELL_75),size=.4,color='green')+
  stat_summary(geom='line',aes(y = SWELL_90),size=.4,color='red')+
  stat_summary(geom='point',aes(y = reference),size=2)+
  scale_color_manual(values=c('green','darkgreen','green3','red','brown'))+
  stat_summary(geom='area',aes(y = dormancyInductionRate),fill='red',alpha=0.2)+
   stat_summary(geom='area',aes(y = endodormancyRate),fill='blue',alpha=0.2)+
  stat_summary(geom='area',aes(y = ecodormancyRate),fill='orange',alpha=0.9)+
  stat_summary(geom='area',aes(y = growthRate),fill='green',alpha=0.2)+
  stat_summary(geom='area',aes(y = greendownRate),fill='darkgreen',alpha=0.2)+
  stat_summary(geom='area',aes(y = declineRate),fill='orange4',alpha=0.2)
  facet_wrap(~group,nrow=2)



#find.package("SWELL")
#installed.packages()["SWELL", "LibPath"]



# Generate weather data with reasonable Tmin and Tmax values
set.seed(123) # For reproducibility

# Create a seasonal pattern for Tmin and Tmax
doy <- 1:365
tmin_pattern <- 5+ 10 * sin((2 * pi * doy / 365) - pi / 2) # Tmin peaks in summer
tmax_pattern <- 15 + 15 * sin((2 * pi * doy / 365) - pi / 2)  # Tmax peaks in summer

# Add small random noise to simulate variability
Tmin <- tmin_pattern + rnorm(365, mean = 0, sd = 2)
Tmax <- tmax_pattern + rnorm(365, mean = 0, sd = 3)

# Create the weather_data data frame
weather_data <- data.frame(
  Latitude = 45.0,
  Longitude = 7.5,
  Date = seq(as.Date("2011-01-01"), as.Date("2011-12-31"), by = "days"),
  Tmin = Tmin,
  Tmax = Tmax
)



doy <- seq(1, 365, by = 8)

# Simulate the seasonal pattern of NDVI-like data
# Use a logistic function to simulate growth in spring, plateau in summer, and decline in autumn
# Logistic growth function for spring and summer
vegetation_index <- 0.2 + (0.6 / (1 + exp(-0.1 * (doy - 90))))  # Rise starting from DOY 90 (March-April)

# Plateau in summer (around DOY 170 to 250)
vegetation_index[doy > 170 & doy <= 250] <- 0.75  # Values plateau at 0.9 during summer months

# Decline in autumn (from DOY 250 to the end of the year)
vegetation_index[doy > 250] <- 0.4 + (0.25 - (doy[doy > 250] - 250) * 0.009)  # Linear decline after DOY 250

# Clamp values to range [0, 1]
vegetation_index <- pmax(pmin(vegetation_index, 1), 0)

# Create the vegetation data frame
vegetation_data <- data.frame(
  PixelID = rep("pixel_1", length(doy)),
  Group = rep("group_1", length(doy)),
  Year = rep(2011, length(doy)),
  Doy = doy,
  Longitude = 7.5,
  Latitude = 45.0,
  VegetationIndex = vegetation_index
)
SWELLparameters <- SWELL::SWELLparameters

vegetation_data
pixelsCalib<-swellCalibration(
  weather_data, vegetation_data,
  vegetationIndex = "EVI",
  SWELL::SWELLparameters, start_year = 2011, end_year = 2011,
  simplexes=5, iteration=3000
)

pixelsCalib[[2]]
ggplot(pixelsCalib[[1]], aes(x=date)) +
  stat_summary(geom='line',aes(y = SWELL),size=.4)+
  stat_summary(geom='point',aes(y = reference),size=2)+
  scale_color_manual(values=c('green','darkgreen','green3','red','brown'))



SWELLparametersCalibrated <- data.frame(
  group = rep("group_1", 14),
  class = c("parVegetationIndex", "parVegetationIndex", "parVegetationIndex", "parVegetationIndex","parVegetationIndex", "parVegetationIndex", "parVegetationIndex", "parVegetationIndex", "parEndodormancy","parDormancy", "parEcodormancy",
    "parSenescence", "parGreendown", "parGrowth"),
  parameter = c("maximumNDVI", "minimumNDVI", "nNDVIDecline","nNDVIEcodormancy", "nNDVIEndodormancy", "nNDVIGreendown", "nNDVIGrowth","pixelTemperatureShift",
    "chillingThreshold", "photoThermalThreshold", "photoThermalThreshold", "photoThermalThreshold", "thermalThreshold", "thermalThreshold"),
  mean = c(107, 0.81, 0.11, 1.1,1.66, 1.01, 0.36, 1.78, 41.2, 13.8, 62.7, 2.7, 74.3, 47.9),
  sd = c(24.1, 0.07, 0.05, 0.58, 0.62, 0.51, 0.07, 0.72, 11.7, 5.0, 14.1, 2.1,
    15.7, 11.6)
)


pixelsValid<-swellValidation(
  weather_data, vegetation_data, vegetationIndex = "EVI",
  SWELLparameters, pixelsCalib[[2]],
  start_year = 2011, end_year = 2011, validationReplicates = 100
)

ggplot(pixelsValid, aes(x=date)) +
  stat_summary(geom='line',aes(y = SWELL),size=.4)+
  stat_summary(geom='line',aes(y = SWELL_10),size=.4,color='red')+
  stat_summary(geom='line',aes(y = SWELL_25),size=.4,color='green')+
  stat_summary(geom='line',aes(y = SWELL_40),size=.4,color='blue')+
  stat_summary(geom='line',aes(y = SWELL_60),size=.4,color='blue')+
  stat_summary(geom='line',aes(y = SWELL_75),size=.4,color='green')+
  stat_summary(geom='line',aes(y = SWELL_90),size=.4,color='red')+
  stat_summary(geom='point',aes(y = reference),size=2)+
  scale_color_manual(values=c('green','darkgreen','green3','red','brown'))
  #stat_summary(geom='area',aes(y = dormancyInductionRate),fill='red',alpha=0.2)+
  # stat_summary(geom='area',aes(y = endodormancyRate),fill='blue',alpha=0.2)+
  #stat_summary(geom='area',aes(y = ecodormancyRate),fill='orange',alpha=0.9)+
  #stat_summary(geom='area',aes(y = growthRate),fill='green',alpha=0.2)+
  #stat_summary(geom='area',aes(y = greendownRate),fill='darkgreen',alpha=0.2)+
  #stat_summary(geom='area',aes(y = declineRate),fill='orange4',alpha=0.2)

