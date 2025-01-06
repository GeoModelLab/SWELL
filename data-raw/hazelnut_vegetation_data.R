## code to prepare `SWELLparameters` dataset goes here
library(tidyverse)

hazelnut_vegetation_data <- read.csv('hazelnut_evi.csv') |>
  left_join(read.csv('hazelnut_ndvi.csv')) |>
  filter(orchard_id %in% unique(NDVI_metrics$pixel),
         year >=2009) |>
  select(orchard_id,Municipality,latitude,longitude,year,doy,ndvi,evi)

# Save the dataset to a file
usethis::use_data(hazelnut_vegetation_data,overwrite = T)
