rm(list=ls())
#install.packages("pkgdown")
#if (!requireNamespace("devtools", quietly = TRUE)) {
#  install.packages("devtools")
#}
#install.packages("jsonlite")
remove.packages("SWELL")

library(devtools)

library(nasapower)
library(jsonlite)
library(tidyverse)


install_github("https://github.com/GeoModelLab/SWELL.git")
library(SWELL)
setwd(dirname(rstudioapi::getActiveDocumentContext()$path))

#source("..\\..\\R\\Main_backup.R")
# 1. load data ----


vegetation_data <- read.csv("..\\..\\inst\\extdata\\files\\referenceData\\pixelsCalibrationEVI.csv")
vegetation_data <- vegetation_data

weather_data <- get_power(
  community = "ag",
  #lonlat = c(unique(vegetation_data$long), unique(vegetation_data$lat)),
  lonlat = c(26, 45),
  pars = c("T2M_MAX", "T2M_MIN"),
  dates = c("2000-01-01", "2024-01-01"),
  temporal_api = "daily"
)

weather_data <- weather_data |>
  rename(Longitude = LON,
         Latitude = LAT,
         Date = YYYYMMDD,
         Tmin = T2M_MIN,
         Tmax = T2M_MAX) |>
  select(-c(DOY, MM, DD, YEAR))

start_year <- 2001
end_year   <- 2021
simplexes  <- 10
iterations <- 1000
SWELLparameters<-SWELL::SWELLparameters
SWELLparameters$beech$parVegetationIndex$minimumVI$max<-0.25
SWELLparameters$beech$parVegetationIndex$minimumVI$min<-0.1
SWELLparameters$beech$parVegetationIndex$maximumVI$max<-0.9
SWELLparameters$beech$parEndodormancy$notLimitingLowerTemperature$value <- -3.4
SWELLparameters$beech$parEndodormancy$limitingLowerTemperature$value <- -10


pixels <- swellCalibration(
  weather_data,
  vegetation_data |> filter(id%in%c('10000a','10006a')),
  vegetationIndex = 'NDVI',
  SWELLparameters = SWELLparameters,
  species = 'beech',
  start_year=start_year,
  end_year=end_year,
  simplexes=1,
  iterations=iterations
  )

ggplot(pixels[[1]] |> filter(year>=2013), aes(x=doy)) +
  stat_summary(geom='line',aes(y = SWELL),size=2)+
  stat_summary(geom='point',aes(y = reference),size=2)+
  scale_color_manual(values=c('green','darkgreen','green3','red','brown'))+
  stat_summary(geom='area',aes(y = dormancyInductionRate),fill='red',alpha=0.2)+
  stat_summary(geom='area',aes(y = endodormancyRate),fill='blue',alpha=0.2)+
  stat_summary(geom='area',aes(y = ecodormancyRate),fill='orange',alpha=0.9)+
  stat_summary(geom='area',aes(y = growthRate),fill='green',alpha=0.2)+
  stat_summary(geom='area',aes(y = greendownRate),fill='darkgreen',alpha=0.2)+
  stat_summary(geom='area',aes(y = declineRate),fill='orange4',alpha=0.2)+
  facet_grid(pixel~year)

results<-pixels$calibration_results
paramPixels<-pixels$parameters_pixels
paramGroups<-pixels$parameters_group

ggplot(pixels[[2]]) + geom_boxplot(aes(y = value,fill=group))+
  facet_wrap(~param+class,scales='free_y')


SWELLparametersCalibrated<-paramGroups

unique(vegetation_data$country)

pixelsValid <- swellValidation(weather_data, vegetation_data, vegetationIndex = 'EVI',
                               SWELL::SWELLparameters,
                               paramGroups,
                               start_year=2011,end_year=2021,validationReplicates = 50)
ggplot(pixelsValid |> filter(year>=2011), aes(x=doy)) +
  stat_summary(geom='line',aes(y = SWELL),size=.4)+
  stat_summary(geom='line',aes(y = SWELL_10),size=.4,color='red')+
  stat_summary(geom='line',aes(y = SWELL_25),size=.4,color='green')+
  stat_summary(geom='line',aes(y = SWELL_40),size=.4,color='blue')+
  stat_summary(geom='line',aes(y = SWELL_60),size=.4,color='blue')+
  stat_summary(geom='line',aes(y = SWELL_75),size=.4,color='green')+
  stat_summary(geom='line',aes(y = SWELL_90),size=.4,color='red')+
  stat_summary(geom='point',aes(y = reference),size=2)+
  scale_color_manual(values=c('green','darkgreen','green3','red','brown'))+
  # stat_summary(geom='area',aes(y = dormancyInductionRate),fill='red',alpha=0.2)+
  # stat_summary(geom='area',aes(y = endodormancyRate),fill='blue',alpha=0.2)+
  # stat_summary(geom='area',aes(y = ecodormancyRate),fill='orange',alpha=0.9)+
  # stat_summary(geom='area',aes(y = growthRate),fill='green',alpha=0.2)+
  # stat_summary(geom='area',aes(y = greendownRate),fill='darkgreen',alpha=0.2)+
  # stat_summary(geom='area',aes(y = declineRate),fill='orange4',alpha=0.2)+
  facet_wrap(~pixel)


pixelsValid <- swellValidation(weather_data, vegetation_data, vegetationIndex = 'EVI',
                               SWELL::SWELLparameters,
                               paramGroups,
                               start_year=2011,end_year=2021,validationReplicates = 50)
