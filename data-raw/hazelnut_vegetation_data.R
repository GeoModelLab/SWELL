## code to prepare `SWELLparameters` dataset goes here
library(tidyverse)

hazelnut_vegetation_data <- read.csv(paste0(getwd(),'/data-raw/hazelnut_evi.csv')) |>
  left_join(read.csv(paste0(getwd(),'/data-raw/hazelnut_ndvi.csv'))) |>
  filter(year >=2009) |>
  select(orchard_id,Municipality,latitude,longitude,year,doy,ndvi,evi)

# Save the dataset to a file
usethis::use_data(hazelnut_vegetation_data,overwrite = T)
