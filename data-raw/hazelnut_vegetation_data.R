## code to prepare `SWELLparameters` dataset goes here
library(tidyverse)

hazelnut_vegetation_data <- read.csv('data-raw/hazelnut_evi.csv') |>
  left_join(read.csv('data-raw/hazelnut_ndvi.csv')) |> # Replace with actual join column
  filter(year >= 2009) |>
  select(orchard_id, Municipality, latitude, longitude, year, doy, ndvi, evi)

# Save the dataset to the `data/` folder of the package
usethis::use_data(hazelnut_vegetation_data, overwrite = TRUE)

